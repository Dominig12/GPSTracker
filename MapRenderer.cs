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
        private Bitmap _baseLayer;
        private Bitmap _dynamicLayer;
        private Graphics _baseGraphics;
        private Graphics _dynamicGraphics;
        private int Width { get; set; }
        private int Height { get; set; }

        public Map(List<MapPoint> points, int width = 255, int height = 255)
        {
            MapPoints = points;
            Width = width;
            Height = height;
        }
        
        private void InitializeLayers(int width, int height)
        {
            _baseLayer = new Bitmap(width : width, height : height);
            _dynamicLayer = new Bitmap(width : width, height : height);
            _baseGraphics = Graphics.FromImage(image : _baseLayer);
            _dynamicGraphics = Graphics.FromImage(image : _dynamicLayer);
        }
        
        public void InitStaticMap(int mapWidth, int mapHeight, int stepGrid = 15, float scale = 0.5f)
        {
            InitializeLayers(
                width : mapWidth,
                height : mapHeight );

            _baseGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            _baseGraphics.Clear(color : Color.Black);

            DrawGrid(
                g : _baseGraphics,
                mapW : Width,
                mapH : Height,
                imgW : mapWidth,
                imgH : mapHeight,
                step : stepGrid );
            
            foreach (MapPoint point in MapPoints)
            {
                using var brush = new SolidBrush(color : ColorTranslator.FromHtml(htmlColor : point.ColorHex));

                Rectangle rect = CoordinateHelper.CalculatePosition(
                    mapWidth : Width,
                    mapHeight : Height,
                    imageWidth : mapWidth,
                    imageHeight : mapHeight,
                    x : point.X,
                    y : point.Y,
                    sizeFactor : scale);
                        
                _baseGraphics.FillRectangle(brush : brush, rect : rect);

            }

        }
        
        private void DrawGrid(Graphics g, int mapW, int mapH, int imgW, int imgH, int step)
        {
            float scaleX = (float)imgW / mapW;
            float scaleY = (float)imgH / mapH;
        
            Pen gridPen = new Pen(color : Color.Lime, width : 1);
            Pen axisPen = new Pen(color : Color.Lime, width : 1);

            using Font coordFont = new Font(
                familyName : "Arial",
                emSize : 5 );
            using SolidBrush brush = new SolidBrush( color : Color.White );

            // Вертикальные линии и подписи X
            for (int x = 0; x <= mapW; x += step)
            {
                float xPos = x * scaleX;
                g.DrawLine(pen : x % (step*5) == 0 ? axisPen : gridPen, x1 : xPos, y1 : 0, x2 : xPos, y2 : imgH);

                if ( x <= 0 ) // Не рисуем подпись для нулевой линии
                {
                    continue;
                }

                string text = x.ToString();
                SizeF textSize = g.MeasureString(text : text, font : coordFont);
                g.DrawString(s : text, font : coordFont, brush : brush, x : xPos - textSize.Width, y : imgH - textSize.Height - 2);
            }

            // Горизонтальные линии и подписи Y
            for (int y = 0; y <= mapH; y += step)
            {
                float yPos = imgH - y * scaleY;
                g.DrawLine(pen : y % (step*5) == 0 ? axisPen : gridPen, x1 : 0, y1 : yPos, x2 : imgW, y2 : yPos);

                if ( y <= 0 ) // Не рисуем подпись для нулевой линии
                {
                    continue;
                }

                string text = y.ToString();
                SizeF textSize = g.MeasureString(text : text, font : coordFont);
                g.DrawString(s : text, font : coordFont, brush : brush, 
                    x : 2, y : yPos + textSize.Height);
            }
        }

        public void DrawPoint(
            int mapWidth, 
            int mapHeight,
            Graphics g, 
            MapPoint point,
            Color entityColor, 
            Brush textBrush)
        {
            
            Rectangle rest = CoordinateHelper.CalculatePosition(
                mapWidth : Width,
                mapHeight : Height,
                imageWidth : mapWidth,
                imageHeight : mapHeight,
                x : point.X,
                y : point.Y,
                sizeFactor : 0.2f
            );
            
            // Рисуем маркер
            using (var brush = new SolidBrush(color : entityColor))
            {
                g.FillRectangle(brush : brush, rect : rest);
            }

            Font font = new Font(
                familyName : "Arial",
                emSize : 7 );
            
            // Рисуем текст
            SizeF textSize = g.MeasureString(text : point.Tag, font : font);
            g.DrawString(
                s : point.Tag,
                font : font,
                brush : textBrush,
                x : rest.X - textSize.Width / 2,
                y : rest.Y - textSize.Height
            );
        }

        public Bitmap GetStaticMap()
        {
            return GetCurrentView();
        }

        public List<MapPoint> GetPoints()
        {
            return MapPoints;
        }
        
        public void UpdateDynamicLayer(int oldX, int oldY, MapPoint point)
        {
            // Очищаем предыдущую позицию
            Rectangle oldRect = CoordinateHelper.CalculatePosition(
                mapWidth : Width,
                mapHeight : Height,
                imageWidth : _dynamicLayer.Width,
                imageHeight : _dynamicLayer.Height,
                x : oldX,
                y : oldY );
            
            _dynamicGraphics.FillRectangle(brush : Brushes.Green, rect : oldRect);

            // Рисуем новую позицию
            var newRect = CoordinateHelper.CalculatePosition(
                mapWidth : Width,
                mapHeight : Height,
                imageWidth : _dynamicLayer.Width,
                imageHeight : _dynamicLayer.Height,
                x : point.X,
                y : point.Y );
            
            using var brush = new SolidBrush(color : ColorTranslator.FromHtml(htmlColor : point.ColorHex));
            
            _dynamicGraphics.FillRectangle(brush : brush, rect : newRect);
        }
        
        public Bitmap GetCurrentView()
        {
            var result = new Bitmap(width : _baseLayer.Width, height : _baseLayer.Height);
            using var g = Graphics.FromImage(image : result);

            g.DrawImage(image : _baseLayer, point : Point.Empty);
            g.DrawImage(image : _dynamicLayer, point : Point.Empty);

            return result;
        }
    }
    
    public class MapPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        
        public string ColorHex { get; set; }
        public string Tag { get; set; }

        public bool CheckEqualCoors(
            MapPoint mapPoint )
        {
            return mapPoint.X == X
                   && mapPoint.Y == Y
                   && mapPoint.Z == Z;
        }

        public MapPoint MovePoint(
            MapPoint point,
            Map map )
        {
            map.UpdateDynamicLayer(
                oldX : X,
                oldY : Y,
                point : point );

            return point;
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
            string zLevel = GetZLevel(mapName : mapName);
            if (string.IsNullOrEmpty(value : zLevel))
            {
                return points;
            }

            // Загружаем объекты и их цвета
            LoadVisionObjects();

            // Парсим теги и их цвета
            ParseMapTags(mapPath : mapName);

            // Читаем координаты
            ParseCoordinates(mapPath : mapName, zLevel : zLevel, points : points);

            return points;
        }

        private string GetZLevel(string mapName)
        {
            try
            {
                using (var reader = new StreamReader(path : "maps.txt"))
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
                MessageBox.Show(text : $"Ошибка чтения maps.txt: {ex.Message}");
            }
            return "";
        }

        private void LoadVisionObjects()
        {
            try
            {
                using (var reader = new StreamReader(path : "objects_vision.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('=');
                        var obj = new VisionObject
                        {
                            Name = parts[0].Trim(),
                            Color = parts.Length > 1 ? parts[1].Trim() : "#f6b26b",
                            Priority = parts.Length > 2 ? int.Parse(s : parts[2].Trim()) : 0
                        };
                        _visionObjects.Add(item : obj);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(text : $"Ошибка чтения objects_vision.txt: {ex.Message}");
            }
        }

        private void ParseMapTags(string mapPath)
        {
            try
            {
                using (var reader = new StreamReader(path : mapPath))
                {
                    string line;
                    string currentTag = null;
            
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains(value : " = ("))
                        {
                            currentTag = line.Split('=')[0].Trim().Trim('"');
                        }
                        else if (currentTag != null)
                        {
                            if (line.Contains(value : ")"))
                            {
                                currentTag = null;
                                continue;
                            }

                            foreach (var visionObj in _visionObjects)
                            {
                                if (line.Contains(value : visionObj.Name))
                                {
                                    if (!_tagsObjects.ContainsKey(key : currentTag) || 
                                        visionObj.Priority > (_tagsObjects[key : currentTag]?.Priority ?? 0))
                                    {
                                        _tagsObjects[key : currentTag] = visionObj;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(text : $"Ошибка чтения карты: {ex.Message}");
            }
        }

        private void ParseCoordinates(string mapPath, string zLevel, List<MapPoint> points)
        {
            try
            {
                using (var reader = new StreamReader(path : mapPath))
                {
                    string line;
                    int currentX = 0;
            
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(input : line, pattern : @"\(\d+,1,1\)"))
                        {
                            currentX = int.Parse(s : Regex.Match(input : line, pattern : @"\((\d+),").Groups[groupnum : 1].Value);
                            int currentY = 1;

                            for (int i = 0; i < 255; i++)
                            {
                                line = reader.ReadLine()?.Trim();
                                if (line == null)
                                {
                                    break;
                                }

                                if (_tagsObjects.TryGetValue(key : line, value : out var visionObj) && visionObj != null)
                                {
                                    points.Add(item : new MapPoint
                                    {
                                        X = currentX,
                                        Y = 257 - currentY,
                                        Z = int.Parse(s : zLevel),
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
                MessageBox.Show(text : $"Ошибка парсинга координат: {ex.Message}");
            }
        }
    }
    
    public class CoordinateHelper
    {
        public static Rectangle CalculatePosition(int mapWidth, int mapHeight,
            int imageWidth, int imageHeight,
            int x, int y,
            float sizeFactor = 1.0f)
        {
            // Проверка корректности координат и коэффициента
            if (x < 1 || x > mapWidth || y < 1 || y > mapHeight)
            {
                throw new ArgumentException(message : "Invalid coordinates");
            }

            if (sizeFactor <= 0 || sizeFactor > 1.0f)
            {
                throw new ArgumentOutOfRangeException(paramName : "sizeFactor must be in (0, 1] range");
            }

            // Рассчитываем масштаб
            float scaleX = (float)imageWidth / mapWidth;
            float scaleY = (float)imageHeight / mapHeight;

            // Границы оригинальной ячейки
            float origStartX = (x - 1) * scaleX;
            float origEndX = x * scaleX;
            float origStartY = imageHeight - y * scaleY;
            float origEndY = imageHeight - (y - 1) * scaleY;

            // Рассчитываем центр ячейки
            float centerX = (origStartX + origEndX) / 2;
            float centerY = (origStartY + origEndY) / 2;

            // Размер квадрата с учетом коэффициента
            float squareWidth = (origEndX - origStartX) * sizeFactor;
            float squareHeight = (origEndY - origStartY) * sizeFactor;

            // Новые границы квадрата
            float startX = centerX - squareWidth / 2;
            float endX = centerX + squareWidth / 2;
            float startY = centerY - squareHeight / 2;
            float endY = centerY + squareHeight / 2;

            return new Rectangle(
                x : (int)Math.Floor(d : startX),
                y : (int)Math.Floor(d : startY),
                width : (int)Math.Ceiling(a : endX - startX),
                height : (int)Math.Ceiling(a : endY - startY)
            );
        }
        
        public static (int x, int y) ConvertToMapCoordinates(
            int imageX, 
            int imageY, 
            int mapWidth, 
            int mapHeight, 
            int imageWidth, 
            int imageHeight)
        {
            // Проверка выхода за границы изображения
            if (imageX < 0 || imageX >= imageWidth || 
                imageY < 0 || imageY >= imageHeight)
            {
                return (-1, -1);
            }

            // Рассчитываем масштаб
            float scaleX = (float)imageWidth / mapWidth;
            float scaleY = (float)imageHeight / mapHeight;

            // Вычисляем координаты карты
            int x = (int)(imageX / scaleX) + 1;
            int y = mapHeight - (int)(imageY / scaleY);

            // Проверка корректности результата
            if (x < 1 || x > mapWidth || y < 1 || y > mapHeight)
            {
                return (-1, -1);
            }

            return (x, y);
        }
    }

}
