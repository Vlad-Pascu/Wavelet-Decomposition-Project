using System.Diagnostics;
using System.Windows.Forms;

namespace Wavelet
{
    public partial class fWavelet : Form
    {
        double[,] original = new double[512, 512];
        double[,] wavelet = new double[512, 512];
        readonly double[] analysisL = new double[]
        {
            0.026748757411,
           -0.016864118443,
           -0.078223266529,
            0.266864118443,
            0.602949018236,
            0.266864118443,
           -0.078223266529,
           -0.016864118443,
            0.026748757411
        };
        readonly double[] analysisH = new double[]
        {
            0.000000000000,
            0.091271763114,
           -0.057543526229,
           -0.591271763114,
            1.115087052457,
           -0.591271763114,
           -0.057543526229,
            0.091271763114,
            0.000000000000
        };
        readonly double[] synthesisL = new double[]
        {
            0.000000000000,
           -0.091271763114,
           -0.057543526229,
            0.591271763114,
            1.115087052457,
            0.591271763114,
           -0.057543526229,
           -0.091271763114,
            0.000000000000,
        };
        readonly double[] synthesisH = new double[]
        {
            0.026748757411,
            0.016864118443,
           -0.078223266529,
           -0.266864118443,
            0.602949018236,
           -0.266864118443,
           -0.078223266529,
            0.016864118443,
            0.026748757411
        };

        double scale;
        int offset;
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
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                Bitmap bmp = new Bitmap(filePath);
                int width = bmp.Width;
                int height = bmp.Height;
                Bitmap panelImage = new Bitmap(width, height);
                Color color = new Color();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        color = bmp.GetPixel(x, y);
                        panelImage.SetPixel(x, y, color);
                        original[x, y] = (double)(color.R + color.G + color.B) / 3;
                        wavelet[x, y] = original[x, y];
                    }
                }
                pOrigImage.BackgroundImage = panelImage;
            }
            else
            {
                Debug.WriteLine("Could not load image");
            }
        }

        private void bRefresh_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(512, 512);
            Color color = new Color();
            //scale and offset part
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int colorPart = (int)(wavelet[x, y] * scale);
                    if (colorPart > 255)
                        colorPart = 255;
                    if (colorPart < 0)
                        colorPart = 0;
                    color = Color.FromArgb(colorPart, colorPart, colorPart);
                    bmp.SetPixel(x, y, color);
                }
            }
            pWaveImage.BackgroundImage = bmp;
            Debug.WriteLine("Done");
        }

        private void tbScale_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbScale.Text))
                scale = 0;
            else
                scale = Convert.ToDouble(tbScale.Text);
        }

        private void tbOffset_TextChanged(object sender, EventArgs e)
        {
            offset = Convert.ToInt32(tbOffset.Text);
        }
    }
}
