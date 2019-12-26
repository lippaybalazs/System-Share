using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SystemShare
{
    public partial class Arrangement : Form
    {
        private static List<Rect> Rect = new List<Rect>();
        public static int Pw;
        public static int Ph;

        public Arrangement()
        {
            InitializeComponent();
            Pw = pictureBox1.Width;
            Ph = pictureBox1.Height;
            PopDispData();
            pictureBox1.Refresh();
        }

        #region Form Control
        private void Arrangement_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemShareForm.open = false;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Mouse.down = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Mouse.Prev = Mouse.Pos;
            Mouse.Pos = e.Location;
            if (Mouse.down)
            {
                DragAll();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Mouse.down = true;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < Rect.Count(); i++)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(Rect[i].Image.X - 1, Rect[i].Image.Y - 1, Rect[i].Image.Width + 2, Rect[i].Image.Height + 2));
                if (i == 0)
                {
                    e.Graphics.FillRectangle(new SolidBrush(SystemShareForm.Secondary), new Rectangle(Rect[i].Image.X + 2, Rect[i].Image.Y + 2, Rect[i].Image.Width - 4, Rect[i].Image.Height - 4));
                    e.Graphics.FillRectangle(new SolidBrush(SystemShareForm.Primary), new Rectangle(Rect[i].Image.X + 4, Rect[i].Image.Y + 4, Rect[i].Image.Width - 8, Rect[i].Image.Height - 8));
                }
                else
                {
                    e.Graphics.FillRectangle(new SolidBrush(SystemShareForm.Primary), new Rectangle(Rect[i].Image.X + 2, Rect[i].Image.Y + 2, Rect[i].Image.Width - 4, Rect[i].Image.Height - 4));
                }
                e.Graphics.DrawString(Rect[i].monitor + ", " + Rect[i].name.ToString(), new Font("Arial", 10), Brushes.White, Rect[i].Image.X + 5, Rect[i].Image.Y + 5);
            }
        }
        #endregion

        /// <summary>
        /// Populates display data
        /// </summary>
        private static void PopDispData()
        {
            Rect.Clear();
            for (int i = 0; i < Connection.Clients.Count(); i++)
            {
                for (int j = 0; j < Connection.Clients[i].Area.Count(); j++)
                {
                    Rect.Add(new Rect(Connection.Clients[i].Area[j].Bounds.X, Connection.Clients[i].Area[j].Bounds.Y, Connection.Clients[i].Area[j].Bounds.Width, Connection.Clients[i].Area[j].Bounds.Height, Connection.Clients[i].name, j));
                }
            }
        }

        /// <summary>
        /// Drags the screen
        /// </summary>
        private void DragAll()
        {
            for (int i = 0; i < Rect.Count(); i++)
            {
                Rect[i].Bounds.X = Rect[i].Bounds.X + (Mouse.Pos.X - Mouse.Prev.X) * 10;
                Rect[i].Bounds.Y = Rect[i].Bounds.Y + (Mouse.Pos.Y - Mouse.Prev.Y) * 10;
                Rect[i].Prev.X = Rect[i].Bounds.X;
                Rect[i].Prev.Y = Rect[i].Bounds.Y;
                Rect[i].Image.Location = new Point(Rect[i].Bounds.X / 10, Rect[i].Bounds.Y / 10);
            }
            pictureBox1.Refresh();
        }
    }
}
