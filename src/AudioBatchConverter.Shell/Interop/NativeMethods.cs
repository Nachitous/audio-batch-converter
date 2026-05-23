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

    public const uint MIIM_STRING  = 0x00000040;
    public const uint MIIM_FTYPE   = 0x00000100;
    public const uint MIIM_ID      = 0x00000002;
    public const uint MIIM_STATE   = 0x00000001;
    public const uint MIIM_BITMAP  = 0x00000080;
    public const uint MFT_STRING   = 0x00000000;
    public const uint MFS_ENABLED  = 0x00000000;

    // Icon → premultiplied-alpha HBITMAP (required for Vista+ themed menus)
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, uint nIconIndex);

    [DllImport("user32.dll")]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateDIBSection(IntPtr hDC, ref BITMAPV5HEADER bmi, uint usage, out IntPtr bits, IntPtr hSection, uint offset);

    [DllImport("user32.dll")]
    public static extern bool DrawIconEx(IntPtr hDC, int x, int y, IntPtr hIcon, int cx, int cy, uint istepIfAniCur, IntPtr hbrFlickerFreeDraw, uint diFlags);

    public const uint DI_NORMAL      = 0x3;
    public const uint DIB_RGB_COLORS = 0;
    public const uint BI_BITFIELDS   = 3;

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPV5HEADER
    {
        public uint    bV5Size;
        public int     bV5Width;
        public int     bV5Height;
        public ushort  bV5Planes;
        public ushort  bV5BitCount;
        public uint    bV5Compression;
        public uint    bV5SizeImage;
        public int     bV5XPelsPerMeter;
        public int     bV5YPelsPerMeter;
        public uint    bV5ClrUsed;
        public uint    bV5ClrImportant;
        public uint    bV5RedMask;
        public uint    bV5GreenMask;
        public uint    bV5BlueMask;
        public uint    bV5AlphaMask;
        public uint    bV5CSType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
        public byte[]  bV5Endpoints;
        public uint    bV5GammaRed;
        public uint    bV5GammaGreen;
        public uint    bV5GammaBlue;
        public uint    bV5Intent;
        public uint    bV5ProfileData;
        public uint    bV5ProfileSize;
        public uint    bV5Reserved;
    }
}
