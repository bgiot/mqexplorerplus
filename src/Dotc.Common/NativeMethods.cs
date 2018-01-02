using System;
using System.Runtime.InteropServices;

namespace Dotc.Common
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        internal static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

    }
}
