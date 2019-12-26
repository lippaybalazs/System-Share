using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemShare
{
    public class KeyBoardHook
    {
        #region Dependencies
        public struct KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        public delegate int keyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);
        public static bool CTRL = false;
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;
        IntPtr hhook = IntPtr.Zero;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        private static keyboardHookProc callbackDelegate;
        #endregion

        /// <summary>
        /// Hooks to keyboard
        /// </summary>
        public void Hook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            callbackDelegate = new keyboardHookProc(HookProc);
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, callbackDelegate, hInstance, 0);
        }

        /// <summary>
        /// Unhooks from keyboard
        /// </summary>
        public void Unhook()
        {
            bool ok = UnhookWindowsHookEx(hhook);
            if (!ok) throw new Win32Exception();
            callbackDelegate = null;
        }

        /// <summary>
        /// Sends keypress events
        /// </summary>
        private int HookProc(int code, int wParam, ref KeyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                Keys key = (Keys)lParam.vkCode;
                KeyEventArgs kea = new KeyEventArgs(key);
                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                {
                    KeyDown(this, kea);
                }
                else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                {
                    KeyUp(this, kea);
                }
                if (kea.Handled)
                {
                    return 1;
                }
            }
            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }

    }
}