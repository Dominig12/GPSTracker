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
        public Form1 Form;
        public Map Map;
        public MapPoint InitialCoords;
        private TransparentPanel _panel;
        private PictureBox _map;
        public Form3()
        {
            InitializeComponent();
            toolTip1.ShowAlways = true;
            CreateComponents();
            Shown += (sender, args) =>
            {
                Map.InitStaticMap(mapWidth : _panel.Width, mapHeight : _panel.Height, stepGrid : 1, scale : 1);
                _map.Image = Map.GetStaticMap();
            };
        }
        
        private void CreateComponents()
        {
            _map = new PictureBox
            {
                Size = new Size(width : 330, height : 330),
                Location = new Point(x : 10, y : 10),
                SizeMode = PictureBoxSizeMode.Zoom,
            };
            
            // Прозрачная панель
            _panel = new TransparentPanel
            {
                Size = _map.Size,
                Location = _map.Location,
                Parent = _map.Parent,
                BackColor = Color.Transparent,
            };

            _panel.MouseMove += panel1_MouseMove;
            _panel.MouseDown += panel1_MouseDown;
            _panel.Paint += display_Paint;
    
            // Добавление на форму
            Controls.Add(value : _map);
            Controls.Add(value : _panel);
            
            _panel.BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _map.Invalidate();
            _panel.Invalidate();
        }
        
        private void display_Paint(object sender, PaintEventArgs e)
        {
            MapPoint nullPoint = new MapPoint()
            {
                X = InitialCoords.X - 5,
                Y = InitialCoords.Y - 5,
                Z = InitialCoords.Z
            };
            
            if (Form.PlayerGps == null)
            {
                return;
            }
            
            MapPoint pointPlayer = new MapPoint()
            {
                X = Form.PlayerGps.X - nullPoint.X,
                Y = Form.PlayerGps.Y - nullPoint.Y,
                Z = Form.PlayerGps.Z,
                Tag = Form.PlayerGps.Tag
            };
            
            if (Form.PlayerGps.Z == InitialCoords.Z)
            {
                // Отрисовка игрока
                Map.DrawPoint(
                    mapWidth : _map.Width,
                    mapHeight : _map.Height,
                    g : e.Graphics,
                    point : pointPlayer,
                    entityColor : Color.Red,
                    textBrush : Brushes.White
                );
            }
            
            Form.SignalsGps ??= new List<MapPoint>();
            
            foreach (MapPoint signalsGps in Form.SignalsGps)
            {
                if (signalsGps.Z != InitialCoords.Z)
                {
                    continue;
                }
                
                MapPoint point = new MapPoint()
                {
                    X = signalsGps.X - nullPoint.X + 1,
                    Y = signalsGps.Y - nullPoint.Y + 1,
                    Z = signalsGps.Z,
                    Tag = signalsGps.Tag
                };
                
                Map.DrawPoint(
                    mapWidth : _map.Width,
                    mapHeight : _map.Height,
                    g : e.Graphics,
                    point : point,
                    entityColor : Color.Red,
                    textBrush : Brushes.White
                );
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            (int x, int y) coords = CoordinateHelper.ConvertToMapCoordinates(
                imageX : e.X,
                imageY : e.Y,
                mapWidth : 11,
                mapHeight : 11,
                imageWidth : _panel.Width,
                imageHeight : _panel.Height );
            
            int x = InitialCoords.X - 6 + coords.x;
            int y = InitialCoords.Y - 6 + coords.y;
            int z = Convert.ToInt32(value : InitialCoords.Z);
            
            string coordsStr = String.Format(format : "{0} {1} {2}", arg0 : x, arg1 : y, arg2 : z);
            StreamWriter writer = new StreamWriter(path : Config.PathOpenWormhole, append : false);
            writer.WriteLine(value : coordsStr);
            writer.Close();
            this.Close();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            (int x, int y) coords = CoordinateHelper.ConvertToMapCoordinates(
                imageX : e.X,
                imageY : e.Y,
                mapWidth : 11,
                mapHeight : 11,
                imageWidth : _panel.Width,
                imageHeight : _panel.Height );
            
            int x = InitialCoords.X - 6 + coords.x;
            int y = InitialCoords.Y - 6 + coords.y;
            int z = Convert.ToInt32(value : InitialCoords.Z);
            
            string coordsText = String.Format(format : "{0} {1} {2}", arg0 : x, arg1 : y, arg2 : z);
            toolTip1.SetToolTip(control : _panel, caption : coordsText);
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
