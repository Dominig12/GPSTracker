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
}

public class MapRenderer
{
    private Dictionary<string, string> _visionObjects = new Dictionary<string, string>();
    private Dictionary<string, string> _tagsColors = new Dictionary<string, string>();

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
                    string key = parts[0].Trim();
                    string value = parts.Length > 1 ? parts[1].Trim() : "#f6b26b";
                    _visionObjects[key] = value;
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
                    else if (currentTag != null && line.Contains(")"))
                    {
                        currentTag = null;
                    }
                    else if (currentTag != null)
                    {
                        foreach (var obj in _visionObjects)
                        {
                            if (line.Contains(obj.Key))
                            {
                                _tagsColors[currentTag] = obj.Value;
                                break;
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
                int currentY;

                while ((line = reader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(line, @"\(\d+,1,1\)"))
                    {
                        currentX = int.Parse(Regex.Match(line, @"\((\d+),").Groups[1].Value);
                        currentY = 1;

                        for (int i = 0; i < 255; i++)
                        {
                            line = reader.ReadLine();
                            if (line == null) break;

                            string trimmedLine = line.Trim();
                            if (_tagsColors.TryGetValue(trimmedLine, out string color))
                            {
                                points.Add(new MapPoint
                                {
                                    X = currentX,
                                    Y = 257 - currentY,
                                    Z = int.Parse(zLevel),
                                    ColorHex = color
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
