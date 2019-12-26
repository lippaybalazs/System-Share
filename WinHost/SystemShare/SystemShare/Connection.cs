using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SystemShare
{
    class Connection
    {
        private static Thread ConnectionThread;
        private static Thread MouseThread;
        private static bool disconnected = false;
        public static List<HandleClinet> Clients = new List<HandleClinet>();
        public static int focus = 0;
        public static string Keys = "";
        public static string Mice = "";
        public static bool mode = true;
        public static int selected = 0;
        public static string Clip = "";
        public static int poli = 0;

        /// <summary>
        /// Starts server
        /// </summary>
        public static void Connect()
        {
            Thread.Sleep(1000);
            disconnected = false;
            ConnectionThread = new Thread(Start);
            MouseThread = new Thread(GatherCommands);
            ConnectionThread.Start();
            MouseThread.Start();
        }

        /// <summary>
        /// Stops server
        /// </summary>
        public static void Disconnect()
        {
            FullScreenCheck.Skip.Clear();
            disconnected = true;
            try
            {
                for (int i = 1; i < Clients.Count(); i++)
                {
                    for (int j = 0; j < Clients[i].Area.Count(); j++)
                    {
                        Clients[i].Response += "q;";
                        Clients[i].SendCommand();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Starts TCP Server
        /// </summary>
        public static void Start()
        {
            Clients.Clear();
            Clients.Add(new HandleClinet());
            Clients[0].mac = MainForm.mac;
            for (int i = 0; i < MainForm.Comp[0].Monitors.Count(); i++)
            {
                Clients[0].Area.Add(new Display(MainForm.Comp[0].Monitors[i].Bounds));
            }
            Clients[0].name = MainForm.name;
            CheckDistAll();
            TcpListener listen = new TcpListener(IPAddress.Any, MainForm.port);
            listen.Start();
            while (!disconnected)
            {
                if (!listen.Pending())
                {
                    Thread.Sleep(500);
                    continue;
                }
                TcpClient clientSocket = listen.AcceptTcpClient();
                Clients.Add(new HandleClinet(clientSocket));
                Clients[Clients.Count() - 1].SendCommand();
            }
            listen.Stop();
        }

        /// <summary>
        /// Piles up commands to be sent
        /// </summary>
        public static void GatherCommands()
        {
            int counter = 0;
            while (!disconnected)
            {
                VMousePosition.SetPos(); // update position
                try
                {
                    bool onit = false;
                    for (int i = 1; i < Clients.Count(); i++)
                    {
                        for (int j = 0; j < Clients[i].Area.Count(); j++)
                        {
                            string message = "";
                            if (counter >= 100) // ping every 100 ticks
                            {
                                message += "v;";
                            }
                            if (selected == i)
                            {
                                if (!string.IsNullOrWhiteSpace(Clip)) // send clipboard
                                {
                                    Clients[i].Clip = Clip;
                                    Clip = "";
                                }
                                if (!string.IsNullOrWhiteSpace(Keys)) // send keypress
                                {
                                    string temp = Keys;
                                    Keys = Keys.Substring(temp.Length);
                                    message += temp;
                                }
                            }
                            if (Clients[i].Area[j].Bounds.Contains(VMousePosition.Pos)) // send mouse 
                            {
                                if (!FullScreenCheck.IsForegroundFullScreen()) // if not fullscreen
                                {
                                    if (!string.IsNullOrWhiteSpace(Mice)) // send mouse presses
                                    {
                                        string temp = Mice;
                                        Mice = Mice.Substring(temp.Length);
                                        message += temp;
                                    }
                                    mode = false;
                                    onit = true;
                                    Clients[i].was = true;
                                    if (VMousePosition.Prev != VMousePosition.Pos) // send mouse movement
                                    {
                                        message += "m" + j.ToString() + "," + (VMousePosition.Pos.X - Clients[i].Area[j].Bounds.X).ToString() + "x" + (VMousePosition.Pos.Y - Clients[i].Area[j].Bounds.Y).ToString() + ";";
                                    }
                                }
                            }
                            else
                            {
                                if (Clients[i].was) // hide unused mouse pointer
                                {
                                    Clients[i].was = false;
                                    message += "m0,0x" + Clients[i].Area[0].Bounds.Width.ToString() + ";";
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(message)) // send piled up commands
                            {
                                Clients[i].Response += message;
                                if (!Clients[i].SendCommand())
                                {
                                    Clients.RemoveAt(i);
                                    mode = true;
                                    VMousePosition.on = 0;
                                    VMouse.SetCursorPosition(0, 0);
                                    ResetOnline();
                                }
                            }
                        }
                    }
                    if (!onit)
                    {
                        if (!mode)
                        {
                            mode = true;
                            for (int i = 0; i < Clients[0].Area.Count(); i++)
                            {
                                if (Clients[0].Area[i].Bounds.Contains(VMousePosition.Pos)) // overwrite windows display  configuration
                                {
                                    VMousePosition.on = i;
                                    VMousePosition.Prev = VMousePosition.Pos;
                                    VMouse.SetCursorPosition(MainForm.Monit[i].Bounds.X - Clients[0].Area[i].Bounds.X + VMousePosition.Pos.X, MainForm.Monit[i].Bounds.Y - Clients[0].Area[i].Bounds.Y + VMousePosition.Pos.Y);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                counter++;
                if (counter > 100)
                {
                    counter = 0;
                }
                Thread.Sleep(1); // anti-cpu-fryer
            }
        }

        /// <summary>
        /// Collision detection for local displays
        /// </summary>
        public static bool CheckCollidedMain(int n, int m)
        {
            for (int i = 0; i < MainForm.Comp.Count; i++)
            {
                for (int j = 0; j < MainForm.Comp[i].Monitors.Count; j++)
                {
                    if (i != n || j != m)
                    {
                        Rectangle rec = Rectangle.Intersect(MainForm.Comp[n].Monitors[m].Bounds, MainForm.Comp[i].Monitors[j].Bounds);
                        if (rec.Width > 1 && rec.Height > 1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Collision detection for online-displays
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool CheckCollided(int n, int m)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                for (int j = 0; j < Clients[i].Area.Count; j++)
                {
                    if (!(i == n && j == m))
                    {
                        Rectangle rec = Rectangle.Intersect(Clients[n].Area[m].Bounds, Clients[i].Area[j].Bounds);
                        if (rec.Width > 1 && rec.Height > 1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Automatic display allignment
        /// </summary>
        public static bool CheckDist(int a, int b)
        {
            bool was = false;
            int close1 = 0;
            int close2 = 0;
            int dist = 0;
            for (int i = 0; i < Clients.Count(); i++)
            {
                for (int j = 0; j < Clients[i].Area.Count(); j++)
                {
                    if ((i != a || j != b) && Clients[i].Area[j].poli != Clients[a].Area[b].poli)
                    {
                        close1 = i;
                        close2 = j;
                        int x1 = Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2;
                        int y1 = Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2;
                        int x2 = Clients[i].Area[j].Bounds.X + Clients[i].Area[j].Bounds.Width / 2;
                        int y2 = Clients[i].Area[j].Bounds.Y + Clients[i].Area[j].Bounds.Height / 2;
                        if (x1 < x2)
                        {
                            if (Clients[a].Area[b].Bounds.Contains(Clients[a].Area[b].Bounds.Right - 1, ((Clients[a].Area[b].Bounds.Right - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((Clients[a].Area[b].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = Clients[a].Area[b].Bounds.Right;
                            }
                            if (Clients[i].Area[j].Bounds.Contains(Clients[i].Area[j].Bounds.Left + 1, ((Clients[i].Area[j].Bounds.Left + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((Clients[i].Area[j].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = Clients[i].Area[j].Bounds.Left;
                            }
                            if (y1 > y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Top + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Top;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Bottom + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Bottom + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Bottom;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Top + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = Clients[a].Area[b].Bounds.Right;
                                x2 = Clients[i].Area[j].Bounds.Left;
                            }
                        }
                        else if (x1 > x2)
                        {
                            if (Clients[a].Area[b].Bounds.Contains(Clients[a].Area[b].Bounds.Left - 1, ((Clients[a].Area[b].Bounds.Left - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((Clients[a].Area[b].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = Clients[a].Area[b].Bounds.Left;
                            }
                            if (Clients[i].Area[j].Bounds.Contains(Clients[i].Area[j].Bounds.Right + 1, ((Clients[i].Area[j].Bounds.Right + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((Clients[i].Area[j].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = Clients[i].Area[j].Bounds.Right;
                            }
                            if (y1 > y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Top + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Top;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Bottom + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Bottom + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Bottom;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Top + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = Clients[a].Area[b].Bounds.Left;
                                x2 = Clients[i].Area[j].Bounds.Right;
                            }
                        }
                        else
                        {
                            if (y1 < y2)
                            {
                                y1 = Clients[a].Area[b].Bounds.Bottom;
                                y2 = Clients[i].Area[j].Bounds.Top;
                            }
                            else
                            {
                                y1 = Clients[a].Area[b].Bounds.Top;
                                y2 = Clients[i].Area[j].Bounds.Bottom;
                            }
                        }
                        dist = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        break;
                    }
                }
            }
            for (int i = 0; i < Clients.Count(); i++)
            {
                for (int j = 0; j < Clients[i].Area.Count(); j++)
                {
                    if ((i != a || j != b) && Clients[i].Area[j].poli != Clients[a].Area[b].poli)
                    {
                        int x1 = Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2;
                        int y1 = Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2;
                        int x2 = Clients[i].Area[j].Bounds.X + Clients[i].Area[j].Bounds.Width / 2;
                        int y2 = Clients[i].Area[j].Bounds.Y + Clients[i].Area[j].Bounds.Height / 2;
                        if (x1 < x2)
                        {
                            if (Clients[a].Area[b].Bounds.Contains(Clients[a].Area[b].Bounds.Right - 1, ((Clients[a].Area[b].Bounds.Right - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((Clients[a].Area[b].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = Clients[a].Area[b].Bounds.Right;
                            }
                            if (Clients[i].Area[j].Bounds.Contains(Clients[i].Area[j].Bounds.Left + 1, ((Clients[i].Area[j].Bounds.Left + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((Clients[i].Area[j].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = Clients[i].Area[j].Bounds.Left;
                            }
                            if (y1 > y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Top + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Top;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Bottom + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Bottom + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Bottom;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Top + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = Clients[a].Area[b].Bounds.Right;
                                x2 = Clients[i].Area[j].Bounds.Left;
                            }
                        }
                        else if (x1 > x2)
                        {
                            if (Clients[a].Area[b].Bounds.Contains(Clients[a].Area[b].Bounds.Left - 1, ((Clients[a].Area[b].Bounds.Left - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((Clients[a].Area[b].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = Clients[a].Area[b].Bounds.Left;
                            }
                            if (Clients[i].Area[j].Bounds.Contains(Clients[i].Area[j].Bounds.Right + 1, ((Clients[i].Area[j].Bounds.Right + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((Clients[i].Area[j].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = Clients[i].Area[j].Bounds.Right;
                            }
                            if (y1 > y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Top + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Top;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Bottom + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (Clients[a].Area[b].Bounds.Contains((Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[a].Area[b].Bounds.Bottom + 1))
                                {
                                    x1 = (Clients[a].Area[b].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = Clients[a].Area[b].Bounds.Bottom;
                                }
                                if (Clients[i].Area[j].Bounds.Contains((Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), Clients[i].Area[j].Bounds.Top + 1))
                                {
                                    x2 = (Clients[i].Area[j].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = Clients[i].Area[j].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = Clients[a].Area[b].Bounds.Left;
                                x2 = Clients[i].Area[j].Bounds.Right;
                            }
                        }
                        else
                        {
                            if (y1 < y2)
                            {
                                y1 = Clients[a].Area[b].Bounds.Bottom;
                                y2 = Clients[i].Area[j].Bounds.Top;
                            }
                            else
                            {
                                y1 = Clients[a].Area[b].Bounds.Top;
                                y2 = Clients[i].Area[j].Bounds.Bottom;
                            }
                        }
                        int distThis = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        if (dist > distThis)
                        {
                            dist = distThis;
                            close1 = i;
                            close2 = j;
                        }
                    }
                }
            }
            if (Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2 < Clients[close1].Area[close2].Bounds.X + Clients[close1].Area[close2].Bounds.Width / 2)
            {
                if (Clients[a].Area[b].Bounds.Right > Clients[close1].Area[close2].Bounds.Left)
                {
                    if (Clients[a].Area[b].Bounds.Y > Clients[close1].Area[close2].Bounds.Y)
                    {
                        int prev1 = Clients[a].Area[b].Bounds.Y;
                        do
                        {
                            Clients[a].Area[b].Bounds.Y--;
                        } while (CheckCollided(a, b));
                        Clients[a].Area[b].Bounds.Y++;
                        if (Clients[a].Area[b].Bounds.Y != prev1)
                        {
                            was = true;
                        }

                    }
                    else
                    {
                        int prev1 = Clients[a].Area[b].Bounds.Y;
                        do
                        {
                            Clients[a].Area[b].Bounds.Y++;
                        } while (CheckCollided(a, b));
                        Clients[a].Area[b].Bounds.Y--;
                        if (Clients[a].Area[b].Bounds.Y != prev1)
                        {
                            was = true;
                        }
                    }
                }
                else
                {
                    if (Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2 < Clients[close1].Area[close2].Bounds.Y + Clients[close1].Area[close2].Bounds.Height / 2)
                    {
                        if (Clients[a].Area[b].Bounds.Bottom > Clients[close1].Area[close2].Bounds.Top)
                        {
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            do
                            {
                                Clients[a].Area[b].Bounds.X++;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X--;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                        }
                        else
                        {
                            int x1 = Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2;
                            int y1 = Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2;
                            int x2 = Clients[close1].Area[close2].Bounds.X + Clients[close1].Area[close2].Bounds.Width / 2;
                            int y2 = Clients[close1].Area[close2].Bounds.Y + Clients[close1].Area[close2].Bounds.Height / 2;
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            int prev2;
                            do
                            {
                                prev2 = Clients[a].Area[b].Bounds.Y;
                                Clients[a].Area[b].Bounds.X++;
                                Clients[a].Area[b].Bounds.Y = ((Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - Clients[a].Area[b].Bounds.Height / 2;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X--;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                            Clients[a].Area[b].Bounds.Y = prev2;
                        }
                    }
                    else
                    {
                        if (Clients[a].Area[b].Bounds.Top < Clients[close1].Area[close2].Bounds.Bottom)
                        {
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            do
                            {
                                Clients[a].Area[b].Bounds.X++;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X--;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                        }
                        else
                        {
                            int x1 = Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2;
                            int y1 = Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2;
                            int x2 = Clients[close1].Area[close2].Bounds.X + Clients[close1].Area[close2].Bounds.Width / 2;
                            int y2 = Clients[close1].Area[close2].Bounds.Y + Clients[close1].Area[close2].Bounds.Height / 2;
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            int prev2;
                            do
                            {
                                prev2 = Clients[a].Area[b].Bounds.Y;
                                Clients[a].Area[b].Bounds.X++;
                                Clients[a].Area[b].Bounds.Y = ((Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - Clients[a].Area[b].Bounds.Height / 2;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X--;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                            Clients[a].Area[b].Bounds.Y = prev2;
                        }
                    }
                }
            }
            else
            {
                if (Clients[a].Area[b].Bounds.Left < Clients[close1].Area[close2].Bounds.Right)
                {
                    if (Clients[a].Area[b].Bounds.Y > Clients[close1].Area[close2].Bounds.Y)
                    {
                        int prev1 = Clients[a].Area[b].Bounds.Y;
                        do
                        {
                            Clients[a].Area[b].Bounds.Y--;
                        } while (CheckCollided(a, b));
                        Clients[a].Area[b].Bounds.Y++;
                        if (Clients[a].Area[b].Bounds.Y != prev1)
                        {
                            was = true;
                        }

                    }
                    else
                    {
                        int prev1 = Clients[a].Area[b].Bounds.Y;
                        do
                        {
                            Clients[a].Area[b].Bounds.Y++;
                        } while (CheckCollided(a, b));
                        Clients[a].Area[b].Bounds.Y--;
                        if (Clients[a].Area[b].Bounds.Y != prev1)
                        {
                            was = true;
                        }
                    }
                }
                else
                {
                    if (Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2 < Clients[close1].Area[close2].Bounds.Y + Clients[close1].Area[close2].Bounds.Height / 2)
                    {
                        if (Clients[a].Area[b].Bounds.Bottom > Clients[close1].Area[close2].Bounds.Top)
                        {
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            do
                            {
                                Clients[a].Area[b].Bounds.X--;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X++;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                        }
                        else
                        {
                            int x1 = Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2;
                            int y1 = Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2;
                            int x2 = Clients[close1].Area[close2].Bounds.X + Clients[close1].Area[close2].Bounds.Width / 2;
                            int y2 = Clients[close1].Area[close2].Bounds.Y + Clients[close1].Area[close2].Bounds.Height / 2;
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            int prev2;
                            do
                            {
                                prev2 = Clients[a].Area[b].Bounds.Y;
                                Clients[a].Area[b].Bounds.X--;
                                Clients[a].Area[b].Bounds.Y = ((Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - Clients[a].Area[b].Bounds.Height / 2;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X++;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                            Clients[a].Area[b].Bounds.Y = prev2;
                        }
                    }
                    else
                    {
                        if (Clients[a].Area[b].Bounds.Top < Clients[close1].Area[close2].Bounds.Bottom)
                        {
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            do
                            {
                                Clients[a].Area[b].Bounds.X--;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X++;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                        }
                        else
                        {
                            int x1 = Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2;
                            int y1 = Clients[a].Area[b].Bounds.Y + Clients[a].Area[b].Bounds.Height / 2;
                            int x2 = Clients[close1].Area[close2].Bounds.X + Clients[close1].Area[close2].Bounds.Width / 2;
                            int y2 = Clients[close1].Area[close2].Bounds.Y + Clients[close1].Area[close2].Bounds.Height / 2;
                            int prev1 = Clients[a].Area[b].Bounds.X;
                            int prev2;
                            do
                            {
                                prev2 = Clients[a].Area[b].Bounds.Y;
                                Clients[a].Area[b].Bounds.X--;
                                Clients[a].Area[b].Bounds.Y = ((Clients[a].Area[b].Bounds.X + Clients[a].Area[b].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - Clients[a].Area[b].Bounds.Height / 2;
                            } while (CheckCollided(a, b));
                            Clients[a].Area[b].Bounds.X++;
                            if (Clients[a].Area[b].Bounds.X != prev1)
                            {
                                was = true;
                            }
                            Clients[a].Area[b].Bounds.Y = prev2;
                        }
                    }
                }
            }
            return was;
        }

        /// <summary>
        /// Allighn all displays
        /// </summary>
        public static void CheckDistAll()
        {
            if (Clients.Count() > 1 || Clients[0].Area.Count() > 1)
            {
                bool was;
                do
                {
                    poli = 0;
                    for (int i = 0; i < Clients.Count(); i++) // partition all into poligons
                    {
                        for (int j = 0; j < Clients[i].Area.Count(); j++)
                        {
                            if (Clients[i].Area[j].poli == -1)
                            {
                                Clients[i].Area[j].Poligon(poli);
                                poli++;
                            }
                        }
                    }
                    was = false;
                    if (poli != 1) // allighn poligons
                    {
                        for (int p = poli; p >= 0; p--)
                        {
                            for (int i = 0; i < Clients.Count(); i++)
                            {
                                for (int j = 0; j < Clients[i].Area.Count(); j++)
                                {
                                    if (Clients[i].Area[j].poli == p)
                                    {
                                        was = was || CheckDist(i, j);
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < Clients.Count(); i++) // reset poligon data
                    {
                        for (int j = 0; j < Clients[i].Area.Count(); j++)
                        {
                            Clients[i].Area[j].poli = -1;
                        }
                    }
                }while (was);
            }
        }

        /// <summary>
        /// Resets allighnment configuration and reallighns every display
        /// </summary>
        public static void ResetOnline()
        {
            for (int i = 0; i < Clients.Count(); i++)
            {
                for (int z = 0; z < MainForm.Comp.Count(); z++)
                {
                    if (MainForm.Comp[z].mac == Clients[i].mac)
                    {
                        for (int j = 0; j < Clients[i].Area.Count(); j++)
                        {
                            Clients[i].Area[j].Bounds = new Rectangle(MainForm.Comp[z].Monitors[j].Bounds.X, MainForm.Comp[z].Monitors[j].Bounds.Y, MainForm.Comp[z].Monitors[j].Bounds.Width, MainForm.Comp[z].Monitors[j].Bounds.Height);
                        }
                    }
                }
            }
            CheckDistAll();
            int x = Clients[0].Area[0].Bounds.X;
            int y = Clients[0].Area[0].Bounds.Y;
            for (int i = 0; i < Clients.Count(); i++) // center allighnment
            {
                for (int j = 0; j < Clients[i].Area.Count(); j++)
                {
                    Clients[i].Area[j].Bounds.X -= x;
                    Clients[i].Area[j].Bounds.Y -= y;
                }
            }
        }
    }

    public static class VMousePosition
    {
        public static Point Pos;
        public static Point Prev;
        private static bool open = false;
        public static int on = 0;
        public static int prevon = 0;

        /// <summary>
        /// Checks if inside any display
        /// </summary>
        private static bool CheckPoint(Point e)
        {
            for (int i = 0; i < Connection.Clients.Count(); i++)
            {
                for (int j = 0; j < Connection.Clients[i].Area.Count(); j++)
                {
                    if (Connection.Clients[i].Area[j].Bounds.Contains(e))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Sets virtual mouse position relative to real position
        /// </summary>
        public static void SetPos()
        {
            if (Connection.mode) // if inside main computer
            {
                if (open) // initialization
                {
                    for (int i = 0; i < Connection.Clients[0].Area.Count(); i++)
                    {
                        if (Connection.Clients[0].Area[i].Bounds.Contains(Pos))
                        {
                            on = i;
                            Prev = Pos;
                            VMouse.SetCursorPosition(MainForm.Monit[i].Bounds.X - Connection.Clients[0].Area[i].Bounds.X + Pos.X, MainForm.Monit[i].Bounds.Y - Connection.Clients[0].Area[i].Bounds.Y + Pos.Y);
                        }
                    }
                    MainForm.MainF.BackColor = Color.White;
                    open = false;
                }
                Prev = Pos;
                VMouse.MousePoint p = VMouse.GetCursorPosition();
                if (!MainForm.Monit[on].Bounds.Contains(p.X, p.Y)) // if pointer left current screen
                {
                    bool there = false;
                    for (int i = 0; i < Connection.Clients[0].Area.Count(); i++)
                    {
                        if (Connection.Clients[0].Area[i].Bounds.Contains(Connection.Clients[0].Area[on].Bounds.X + p.X - MainForm.Monit[on].Bounds.X, Connection.Clients[0].Area[on].Bounds.Y + p.Y - MainForm.Monit[on].Bounds.Y))
                        {
                            there = true;
                            break;
                        }
                    }
                    if (!there) // if mouse inside invalid area
                    {
                        if (p.X < MainForm.Monit[on].Bounds.Left)
                        {
                            p.X = MainForm.Monit[on].Bounds.Left + 1;
                        }
                        else if (p.X > MainForm.Monit[on].Bounds.Right)
                        {
                            p.X = MainForm.Monit[on].Bounds.Right - 1;
                        }
                        if (p.Y < MainForm.Monit[on].Bounds.Top)
                        {
                            p.Y = MainForm.Monit[on].Bounds.Top + 1;
                        }
                        else if (p.Y > MainForm.Monit[on].Bounds.Bottom)
                        {
                            p.Y = MainForm.Monit[on].Bounds.Bottom - 1;
                        }
                        VMouse.SetCursorPosition(p.X, p.Y);
                    }
                }
                Pos.X = Connection.Clients[0].Area[on].Bounds.X + p.X - MainForm.Monit[on].Bounds.X;
                Pos.Y = Connection.Clients[0].Area[on].Bounds.Y + p.Y - MainForm.Monit[on].Bounds.Y;
                if (!Connection.Clients[0].Area[prevon].Bounds.Contains(Pos))
                {
                    prevon = on;
                }
                for (int i = 0; i < Connection.Clients[0].Area.Count(); i++)
                {
                    if (i != on && i != prevon)
                    {
                        if (Connection.Clients[0].Area[i].Bounds.Contains(Pos))
                        {
                            VMouse.SetCursorPosition(Pos.X - Connection.Clients[0].Area[i].Bounds.X + MainForm.Monit[i].Bounds.X, Pos.Y - Connection.Clients[0].Area[i].Bounds.Y + MainForm.Monit[i].Bounds.Y);
                            prevon = on;
                            on = i;
                            break;
                        }
                    }
                }
            }
            else // if inside virtual computers
            {
                if (!open) // initialization
                {
                    MainForm.MainF.BackColor = Color.Black;
                    open = true;
                    VMouse.SetCursorPosition(MainForm.Monit[0].Bounds.Width / 2, MainForm.Monit[0].Bounds.Height / 2);
                }
                VMouse.MousePoint p = VMouse.GetCursorPosition();
                Point pos = new Point(p.X - MainForm.Monit[0].Bounds.Width / 2 + Pos.X, p.Y - MainForm.Monit[0].Bounds.Height / 2 + Pos.Y);
                Prev = Pos;
                if (CheckPoint(pos))
                {
                    Pos = pos;
                }
                else // gets virtual mouse back in bounds
                {
                    int x = Prev.X - pos.X;
                    int y = Prev.Y - pos.Y;
                    if (x >= 0)
                    {
                        while (CheckPoint(Pos))
                        {
                            if (Pos.X >= pos.X)
                            {
                                Pos.X--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Pos.X++;
                    }
                    else
                    {
                        while (CheckPoint(Pos))
                        {
                            if (Pos.X <= pos.X)
                            {
                                Pos.X++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Pos.X--;
                    }

                    if (y >= 0)
                    {
                        while (CheckPoint(Pos))
                        {
                            if (Pos.Y >= pos.Y)
                            {
                                Pos.Y--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Pos.Y++;
                    }
                    else
                    {
                        while (CheckPoint(Pos))
                        {
                            if (Pos.Y <= pos.Y)
                            {
                                Pos.Y++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Pos.Y--;
                    }
                }
                VMouse.SetCursorPosition(MainForm.Monit[0].Bounds.Width / 2, MainForm.Monit[0].Bounds.Height / 2);
            }
        }
    }

    public class HandleClinet
    {
        public string Msg;
        public string Response;
        public string mac;
        public string name = "";
        public List<Display> Area = new List<Display>();
        private TcpClient clientSocket;
        public bool on = false;
        private byte[] Commandbytes = null;
        private string dataFromServer = null;
        public bool was = false;
        public string Clip = ""; 

        public HandleClinet(TcpClient inClientSocket)
        {
            clientSocket = inClientSocket;
            Response = "j;"; // send instant autentification request
        }

        public HandleClinet()
        {

        }

        /// <summary>
        /// Packet i/o
        /// </summary>
        /// <returns></returns>
        public bool SendCommand()
        {
            try
            {
                string serverPing;
                int copyLength = 0;

                while (!string.IsNullOrWhiteSpace(Response) || Msg.Length < copyLength)
                {
                    string r;
                    try
                    {
                        r = Response.Substring(0, 1022);
                        r = r.Substring(0, r.LastIndexOf(';') + 1);
                    }
                    catch (Exception)
                    {
                        r = Response;
                    }
                    NetworkStream networkStream = clientSocket.GetStream();
                    Response = Response.Substring(r.Length);
                    serverPing = r + "\0";
                    Commandbytes = new byte[1024];
                    Commandbytes = Encoding.UTF8.GetBytes(serverPing);
                    networkStream.Write(Commandbytes, 0, Commandbytes.Length);
                    dataFromServer = null;
                    Commandbytes = new byte[1024];
                    networkStream.ReadTimeout = 500;
                    networkStream.Read(Commandbytes, 0, 1024);
                    dataFromServer = Encoding.UTF8.GetString(Commandbytes);
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
                        copyLength = 0;
                        Response += Decoder();
                    }
                    dataFromServer = null;
                    networkStream.Flush();
                }
                if (!string.IsNullOrWhiteSpace(Clip))
                {
                    Response += "p<" + Clip.Length.ToString() + ">" + Clip + ";";
                    Clip = "";
                    while (!string.IsNullOrWhiteSpace(Response))
                    {
                        string r;
                        try
                        {
                            r = Response.Substring(0, 1022);
                        }
                        catch (Exception)
                        {
                            r = Response;
                        }
                        NetworkStream networkStream = clientSocket.GetStream();
                        Response = Response.Substring(r.Length);
                        serverPing = r + "\0";
                        Commandbytes = new byte[1024];
                        Commandbytes = Encoding.UTF8.GetBytes(serverPing);
                        networkStream.Write(Commandbytes, 0, Commandbytes.Length);
                        dataFromServer = null;
                        Commandbytes = new byte[1024];
                        networkStream.ReadTimeout = 500;
                        networkStream.Read(Commandbytes, 0, 1024);
                        dataFromServer = Encoding.UTF8.GetString(Commandbytes);
                        dataFromServer = dataFromServer.Substring(0, dataFromServer.IndexOf("\0"));
                        Msg += dataFromServer;
                        dataFromServer = null;
                        Response += Decoder();
                        networkStream.Flush();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Decodes the command and returns the answer
        /// </summary>
        /// <code>
        /// Login request + MAC - 'l74D02BE1962D;'
        /// Name change, - 'n,ASUS;'
        /// Display data - 'd,1920x1080;'
        /// Data transfer end - 'u;'
        /// Paste - 'p<13>Text to paste;'
        /// Empty command - 'u;'
        /// ping - '!;'
        /// </code>
        public string Decoder()
        {
            string s = "";
            if (!string.IsNullOrWhiteSpace(Msg))
            {
                s = Msg;
                Msg = Msg.Substring(s.Length);
            }
            while (!string.IsNullOrWhiteSpace(s))
            {
                switch (s[0])
                {
                    case 'l':
                        LoginRequest(s);
                        break;
                    case 'n':
                        NameChange(s);
                        break;
                    case 'd':
                        DataRegister(s);
                        break;
                    case 'u':
                        FinalizeRegister();
                        on = true;
                        break;
                    case 'p':
                        Paste(ref s);
                        break;
                    case 'c':
                        Pass();
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
            string r;
            try
            {
                r = Response.Substring(0, 1024);
                r = r.Substring(0, r.LastIndexOf(';') + 1);
            }
            catch (Exception)
            {
                r = Response;
            }
            Response = Response.Substring(r.Length);
            return r;
        }

        /// <summary>
        /// Authenticates client, sends a data request if missing
        /// </summary>
        private void LoginRequest(string s)
        {
            mac = s.Substring(1, s.IndexOf(';') - 1);
            bool there = false;
            for (int i = 0; i < MainForm.Comp.Count(); i++)
            {
                if (MainForm.Comp[i].mac == mac)
                {
                    there = true;
                    for (int j = 0; j < MainForm.Comp[i].Monitors.Count(); j++)
                    {
                        Area.Add(new Display(MainForm.Comp[i].Monitors[j].Bounds));
                    }
                    name = MainForm.Comp[i].name;
                    Connection.ResetOnline();
                    on = true;
                    break;
                }
            }
            if (!there)
            {
                MainForm.Comp.Add(new Computer(mac));
                Response += "d;";
                SendCommand();
            }
        }

        /// <summary>
        /// Changes the name of the client
        /// </summary>
        private void NameChange(string s)
        {
            for (int i = 0; i < MainForm.Comp.Count(); i++)
            {
                if (MainForm.Comp[i].mac == mac)
                {
                    name = s.Substring(2, s.IndexOf(';') - 2);
                    MainForm.Comp[i].name = name;
                    SystemShareForm.PopDispData();
                    SystemShareForm.SaveDispData();
                    break;
                }
            }
        }

        /// <summary>
        /// Saves display data
        /// </summary>
        /// <param name="s"></param>
        private void DataRegister(string s)
        {
            for (int i = 0; i < MainForm.Comp.Count(); i++)
            {
                if (MainForm.Comp[i].mac == mac)
                {
                    int w = Convert.ToInt32(s.Substring(2, s.IndexOf('x') - 2));
                    int h = Convert.ToInt32(s.Substring(s.IndexOf('x') + 1, s.IndexOf(';') - s.IndexOf('x') - 1));
                    MainForm.Comp[i].Monitors.Add(new Display(w, h, 0, 0));
                    while (!Connection.CheckCollidedMain(i, MainForm.Comp[i].Monitors.Count() - 1))
                    {
                        MainForm.Comp[i].Monitors[MainForm.Comp[i].Monitors.Count() - 1].Bounds.X--;
                    }
                    SystemShareForm.PopDispData();
                    SystemShareForm.SaveDispData();
                    Area.Add(new Display(MainForm.Comp[i].Monitors[MainForm.Comp[i].Monitors.Count() - 1].Bounds));
                    break;
                }
            }
        }

        /// <summary>
        /// Finalizes the registration
        /// </summary>
        private void FinalizeRegister()
        {
            SystemShareForm.PopDispData();
            SystemShareForm.SaveDispData();
            Connection.ResetOnline();
            on = true;
        }

        /// <summary>
        /// Updates clipboard
        /// </summary>
        /// <param name="s"></param>
        private void Paste(ref string s)
        {
            ClipBoard.Set(s.Substring(s.IndexOf('>') + 1, s.LastIndexOf(';') - s.IndexOf('>') - 1));
            s = s.Substring(s.LastIndexOf(';') + 1);
        }

        /// <summary>
        /// Sends an empty command
        /// </summary>
        private void Pass()
        {
            Response += "v;";
        }
    }
}
