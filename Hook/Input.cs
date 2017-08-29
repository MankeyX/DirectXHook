using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hook
{
    public class Input
    {
        private const int KeyDown = 0x8000;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        public static bool GetKey(Keys virtualKey)
        {
            return 0 != (GetAsyncKeyState((int)virtualKey) & KeyDown);
        }
    }
}
