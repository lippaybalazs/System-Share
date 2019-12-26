using System.Runtime.InteropServices;

namespace SystemShare
{
    public class VMouse
    {
        #region Dependencies
        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);
        #endregion

        /// <summary>
        /// Sets cursor position
        /// </summary>
        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        /// <summary>
        /// Gets cursor position
        /// </summary>
        /// <returns></returns>
        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint)
            {
                currentMousePoint = new MousePoint(0, 0);
            }
            return currentMousePoint;
        }

        
    }
}
