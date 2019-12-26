using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SystemShare
{
    static class Connection
    {
        private static string Msg = "";
        private static string Response = "";
        private static TcpClient tcpclnt;
        private static Thread ConnectionThread;
        private static bool disconnected;
        private static NetworkStream networkStream;
        private static string Clip = "";

        ///<summary>
        ///Initializes some variables and starts the connection
        ///</summary>
        public static void Connect()
        {
            Thread.Sleep(1000);
            disconnected = false;
            ConnectionThread = new Thread(DoChat);
            ConnectionThread.Priority = ThreadPriority.Highest;
            ConnectionThread.Start();
        }

        ///<summary>
        /// Stops the connection
        ///</summary>
        public static void Disconnect()
        {
            disconnected = true;
            try
            {
                ConnectionThread.Abort();
            }
            catch
            {

            }
        }

        #region Msg decoder

        ///<summary>
        /// Decodes the given command
        ///</summary>
        /// <code>
        /// m - mouse move: m0,500x500;
        /// l - Left click: lu; / ld;
        /// r - Right click: ru; / rd;
        /// w - Middle click: wu; / wd;
        /// s - Wheel x number of lines: s500; / s-500;
        /// k - Key: ku12; / kd12;
        /// d - data request: d;
        /// q - close command: q;
        /// j - logic request: j;
        /// p - paste command: p<3>asd;
        /// c - copy request: c;
        /// </code>
        private static void Decoder()
        {
            string s = "";
            if (!string.IsNullOrWhiteSpace(Msg))
            {
                s = Msg;
                Msg = Msg.Substring(s.Length);
            }
            while (!string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    switch (s[0])
                    {
                        case 'm':
                            MoveMouse(s);
                            break;
                        case 'l':
                            LeftClick(s);
                            break;
                        case 'r':
                            RightClick(s);
                            break;
                        case 'w':
                            MiddleClick(s);
                            break;
                        case 's':
                            Scroll(s);
                            break;
                        case 'k':
                            KeyPress(s);
                            break;
                        case 'd':
                            SendData();
                            break;
                        case 'q':
                            CloseNetwork();
                            break;
                        case 'j':
                            SendLogin();
                            break;
                        case 'p':
                            Paste(ref s);
                            break;
                        case 'c':
                            Copy();
                            break;

                    }
                    if (s.IndexOf(';') >= 0)
                    {
                        s = s.Substring(s.IndexOf(';') + 1);
                    }
                    else
                    {
                        s = "";
                    }
                }
                catch (Exception)
                {
                    s = "";
                }
            }
        }

        /// <summary>
        /// Sets the mouse to the provided coordinates in a string,format: 'm0,500x500;'
        /// </summary>
        /// <param name="s">coded command</param>
        private static void MoveMouse(string s)
        {
            int x = Convert.ToInt32(s.Substring(s.IndexOf(",") + 1, s.IndexOf("x") - s.IndexOf(",") - 1)) + MainForm.Disp[Convert.ToInt32(s.Substring(1, s.IndexOf(",") - 1))].X;
            int y = Convert.ToInt32(s.Substring(s.IndexOf("x") + 1, s.IndexOf(';') - s.IndexOf("x") - 1)) + MainForm.Disp[Convert.ToInt32(s.Substring(1, s.IndexOf(",") - 1))].Y;
            VMouse.SetCursorPosition(x, y);
        }

        /// <summary>
        /// Sends a left click, format: 'lu;' or 'ld;'
        /// </summary>
        /// <param name="s">coded command</param>
        private static void LeftClick(string s)
        {
            if (s[1] == 'd')
            {
                VMouse.MouseEvent(VMouse.MouseEventFlags.LeftDown);
            }
            else if (s[1] == 'u')
            {
                VMouse.MouseEvent(VMouse.MouseEventFlags.LeftUp);
            }
        }

        /// <summary>
        /// Sends a right click, format: 'ru;' or 'rd;'
        /// </summary>
        /// <param name="s">coded command</param>
        private static void RightClick(string s)
        {
            if (s[1] == 'd')
            {
                VMouse.MouseEvent(VMouse.MouseEventFlags.RightDown);
            }
            else if (s[1] == 'u')
            {
                VMouse.MouseEvent(VMouse.MouseEventFlags.RightUp);
            }
        }

        /// <summary>
        /// Sends a middle click, format: 'wu;' or 'wd;'
        /// </summary>
        /// <param name="s">coded command</param>
        private static void MiddleClick(string s)
        {
            if (s[1] == 'd')
            {
                VMouse.MouseEvent(VMouse.MouseEventFlags.MiddleDown);
            }
            else if (s[1] == 'u')
            {
                VMouse.MouseEvent(VMouse.MouseEventFlags.MiddleUp);
            }
        }

        /// <summary>
        /// Sends lines to scroll provided in a string, format: 's500;' or 's-500;'
        /// </summary>
        /// <param name="s"></param>
        private static void Scroll(string s)
        {
            VMouse.MouseWheel(Convert.ToInt32(s.Substring(1, s.IndexOf(';') - 1)));
        }

        /// <summary>
        /// Sends a keypress, format: 'ku12;' or 'kd12;'
        /// </summary>
        /// <param name="s"></param>
        private static void KeyPress(string s)
        {
            if (s[1] == 'd')
            {
                VKeyBoard.KeyDown(Convert.ToByte(s.Substring(2, s.IndexOf(';') - 2)));
            }
            else if (s[1] == 'u')
            {
                VKeyBoard.KeyUp(Convert.ToByte(s.Substring(2, s.IndexOf(';') - 2)));
            }
        }

        /// <summary>
        /// Sends display data in the format 'd,widthxheight;'
        /// </summary>
        private static void SendData()
        {
            for (int i = 0; i < MainForm.Disp.Count; i++)
            {
                Response += "d," + MainForm.Disp[i].Width.ToString() + "x" + MainForm.Disp[i].Height.ToString() + ";";
            }
            Response += "u;";
        }

        /// <summary>
        /// Closes the network stream
        /// </summary>
        private static void CloseNetwork()
        {
            networkStream.Close();
        }

        /// <summary>
        /// Sends a login request
        /// </summary>
        private static void SendLogin()
        {
            Response += "l" + MainForm.mac + ";" + "n," + MainForm.name + ";" + "\0";
        }

        /// <summary>
        /// Updates clipboard with the message, format: 'p<3>asd;'
        /// </summary>
        /// <param name="s"></param>
        private static void Paste(ref string s)
        {
            int textlen = Convert.ToInt32(s.Substring(s.IndexOf('<') + 1, s.IndexOf('>') - s.IndexOf('<') - 1));
            ClipBoard.Set(s.Substring(s.IndexOf('>') + 1, textlen));
            s = s.Substring(s.LastIndexOf(';'));
        }

        /// <summary>
        /// Sends the clipboard
        /// </summary>
        private static void Copy()
        {
            Thread.Sleep(100); /// wait for windows clipboard to update
            ClipBoard.Get();
            Response += "c;";
            while (ClipBoard.ClipWait)
            {
                Thread.Sleep(1); /// wait for thread to update clipboard data
            }
            Clip = ClipBoard.Clip;
        }
        #endregion

        #region Packet exchange

        /// <summary>
        /// The 'main loop'
        /// </summary>
        private static void DoChat()
        {
            Thread.Sleep(1000);
            while (!disconnected)
            {
                tcpclnt = new TcpClient();
                LoopConnect();
                networkStream = tcpclnt.GetStream();
                LoopPacket();
                tcpclnt.Close();
            }
        }

        /// <summary>
        /// Packet i/o
        /// </summary>
        private static void LoopPacket()
        {
            while (!disconnected)
            {
                try
                {
                    byte[] bytesFrom = new byte[1024];
                    int copyLength = 0;
                    networkStream.ReadTimeout = 500;
                    networkStream.Read(bytesFrom, 0, 1024);
                    string dataFromServer = Encoding.UTF8.GetString(bytesFrom);
                    dataFromServer = dataFromServer.Substring(0, dataFromServer.IndexOf("\0"));
                    Msg += dataFromServer;
                    if (Msg[0] == 'p')
                    {
                        copyLength = Convert.ToInt32(Msg.Substring(2, Msg.IndexOf('>') - 2));
                        copyLength += copyLength.ToString().Length;
                        copyLength += 4;
                    }
                    if (Msg.Length >= copyLength)
                    {
                        Decoder();
                        copyLength = 0;
                    }
                    string serverPing = "!;" + Response + "\0";
                    Response = "";
                    byte[] Commandbytes = Encoding.UTF8.GetBytes(serverPing);
                    networkStream.Write(Commandbytes, 0, Commandbytes.Length);
                    if (string.IsNullOrWhiteSpace(dataFromServer))
                    {
                        break;
                    }
                    dataFromServer = null;
                    bytesFrom = new byte[1024];
                    networkStream.Flush();

                    if (!string.IsNullOrWhiteSpace(Clip))
                    {
                        Response += "p<" + Clip.Length.ToString() + ">" + Clip + ";";
                        Clip = "";
                        while (!string.IsNullOrWhiteSpace(Response))
                        {
                            dataFromServer = null;
                            Commandbytes = new byte[1024];
                            networkStream.ReadTimeout = 500;
                            networkStream.Read(Commandbytes, 0, 1024);
                            dataFromServer = Encoding.UTF8.GetString(Commandbytes);
                            dataFromServer = dataFromServer.Substring(0, dataFromServer.IndexOf("\0"));
                            Msg += dataFromServer;
                            dataFromServer = null;
                            Commandbytes = new byte[1024];
                            Decoder();
                            string r;
                            try
                            {
                                r = Response.Substring(0, 1022);
                            }
                            catch (Exception)
                            {
                                r = Response;
                            }
                            Response = Response.Substring(r.Length);
                            serverPing = r + "\0";
                            Commandbytes = Encoding.UTF8.GetBytes(serverPing);
                            networkStream.Write(Commandbytes, 0, Commandbytes.Length);
                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
            Msg = "";
        }

        /// <summary>
        /// Server search
        /// </summary>
        private static void LoopConnect()
        {
            while (!disconnected)
            {
                try
                {
                    tcpclnt.Connect(IPAddress.Parse(MainForm.ip), MainForm.port);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(1);
                }

            }
        }
        #endregion
    }
}
