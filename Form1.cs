using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace GPSTracker
{
    public partial class Form1 : Form
    {
        private Dictionary<int, Map> Maps { get; set; } = new Dictionary<int, Map>();
        private TransparentPanel _panel;
        private PictureBox _map;
        private int _selectedZ;
        public MapPoint PlayerGps;
        public List<MapPoint> SignalsGps;

        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;
            CreateComponents();
            InitMaps();
            InitEmpty(new[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9
            });
        }

        private void InitEmpty(int[] zList)
        {
            foreach (int z in zList)
            {
                if (Maps.ContainsKey(z))
                {
                    continue;
                }
                
                List<MapPoint> points = new List<MapPoint>();
                    
                Map newMap = new Map(points);
                
                newMap.InitStaticMap(_map.Width, _map.Height, 15, 1f);
                
                Maps.Add(z, newMap);
            }
        }

        private void InitMaps()
        {
            var renderer = new MapRenderer();
            string[] lines = File.ReadAllLines("maps.txt");

            foreach (string line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    List<MapPoint> points = renderer.ParseMap(parts[0]);
                    
                    Map newMap = new Map(points);
                    
                    Maps.Add(int.Parse(parts[1]), newMap);
                }
            }
            
            foreach (KeyValuePair<int,Map> map in Maps)
            {
                map.Value.InitStaticMap(_map.Width, _map.Height, 15, 1f);
            }

            if (Maps.Count > 0)
            {
                KeyValuePair<int, Map> map = Maps.First();
                _map.Image = map.Value.GetStaticMap();
                _selectedZ = map.Key;
            }
        }

        private void CreateComponents()
        {
            Size = new Size(Config.Width, Config.Height);
            _map = new PictureBox
            {
                Size = new Size(Config.Width, Config.Width),
                Location = new Point(10, 10),
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

            _panel.Paint += display_Paint;
            _panel.MouseMove += display_MouseMove;
            _panel.MouseDown += display_MouseDown;
    
            // Добавление на форму
            Controls.Add(_map);
            Controls.Add(_panel);
            
            _panel.BringToFront();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.D1:
                    _selectedZ = 1;
                    if (Maps.ContainsKey(1))
                    {
                        _map.Image = Maps[1].GetStaticMap();
                    }
                    break;
                case Keys.D2:
                    _selectedZ = 2;
                    if (Maps.ContainsKey(2))
                    {
                        _map.Image = Maps[2].GetStaticMap();
                    }
                    break;
                case Keys.D3:
                    _selectedZ = 3;
                    if (Maps.ContainsKey(3))
                    {
                        _map.Image = Maps[3].GetStaticMap();
                    }
                    break;
                case Keys.D4:
                    _selectedZ = 4;
                    if (Maps.ContainsKey(4))
                    {
                        _map.Image = Maps[4].GetStaticMap();
                    }
                    break;
                case Keys.D5:
                    _selectedZ = 5;
                    if (Maps.ContainsKey(5))
                    {
                        _map.Image = Maps[5].GetStaticMap();
                    }
                    break;
                case Keys.D6:
                    _selectedZ = 6;
                    if (Maps.ContainsKey(6))
                    {
                        _map.Image = Maps[6].GetStaticMap();
                    }
                    break;
                case Keys.D7:
                    _selectedZ = 7;
                    if (Maps.ContainsKey(7))
                    {
                        _map.Image = Maps[7].GetStaticMap();
                    }
                    break;
                case Keys.D8:
                    _selectedZ = 8;
                    if (Maps.ContainsKey(8))
                    {
                        _map.Image = Maps[8].GetStaticMap();
                    }
                    break;
                case Keys.D9:
                    _selectedZ = 9;
                    if (Maps.ContainsKey(9))
                    {
                        _map.Image = Maps[9].GetStaticMap();
                    }
                    break;
            }
        }

        private void display_Paint(object sender, PaintEventArgs e)
        {
            if (PlayerGps == null)
            {
                return;
            }

            MapPoint pointPlayer = new MapPoint()
            {
                X = PlayerGps.X,
                Y = PlayerGps.Y + 1,
                Z = PlayerGps.Z,
                Tag = PlayerGps.Tag
            };
            if (PlayerGps.Z == _selectedZ)
            {
                // Отрисовка игрока
                Maps[_selectedZ].DrawPoint(
                    _map.Width,
                    _map.Height,
                    g : e.Graphics,
                    pointPlayer,
                    entityColor : Color.Red,
                    textBrush : Brushes.White
                );
            }

            SignalsGps ??= new List<MapPoint>();

            foreach (MapPoint signalsGps in SignalsGps)
            {
                if (signalsGps.Z != _selectedZ)
                {
                    continue;
                }
                
                MapPoint point = new MapPoint()
                {
                    X = signalsGps.X,
                    Y = signalsGps.Y + 1,
                    Z = signalsGps.Z,
                    Tag = signalsGps.Tag
                };
                
                Maps[_selectedZ].DrawPoint(
                    _map.Width,
                    _map.Height,
                    g : e.Graphics,
                    point,
                    entityColor : Color.Red,
                    textBrush : Brushes.White
                );
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _map.Invalidate();
            _panel.Invalidate();
        }
        
        private void timer2_Tick(object sender, EventArgs e)
        {
            UpdateCoords();
        }

        private void UpdateCoords()
        {
            string[] lines = File.ReadAllLines(Config.PathGpsData);

            if (lines.Length < 1)
            {
                return;
            }


            GPS gps = null;

            try
            {
                gps = JsonConvert.DeserializeObject<GPS>(lines[0]);
            }
            catch 
            {
                // ignore
            }

            if (gps == null || gps.Position == null || gps.Position.Count < 3)
            {
                return;
            }
            
            PlayerGps = new MapPoint()
            {
                X = gps.Position[0],
                Y = gps.Position[1],
                Z = gps.Position[2],
                Tag = gps.Tag
            };

            SignalsGps = new List<MapPoint>();
            foreach (Signal gpsSignal in gps.Signals)
            {
                if (gpsSignal.Position == null || gpsSignal.Position.Count < 3)
                {
                    continue;
                }

                MapPoint signal = new MapPoint()
                {
                    X = gpsSignal.Position[0],
                    Y = gpsSignal.Position[1],
                    Z = gpsSignal.Position[2],
                    Tag = gpsSignal.Tag
                };
                
                SignalsGps.Add(signal);
            }
        }

        private void display_MouseDown(object sender, MouseEventArgs e)
        {
            float coef = _panel.Width / 255f;
            int x = Convert.ToInt32(value : (e.X - coef / 2) / coef);
            int y = Convert.ToInt32(value : 255f - (e.Y - coef / 2) / coef);
            int z = Convert.ToInt32(value : _selectedZ.ToString());
            Form3 form = new Form3();
            MapPoint center = new MapPoint()
            {
                X = x,
                Y = y,
                Z = z
            };
            form.InitialCoords = center;

            MapPoint nullPoint = new MapPoint()
            {
                X = center.X - 5,
                Y = center.Y - 6,
                Z = center.Z
            };

            List<MapPoint> pointsMiniMap = Maps[_selectedZ].GetPoints().Where(point => Math.Abs(point.X - center.X) < 7)
                .Where(point => Math.Abs(point.Y - center.Y) < 7).ToList();
            
            List<MapPoint> deltaPoints = new List<MapPoint>();

            foreach (MapPoint mapPoint in pointsMiniMap)
            {
                MapPoint deltaPoint = new MapPoint()
                {
                    X = mapPoint.X - nullPoint.X,
                    Y = mapPoint.Y - nullPoint.Y,
                    Z = nullPoint.Z,
                    ColorHex = mapPoint.ColorHex
                };
                
                deltaPoints.Add(deltaPoint);
            }
            
            form.Map = new Map(deltaPoints, 11, 11);
            form.Form = this;
            form.Show();
        }

        private void display_MouseMove(object sender, MouseEventArgs e)
        {
            float coef = _panel.Width / 255f;
            int x = Convert.ToInt32(value : (e.X - coef / 2) / coef);
            int y = Convert.ToInt32(value : 255f - (e.Y - coef / 2) / coef);
            int z = Convert.ToInt32(value : _selectedZ);
            string coordsText = String.Format(format : "{0} {1} {2}", arg0 : x, arg1 : y, arg2 : z);
            toolTip1.SetToolTip(control : _panel, caption : coordsText);
        }
    }
    public class PositionGps
    {
        public string Position { get; set; }
        public string SavedPosition { get; set; }
        public List<string> OtherGps { get; set; }
    }
}
