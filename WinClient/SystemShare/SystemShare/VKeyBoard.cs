using System.Runtime.InteropServices;

namespace SystemShare
{
    class VKeyBoard
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        /// <summary>
        /// Sends a keyboarddown event to the operating system.
        /// </summary>
        public static void KeyDown(byte Key)
        {
            keybd_event(Key, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
        }

        /// <summary>
        /// Sends a keyboardup event to the operating system.
        /// </summary>
        public static void KeyUp(byte Key)
        {
            keybd_event(Key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
    }
}
