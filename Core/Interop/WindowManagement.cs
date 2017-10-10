using System;
using System.Runtime.InteropServices;

namespace Core.Interop
{
    public static class WindowManagement
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr windowHandle, [MarshalAs(UnmanagedType.I4)]WindowOptions nCmdShow);
    }

    public enum WindowOptions
    {
        /// <summary>
        /// Hides the window and activates another.
        /// </summary>
        Hide = 0,
        /// <summary>
        /// Shows, activates and restores the window to its default position and size.
        /// </summary>
        ShowNormal,
        /// <summary>
        /// Activates the window and shows it as minimized.
        /// </summary>
        ShowMinimized,
        /// <summary>
        /// Activates the window and shows it as maximized.
        /// </summary>
        ShowMaximized,
        /// <summary>
        /// Similiar to <see cref="WindowOptions.ShowNormal"/>, except it does not activate the window.
        /// </summary>
        ShowNormalNoFocus,
        /// <summary>
        /// Shows and activates the window in its current position and size.
        /// </summary>
        Show,
        /// <summary>
        /// Minimize the window and activate the next window in Z-order.
        /// </summary>
        Minimize,
        /// <summary>
        /// Similiar to <see cref="WindowOptions.ShowMinimized"/>, except it does not activate the window.
        /// </summary>
        ShowMinimizedNoFocus,
        /// <summary>
        /// Similiar to <see cref="WindowOptions.Show"/>, except it does not activate the window.
        /// </summary>
        ShowNoFocus,
        /// <summary>
        /// Activates and displays the window to its original position and size.
        /// </summary>
        Restore,
        /// <summary>
        /// Shows the window using its default position and size.
        /// </summary>
        ShowDefault,
        /// <summary>
        /// Minimizes a window, even if the target window is not responding.
        /// </summary>
        ForceMinimize
    }
}
