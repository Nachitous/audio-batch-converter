using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AudioBatchConverter.Shell.Interop;

namespace AudioBatchConverter.Shell;

/// <summary>
/// COM shell extension that adds "Convert to MP3" and "Convert folder…" to the
/// Windows Explorer context menu.
/// CLSID: {A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
[ProgId("AudioBatchConverter.ContextMenuHandler")]
public class ContextMenuHandler : IShellExtInit, IContextMenu
{
    // Command offsets relative to idCmdFirst
    private const uint CmdConvert = 0;
    private const uint CmdOpenFolder = 1;
    private const uint VerbsAdded = 2;

    private static readonly (uint Offset, string Verb, string Label)[] Verbs =
    [
        (CmdConvert,    "ConvertToMp3",       "Convert to MP3"),
        (CmdOpenFolder, "ConvertFolderPicker", "Convert folder…"),
    ];

    private readonly List<string> _selectedPaths = [];

    // ── IShellExtInit ──────────────────────────────────────────────────────────

    void IShellExtInit.Initialize(IntPtr pidlFolder, object pDataObj, IntPtr hKeyProgID)
    {
        _selectedPaths.Clear();

        if (pDataObj is not Interop.IDataObject dataObject)
            return;

        var fmt = new FORMATETC
        {
            cfFormat = NativeMethods.CF_HDROP,
            ptd = IntPtr.Zero,
            dwAspect = 1,   // DVASPECT_CONTENT
            lindex = -1,
            tymed = NativeMethods.TYMED_HGLOBAL,
        };

        if (dataObject.GetData(ref fmt, out var medium) != NativeMethods.S_OK)
            return;

        try
        {
            var hDrop = NativeMethods.GlobalLock(medium.unionmember);
            if (hDrop == IntPtr.Zero) return;

            try
            {
                var count = NativeMethods.DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                for (uint i = 0; i < count; i++)
                {
                    var buf = new char[260];
                    var len = NativeMethods.DragQueryFile(hDrop, i, buf, (uint)buf.Length);
                    if (len > 0)
                        _selectedPaths.Add(new string(buf, 0, (int)len));
                }
            }
            finally
            {
                NativeMethods.GlobalUnlock(medium.unionmember);
            }
        }
        finally
        {
            if (medium.pUnkForRelease != IntPtr.Zero)
                Marshal.Release(medium.pUnkForRelease);
        }
    }

    // ── IContextMenu ───────────────────────────────────────────────────────────

    int IContextMenu.QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags)
    {
        if ((uFlags & 0x000F) > 1)   // CMF_DEFAULTONLY or similar exclusive flags
            return NativeMethods.S_OK;

        uint slot = indexMenu;
        foreach (var (offset, _, label) in Verbs)
        {
            var mii = new NativeMethods.MENUITEMINFO
            {
                cbSize = (uint)Marshal.SizeOf<NativeMethods.MENUITEMINFO>(),
                fMask = NativeMethods.MIIM_STRING | NativeMethods.MIIM_FTYPE
                      | NativeMethods.MIIM_ID | NativeMethods.MIIM_STATE,
                fType = NativeMethods.MFT_STRING,
                fState = NativeMethods.MFS_ENABLED,
                wID = idCmdFirst + offset,
                dwTypeData = label,
            };
            NativeMethods.InsertMenuItem(hmenu, slot++, true, ref mii);
        }

        // Return the number of verbs added
        return NativeMethods.S_OK | (int)VerbsAdded;
    }

    int IContextMenu.InvokeCommand(IntPtr pici)
    {
        // CMINVOKECOMMANDINFO layout on x64:
        //   DWORD cbSize  (+0, 4 bytes)
        //   DWORD fMask   (+4, 4 bytes)
        //   HWND  hwnd    (+8, 8 bytes)
        //   LPCSTR lpVerb (+16, 8 bytes) — MAKEINTRESOURCE(n) when HIWORD==0
        var lpVerb = Marshal.ReadIntPtr(pici, 16);
        var hiWord = (ulong)lpVerb.ToInt64() >> 16;
        uint cmdId;

        if (hiWord == 0)
        {
            cmdId = (uint)((ulong)lpVerb.ToInt64() & 0xFFFF);
        }
        else
        {
            // String verb — map back to offset
            var verbStr = Marshal.PtrToStringAnsi(lpVerb) ?? "";
            var match = Array.Find(Verbs, v => v.Verb == verbStr);
            cmdId = match == default ? CmdConvert : match.Offset;
        }

        if (cmdId == CmdOpenFolder)
            LaunchUiWithFolderPicker();
        else
            LaunchUi(_selectedPaths);

        return NativeMethods.S_OK;
    }

    int IContextMenu.GetCommandString(UIntPtr idCmd, uint uType, IntPtr pReserved, byte[] pszName, uint cchMax)
    {
        var id = idCmd.ToUInt32();
        var entry = Array.Find(Verbs, v => v.Offset == id);
        if (entry == default) return NativeMethods.E_NOTIMPL;

        bool unicode = (uType & 0x04) != 0;
        byte[] bytes = unicode
            ? Encoding.Unicode.GetBytes(entry.Verb + '\0')
            : Encoding.ASCII.GetBytes(entry.Verb + '\0');
        var maxBytes = unicode ? (int)cchMax * 2 : (int)cchMax;
        Buffer.BlockCopy(bytes, 0, pszName, 0, Math.Min(bytes.Length, maxBytes));
        return NativeMethods.S_OK;
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static string ResolveUiExe()
    {
        var exeDir = Path.GetDirectoryName(typeof(ContextMenuHandler).Assembly.Location)!;
        return Path.Combine(exeDir, "AudioBatchConverter.UI.exe");
    }

    private static void LaunchUi(IEnumerable<string> paths)
    {
        var uiExe = ResolveUiExe();
        if (!File.Exists(uiExe)) { ShowMissingExeError(uiExe); return; }

        var args = string.Join(" ", paths.Select(p => $"\"{p}\""));
        Process.Start(new ProcessStartInfo(uiExe, args) { UseShellExecute = true });
    }

    private static void LaunchUiWithFolderPicker()
    {
        var uiExe = ResolveUiExe();
        if (!File.Exists(uiExe)) { ShowMissingExeError(uiExe); return; }

        // --pick-folder tells the UI to show a FolderBrowserDialog on startup
        Process.Start(new ProcessStartInfo(uiExe, "--pick-folder") { UseShellExecute = true });
    }

    private static void ShowMissingExeError(string path) =>
        System.Windows.Forms.MessageBox.Show(
            $"UI executable not found:\n{path}",
            "Audio Batch Converter",
            System.Windows.Forms.MessageBoxButtons.OK,
            System.Windows.Forms.MessageBoxIcon.Error);

    // ── COM registration helpers ───────────────────────────────────────────────

    [ComRegisterFunction]
    private static void Register(Type t)
    {
        var clsid = $"{{{t.GUID}}}";
        AddHandler(@"Directory\shellex\ContextMenuHandlers\ConvertToMp3", clsid);
        AddHandler(@"Directory\Background\shellex\ContextMenuHandlers\ConvertToMp3", clsid);
        AddHandler(@"SystemFileAssociations\audio\shellex\ContextMenuHandlers\ConvertToMp3", clsid);
    }

    [ComUnregisterFunction]
    private static void Unregister(Type t)
    {
        RemoveHandler(@"Directory\shellex\ContextMenuHandlers\ConvertToMp3");
        RemoveHandler(@"Directory\Background\shellex\ContextMenuHandlers\ConvertToMp3");
        RemoveHandler(@"SystemFileAssociations\audio\shellex\ContextMenuHandlers\ConvertToMp3");
    }

    private static void AddHandler(string keyPath, string clsid)
    {
        using var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(keyPath);
        key?.SetValue("", clsid);
    }

    private static void RemoveHandler(string keyPath)
    {
        try { Microsoft.Win32.Registry.ClassesRoot.DeleteSubKey(keyPath, throwOnMissingSubKey: false); }
        catch { /* ignore */ }
    }
}
