using System.Drawing;
using System.Windows.Forms;

namespace GPSTracker
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            groupBox1.Visible = false;
            this.TopMost = true;
            TransparencyKey = BackColor;
        }

        public Bitmap ScreenShot()
        {
            Bitmap screen = new Bitmap(groupBox1.Width, groupBox1.Height);
            using (Graphics gr = Graphics.FromImage(screen))
            {
                gr.CopyFromScreen(this.PointToScreen(Point.Empty), Point.Empty, this.Size);
            }
            return screen;
        }

        private void groupBox1_Enter(object sender, System.EventArgs e)
        {

        }
    }
}
