using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SystemShare
{
    public partial class MainForm : Form
    {
        public static string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static KeyBoardHook gkh = new KeyBoardHook();
        public static MainForm MainF;
        private static SystemShareForm Form;
        public static List<Computer> Comp = new List<Computer>();
        public static List<Display> Monit = new List<Display>();
        public static int port = 0;
        public static string name = "PC Name";
        public static string ip;
        public static string mac = "";
        public static bool fullscreen = false;
        public static bool open = false;
        private static bool hooked = false;

        public MainForm()
        {
            GetPath();
            StartUp();
            InitializeComponent();
            if (!PopData())
            {
                Form = new SystemShareForm();
                Form.Show();
                open = true;
            }
            else
            {
                PopDispData();
                StartServer();
            }
        }

        /// <summary>
        /// Initializes event subscriptions, threads, etc.
        /// </summary>
        private void StartUp()
        {
            MouseWheel += new MouseEventHandler(MainForm_MouseWheel);
            MouseHook.Start();
            MouseHook.MouseAction += new EventHandler(LeftClick);
            gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);
            gkh.KeyUp += new KeyEventHandler(gkh_KeyUp);
            MainF = this;
            GetMac();
            GetIP();
        }

        /// <summary>
        /// Fetches the MAC address
        /// </summary>
        private static void GetMac()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {

                if (nic.OperationalStatus == OperationalStatus.Up && (!nic.Description.Contains("Virtual") && !nic.Description.Contains("Pseudo")))
                {
                    if (nic.GetPhysicalAddress().ToString() != "")
                    {
                        mac = nic.GetPhysicalAddress().ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Fetches the IP address
        /// </summary>
        private static void GetIP()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ip = endPoint.Address.ToString();
            }
        }

        /// <summary>
        /// Gets a path to %appdata%
        /// </summary>
        private static void GetPath()
        {
            dir = Path.Combine(dir, "SystemShareHost");
            Directory.CreateDirectory(dir);
            dir = dir + @"\";
        }

        #region FormControl
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();
        }

        private void openSystemShareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!open)
            {
                Form = new SystemShareForm();
                Form.Show();
                open = true;
            }
        }

        private void initializeConnactionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Connection.Disconnect();
            Connection.Connect();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!open)
            {
                Form = new SystemShareForm();
                Form.Show();
                open = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
        }
        #endregion

        /// <summary>
        /// Reads local data from database
        /// </summary>
        /// <returns></returns>
        public static bool PopData()
        {
            bool correct = true;
            try
            {
                using (StreamReader readtext = new StreamReader(dir + "LocalData.txt"))
                {
                    name = readtext.ReadLine();
                    port = Convert.ToInt32(readtext.ReadLine());
                    fullscreen = 1 == Convert.ToInt32(readtext.ReadLine());
                    int A, R, G, B;
                    A = Convert.ToInt32(readtext.ReadLine());
                    R = Convert.ToInt32(readtext.ReadLine());
                    G = Convert.ToInt32(readtext.ReadLine());
                    B = Convert.ToInt32(readtext.ReadLine());
                    SystemShareForm.Primary = Color.FromArgb(A,R,G,B);
                    A = Convert.ToInt32(readtext.ReadLine());
                    R = Convert.ToInt32(readtext.ReadLine());
                    G = Convert.ToInt32(readtext.ReadLine());
                    B = Convert.ToInt32(readtext.ReadLine());
                    SystemShareForm.Secondary = Color.FromArgb(A, R, G, B);

                }
            }
            catch (Exception)
            {
                correct = false;
            }
            Monit.Clear();
            try
            {
                using (StreamReader readtext = new StreamReader(dir + "LocalDisplayData.txt"))
                {
                    do
                    {
                        string data = readtext.ReadLine();
                        int ResX = Convert.ToInt32(data.Substring(3, data.IndexOf(';') - 3));
                        data = data.Substring(data.IndexOf(';') + 1);
                        int ResY = Convert.ToInt32(data.Substring(3, data.IndexOf(';') - 3));
                        data = data.Substring(data.IndexOf(';') + 1);
                        int RelPosX = Convert.ToInt32(data.Substring(3, data.IndexOf(';') - 3));
                        data = data.Substring(data.IndexOf(';') + 1);
                        int RelPosY = Convert.ToInt32(data.Substring(3, data.IndexOf(';') - 3));
                        data = data.Substring(data.IndexOf(';') + 1);
                        Monit.Add(new Display(ResX, ResY, RelPosX, RelPosY));
                    } while (!readtext.EndOfStream);
                }
            }
            catch (Exception)
            {
                correct = false;
            }
            if (!Monit.Any())
            {
                using (StreamWriter writetext = new StreamWriter(dir + "LocalDisplayData.txt"))
                {
                    foreach (Screen screen in Screen.AllScreens)
                    {
                        int resy = screen.Bounds.Height;
                        int resx = screen.Bounds.Width;
                        int relposx = screen.Bounds.X;
                        int relposy = screen.Bounds.Y;
                        writetext.WriteLine("W: " + resx.ToString() + ";" + "H: " + resy.ToString() + ";" + "X: " + relposx.ToString() + ";" + "Y: " + relposy.ToString() + ";");
                        Monit.Add(new Display(resx, resy, relposx, relposy));
                    }
                }
                correct = false;
                PopDispData();
            }
            return correct;
        }

        /// <summary>
        /// Reads display data from database
        /// </summary>
        public static void PopDispData()
        {
            try
            {
                using (StreamReader readtext = new StreamReader(dir + "DisplayData.txt"))
                {
                    do
                    {
                        string temp;
                        temp = readtext.ReadLine();
                        int width = Convert.ToInt32(temp.Substring(3, temp.IndexOf(';') - 3));
                        temp = temp.Substring(temp.IndexOf(';') + 1);
                        int height = Convert.ToInt32(temp.Substring(3, temp.IndexOf(';') - 3));
                        temp = temp.Substring(temp.IndexOf(';') + 1);
                        int X = Convert.ToInt32(temp.Substring(3, temp.IndexOf(';') - 3));
                        temp = temp.Substring(temp.IndexOf(';') + 1);
                        int Y = Convert.ToInt32(temp.Substring(3, temp.IndexOf(';') - 3));
                        temp = temp.Substring(temp.IndexOf(';') + 1);
                        string n = temp.Substring(3, temp.IndexOf(';') - 3);
                        temp = temp.Substring(temp.IndexOf(';') + 1);
                        string m = temp.Substring(3, temp.IndexOf(';') - 3);
                        bool there = false;
                        for (int i = 0; i < Comp.Count(); i++)
                        {
                            if (Comp[i].mac == m)
                            {
                                Comp[i].Monitors.Add(new Display(width, height, X, Y));
                                there = true;
                                break;
                            }
                        }
                        if (!there)
                        {
                            Comp.Add(new Computer(m));
                            Comp[Comp.Count() - 1].name = n;
                            Comp[Comp.Count() - 1].Monitors.Add(new Display(width, height, X, Y));
                        }
                    } while (!readtext.EndOfStream);
                }
            }
            catch (Exception)
            {
                Comp.Clear();
                using (StreamWriter writetext = new StreamWriter(dir + "DisplayData.txt"))
                {
                    Comp.Add(new Computer(mac));
                    Comp[0].name = name;
                    for (int i = 0; i < Monit.Count(); i++)
                    {
                        Comp[0].Monitors.Add(new Display(Monit[i].Bounds.Width, Monit[i].Bounds.Height, Monit[i].Bounds.X, Monit[i].Bounds.Y));
                        writetext.WriteLine("W: " + Monit[i].Bounds.Width.ToString() + ";H: " + Monit[i].Bounds.Height.ToString() + ";X: " + Monit[i].Bounds.X.ToString() + ";Y: " + Monit[i].Bounds.Y.ToString() + ";N: " + name + ";M: " + mac + ";");
                    }
                }
            }
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public static void StartServer()
        {
            Connection.Connect();
        }

        #region Input Capture
        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            int lines = SystemInformation.MouseWheelScrollLines * e.Delta / 3;
            Connection.Mice += "s" + lines.ToString() + ";";
        }

        private void MainForm_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void MainForm_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void LeftClick(object sender, EventArgs e)
        {
            if (Connection.Clients.Count() > 0)
            {
                if (Connection.mode)
                {
                    for (int j = 0; j < Connection.Clients[0].Area.Count(); j++)
                    {
                        if (Connection.Clients[0].Area[j].Bounds.Contains(VMousePosition.Pos))
                        {
                            Connection.selected = 0;
                            if (hooked)
                            {
                                gkh.Unhook();
                                hooked = false;
                            }
                        }
                    }
                }
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Connection.Mice += "ld;";
                    for (int i = 1; i < Connection.Clients.Count(); i++)
                    {
                        for (int j = 0; j < Connection.Clients[i].Area.Count(); j++)
                        {
                            if (Connection.Clients[i].Area[j].Bounds.Contains(VMousePosition.Pos))
                            {
                                Connection.selected = i;
                                if (!hooked)
                                {
                                    gkh.Hook();
                                    hooked = true;
                                }
                            }
                        }
                    }
                    break;
                case MouseButtons.Right:
                    Connection.Mice += "rd;";
                    break;
                case MouseButtons.Middle:
                    Connection.Mice += "wd;";
                    break;
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Connection.Mice += "lu;";
                    break;
                case MouseButtons.Right:
                    Connection.Mice += "ru;";
                    break;
                case MouseButtons.Middle:
                    Connection.Mice += "wu;";
                    break;
            }
        }

        void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 17 || e.KeyValue == 162)
            {
                KeyBoardHook.CTRL = false;
            }
            Connection.Keys += "ku" + e.KeyValue.ToString() + ";";
            e.Handled = true;
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 17 || e.KeyValue == 162)
            {
                KeyBoardHook.CTRL = true;
            }
            if (KeyBoardHook.CTRL && e.KeyValue == 86)
            {
                ClipBoard.Get();
            }
            else
            {
                Connection.Keys += "kd" + e.KeyValue.ToString() + ";";
            }
            if (KeyBoardHook.CTRL && (e.KeyValue == 67 || e.KeyValue == 88))
            {
                Connection.Keys += "c;";
            }
            e.Handled = true;
        }
        #endregion
    }

    public class Computer
    {
        public List<Display> Monitors = new List<Display>();
        public string name;
        public string mac;

        public Computer(string m)
        {
            mac = m;
        }
    }

    public class Display
    {
        public Rectangle Bounds;
        public int poli = -1;

        public Display(int w, int h, int x, int y)
        {
            Bounds = new Rectangle(x, y, w, h);
        }

        public Display(Rectangle s)
        {
            Bounds = new Rectangle(s.X, s.Y, s.Width, s.Height);
        }

        /// <summary>
        /// Partitions touching displays into poligons
        /// </summary>
        /// <param name="p"></param>
        public void Poligon(int p)
        {
            poli = p;
            for (int i = 0; i < Connection.Clients.Count(); i++)
            {
                for (int j = 0; j < Connection.Clients[i].Area.Count(); j++)
                {
                    Rectangle rec = Rectangle.Intersect(Bounds, Connection.Clients[i].Area[j].Bounds);
                    if (rec.Width > 0 || rec.Height > 0)
                    {
                        if (Connection.Clients[i].Area[j].poli == -1)
                        {
                            Connection.Clients[i].Area[j].Poligon(p);
                        }
                    }
                }
            }
        }
    }
}
