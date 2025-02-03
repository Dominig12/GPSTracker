using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GPSTracker
{

    public class MapPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string ColorHex { get; set; }

        public int GetScaledX(
            int scale )
        {
            return X * scale;
        }
        
        public int GetScaledY(
            int scale )
        {
            return Y * scale;
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
