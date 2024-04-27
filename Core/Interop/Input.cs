using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Core.Interop
{
    public class Input
    {
        private const int KeyDown = 0x8000;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        public static bool GetKey(Keys virtualKey)
        {
            return (GetAsyncKeyState((int)virtualKey) & KeyDown) == KeyDown;
        }
    }
}
