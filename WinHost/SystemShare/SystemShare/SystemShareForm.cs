using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace SystemShare
{
    public partial class SystemShareForm : Form
    {
        static Arrangement Arr;
        public static SystemShareForm SForm;
        public static List<Rect> rect = new List<Rect>();
        public static int Pw;
        public static int Ph;
        public static int Occupied = 0;
        public static bool oc = false;
        private static string selected = "";
        public static bool open = false;
        public static Color Primary = Color.RoyalBlue;
        public static Color Secondary = Color.LightBlue;

        public SystemShareForm()
        {
            InitializeComponent();
            StartUp();
            InitDataPop();
            PopDispData();
            CheckDistAll();
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Initializes starting variables and everything
        /// </summary>
        private void StartUp()
        {
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker1.WorkerReportsProgress = true;
            selected = "";
            SForm = this;
            Pw = pictureBox1.Width;
            Ph = pictureBox1.Height;
        }

        /// <summary>
        /// Populates data in form
        /// </summary>
        private void InitDataPop()
        {
            checkBox1.Checked = MainForm.fullscreen;
            if (checkBox1.Checked)
            {
                listBox1.Items.Clear();
                try
                {
                    using (StreamReader readtext = new StreamReader(MainForm.dir + "FullScreenEvasion.txt"))
                    {
                        while (!readtext.EndOfStream)
                        {
                            string name = readtext.ReadLine();
                            listBox1.Items.Add(name);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            if (!checkBox1.Checked)
            {
                listBox1.Enabled = false;
                textBox2.Enabled = false;
                button4.Enabled = false;
                button9.Enabled = false;
            }
            textBox1.Text = MainForm.name;
            if (MainForm.port != 0)
            {
                textBox4.Text = MainForm.port.ToString();
            }
            else
            {
                GenPort();
            }
            textBox5.Text = MainForm.ip;
        }

        /// <summary>
        /// Populates display data
        /// </summary>
        public static void PopDispData()
        {
            rect.Clear();
            for (int i = 0; i < MainForm.Comp.Count(); i++)
            {
                for (int j = 0; j < MainForm.Comp[i].Monitors.Count(); j++)
                {
                    rect.Add(new Rect(MainForm.Comp[i].Monitors[j].Bounds.X, MainForm.Comp[i].Monitors[j].Bounds.Y, MainForm.Comp[i].Monitors[j].Bounds.Width, MainForm.Comp[i].Monitors[j].Bounds.Height, j, MainForm.Comp[i].name, MainForm.Comp[i].mac));
                }
            }
            CheckDistAll();
        }

        /// <summary>
        /// Saves display data to database
        /// </summary>
        public static void SaveDispData()
        {
            MainForm.Comp.Clear();
            using (StreamWriter writetext = new StreamWriter(MainForm.dir + "DisplayData.txt"))
            {
                foreach (Rect mon in rect)
                {
                    bool there = false;
                    for (int i = 0; i < MainForm.Comp.Count(); i++)
                    {
                        if (MainForm.Comp[i].mac == mon.mac)
                        {
                            MainForm.Comp[i].Monitors.Add(new Display(mon.Bounds.Width + 2, mon.Bounds.Height + 2, mon.Bounds.X - rect[0].Bounds.X, mon.Bounds.Y - rect[0].Bounds.Y));
                            there = true;
                        }
                    }
                    if (!there)
                    {
                        MainForm.Comp.Add(new Computer(mon.mac));
                        MainForm.Comp[MainForm.Comp.Count() - 1].name = mon.name;
                        MainForm.Comp[MainForm.Comp.Count() - 1].Monitors.Add(new Display(mon.Bounds.Width + 2, mon.Bounds.Height + 2, mon.Bounds.X - rect[0].Bounds.X, mon.Bounds.Y - rect[0].Bounds.Y));
                    }
                    writetext.WriteLine("W: " + (mon.Bounds.Width + 2).ToString() + ";H: " + (mon.Bounds.Height + 2).ToString() + ";X: " + (mon.Bounds.X - rect[0].Bounds.X).ToString() + ";Y: " + (mon.Bounds.Y - rect[0].Bounds.Y).ToString() + ";N: " + mon.name + ";M: " + mon.mac + ";");
                }
            }
        }

        /// <summary>
        /// Generates a new unused port
        /// </summary>
        private void GenPort()
        {
            int port;
            Random rng = new Random();
            bool isAvailable;
            do
            {
                port = rng.Next(8000, 9999);
                isAvailable = true;
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            } while (!isAvailable);
            textBox4.Text = port.ToString();
        }

        /// <summary>
        /// Saves data to database
        /// </summary>
        private void SaveData()
        {
            using (StreamWriter writetext = new StreamWriter(MainForm.dir + "LocalData.txt"))
            {
                writetext.WriteLine(textBox1.Text);
                writetext.WriteLine(textBox4.Text);
                if (checkBox1.Checked)
                {
                    writetext.WriteLine("1");
                }
                else
                {
                    writetext.WriteLine("0");
                }
                writetext.WriteLine(Primary.A.ToString());
                writetext.WriteLine(Primary.R.ToString());
                writetext.WriteLine(Primary.G.ToString());
                writetext.WriteLine(Primary.B.ToString());
                writetext.WriteLine(Secondary.A.ToString());
                writetext.WriteLine(Secondary.R.ToString());
                writetext.WriteLine(Secondary.G.ToString());
                writetext.WriteLine(Secondary.B.ToString());
            }
            using (StreamWriter writetext = new StreamWriter(MainForm.dir + "FullScreenEvasion.txt"))
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    writetext.WriteLine(listBox1.Items[i]);
                }
            }
        }

        /// <summary>
        /// Updates data
        /// </summary>
        private void UpdateData()
        {
            MainForm.name = textBox1.Text;
            foreach (Rect mon in rect)
            {
                if (mon.mac == MainForm.mac)
                {
                    mon.name = MainForm.name;
                }
            }
            MainForm.Comp[0].name = MainForm.name;
            MainForm.port = Convert.ToInt32(textBox4.Text);
            MainForm.fullscreen = checkBox1.Checked;
        }

        /// <summary>
        /// Saves everything
        /// </summary>
        private void SaveAll()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                label3.Visible = true;
            }
            else
            {
                label3.Visible = false;
                Connection.Disconnect();
                UpdateData();
                SaveData();
                SaveDispData();
                InitDataPop();
                PopDispData();
                pictureBox1.Refresh();
                Reconnect();
            }
        }

        /// <summary>
        /// Resets all display data
        /// </summary>
        private void ResetAll()
        {
            rect.Clear();
            Connection.Disconnect();
            MainForm.Monit.Clear();
            using (StreamWriter writetext = new StreamWriter(MainForm.dir + "LocalDisplayData.txt"))
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    int resy = screen.Bounds.Height;
                    int resx = screen.Bounds.Width;
                    int relposx = screen.Bounds.X;
                    int relposy = screen.Bounds.Y;
                    writetext.WriteLine("W: " + resx.ToString() + ";" + "H: " + resy.ToString() + ";" + "X: " + relposx.ToString() + ";" + "Y: " + relposy.ToString() + ";");
                    MainForm.Monit.Add(new Display(resx, resy, relposx, relposy));
                }
            }
            MainForm.Comp.Clear();
            using (StreamWriter writetext = new StreamWriter(MainForm.dir + "DisplayData.txt"))
            {
                MainForm.Comp.Add(new Computer(MainForm.mac));
                MainForm.Comp[0].name = MainForm.name;
                for (int i = 0; i < MainForm.Monit.Count(); i++)
                {
                    MainForm.Comp[0].Monitors.Add(new Display(MainForm.Monit[i].Bounds.Width, MainForm.Monit[i].Bounds.Height, MainForm.Monit[i].Bounds.X, MainForm.Monit[i].Bounds.Y));
                    writetext.WriteLine("W: " + MainForm.Monit[i].Bounds.Width.ToString() + ";H: " + MainForm.Monit[i].Bounds.Height.ToString() + ";X: " + MainForm.Monit[i].Bounds.X.ToString() + ";Y: " + MainForm.Monit[i].Bounds.Y.ToString() + ";N: " + MainForm.name + ";M: " + MainForm.mac + ";");
                }
            }
        }

        /// <summary>
        /// Restarts the server
        /// </summary>
        private void Reconnect()
        {
            Connection.Disconnect();
            Connection.Connect();
        }

        #region Form Control
        private void button1_Click(object sender, EventArgs e)
        {
            GenPort();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveAll();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ResetAll();
            InitDataPop();
            PopDispData();
            pictureBox1.Refresh();
            Reconnect();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            PopDispData();
            pictureBox1.Refresh();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!open)
            {
                open = true;
                Arr = new Arrangement();
                Arr.Show();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Reconnect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox2.Text) && textBox2.Text != "Process name (Task Manager -> Details)")
            {
                listBox1.Items.Add(textBox2.Text);
                textBox2.Text = "Process name (Task Manager -> Details)";
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Primary = colorDialog1.Color;
            }
            pictureBox1.Refresh();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Secondary = colorDialog1.Color;
            }
            pictureBox1.Refresh();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Primary = Color.RoyalBlue;
            Secondary = Color.LightBlue;
            pictureBox1.Refresh();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < rect.Count(); i++)
            {
                if (rect[i].mac == selected)
                {
                    if (rect[i].mac == MainForm.mac)
                    {
                        MessageBox.Show("Can not remove host computer.");
                        break;
                    }
                    else
                    {
                        rect.RemoveAt(i);
                        i--;
                    }
                }
            }
            pictureBox1.Refresh();
            Reconnect();
        }

        private void SystemShareForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.open = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                listBox1.Enabled = true;
                textBox2.Enabled = true;
                button4.Enabled = true;
                button9.Enabled = true;
            }
            else
            {
                listBox1.Enabled = false;
                textBox2.Enabled = false;
                button4.Enabled = false;
                button9.Enabled = false;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Process name (Task Manager -> Details)")
            {
                textBox2.Text = "";
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "Process name (Task Manager -> Details)";
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                for (int i = 0; i < rect.Count(); i++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(rect[i].Image.X - 1, rect[i].Image.Y - 1, rect[i].Image.Width + 2, rect[i].Image.Height + 2));
                    if (rect[i].mac == selected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Secondary), new Rectangle(rect[i].Image.X + 2, rect[i].Image.Y + 2, rect[i].Image.Width - 4, rect[i].Image.Height - 4));
                        e.Graphics.FillRectangle(new SolidBrush(Primary), new Rectangle(rect[i].Image.X + 4, rect[i].Image.Y + 4, rect[i].Image.Width - 8, rect[i].Image.Height - 8));
                    }
                    else
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Primary), new Rectangle(rect[i].Image.X + 2, rect[i].Image.Y + 2, rect[i].Image.Width - 4, rect[i].Image.Height - 4));
                    }
                    e.Graphics.DrawString(rect[i].monitor + ", " + rect[i].name.ToString(), new Font("Arial", 10), Brushes.White, rect[i].Image.X + 5, rect[i].Image.Y + 5);
                }
            }
            catch (Exception)
            {

            }
        }

        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = sender as System.ComponentModel.BackgroundWorker;
            while (true)
            {
                worker.ReportProgress(1);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void BackgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            pictureBox1.Refresh();
        }
        #endregion

        /// <summary>
        /// Sellects a rectangle
        /// </summary>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < rect.Count(); i++)
            {
                if (rect[i].IsThere(Mouse.Pos))
                {
                    Occupied = i;
                    selected = rect[i].mac;
                    Mouse.Offset.X = Mouse.Pos.X - rect[i].Image.X - rect[i].Image.Width / 2;
                    Mouse.Offset.Y = Mouse.Pos.Y - rect[i].Image.Y - rect[i].Image.Height / 2;
                    oc = true;
                }
            }
            Mouse.down = true;
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Moves the sellected rectangle or everything
        /// </summary>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Mouse.Prev = Mouse.Pos;
            Mouse.Pos = e.Location;
            if (oc)
            {
                rect[Occupied].Move();
                CheckColision(Occupied);
                rect[Occupied].Prev.X = rect[Occupied].Bounds.X;
                rect[Occupied].Prev.Y = rect[Occupied].Bounds.Y;
                rect[Occupied].Image.Location = new Point(rect[Occupied].Bounds.X / 10, rect[Occupied].Bounds.Y / 10);
                pictureBox1.Refresh();
            }
            else
            {
                if (Mouse.down)
                {
                    DragAll();
                }
            }
        }

        /// <summary>
        /// Allighns every rectangle to working standards, deselects selected rectangle
        /// </summary>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (oc)
            {
                CheckDist(Occupied);
                CheckDistAll();
                pictureBox1.Refresh();
            }
            oc = false;
            Mouse.down = false;
        }

        /// <summary>
        /// Checks if the specified rectangle is in collision with any other
        /// </summary>
        /// <param name="a">rectangle index</param>
        public static bool CheckCollided(int a)
        {
            for (int i = 0; i < rect.Count; i++)
            {
                if (i != a)
                {
                    Rectangle rec = Rectangle.Intersect(rect[a].Bounds, rect[i].Bounds);
                    if (!rec.IsEmpty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Validates movement, snaps rectangle to latest valid position
        /// </summary>
        /// <param name="a">rectangle index</param>
        public static void CheckColision(int a)
        {
            if (!CheckCollided(a))
            {
                int x = rect[a].Prev.X - rect[a].Bounds.X;
                int y = rect[a].Prev.Y - rect[a].Bounds.Y;
                int destx = rect[a].Bounds.X;
                int desty = rect[a].Bounds.Y;
                rect[a].Bounds.X = rect[a].Prev.X;
                rect[a].Bounds.Y = rect[a].Prev.Y;
                if (x >= 0)
                {
                    while (CheckCollided(a))
                    {
                        if (rect[a].Bounds.X > destx)
                        {
                            rect[a].Prev.X = rect[a].Bounds.X;
                            rect[a].Bounds.X--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    rect[a].Bounds.X = rect[a].Prev.X;
                }
                else
                {
                    while (CheckCollided(a))
                    {
                        if (rect[a].Bounds.X < destx)
                        {
                            rect[a].Prev.X = rect[a].Bounds.X;
                            rect[a].Bounds.X++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    rect[a].Bounds.X = rect[a].Prev.X;
                }

                if (y >= 0)
                {
                    while (CheckCollided(a))
                    {
                        if (rect[a].Bounds.Y > desty)
                        {
                            rect[a].Prev.Y = rect[a].Bounds.Y;
                            rect[a].Bounds.Y--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    rect[a].Bounds.Y = rect[a].Prev.Y;
                }
                else
                {
                    while (CheckCollided(a))
                    {
                        if (rect[a].Bounds.Y < desty)
                        {
                            rect[a].Prev.Y = rect[a].Bounds.Y;
                            rect[a].Bounds.Y++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    rect[a].Bounds.Y = rect[a].Prev.Y;
                }
            }
        }

        /// <summary>
        /// Alligns rectangle to the closest valid position automatically using geometry
        /// </summary>
        /// <param name="Occ"></param>
        /// <returns></returns>
        public static bool CheckDist(int Occ)
        {
            bool was = false;
            rect[Occ].Bounds.X += 1;
            if (!CheckCollided(Occ))
            {
                was = true;
            }
            rect[Occ].Bounds.X -= 2;
            if (!CheckCollided(Occ))
            {
                was = true;
            }
            rect[Occ].Bounds.X += 1;
            rect[Occ].Bounds.Y += 1;
            if (!CheckCollided(Occ))
            {
                was = true;
            }
            rect[Occ].Bounds.Y -= 2;
            if (!CheckCollided(Occ))
            {
                was = true;
            }
            rect[Occ].Bounds.Y += 1;
            if (!was)
            {
                int close = 0;
                int dist = 0;
                for (int i = 0; i < rect.Count(); i++)
                {
                    if (i != Occ)
                    {
                        close = i;
                        int x1 = rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2;
                        int y1 = rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2;
                        int x2 = rect[i].Bounds.X + rect[i].Bounds.Width / 2;
                        int y2 = rect[i].Bounds.Y + rect[i].Bounds.Height / 2;
                        if (x1 < x2)
                        {
                            if (rect[Occ].Bounds.Contains(rect[Occ].Bounds.Right - 1, ((rect[Occ].Bounds.Right - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((rect[Occ].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = rect[Occ].Bounds.Right;
                            }
                            if (rect[i].Bounds.Contains(rect[i].Bounds.Left + 1, ((rect[i].Bounds.Left + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((rect[i].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = rect[i].Bounds.Left;
                            }
                            if (y1 > y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Top + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Top;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Bottom + 1))
                                {
                                    x2 = (rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Bottom + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Bottom;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Top + 1))
                                {
                                    x2 = (rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = rect[Occ].Bounds.Right;
                                x2 = rect[i].Bounds.Left;
                            }
                        }
                        else if (x1 > x2)
                        {
                            if (rect[Occ].Bounds.Contains(rect[Occ].Bounds.Left - 1, ((rect[Occ].Bounds.Left - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((rect[Occ].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = rect[Occ].Bounds.Left;
                            }
                            if (rect[i].Bounds.Contains(rect[i].Bounds.Right + 1, ((rect[i].Bounds.Right + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((rect[i].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = rect[i].Bounds.Right;
                            }
                            if (y1 > y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Top + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Top;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Bottom + 1))
                                {
                                    x2 = (rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Bottom + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Bottom;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Top + 1))
                                {
                                    x2 = (rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = rect[Occ].Bounds.Left;
                                x2 = rect[Occ].Bounds.Right;
                            }
                        }
                        else
                        {
                            if (y1 < y2)
                            {
                                y1 = rect[Occ].Bounds.Bottom;
                                y2 = rect[i].Bounds.Top;
                            }
                            else
                            {
                                y1 = rect[Occ].Bounds.Top;
                                y2 = rect[i].Bounds.Bottom;
                            }
                        }
                        dist = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        break;
                    }
                }
                for (int i = close + 1; i < rect.Count(); i++)
                {
                    if (i != Occ)
                    {
                        int x1 = rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2;
                        int y1 = rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2;
                        int x2 = rect[i].Bounds.X + rect[i].Bounds.Width / 2;
                        int y2 = rect[i].Bounds.Y + rect[i].Bounds.Height / 2;
                        if (x1 < x2)
                        {
                            if (rect[Occ].Bounds.Contains(rect[Occ].Bounds.Right - 1, ((rect[Occ].Bounds.Right - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((rect[Occ].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = rect[Occ].Bounds.Right;
                            }
                            if (rect[i].Bounds.Contains(rect[i].Bounds.Left + 1, ((rect[i].Bounds.Left + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((rect[i].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = rect[i].Bounds.Left;
                            }
                            if (y1 > y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Top + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Top;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Bottom + 1))
                                {
                                    x2 = (rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Bottom + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Bottom;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Top + 1))
                                {
                                    x2 = (rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = rect[Occ].Bounds.Right;
                                x2 = rect[i].Bounds.Left;
                            }
                        }
                        else if (x1 > x2)
                        {
                            if (rect[Occ].Bounds.Contains(rect[Occ].Bounds.Left - 1, ((rect[Occ].Bounds.Left - 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y1 = ((rect[Occ].Bounds.Left - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x1 = rect[Occ].Bounds.Left;
                            }
                            if (rect[i].Bounds.Contains(rect[i].Bounds.Right + 1, ((rect[i].Bounds.Right + 1 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1)))
                            {
                                y2 = ((rect[i].Bounds.Right - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1);
                                x2 = rect[i].Bounds.Right;
                            }
                            if (y1 > y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Top + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Top;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Bottom + 1))
                                {
                                    x2 = (rect[i].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Bottom;
                                }
                            }
                            else if (y1 < y2)
                            {
                                if (rect[Occ].Bounds.Contains((rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[Occ].Bounds.Bottom + 1))
                                {
                                    x1 = (rect[Occ].Bounds.Bottom * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y1 = rect[Occ].Bounds.Bottom;
                                }
                                if (rect[i].Bounds.Contains((rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1), rect[i].Bounds.Top + 1))
                                {
                                    x2 = (rect[i].Bounds.Top * (x2 - x1) + x1 * (y2 - y1) - y1 * (x2 - x1)) / (y2 - y1);
                                    y2 = rect[i].Bounds.Top;
                                }
                            }
                            else
                            {
                                x1 = rect[Occ].Bounds.Left;
                                x2 = rect[Occ].Bounds.Right;
                            }
                        }
                        else
                        {
                            if (y1 < y2)
                            {
                                y1 = rect[Occ].Bounds.Bottom;
                                y2 = rect[i].Bounds.Top;
                            }
                            else
                            {
                                y1 = rect[Occ].Bounds.Top;
                                y2 = rect[i].Bounds.Bottom;
                            }
                        }
                        int distThis = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        if (dist > distThis)
                        {
                            dist = distThis;
                            close = i;
                        }
                    }
                }
                if (rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2 < rect[close].Bounds.X + rect[close].Bounds.Width / 2)
                {
                    if (rect[Occ].Bounds.Right > rect[close].Bounds.Left)
                    {
                        if (rect[Occ].Bounds.Y > rect[close].Bounds.Y)
                        {
                            do
                            {
                                rect[Occ].Bounds.Y--;
                            } while (CheckCollided(Occ));
                            rect[Occ].Bounds.Y++;

                        }
                        else
                        {
                            do
                            {
                                rect[Occ].Bounds.Y++;
                            } while (CheckCollided(Occ));
                            rect[Occ].Bounds.Y--;
                        }
                    }
                    else
                    {
                        if (rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2 < rect[close].Bounds.Y + rect[close].Bounds.Height / 2)
                        {
                            if (rect[Occ].Bounds.Bottom > rect[close].Bounds.Top)
                            {
                                do
                                {
                                    rect[Occ].Bounds.X++;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X--;
                            }
                            else
                            {
                                int x1 = rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2;
                                int y1 = rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2;
                                int x2 = rect[close].Bounds.X + rect[close].Bounds.Width / 2;
                                int y2 = rect[close].Bounds.Y + rect[close].Bounds.Height / 2;
                                do
                                {
                                    rect[Occ].Prev.Y = rect[Occ].Bounds.Y;
                                    rect[Occ].Bounds.X++;
                                    rect[Occ].Bounds.Y = ((rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - rect[Occ].Bounds.Height / 2;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X--;
                                if (rect[Occ].Bounds.X != rect[Occ].Prev.X)
                                {
                                    was = true;
                                }
                                rect[Occ].Prev.X = rect[Occ].Bounds.X;
                                rect[Occ].Bounds.Y = rect[Occ].Prev.Y;
                            }
                        }
                        else
                        {
                            if (rect[Occ].Bounds.Top < rect[close].Bounds.Bottom)
                            {
                                do
                                {
                                    rect[Occ].Bounds.X++;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X--;
                            }
                            else
                            {
                                int x1 = rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2;
                                int y1 = rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2;
                                int x2 = rect[close].Bounds.X + rect[close].Bounds.Width / 2;
                                int y2 = rect[close].Bounds.Y + rect[close].Bounds.Height / 2;
                                do
                                {
                                    rect[Occ].Prev.Y = rect[Occ].Bounds.Y;
                                    rect[Occ].Bounds.X++;
                                    rect[Occ].Bounds.Y = ((rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - rect[Occ].Bounds.Height / 2;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X--;
                                if (rect[Occ].Bounds.X != rect[Occ].Prev.X)
                                {
                                    was = true;
                                }
                                rect[Occ].Prev.X = rect[Occ].Bounds.X;
                                rect[Occ].Bounds.Y = rect[Occ].Prev.Y;
                            }
                        }
                    }
                }
                else
                {
                    if (rect[Occ].Bounds.Left < rect[close].Bounds.Right)
                    {
                        if (rect[Occ].Bounds.Y > rect[close].Bounds.Y)
                        {
                            do
                            {
                                rect[Occ].Bounds.Y--;
                            } while (CheckCollided(Occ));
                            rect[Occ].Bounds.Y++;

                        }
                        else
                        {
                            do
                            {
                                rect[Occ].Bounds.Y++;
                            } while (CheckCollided(Occ));
                            rect[Occ].Bounds.Y--;
                        }
                    }
                    else
                    {
                        if (rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2 < rect[close].Bounds.Y + rect[close].Bounds.Height / 2)
                        {
                            if (rect[Occ].Bounds.Bottom > rect[close].Bounds.Top)
                            {
                                do
                                {
                                    rect[Occ].Bounds.X--;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X++;
                            }
                            else
                            {
                                int x1 = rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2;
                                int y1 = rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2;
                                int x2 = rect[close].Bounds.X + rect[close].Bounds.Width / 2;
                                int y2 = rect[close].Bounds.Y + rect[close].Bounds.Height / 2;
                                do
                                {
                                    rect[Occ].Prev.Y = rect[Occ].Bounds.Y;
                                    rect[Occ].Bounds.X--;
                                    rect[Occ].Bounds.Y = ((rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - rect[Occ].Bounds.Height / 2;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X++;
                                if (rect[Occ].Bounds.X != rect[Occ].Prev.X)
                                {
                                    was = true;
                                }
                                rect[Occ].Prev.X = rect[Occ].Bounds.X;
                                rect[Occ].Bounds.Y = rect[Occ].Prev.Y;
                            }
                        }
                        else
                        {
                            if (rect[Occ].Bounds.Top < rect[close].Bounds.Bottom)
                            {
                                do
                                {
                                    rect[Occ].Bounds.X--;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X++;
                            }
                            else
                            {
                                int x1 = rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2;
                                int y1 = rect[Occ].Bounds.Y + rect[Occ].Bounds.Height / 2;
                                int x2 = rect[close].Bounds.X + rect[close].Bounds.Width / 2;
                                int y2 = rect[close].Bounds.Y + rect[close].Bounds.Height / 2;
                                do
                                {
                                    rect[Occ].Prev.Y = rect[Occ].Bounds.Y;
                                    rect[Occ].Bounds.X--;
                                    rect[Occ].Bounds.Y = ((rect[Occ].Bounds.X + rect[Occ].Bounds.Width / 2 - x1) * (y2 - y1) + y1 * (x2 - x1)) / (x2 - x1) - rect[Occ].Bounds.Height / 2;
                                } while (CheckCollided(Occ));
                                rect[Occ].Bounds.X++;
                                if (rect[Occ].Bounds.X != rect[Occ].Prev.X)
                                {
                                    was = true;
                                }
                                rect[Occ].Prev.X = rect[Occ].Bounds.X;
                                rect[Occ].Bounds.Y = rect[Occ].Prev.Y;
                            }
                        }
                    }
                }
                if (rect[Occ].Bounds.X != rect[Occ].Prev.X || rect[Occ].Bounds.Y != rect[Occ].Prev.Y)
                {
                    was = true;
                }
                rect[Occ].Prev.X = rect[Occ].Bounds.X;
                rect[Occ].Prev.Y = rect[Occ].Bounds.Y;
            }
            else
            {
                was = false;
            }
            rect[Occ].Image.Location = new Point(rect[Occ].Bounds.X / 10, rect[Occ].Bounds.Y / 10);
            return was;
        }

        /// <summary>
        /// Allighns every rectangle to valid positions
        /// </summary>
        public static void CheckDistAll()
        {
            if (rect.Count() > 1)
            {
                bool was = true;
                while (was)
                {
                    was = false;
                    for (int i = 0; i < rect.Count; i++)
                    {
                        was = was || CheckDist(i);
                    }
                }
            }
        }

        /// <summary>
        /// Drags every rectangle, screen drag effect
        /// </summary>
        private void DragAll()
        {
            for (int i = 0; i < rect.Count(); i++)
            {
                rect[i].Bounds.X = rect[i].Bounds.X + (Mouse.Pos.X - Mouse.Prev.X) * 10;
                rect[i].Bounds.Y = rect[i].Bounds.Y + (Mouse.Pos.Y - Mouse.Prev.Y) * 10;
                rect[i].Prev.X = rect[i].Bounds.X;
                rect[i].Prev.Y = rect[i].Bounds.Y;
                rect[i].Image.Location = new Point(rect[i].Bounds.X / 10, rect[i].Bounds.Y / 10);
            }
            pictureBox1.Refresh();
        }
    }

    public class Mouse
    {
        public static Point Pos;
        public static Point Offset;
        public static Point Prev;
        public static bool down = false;
    }

    public class Rect
    {
        public Rectangle Bounds;
        public Rectangle Image;
        public int monitor; 
        public string name;
        public string mac; 
        public Point Prev;

        public Rect(int x, int y, int w, int h, int j, string n, string m)
        {
            Bounds = new Rectangle(x + 1 + SystemShareForm.Pw * 5, y + 1 + SystemShareForm.Ph * 5, w - 2, h - 2);
            Image = new Rectangle((x + 1)/ 10 + SystemShareForm.Pw / 2, (y + 1) / 10 + SystemShareForm.Ph / 2, (w -2) /10, (h - 2) / 10);
            monitor = j;
            name = n;
            mac = m;
        }
        public Rect(int x, int y, int w, int h, string n, int j)
        {
            Bounds = new Rectangle(x + 1 + Arrangement.Pw * 5, y + 1 + Arrangement.Ph * 5, w - 2, h - 2);
            Image = new Rectangle((x + 1) / 10 + Arrangement.Pw / 2, (y + 1) / 10 + Arrangement.Ph / 2, (w - 2) / 10, (h - 2) / 10);
            monitor = j;
            name = n;
        }

        /// <summary>
        /// Updates Bounds
        /// </summary>
        public void Move()
        {
            Image.Location = new Point(Mouse.Pos.X - Mouse.Offset.X - Image.Width / 2, Mouse.Pos.Y - Mouse.Offset.Y - Image.Height / 2);
            Bounds.X = Image.X * 10;
            Bounds.Y = Image.Y * 10;
        }

        /// <summary>
        /// Checks if a point is inside the image
        /// </summary>
        /// <returns></returns>
        public bool IsThere(Point p)
        {
            if (Image.Contains(p))
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
