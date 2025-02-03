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
                Map.InitStaticMap(_panel.Width, _panel.Height, 1, 1);
                _map.Image = Map.GetStaticMap();
            };
        }
        
        private void CreateComponents()
        {
            _map = new PictureBox
            {
                Size = new Size(330, 330),
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

            _panel.MouseMove += panel1_MouseMove;
            _panel.MouseDown += panel1_MouseDown;
    
            // Добавление на форму
            Controls.Add(_map);
            Controls.Add(_panel);
            
            _panel.BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _panel.Invalidate();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            float coef = _panel.Width / 11;
            int x = InitialCoords.X - 5 + Convert.ToInt32(Math.Round((e.X - coef / 2) / coef, MidpointRounding.ToEven));
            int y = InitialCoords.Y + 5 - Convert.ToInt32(Math.Round((e.Y - coef / 2) / coef, MidpointRounding.ToEven));
            int z = Convert.ToInt32(InitialCoords.Z);
            string coords = String.Format("{0} {1} {2}", x, y, z);
            StreamWriter writer = new StreamWriter(Config.PathOpenWormhole, false);
            writer.WriteLine(coords);
            writer.Close();
            this.Close();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            float coef = _panel.Width / 11;
            int x = InitialCoords.X - 5 + Convert.ToInt32(Math.Round((e.X - coef / 2) / coef, MidpointRounding.ToEven));
            int y = InitialCoords.Y + 5 - Convert.ToInt32(Math.Round((e.Y - coef / 2) / coef, MidpointRounding.ToEven));
            int z = Convert.ToInt32(InitialCoords.Z);
            string coords_text = String.Format("{0} {1} {2}", x, y, z);
            toolTip1.SetToolTip(_panel, coords_text);
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
