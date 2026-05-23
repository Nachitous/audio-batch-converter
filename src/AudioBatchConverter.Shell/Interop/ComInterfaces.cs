using System.Runtime.InteropServices;
using System.Text;

namespace AudioBatchConverter.Shell.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214E8-0000-0000-C000-000000000046")]
internal interface IShellExtInit
{
    void Initialize(
        [In] IntPtr pidlFolder,
        [In, MarshalAs(UnmanagedType.Interface)] object pDataObj,
        [In] IntPtr hKeyProgID);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214E4-0000-0000-C000-000000000046")]
internal interface IContextMenu
{
    [PreserveSig]
    int QueryContextMenu(
        [In] IntPtr hmenu,
        [In] uint indexMenu,
        [In] uint idCmdFirst,
        [In] uint idCmdLast,
        [In] uint uFlags);

    [PreserveSig]
    int InvokeCommand([In] IntPtr pici);

    [PreserveSig]
    int GetCommandString(
        [In] UIntPtr idCmd,
        [In] uint uType,
        [In] IntPtr pReserved,
        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pszName,
        [In] uint cchMax);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("0000010e-0000-0000-C000-000000000046")]
internal interface IDataObject
{
    // We only need GetData for CF_HDROP; all other methods can be stubs.
    [PreserveSig]
    int GetData(ref FORMATETC format, out STGMEDIUM medium);

    [PreserveSig]
    int GetDataHere(ref FORMATETC format, ref STGMEDIUM medium);

    [PreserveSig]
    int QueryGetData(ref FORMATETC format);

    [PreserveSig]
    int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut);

    [PreserveSig]
    int SetData(ref FORMATETC format, ref STGMEDIUM medium, bool release);

    [PreserveSig]
    int EnumFormatEtc(uint direction, out IntPtr enumFormatEtc);

    [PreserveSig]
    int DAdvise(ref FORMATETC format, uint advf, IntPtr advSink, out uint connection);

    [PreserveSig]
    int DUnadvise(uint connection);

    [PreserveSig]
    int EnumDAdvise(out IntPtr enumAdvise);
}

[StructLayout(LayoutKind.Sequential)]
internal struct FORMATETC
{
    public ushort cfFormat;
    public IntPtr ptd;
    public uint dwAspect;
    public int lindex;
    public uint tymed;
}

[StructLayout(LayoutKind.Sequential)]
internal struct STGMEDIUM
{
    public uint tymed;
    public IntPtr unionmember;
    public IntPtr pUnkForRelease;
}
