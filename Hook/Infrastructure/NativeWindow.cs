using System;
using System.Runtime.InteropServices;

namespace Hook.Infrastructure
{
    /// <summary>
    /// A hidden window to create the temporary swap chain for.
    /// </summary>
    internal static partial class NativeWindow
    {
        private const int OverlappedWindowStyle = 0x00CF0000;

        public static IntPtr Create()
        {
            // The predefined STATIC class saves registering a window class of our own.
            return CreateWindowEx(0, "STATIC", "DirectXHook", OverlappedWindowStyle, 0, 0, 100, 100,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        public static void Destroy(IntPtr window)
        {
            DestroyWindow(window);
        }

        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style,
            int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(IntPtr window);
    }
}
