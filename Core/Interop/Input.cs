using System.Runtime.InteropServices;

namespace Core.Interop
{
    public class Input
    {
        private const int KeyDown = 0x8000;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        public static bool GetKey(VirtualKey virtualKey)
        {
            return (GetAsyncKeyState((int)virtualKey) & KeyDown) == KeyDown;
        }
    }

    /// <summary>
    /// Windows virtual-key codes used by the hook.
    /// </summary>
    public enum VirtualKey
    {
        End = 0x23,
        Insert = 0x2D,
        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad5 = 0x65
    }
}
