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
            InitEmpty(zList : new[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9
            });
        }

        private void InitEmpty(int[] zList)
        {
            foreach (int z in zList)
            {
                if (Maps.ContainsKey(key : z))
                {
                    continue;
                }
                
                List<MapPoint> points = new List<MapPoint>();
                    
                Map newMap = new Map(points : points);
                
                newMap.InitStaticMap(mapWidth : _map.Width, mapHeight : _map.Height, stepGrid : 15, scale : 1f);
                
                Maps.Add(key : z, value : newMap);
            }
        }

        private void InitMaps()
        {
            var renderer = new MapRenderer();
            string[] lines = File.ReadAllLines(path : "maps.txt");

            foreach (string line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    List<MapPoint> points = renderer.ParseMap(mapName : parts[0]);
                    
                    Map newMap = new Map(points : points);
                    
                    Maps.Add(key : int.Parse(s : parts[1]), value : newMap);
                }
            }
            
            foreach (KeyValuePair<int,Map> map in Maps)
            {
                map.Value.InitStaticMap(mapWidth : _map.Width, mapHeight : _map.Height, stepGrid : 15, scale : 1f);
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
            Size = new Size(width : Config.Width, height : Config.Height);
            _map = new PictureBox
            {
                Size = new Size(width : Config.Width, height : Config.Width),
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

            _panel.Paint += display_Paint;
            _panel.MouseMove += display_MouseMove;
            _panel.MouseDown += display_MouseDown;
            
            //_map.Paint += MapOnPaint;
    
            // Добавление на форму
            Controls.Add(value : _map);
            Controls.Add(value : _panel);
            
            _panel.BringToFront();
        }

        // private void MapOnPaint(
        //     object sender,
        //     PaintEventArgs e )
        // {
        //     _map.Image = Maps[key : _selectedZ]
        //         .GetCurrentView();
        // }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e : e);

            switch (e.KeyCode)
            {
                case Keys.D1:
                    _selectedZ = 1;
                    if (Maps.ContainsKey(key : 1))
                    {
                        _map.Image = Maps[key : 1].GetStaticMap();
                    }
                    break;
                case Keys.D2:
                    _selectedZ = 2;
                    if (Maps.ContainsKey(key : 2))
                    {
                        _map.Image = Maps[key : 2].GetStaticMap();
                    }
                    break;
                case Keys.D3:
                    _selectedZ = 3;
                    if (Maps.ContainsKey(key : 3))
                    {
                        _map.Image = Maps[key : 3].GetStaticMap();
                    }
                    break;
                case Keys.D4:
                    _selectedZ = 4;
                    if (Maps.ContainsKey(key : 4))
                    {
                        _map.Image = Maps[key : 4].GetStaticMap();
                    }
                    break;
                case Keys.D5:
                    _selectedZ = 5;
                    if (Maps.ContainsKey(key : 5))
                    {
                        _map.Image = Maps[key : 5].GetStaticMap();
                    }
                    break;
                case Keys.D6:
                    _selectedZ = 6;
                    if (Maps.ContainsKey(key : 6))
                    {
                        _map.Image = Maps[key : 6].GetStaticMap();
                    }
                    break;
                case Keys.D7:
                    _selectedZ = 7;
                    if (Maps.ContainsKey(key : 7))
                    {
                        _map.Image = Maps[key : 7].GetStaticMap();
                    }
                    break;
                case Keys.D8:
                    _selectedZ = 8;
                    if (Maps.ContainsKey(key : 8))
                    {
                        _map.Image = Maps[key : 8].GetStaticMap();
                    }
                    break;
                case Keys.D9:
                    _selectedZ = 9;
                    if (Maps.ContainsKey(key : 9))
                    {
                        _map.Image = Maps[key : 9].GetStaticMap();
                    }
                    break;
            }
        }

        private void display_Paint(object sender, PaintEventArgs e)
        {
            PlayerGps ??= new MapPoint()
            {
                ColorHex = "#f44336",
                Tag = "Dark Water",
                X = 10,
                Y = 10,
                Z = 2
            };

            Map map = Maps[key : _selectedZ];

            PlayerGps = PlayerGps.MovePoint(
                point : new MapPoint()
                {
                    X = PlayerGps.X + 1,
                    Y = PlayerGps.Y,
                    Z = PlayerGps.Z,
                    Tag = PlayerGps.Tag,
                    ColorHex = PlayerGps.ColorHex
                } , map : map);

            if ( PlayerGps.X >= 255 )
            {
                PlayerGps.X = 10;
            }
            
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
                Maps[key : _selectedZ].DrawPoint(
                    mapWidth : _map.Width,
                    mapHeight : _map.Height,
                    g : e.Graphics,
                    point : pointPlayer,
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
                
                Maps[key : _selectedZ].DrawPoint(
                    mapWidth : _map.Width,
                    mapHeight : _map.Height,
                    g : e.Graphics,
                    point : point,
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
            string[] lines = File.ReadAllLines(path : Config.PathGpsData);

            if (lines.Length < 1)
            {
                return;
            }


            GPS gps = null;

            try
            {
                gps = JsonConvert.DeserializeObject<GPS>(value : lines[0]);
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
                X = gps.Position[index : 0],
                Y = gps.Position[index : 1],
                Z = gps.Position[index : 2],
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
                    X = gpsSignal.Position[index : 0],
                    Y = gpsSignal.Position[index : 1],
                    Z = gpsSignal.Position[index : 2],
                    Tag = gpsSignal.Tag
                };
                
                SignalsGps.Add(item : signal);
            }
        }

        private void display_MouseDown(object sender, MouseEventArgs e)
        {
            (int x, int y) coords = CoordinateHelper.ConvertToMapCoordinates(
                imageX : e.X,
                imageY : e.Y,
                mapWidth : 255,
                mapHeight : 255,
                imageWidth : _panel.Width,
                imageHeight : _panel.Height );
            
            int z = Convert.ToInt32(value : _selectedZ.ToString());
            Form3 form = new Form3();
            MapPoint center = new MapPoint()
            {
                X = coords.x,
                Y = coords.y,
                Z = z
            };
            
            form.InitialCoords = center;

            MapPoint nullPoint = new MapPoint()
            {
                X = center.X - 5,
                Y = center.Y - 5,
                Z = center.Z
            };

            List<MapPoint> pointsMiniMap = Maps[key : _selectedZ].GetPoints()
                .Where(predicate : point => point.X >= nullPoint.X && point.X <= nullPoint.X + 10)
                .Where(predicate : point => point.Y >= nullPoint.Y && point.Y <= nullPoint.Y + 10)
                .ToList();
            
            List<MapPoint> deltaPoints = new List<MapPoint>();

            foreach (MapPoint mapPoint in pointsMiniMap)
            {
                MapPoint deltaPoint = new MapPoint()
                {
                    X = mapPoint.X - nullPoint.X + 1,
                    Y = mapPoint.Y - nullPoint.Y + 1,
                    Z = nullPoint.Z,
                    ColorHex = mapPoint.ColorHex
                };
                
                deltaPoints.Add(item : deltaPoint);
            }
            
            form.Map = new Map(points : deltaPoints, width : 11, height : 11);
            form.Form = this;
            form.Show();
        }

        private void display_MouseMove(object sender, MouseEventArgs e)
        {
            (int x, int y) coords = CoordinateHelper.ConvertToMapCoordinates(
                imageX : e.X,
                imageY : e.Y,
                mapWidth : 255,
                mapHeight : 255,
                imageWidth : _panel.Width,
                imageHeight : _panel.Height );
            
            int z = Convert.ToInt32(value : _selectedZ);
            string coordsText = String.Format(format : "{0} {1} {2}", arg0 : coords.x, arg1 : coords.y, arg2 : z);
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
