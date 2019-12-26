using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace SystemShare
{
    public partial class MainForm : Form
    {
        public static string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string mac = "";
        public static int monitors;
        public static int port;
        public static string name;
        public static string ip;
        public static List<Rectangle> Disp = new List<Rectangle>();
        public static bool open = false;

        /// <summary>
        /// Gets data, starts connection if successful, opens a form if not
        /// </summary>
        public MainForm()
        {
            GetDirectory();
            InitializeComponent();
            if (!PopData())
            {
                OpenForm();
            }
            else
            {
                Connection.Connect();
            }
        }

        #region FormControl

        /// <summary>
        /// Hide from alt-tab
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenForm();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
            Application.Exit();
        }

        private void initializeConnactionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!PopData())
            {
                OpenForm();
            }
            else
            {
                Connection.Disconnect();
                Connection.Connect();
            }
        }

        private void openSystemShareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
        }
        #endregion

        /// <summary>
        /// Opens a SystemShareForm window
        /// </summary>
        public static void OpenForm()
        {
            if (!open)
            {
                SystemShareForm Form = new SystemShareForm();
                Form.Show();
                open = true;
            }
        }

        /// <summary>
        /// Gets the loval MacAddress
        /// </summary>
        public static void GetMacAddress()
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
        /// Gets a path to %appdata%
        /// </summary>
        public static void GetDirectory()
        {
            dir = Path.Combine(dir, "SystemShareClient");
            Directory.CreateDirectory(dir);
            dir = dir + @"\";
        }

        /// <summary>
        /// Get data from database and constants
        /// </summary>
        /// <returns></returns>
        public static bool PopData()
        {
            GetMacAddress();
            bool correct = true;
            try
            {
                using (StreamReader readtext = new StreamReader(dir + "ClientData.txt"))
                {
                    name = readtext.ReadLine();
                    monitors = Convert.ToInt32(readtext.ReadLine());
                    ip = readtext.ReadLine();
                    port = Convert.ToInt32(readtext.ReadLine());
                }
            }
            catch (Exception)
            {
                correct = false;
            }
            if (!correct)
            {
                monitors = 0;
            }
            Disp.Clear();
            try
            {
                using (StreamReader readtext = new StreamReader(dir + "DisplayData.txt"))
                {
                    for (int i = 0; i < monitors; i++)
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
                        Disp.Add(new Rectangle(RelPosX, RelPosY, ResX, ResY));
                    }
                }
            }
            catch (Exception)
            {
                correct = false;
            }
            if (!Disp.Any())
            {
                correct = false;
            }
            return correct;
        }
    }
}