using System.Runtime.InteropServices;

namespace AudioBatchConverter.UI;

internal static class NativeFolderPicker
{
    private static readonly Guid ClsidFileOpenDialog = new("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7");

    [ComImport]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileDialog
    {
        [PreserveSig] uint Show(IntPtr parent);
        [PreserveSig] uint SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
        [PreserveSig] uint SetFileTypeIndex(uint iFileType);
        [PreserveSig] uint GetFileTypeIndex(out uint piFileType);
        [PreserveSig] uint Advise(IntPtr pfde, out uint pdwCookie);
        [PreserveSig] uint Unadvise(uint dwCookie);
        [PreserveSig] uint SetOptions(uint fos);
        [PreserveSig] uint GetOptions(out uint pfos);
        [PreserveSig] uint SetDefaultFolder(IShellItem psi);
        [PreserveSig] uint SetFolder(IShellItem psi);
        [PreserveSig] uint GetFolder(out IShellItem ppsi);
        [PreserveSig] uint GetCurrentSelection(out IShellItem ppsi);
        [PreserveSig] uint SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [PreserveSig] uint GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
        [PreserveSig] uint SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        [PreserveSig] uint SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig] uint SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        [PreserveSig] uint GetResult(out IShellItem ppsi);
        [PreserveSig] uint AddPlace(IShellItem psi, uint fdap);
        [PreserveSig] uint SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        [PreserveSig] uint Close(uint hr);
        [PreserveSig] uint SetClientGuid(ref Guid guid);
        [PreserveSig] uint ClearClientData();
        [PreserveSig] uint SetFilter(IntPtr pFilter);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        [PreserveSig] uint BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
        [PreserveSig] uint GetParent(out IShellItem ppsi);
        [PreserveSig] uint GetDisplayName(uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
        [PreserveSig] uint GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        [PreserveSig] uint Compare(IShellItem psi, uint hint, out int piOrder);
    }

    private const uint FOS_PICKFOLDERS    = 0x00000020;
    private const uint FOS_FORCEFILESYSTEM = 0x00000040;
    private const uint SIGDN_FILESYSPATH  = 0x80058000;
    private const uint S_OK               = 0x00000000;
    private const uint HRESULT_CANCELLED  = 0x800704C7;

    /// <summary>
    /// Shows the native Windows folder picker. Returns the selected path, or null if cancelled.
    /// </summary>
    public static string? Pick(IntPtr ownerHwnd, string title = "Select a folder")
    {
        var type = Type.GetTypeFromCLSID(ClsidFileOpenDialog, throwOnError: false);
        if (type is null) return null;

        var dialog = (IFileDialog)Activator.CreateInstance(type)!;
        try
        {
            dialog.SetOptions(FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);
            dialog.SetTitle(title);

            var hr = dialog.Show(ownerHwnd);
            if (hr == HRESULT_CANCELLED || hr != S_OK) return null;

            dialog.GetResult(out var item);
            try
            {
                item.GetDisplayName(SIGDN_FILESYSPATH, out var path);
                return path;
            }
            finally
            {
                Marshal.ReleaseComObject(item);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(dialog);
        }
    }
}
