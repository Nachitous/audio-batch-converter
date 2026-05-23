using System.Runtime.InteropServices;

namespace AudioBatchConverter.Shell.Interop;

internal static class NativeMethods
{
    public const ushort CF_HDROP = 15;
    public const uint TYMED_HGLOBAL = 1;
    public const uint MF_STRING = 0x00000000;
    public const uint MF_BYPOSITION = 0x00000400;
    public const int S_OK = 0;
    public const int E_NOTIMPL = unchecked((int)0x80004001);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, char[]? lpszFile, uint cch);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool InsertMenuItem(IntPtr hmenu, uint uItem, bool fByPosition, ref MENUITEMINFO lpmii);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalUnlock(IntPtr hMem);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MENUITEMINFO
    {
        public uint cbSize;
        public uint fMask;
        public uint fType;
        public uint fState;
        public uint wID;
        public IntPtr hSubMenu;
        public IntPtr hbmpChecked;
        public IntPtr hbmpUnchecked;
        public IntPtr dwItemData;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string dwTypeData;
        public uint cch;
        public IntPtr hbmpItem;
    }

    public const uint MIIM_STRING = 0x00000040;
    public const uint MIIM_FTYPE = 0x00000100;
    public const uint MIIM_ID = 0x00000002;
    public const uint MIIM_STATE = 0x00000001;
    public const uint MFT_STRING = 0x00000000;
    public const uint MFS_ENABLED = 0x00000000;
}
