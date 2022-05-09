using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tesseract;

namespace GPSTracker
{
    public partial class Form1 : Form
    {
        SortedList<int, List<KeyValuePair<int, int>>> z_layers_player = new SortedList<int, List<KeyValuePair<int, int>>>();
        SortedList<int, List<KeyValuePair<int, int>>> z_layers_gps = new SortedList<int, List<KeyValuePair<int, int>>>();

        Form2 screnner;

        bool active = false;

        public Form1()
        {
            InitializeComponent();
            screnner = new Form2();
            GetCoordsInFile();
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

        private void AddLayer(int index, SortedList<int, List<KeyValuePair<int, int>>> z_layers)
        {
            z_layers.Add(index, new List<KeyValuePair<int, int>>());
            z_layer_combo_box.Items.Add(index);
        }

        private void UpdateValues(int x, int y, int z, SortedList<int, List<KeyValuePair<int, int>>> z_layers)
        {
            if (!z_layers.ContainsKey(z))
                AddLayer(z, z_layers);

            z_layers[z].Add(new KeyValuePair<int, int>(x, y));
        }

        private void UpdateGPSValues()
        {
            List<int[]> coords_array_gps = GetGpsData();
            foreach (int[] coords_int in coords_array_gps)
            {
                UpdateValues(coords_int[0], coords_int[1], coords_int[2], z_layers_gps);
            }
        }

        private void display_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = display.CreateGraphics();
            g.Clear(Color.Black);
            GridDraw(g);
            PaintPoints(g, z_layers_player, new SolidBrush(Color.White));
            PaintPoints(g, z_layers_gps, new SolidBrush(Color.Red), 1.5, false);
        }

        private void PaintPoints(Graphics g, SortedList<int, List<KeyValuePair<int, int>>> z_layers, Brush color, double coef_scale = 1, bool connect = true)
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

            foreach (KeyValuePair<int, int> coords in z_layers[z])
            {
                int[] point = new int[2]
                {
                    (coords.Key * coef_X),
                    (height - (coords.Value * coef_y))
                };

                g.FillRectangle(color, point[0], point[1], float.Parse((coef_X * coef_scale).ToString()), float.Parse((coef_y * coef_scale).ToString()));
                if (last[0] != -1 && last[1] != -1 && connect)
                    g.DrawLine(new Pen(color), last[0], last[1], point[0], point[1]);

                last = point;
            }
        }

        private void GridDraw(Graphics g)
        {
            for (int i = 0; i < display.Width; i += (display.Width / 255) * 15)
            {
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGreen)), 0, i, display.Width, i);
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGreen)), i, 0, i, display.Width);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            screnner.Show();
            UpdateGPSValues();
            if (!active)
                return;
            int[] coords = GetPlayerGPSData();
            if (coords.Length < 3 || coords[0] == -1)
                return;
            UpdateValues(coords[0], coords[1], coords[2], z_layers_player);
            z_layer_combo_box.SelectedItem = coords[2];
            Refresh();
        }

        private List<int[]> GetGpsData()
        {
            List<string> coords_gps_list = ParseCoordsList(textBox1.Text);
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
            Bitmap image = screnner.ScreenShot();
            TesseractEngine tes = new TesseractEngine(@"./tesseractdata", "eng");
            var res = tes.Process(image);
            string coords = ParseCoords(res.GetText());

            string[] coords_array_string = coords.Split(' ');

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
            z_layer_combo_box.Items.Clear();
            Refresh();
        }

        private void z_layer_combo_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UpdateGPSValues();
            z_layer_combo_box.SelectedItem = z_layer_combo_box.Items[0];
            Refresh();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
