using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace GPSTracker
{
    public partial class Form1 : Form
    {
        private List<MapPoint> _mapPoints = new List<MapPoint>();
        private Panel _panel;

        bool active = false;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            
            _panel = display;
            _panel.Paint += display_Paint;
            
            var renderer = new MapRenderer();
            _mapPoints = renderer.ParseMap(mapName : "boxstation.dmm");
        }

        private void display_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(color : Color.Black);
            float coef = display.Width / 255;
            for (float i = display.Width; i >= 0; i -= coef * 15)
            {
                e.Graphics.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : 0, y1 : i, x2 : display.Width, y2 : i);
                e.Graphics.DrawString(s : (256 - (i / coef)).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : 0, y : i);
                e.Graphics.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : i, y1 : 0, x2 : i, y2 : display.Width);
                e.Graphics.DrawString(s : (i / coef).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : i, y : 0);
            }
            
            int width = display.Width;
            int height = display.Height;
            int coef_X = width / 255;
            int coef_y = height / 255;
            
            foreach (MapPoint point in _mapPoints)
            {
                point.X = point.X * coef_X;
                point.Y = point.Y * coef_y;
                using (var brush = new SolidBrush(color : ColorTranslator.FromHtml(htmlColor : point.ColorHex)))
                {
                    e.Graphics.FillRectangle(brush : brush, x : point.X, y : point.Y, width : coef_X * 0.5f, height : 1);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            display.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            active = !active;
            if (active)
            {
                label3.Text = "ON";
            }
            else
            {
                label3.Text = "OFF";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            // display_Paint();
        }

        private void z_layer_combo_box_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            z_layer_combo_box.SelectedItem = z_layer_combo_box.Items[index : 0];
            // display_Paint();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void display_MouseDown(object sender, MouseEventArgs e)
        {
            // float coef = display.Width / 255;
            // int x = Convert.ToInt32(value : (e.X - coef / 2) / coef);
            // int y = Convert.ToInt32(value : 255 - (e.Y - coef / 2) / coef);
            // int z = Convert.ToInt32(value : z_layer_combo_box.SelectedItem.ToString());
            // Form3 form = new Form3();
            // form.form = this;
            // form.z_layers_gps = z_layers_gps;
            // form.z_layers_map = z_layers_map;
            // form.z_layers_player = z_layers_player;
            // form.initial_coords = new KeyValuePair<int, KeyValuePair<int, int>>(key : z, value : new KeyValuePair<int, int>(key : x, value : y));
            // form.Show();
        }

        private void display_MouseMove(object sender, MouseEventArgs e)
        {
            float coef = display.Width / 255;
            int x = Convert.ToInt32(value : (e.X - coef / 2) / coef);
            int y = Convert.ToInt32(value : 255 - (e.Y - coef / 2) / coef);
            int z = Convert.ToInt32(value : 2);
            string coords_text = String.Format(format : "{0} {1} {2}", arg0 : x, arg1 : y, arg2 : z);
            toolTip1.SetToolTip(control : display, caption : coords_text);
        }

        private void coords_LocationChanged(object sender, EventArgs e)
        {
            
        }
    }
    public class PositionGps
    {
        public string Position { get; set; }
        public string SavedPosition { get; set; }
        public List<string> OtherGps { get; set; }
    }
}
