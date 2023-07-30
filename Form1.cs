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
        public SortedList<int, SortedList<string, string>> z_layers_player = new SortedList<int, SortedList<string, string>>();
        public SortedList<int, SortedList<string, string>> z_layers_gps = new SortedList<int, SortedList<string, string>>();
        public SortedList<int, SortedList<string, string>> z_layers_map = new SortedList<int, SortedList<string, string>>();
        public SortedList<int, GraphicsState> z_map = new SortedList<int, GraphicsState>();
        Graphics g = null;

        bool active = false;

        public Form1()
        {
            InitializeComponent();
            g = display.CreateGraphics();
            UpdateMapValues();

            GetCoordsInFile();
            UpdateGPSValues();
            AddLayer(3, z_layers_gps);
            AddLayer(4, z_layers_gps);
            AddLayer(5, z_layers_gps);
            AddLayer(6, z_layers_gps);
            z_layer_combo_box.SelectedItem = z_layer_combo_box.Items[0];
            toolTip1.ShowAlways = true;
            //Refresh();
        }

        private void GetCoordsInFile()
        {
            List<string> coordsList = new List<string>();
            StreamReader reader = new StreamReader("coordinates.txt", System.Text.Encoding.GetEncoding(1251));
            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                textBox1.Text += line + Environment.NewLine;
            }
            reader.Dispose();
            reader.Close();
        }

        private void AddLayer(int index, SortedList<int, SortedList<string, string>> z_layers)
        {
            if(!z_layers.ContainsKey(index))
                z_layers.Add(index, new SortedList<string, string>());
            if (!z_layer_combo_box.Items.Contains(index))
            {
                z_layer_combo_box.Items.Add(index);
                z_layer_combo_box.SelectedItem = index;
            }
        }

        private void UpdateValues(int x, int y, int z, string color, SortedList<int, SortedList<string, string>> z_layers)
        {
            if (!z_layers.ContainsKey(z))
                AddLayer(z, z_layers);

            string key = String.Format("{0}:{1}", x, y);

            if (!z_layers[z].ContainsKey(key))
                z_layers[z].Add(key, color);

            z_layers[z][key] = color;
        }

        private void UpdateGPSValues()
        {
            List<int[]> coords_array_gps = GetGpsData();
            foreach (int[] coords_int in coords_array_gps)
            {
                UpdateValues(coords_int[0], coords_int[1], coords_int[2], "#FF0000", z_layers_gps);
            }
        }

        private void UpdateMapValues()
        {
            List<string> mapsList = new List<string>();
            StreamReader reader = new StreamReader("maps.txt", System.Text.Encoding.GetEncoding(1251));
            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                string map = line.Split('=')[0];
                mapsList.Add(map);
            }
            reader.Dispose();
            reader.Close();

            foreach (string maps in mapsList)
            {
                List<KeyValuePair<int[], string>> coords_array_map = GetMapData(maps);
                foreach (KeyValuePair<int[], string> coords_int in coords_array_map)
                {
                    UpdateValues(coords_int.Key[0], coords_int.Key[1], coords_int.Key[2], coords_int.Value, z_layers_map);
                }
            }
        }

        private void display_Paint()
        {
            /*PaintPoints(fone, z_layers_map, new SolidBrush(Color.DarkKhaki), 1, false);
            GridDraw(fone);*/
            int z = Convert.ToInt32(z_layer_combo_box.SelectedItem.ToString());
            if (!z_map.ContainsKey(z) || z_map[z] == null)
            {
                g.Clear(Color.Black);
                PaintPoints(g, z_layers_map, 0.5, false);
                GridDraw(g);
                if (!z_map.ContainsKey(z))
                    z_map.Add(z, g.Save());
                else
                    z_map[z] = g.Save();
            }
            else
                g.Restore(z_map[z]);
            PaintPoints(g, z_layers_player, 0.5);
            PaintPoints(g, z_layers_gps, 0.5, false);
            //Refresh();
        }

        public void PaintPoints(Graphics g, SortedList<int, SortedList<string, string>> z_layers, double coef_scale = 1, bool connect = true)
        {
            if (!Convert.ToBoolean(z_layer_combo_box.SelectedItem))
                return;

            int z = Convert.ToInt32(z_layer_combo_box.SelectedItem.ToString());

            if (!z_layers.ContainsKey(z))
                return;

            int width = display.Width;
            int height = display.Height;
            int coef_X = width / 255;
            int coef_y = height / 255;

            int[] last = new int[2]
            {
                -1,
                -1,
            };

            foreach (string coords in z_layers[z].Keys)
            {
                int[] coords_int = Array.ConvertAll(coords.Split(':'), int.Parse);

                int[] point = new int[2]
                {
                    (coords_int[0] * coef_X),
                    (height - (coords_int[1] * coef_y))
                };

                Color clr = ColorTranslator.FromHtml(z_layers[z][coords]);

                Brush color = new SolidBrush(clr);

                g.FillRectangle(color, point[0], point[1], float.Parse((coef_X * coef_scale).ToString()), float.Parse((coef_y * coef_scale).ToString()));
                if (last[0] != -1 && last[1] != -1 && connect)
                    g.DrawLine(new Pen(color), last[0], last[1], point[0], point[1]);

                last = point;
            }
        }

        public void GridDraw(Graphics g)
        {
            float coef = display.Width / 255;
            for (float i = display.Width; i >= 0; i -= coef * 15)
            {
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGreen)), 0, i, display.Width, i);
                g.DrawString((256 - (i / coef)).ToString(), new Font("Arial", 5), new SolidBrush(Color.White), 0, i);
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGreen)), i, 0, i, display.Width);
                g.DrawString((i / coef).ToString(), new Font("Arial", 5), new SolidBrush(Color.White), i, 0);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateGPSValues();
            if (!active)
                return;
            int[] coords = GetPlayerGPSData();
            if (coords.Length < 3 || coords[0] == -1)
                return;
            UpdateValues(coords[0], coords[1], coords[2], "#ffffff", z_layers_player);
            z_layer_combo_box.SelectedItem = coords[2];
            //z_map[Convert.ToInt32(z_layer_combo_box.SelectedItem)] = null;
            display_Paint();
        }

        private List<int[]> GetGpsData()
        {
            List<string> coords_gps_list = ParseCoordsList(textBox1.Text);
            string GPSData = File.ReadAllText("GPSData.txt");
            PositionGps positionGps = JsonConvert.DeserializeObject<PositionGps>(GPSData);
            foreach (string gpsOther in positionGps.OtherGps)
            {
                string gpsOtherPrepare = gpsOther.Replace("(", "");
                gpsOtherPrepare = gpsOtherPrepare.Replace(")", "");
                gpsOtherPrepare = gpsOtherPrepare.Replace(",", "");
                coords_gps_list.Add(gpsOtherPrepare);
            }
            List<int[]> coords_array = new List<int[]>();

            foreach (string coords_string in coords_gps_list)
            {
                string[] coords_array_string = coords_string.Split(' ');

                if (coords_array_string.Length < 3)
                    continue;

                if (coords_array_string[2] == "unknown")
                    continue;

                int[] coords_array_item = new int[coords_array_string.Length];
                for (int x = 0; x < coords_array_string.Length; x++)
                {
                    coords_array_item[x] = Convert.ToInt32(coords_array_string[x]);
                }

                coords_array.Add(coords_array_item);
            }

            return coords_array;
        }

        private int[] GetPlayerGPSData()
        {
            string GPSData = File.ReadAllText("GPSData.txt");
            PositionGps positionGps = JsonConvert.DeserializeObject<PositionGps>(GPSData);
            string coords = positionGps.Position;
            string gpsOtherPrepare = coords.Replace("(", "");
            gpsOtherPrepare = gpsOtherPrepare.Replace(")", "");
            gpsOtherPrepare = gpsOtherPrepare.Replace(",", "");

            string[] coords_array_string = gpsOtherPrepare.Split(' ');

            if (coords_array_string.Length < 3)
                return new int[3]
                {
                    -1,
                    -1,
                    -1,
                };

            int[] coords_array = new int[coords_array_string.Length];
            for (int x = 0; x < coords_array_string.Length; x++)
            {
                coords_array[x] = Convert.ToInt32(coords_array_string[x]);
            }

            return coords_array;
        }

        private List<KeyValuePair<int[], string>> GetMapData(string map)
        {
            List<KeyValuePair<int[], string>> coords_map = new List<KeyValuePair<int[], string>>();
            StreamReader reader_maps = new StreamReader("maps.txt");
            string z_level = "";
            do
            {
                if (z_level != "")
                    break;
                string map_obj = reader_maps.ReadLine();
                if(map_obj.Split('=')[0] == map)
                    z_level = map_obj.Split('=')[1];
            } while (!reader_maps.EndOfStream);
            reader_maps.Close();

            if (z_level == "")
                return coords_map;

            StreamReader reader = new StreamReader(map);
            List<KeyValuePair<string, string>> tags_vision = new List<KeyValuePair<string, string>>();
            string line = "";

            StreamReader reader_objects = new StreamReader("objects_vision.txt");
            List<KeyValuePair<string, string>> vision_obj = new List<KeyValuePair<string, string>>();
            while ((line = reader_objects.ReadLine()) != null)
            {
                string[] line_obj = line.Split('=');
                if(line_obj.Length <= 1)
                    vision_obj.Add(new KeyValuePair<string, string>(line_obj[0], "#f6b26b"));
                else
                    vision_obj.Add(new KeyValuePair<string, string>(line_obj[0], line_obj[1]));
            }
            reader_objects.Close();

            line = "";

            string find_tag = String.Format(@" \= \(");
            Regex regex_tag = new Regex(find_tag, RegexOptions.Compiled);

            while ((line = reader.ReadLine()) != null)
            {
                if (!regex_tag.IsMatch(line))
                {
                    continue;
                }

                string tag = line.Split(' ')[0];
                tag = tag.Trim(new char[] { '"' });

                bool success = false;
                do
                {
                    if ((line = reader.ReadLine()) == null)
                        break;
                    if (success)
                        continue;

                    foreach (KeyValuePair<string, string> obj in vision_obj)
                    {
                        if (line.Contains(obj.Key))
                        {
                            tags_vision.Add(new KeyValuePair<string, string>(tag, obj.Value));
                            success = true;
                            break;
                        }
                    }

                } while (!line.Contains(")"));
            }
            reader.Close();

            reader = new StreamReader(map);

            for (int i = 1; i <= 255; i++)
            {
                string find = String.Format(@"\({0}\,1\,1\)", i);
                Regex regex = new Regex(find, RegexOptions.Compiled);
                line = "";
                if ((line = reader.ReadLine()) == null)
                {
                    return coords_map;
                }
                while (!regex.IsMatch(line))
                {
                    line = reader.ReadLine();
                }

                for (int o = 1; o <= 255; o++)
                {
                    foreach(KeyValuePair<string, string> tag in tags_vision)
                    {
                        if (line == tag.Key)
                        {
                            coords_map.Add(new KeyValuePair<int[], string>(new int[3] { i, 257 - o, Convert.ToInt32(z_level) }, tag.Value));
                        }
                    }
                    line = reader.ReadLine();
                }
            }

            return coords_map;
        }

        private List<string> ParseCoordsList(string line)
        {
            List<string> coords_list = new List<string>();
            if (line.Length < 1)
                return coords_list;

            Regex regex = new Regex(@"[(]", RegexOptions.Compiled);
            while (regex.IsMatch(line))
            {
                line = line.Substring(regex.Match(line).Index + 1);
                Regex regex_val = new Regex(@"[)]", RegexOptions.Compiled);
                string result = line.Substring(0, regex_val.Match(line).Index);
                regex_val = new Regex(@"\D+", RegexOptions.Compiled);
                result = regex_val.Replace(result, " ");
                if (result.Split(' ').Length < 3 && result.Split(' ').Length > 1)
                    result += " unknown";
                coords_list.Add(result);
            }
            return coords_list;
        }

        private string ParseCoords(string line)
        {
            Regex regex = new Regex(@"[(]", RegexOptions.Compiled);
            Regex regex_val = new Regex(@"[)]", RegexOptions.Compiled);
            if (!regex.IsMatch(line) && !regex_val.IsMatch(line))
                return "";

            regex = new Regex(@"\D+", RegexOptions.Compiled);
            string result = regex.Replace(line, " ");
            regex = new Regex(@"^\s", RegexOptions.Compiled);
            result = regex.Replace(result, "");
            regex = new Regex(@"\s$", RegexOptions.Compiled);
            result = regex.Replace(result, "");
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            active = !active;
            if (active)
                label3.Text = "ON";
            else
                label3.Text = "OFF";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            z_layers_gps.Clear();
            z_layers_player.Clear();
            z_map.Clear();
            textBox1.Text = "";
            GetCoordsInFile();
            UpdateGPSValues();
            display_Paint();
        }

        private void z_layer_combo_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            z_map[Convert.ToInt32(z_layer_combo_box.SelectedItem)] = null;
            display_Paint();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < 1)
                GetCoordsInFile();
            UpdateGPSValues();
            z_layer_combo_box.SelectedItem = z_layer_combo_box.Items[0];
            display_Paint();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void display_MouseDown(object sender, MouseEventArgs e)
        {
            float coef = display.Width / 255;
            int x = Convert.ToInt32((e.X - coef / 2) / coef);
            int y = Convert.ToInt32(255 - (e.Y - coef / 2) / coef);
            int z = Convert.ToInt32(z_layer_combo_box.SelectedItem.ToString());
            Form3 form = new Form3();
            form.form = this;
            form.z_layers_gps = z_layers_gps;
            form.z_layers_map = z_layers_map;
            form.z_layers_player = z_layers_player;
            form.initial_coords = new KeyValuePair<int, KeyValuePair<int, int>>(z, new KeyValuePair<int, int>(x, y));
            form.Show();
        }

        private void display_MouseMove(object sender, MouseEventArgs e)
        {
            float coef = display.Width / 255;
            int x = Convert.ToInt32((e.X - coef / 2) / coef);
            int y = Convert.ToInt32(255 - (e.Y - coef / 2) / coef);
            int z = Convert.ToInt32(z_layer_combo_box.SelectedItem.ToString());
            string coords_text = String.Format("{0} {1} {2}", x, y, z);
            toolTip1.SetToolTip(display, coords_text);
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
