using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemShare
{
    class FullScreenCheck
    {
        #region Dependencies
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        #endregion

        private static Screen screen;
        public static List<string> Skip = new List<string>();

        /// <summary>
        /// Reads skip data from database
        /// </summary>
        private static void GetSkipData()
        {
            Skip.Add("SystemShare");
            try
            {
                using (StreamReader readtext = new StreamReader(MainForm.dir + "FullScreenEvasion.txt"))
                {
                    while (!readtext.EndOfStream)
                    {
                        string name = readtext.ReadLine();
                        Skip.Add(name.Substring(0, name.LastIndexOf('.')));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Checks if something is fullscreened, excludes 'skipped' elements
        /// </summary>
        /// <returns></returns>
        public static bool IsForegroundFullScreen()
        {
            if (!MainForm.fullscreen)
            {
                return false;
            }
            else
            {
                if (screen == null)
                {
                    screen = Screen.PrimaryScreen;
                }
                if (Skip.Count() == 0)
                {
                    GetSkipData();
                }
                RECT rect = new RECT();
                IntPtr hWnd = GetForegroundWindow();
                GetWindowRect(new HandleRef(null, hWnd), ref rect);
                GetWindowThreadProcessId(hWnd, out uint procId);
                var proc = System.Diagnostics.Process.GetProcessById((int)procId);
                for (int i = 0; i < Skip.Count(); i++)
                {
                    if (proc.ProcessName == Skip[i])
                    {
                        return false;
                    }
                }
                if (screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
    }
}
