using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GPSTracker
{

    public class Map
    {
        private List<MapPoint> MapPoints { get; set; }
        private Bitmap _staticMap;
        private float Width { get; set; } = 256;
        private float Height { get; set; } = 256;

        public Map(List<MapPoint> points, int width = 255, int height = 255)
        {
            MapPoints = points;
            Width = width;
            Height = height;
        }
        
        public void InitStaticMap(int mapWidth, int mapHeight, int stepGrid = 15, float scale = 0.5f)
        {
            _staticMap = new Bitmap(width : mapWidth, height : mapHeight);
        
            using ( Graphics g = Graphics.FromImage( image : _staticMap ) )
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(color : Color.Black);
                float coef = mapWidth / Width;
                for (float i = mapWidth; i >= 0; i -= coef * stepGrid)
                {
                    g.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : 0, y1 : i, x2 : mapWidth, y2 : i);
                    g.DrawString(s : (Width + 1 - (i / coef)).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : 0, y : i);
                    g.DrawLine(pen : new Pen(brush : new SolidBrush(color : Color.DarkGreen)), x1 : i, y1 : 0, x2 : i, y2 : mapWidth);
                    g.DrawString(s : (i / coef).ToString(), font : new Font(familyName : "Arial", emSize : 5), brush : new SolidBrush(color : Color.White), x : i, y : 0);
                }
                
                float coefX = mapWidth / Width;
                float coefY = mapHeight / Height;
            
                foreach (MapPoint point in MapPoints)
                {
                    using (var brush = new SolidBrush(color : ColorTranslator.FromHtml(htmlColor : point.ColorHex)))
                    {
                        float x = point.GetScaledX(scale: coefX);
                        float y = point.GetScaledY(scale: coefY, Height);
                        g.FillRectangle(brush : brush, x : x, y : y, width : coefX * scale, height : coefY * scale);
                    }
                }
            }
        }

        public Bitmap GetStaticMap()
        {
            return _staticMap;
        }

        public List<MapPoint> GetPoints()
        {
            return MapPoints;
        }
    }
    public class MapPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string ColorHex { get; set; }
        
        public string Tag { get; set; }

        public float GetScaledX(
            float scale )
        {
            return X * scale;
        }
        
        public float GetScaledY(
            float scale, float height = 255 )
        {
            return (height - Y + 1) * scale;
        }

    }
    
    public class VisionObject
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int Priority { get; set; }
    }

    public class MapRenderer
    {
        private List<VisionObject> _visionObjects = new List<VisionObject>();
        private Dictionary<string, VisionObject> _tagsObjects = new Dictionary<string, VisionObject>();

        public List<MapPoint> ParseMap(string mapName)
        {
            var points = new List<MapPoint>();
            
            // Получаем Z-уровень карты
            string zLevel = GetZLevel(mapName);
            if (string.IsNullOrEmpty(zLevel)) return points;

            // Загружаем объекты и их цвета
            LoadVisionObjects();

            // Парсим теги и их цвета
            ParseMapTags(mapName);

            // Читаем координаты
            ParseCoordinates(mapName, zLevel, points);

            return points;
        }

        private string GetZLevel(string mapName)
        {
            try
            {
                using (var reader = new StreamReader("maps.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2 && parts[0].Trim() == mapName)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения maps.txt: {ex.Message}");
            }
            return "";
        }

        private void LoadVisionObjects()
        {
            try
            {
                using (var reader = new StreamReader("objects_vision.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('=');
                        var obj = new VisionObject
                        {
                            Name = parts[0].Trim(),
                            Color = parts.Length > 1 ? parts[1].Trim() : "#f6b26b",
                            Priority = parts.Length > 2 ? int.Parse(parts[2].Trim()) : 0
                        };
                        _visionObjects.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения objects_vision.txt: {ex.Message}");
            }
        }

        private void ParseMapTags(string mapPath)
        {
            try
            {
                using (var reader = new StreamReader(mapPath))
                {
                    string line;
                    string currentTag = null;
            
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains(" = ("))
                        {
                            currentTag = line.Split('=')[0].Trim().Trim('"');
                        }
                        else if (currentTag != null)
                        {
                            if (line.Contains(")"))
                            {
                                currentTag = null;
                                continue;
                            }

                            foreach (var visionObj in _visionObjects)
                            {
                                if (line.Contains(visionObj.Name))
                                {
                                    if (!_tagsObjects.ContainsKey(currentTag) || 
                                        visionObj.Priority > (_tagsObjects[currentTag]?.Priority ?? 0))
                                    {
                                        _tagsObjects[currentTag] = visionObj;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения карты: {ex.Message}");
            }
        }

        private void ParseCoordinates(string mapPath, string zLevel, List<MapPoint> points)
        {
            try
            {
                using (var reader = new StreamReader(mapPath))
                {
                    string line;
                    int currentX = 0;
            
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, @"\(\d+,1,1\)"))
                        {
                            currentX = int.Parse(Regex.Match(line, @"\((\d+),").Groups[1].Value);
                            int currentY = 1;

                            for (int i = 0; i < 255; i++)
                            {
                                line = reader.ReadLine()?.Trim();
                                if (line == null) break;

                                if (_tagsObjects.TryGetValue(line, out var visionObj) && visionObj != null)
                                {
                                    points.Add(new MapPoint
                                    {
                                        X = currentX,
                                        Y = 257 - currentY,
                                        Z = int.Parse(zLevel),
                                        ColorHex = visionObj.Color
                                    });
                                }
                                currentY++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка парсинга координат: {ex.Message}");
            }
        }
    }

}
