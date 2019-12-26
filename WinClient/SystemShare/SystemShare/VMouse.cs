using System;
using System.Runtime.InteropServices;

namespace SystemShare
{
    public class VMouse
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x00000800
        }

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

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        ///<summary>
        ///Sets the mouse pointer position to the given coordinates
        ///</summary>
        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        ///<summary>
        ///Gets the coordinates of the mouse pointer
        ///</summary>
        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        ///<summary>
        ///Sends a mouse key event to the operating system
        ///</summary>
        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();
            mouse_event((int)value, position.X, position.Y, 0, 0);
        }

        ///<summary>
        ///Sends x lines of scrolling to the operating system
        ///</summary>
        public static void MouseWheel(int x)
        {
            MousePoint position = GetCursorPosition();
            mouse_event((int)MouseEventFlags.Wheel, position.X, position.Y, x, 0);
        }
    }
}
