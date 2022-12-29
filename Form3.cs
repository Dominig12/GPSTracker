using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPSTracker
{
    public partial class Form3 : Form
    {
        public Form1 form;
        public SortedList<int, SortedList<string, string>> z_layers_player = new SortedList<int, SortedList<string, string>>();
        public SortedList<int, SortedList<string, string>> z_layers_gps = new SortedList<int, SortedList<string, string>>();
        public SortedList<int, SortedList<string, string>> z_layers_map = new SortedList<int, SortedList<string, string>>();
        public KeyValuePair<int, KeyValuePair<int, int>> initial_coords;
        public Form3()
        {
            InitializeComponent();
            PaintPanel();
            toolTip1.ShowAlways = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            PaintPanel();
        }

        private void PaintPanel()
        {
            Graphics g = panel1.CreateGraphics();
            g.Clear(Color.Black);
            PaintPoints(g, z_layers_map, 1, false);
            PaintPoints(g, z_layers_player, 0.5);
            PaintPoints(g, z_layers_gps, 1, false);
            float coef = panel1.Width / 11;
            for (float i = 1; i < panel1.Width; i += coef)
            {
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGreen)), 0, i, panel1.Width, i);
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGreen)), i, 0, i, panel1.Width);
            }
        }
        public void PaintPoints(Graphics g, SortedList<int, SortedList<string, string>> z_layers, double coef_scale = 1, bool connect = true)
        {

            int z = Convert.ToInt32(initial_coords.Key);

            if (!z_layers.ContainsKey(z))
                return;

            int width = panel1.Width;
            int height = panel1.Height;
            int coef_X = width / 11;
            int coef_y = height / 11;

            int[] last = new int[2]
            {
                -1,
                -1,
            };
            for (int i = -5; i <= 5; i++)
            {
                for (int o = -5; o <= 5; o++)
                {
                    string key = String.Format("{0}:{1}", initial_coords.Value.Key + i, initial_coords.Value.Value + o);
                    if (z_layers[z].Keys.Contains(key))
                    {
                        int[] point = new int[2]
                        {
                            ((i + 5) * coef_X),
                            (height - ((o + 6) * coef_y))
                        };

                        Color clr = ColorTranslator.FromHtml(z_layers[z][key]);

                        Brush color = new SolidBrush(clr);

                        g.FillRectangle(color, point[0], point[1], float.Parse((coef_X * coef_scale).ToString()), float.Parse((coef_y * coef_scale).ToString()));
                        if (last[0] != -1 && last[1] != -1 && connect)
                            g.DrawLine(new Pen(color), last[0], last[1], point[0], point[1]);

                        last = point;
                    }
                }
            }

            /*foreach (KeyValuePair<int, int> coords in z_layers[z])
            {
                int[] point = new int[2]
                {
                    (coords.Key * coef_X),
                    (height - (coords.Value * coef_y))
                };

                g.FillRectangle(color, point[0], point[1], float.Parse((coef_X * coef_scale).ToString()), float.Parse((coef_y * coef_scale).ToString()));
                if (last[0] != -1 && last[1] != -1 && connect)
                    g.DrawLine(new Pen(color), last[0], last[1], point[0], point[1]);

                last = point;
            }*/
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            float coef = panel1.Width / 11;
            int x = initial_coords.Value.Key - 5 + Convert.ToInt32(Math.Round((e.X - coef / 2) / coef, MidpointRounding.ToEven));
            int y = initial_coords.Value.Value + 5 - Convert.ToInt32(Math.Round((e.Y - coef / 2) / coef, MidpointRounding.ToEven));
            int z = Convert.ToInt32(initial_coords.Key);
            string coords = String.Format("{0} {1} {2}", x, y, z);
            StreamWriter writer = new StreamWriter("open_wormhole.txt", false);
            writer.WriteLine(coords);
            writer.Close();
            this.Close();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            float coef = panel1.Width / 11;
            int x = initial_coords.Value.Key - 5 + Convert.ToInt32(Math.Round((e.X - coef / 2) / coef, MidpointRounding.ToEven));
            int y = initial_coords.Value.Value + 5 - Convert.ToInt32(Math.Round((e.Y - coef / 2) / coef, MidpointRounding.ToEven));
            int z = Convert.ToInt32(initial_coords.Key);
            string coords_text = String.Format("{0} {1} {2}", x, y, z);
            toolTip1.SetToolTip(panel1, coords_text);
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
