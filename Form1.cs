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
        private PictureBox _panel;
        private Bitmap _staticMap;

        private GraphicsState _state;

        bool active = false;

        public Form1()
        {
            InitializeComponent();
            
            _panel = pictureBox1;
            _panel.Paint += display_Paint;
            
            var renderer = new MapRenderer();
            _mapPoints = renderer.ParseMap(mapName : "boxstation.dmm");
            StaticMap();
            mapCard.Refresh();
            mapCard.Paint += map_Paint;
        }

        private void map_Paint(
            object sender,
            PaintEventArgs e )
        {
            _staticMap = new Bitmap(width : mapCard.Width, height : mapCard.Height);
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
            using ( Graphics g = Graphics.FromImage( image : _staticMap ) )
            {
                
                g.Clear(color : Color.Black);
                float coef = _panel.Width / 255;
                for (float i = _panel.Width; i >= 0; i -= coef * 15)
                {
                    g.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : 0, y1 : i, x2 : _panel.Width, y2 : i);
                    g.DrawString(s : (256 - (i / coef)).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : 0, y : i);
                    g.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : i, y1 : 0, x2 : i, y2 : _panel.Width);
                    g.DrawString(s : (i / coef).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : i, y : 0);
                }
            
                int width = _panel.Width;
                int height = _panel.Height;
                int coef_X = width / 255;
                int coef_y = height / 255;
            
                foreach (MapPoint point in _mapPoints)
                {
                    using (var brush = new SolidBrush(color : ColorTranslator.FromHtml(htmlColor : point.ColorHex)))
                    {
                        g.FillRectangle(brush : brush, x : point.GetScaledX( scale : coef_X ), y : point.GetScaledY( scale : coef_y ), width : coef_X * 0.5f, height : 1);
                    }
                }
            }
            mapCard.Image = _staticMap;
        }

        private void StaticMap()
        {
            _staticMap = new Bitmap(width : mapCard.Width, height : mapCard.Height);
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
            using ( Graphics g = Graphics.FromImage( image : _staticMap ) )
            {
                
                g.Clear(color : Color.Black);
                float coef = _panel.Width / 255;
                for (float i = _panel.Width; i >= 0; i -= coef * 15)
                {
                    g.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : 0, y1 : i, x2 : _panel.Width, y2 : i);
                    g.DrawString(s : (256 - (i / coef)).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : 0, y : i);
                    g.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : i, y1 : 0, x2 : i, y2 : _panel.Width);
                    g.DrawString(s : (i / coef).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : i, y : 0);
                }
            
                int width = _panel.Width;
                int height = _panel.Height;
                int coef_X = width / 255;
                int coef_y = height / 255;
            
                foreach (MapPoint point in _mapPoints)
                {
                    using (var brush = new SolidBrush(color : ColorTranslator.FromHtml(htmlColor : point.ColorHex)))
                    {
                        g.FillRectangle(brush : brush, x : point.GetScaledX( scale : coef_X ), y : point.GetScaledY( scale : coef_y ), width : coef_X * 0.5f, height : 1);
                    }
                }
            }
            mapCard.Image = _staticMap;
        }

        private void display_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear( color : Color.Transparent );
            // Отрисовка игрока
            DrawEntity(
                g : e.Graphics,
                position : new[]
                {
                    new Random().Next(minValue : 1, maxValue : 255),
                    new Random().Next(minValue : 1, maxValue : 255)
                },
                tag : "ROSE",
                entityColor : Color.Red,
                textBrush : Brushes.White
            );
            
            // Отрисовка сигналов
            for (int i = 0; i < 1; i++)
            {
                DrawEntity(
                    g : e.Graphics,
                    position : new[]
                    {
                        new Random().Next(minValue : 1, maxValue : 255),
                        new Random().Next(minValue : 1, maxValue : 255)
                    },
                    tag : "EN",
                    entityColor : Color.Blue,
                    textBrush : Brushes.Yellow
                );
            }
        }
        
        private void DrawEntity(
            Graphics g, 
            int[] position, 
            string tag, 
            Color entityColor, 
            Brush textBrush)
        {
            int width = _panel.Width;
            int height = _panel.Height;
            int coef_X = width / 255;
            int coef_y = height / 255;
            
            // Рисуем маркер
            using (var brush = new SolidBrush(color : entityColor))
            {
                g.FillRectangle(brush : brush, x : position[0] * coef_X, y : position[1] * coef_y, width : coef_X * 1f, height : 1);
            }

            // Рисуем текст
            var textSize = g.MeasureString(text : tag, font : SystemFonts.DefaultFont);
            g.DrawString(
                s : tag,
                font : SystemFonts.DefaultFont,
                brush : textBrush,
                x : position[0] - textSize.Width / 2,
                y : position[1]
            );
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            mapCard.Invalidate();
            _panel.Invalidate();
            _panel.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            active = !active;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            mapCard.Invalidate();
            // _panel.Refresh();
            // display_Paint();
        }

        private void z_layer_combo_box_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // z_layer_combo_box.SelectedItem = z_layer_combo_box.Items[index : 0];
            // display_Paint();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void display_MouseDown(object sender, MouseEventArgs e)
        {
            // float coef = _panel.Width / 255;
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
            float coef = _panel.Width / 255;
            int x = Convert.ToInt32(value : (e.X - coef / 2) / coef);
            int y = Convert.ToInt32(value : 255 - (e.Y - coef / 2) / coef);
            int z = Convert.ToInt32(value : 2);
            string coords_text = String.Format(format : "{0} {1} {2}", arg0 : x, arg1 : y, arg2 : z);
            toolTip1.SetToolTip(control : _panel, caption : coords_text);
        }

        private void coords_LocationChanged(object sender, EventArgs e)
        {
            
        }

        private void mapCard_Click(object sender, EventArgs e)
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
