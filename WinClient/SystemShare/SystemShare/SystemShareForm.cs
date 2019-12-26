using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SystemShare
{
    public partial class SystemShareForm : Form
    {
        List<Rectangle> monit = new List<Rectangle>();

        public SystemShareForm()
        {
            InitializeComponent();
            InitDataPop();
        }

        #region Form Control
        private void button4_Click(object sender, EventArgs e)
        {
            GetDisplays();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            UpdateData();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SystemShareForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.open = false;
        }
        #endregion

        /// <summary>
        /// Updates database
        /// </summary>
        private void UpdateData()
        {
            if (FieldCheck())
            {
                label3.Visible = false;
                label7.Visible = false;
                label6.Visible = false;
                label8.Visible = false;
                using (StreamWriter writetext = new StreamWriter(MainForm.dir + "ClientData.txt"))
                {
                    writetext.WriteLine(textBox1.Text);
                    writetext.WriteLine(monit.Count().ToString());
                    writetext.WriteLine(textBox5.Text);
                    writetext.WriteLine(textBox4.Text);
                }
                using (StreamWriter writetext = new StreamWriter(MainForm.dir + "DisplayData.txt"))
                {
                    foreach (Rectangle mon in monit)
                    {
                        writetext.WriteLine("W: " + mon.Width.ToString() + ";" + "H: " + mon.Height.ToString() + ";" + "X: " + mon.X.ToString() + ";" + "Y: " + mon.Y.ToString() + ";");
                    }
                }
                Connection.Disconnect();
                MainForm.PopData();
                Connection.Connect();
            }
        }

        /// <summary>
        /// Checks for correct data formats
        /// </summary>
        /// <returns></returns>
        private bool FieldCheck()
        {
            bool correct = true;
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                label3.Visible = true;
                correct = false;
            }
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                label7.Visible = true;
                correct = false;
            }
            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                label6.Visible = true;
                correct = false;
            }
            try
            {
                Convert.ToInt32(textBox4.Text);
            }
            catch (Exception)
            {
                label8.Visible = true;
                correct = false;
            }
            return correct;
        }

        /// <summary>
        /// Fetches display data from windows
        /// </summary>
        private void GetDisplays()
        {
            listBox1.Items.Clear();
            monit.Clear();
            foreach (Screen screen in Screen.AllScreens)
            {
                int resy = screen.Bounds.Height;
                int resx = screen.Bounds.Width;
                int relposx = screen.Bounds.X;
                int relposy = screen.Bounds.Y;
                monit.Add(new Rectangle(relposx, relposy, resx, resy));
                listBox1.Items.Add(resx.ToString() + "x" + resy.ToString() + " at " + relposx.ToString() + "," + relposy.ToString());
            }
        }

        /// <summary>
        /// Fills out data fields
        /// </summary>
        private void InitDataPop()
        {
            textBox1.Text = MainForm.name;
            if (MainForm.port != 0)
            {
                textBox4.Text = MainForm.port.ToString();
            }
            else
            {
                textBox4.Text = "";
            }
            textBox5.Text = MainForm.ip;
            if (MainForm.Disp.Any())
            {
                listBox1.Items.Clear();
                monit = MainForm.Disp;
                foreach (Rectangle mon in MainForm.Disp)
                {
                    listBox1.Items.Add(mon.Width + "x" + mon.Height + " at " + mon.X + "," + mon.Y);
                }
            }
            else
            {
                GetDisplays();
            }
        }
    }
}
