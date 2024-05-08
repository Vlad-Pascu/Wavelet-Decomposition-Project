using System.Diagnostics;
using System.Windows.Forms;

namespace Wavelet
{
    public partial class fWavelet : Form
    {
        double[,] original = new double[512, 512];
        public fWavelet()
        {
            InitializeComponent();
        }

        private void bLoadImage_Click(object sender, EventArgs e)
        {
            LoadImageOnPanel();
        }

        private void LoadImageOnPanel()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bitmap Image|*.bmp";
            if(ofd.ShowDialog() == DialogResult.OK )
            {
                string filePath= ofd.FileName;
                Bitmap bmp=new Bitmap(filePath);
                int width=bmp.Width;
                int height=bmp.Height;
                Bitmap panelImage = new Bitmap(width,height);
                Color color = new Color();
                for (int y=0; y<height; y++)
                {
                    for(int x=0; x<width; x++)
                    {
                        color = bmp.GetPixel(x, y);
                        panelImage.SetPixel(x, y, color);
                        original[x,y]=color.ToArgb();
                    }
                }
                pOrigImage.BackgroundImage = panelImage;
            }
            else
            {
                Debug.WriteLine("Could not load image");
            }
        }
    }
}
