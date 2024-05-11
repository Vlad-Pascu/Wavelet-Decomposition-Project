using System.Diagnostics;
using System.Windows.Forms;

namespace Wavelet
{
    public partial class fWavelet : Form
    {
        byte[,] original = new byte[512, 512];
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
        int limitX, limitY;
        public fWavelet()
        {
            InitializeComponent();
        }

        private void bLoadImage_Click(object sender, EventArgs e)
        {
            LoadImageOnPanel();
            scale=Convert.ToDouble(tbScale.Text);
            offset=Convert.ToInt32(tbOffset.Text);
            limitX=Convert.ToInt32(tbX.Text);
            limitY=Convert.ToInt32(tbY.Text);
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
                        original[x, y] = (byte)((color.R + color.G + color.B) / 3);
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
                    int colorPart;
                    if(x<limitX && y < limitY)
                        colorPart = (int)(wavelet[x, y]);
                    else
                        colorPart = (int)(wavelet[x, y] * scale + offset);
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
                scale = 1;
            else
                scale = Convert.ToDouble(tbScale.Text);
        }

        private void tbOffset_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbOffset.Text))
                offset = 128;
            else
                offset = Convert.ToInt32(tbOffset.Text);
        }

        private double getValue(double[] vector, int index)
        {
            if (index < 0)
                return vector[-index];
            else if (index >= vector.Length)
                return vector[vector.Length - (index - vector.Length) - 2];
            else
                return vector[index];
        }

        private void AnalysisH(int line, int length)
        {
            double[] originalVector = new double[length];
            for (int c = 0; c < length; c++)
                originalVector[c] = wavelet[line, c];
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = getValue(originalVector, i - 4) * analysisH[0] +
                    getValue(originalVector, i - 3) * analysisH[1] +
                    getValue(originalVector, i - 2) * analysisH[2] +
                    getValue(originalVector, i - 1) * analysisH[3] +
                    getValue(originalVector, i) * analysisH[4] +
                    getValue(originalVector, i + 1) * analysisH[5] +
                    getValue(originalVector, i + 2) * analysisH[6] +
                    getValue(originalVector, i + 3) * analysisH[7] +
                    getValue(originalVector, i + 4) * analysisH[8];
                low[i] = getValue(originalVector, i - 4) * analysisL[0] +
                    getValue(originalVector, i - 3) * analysisL[1] +
                    getValue(originalVector, i - 2) * analysisL[2] +
                    getValue(originalVector, i - 1) * analysisL[3] +
                    getValue(originalVector, i) * analysisL[4] +
                    getValue(originalVector, i + 1) * analysisL[5] +
                    getValue(originalVector, i + 2) * analysisL[6] +
                    getValue(originalVector, i + 3) * analysisL[7] +
                    getValue(originalVector, i + 4) * analysisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            double[] downSampleL = new double[length];
            double[] downSampleH = new double[length];
            for (int i = 0; i < length; i += 2)
            {
                downSampleL[i] = low[i];
                downSampleH[i + 1] = high[i + 1];
                //Debug.WriteLine(downSampleL[i+1] + " " + downSampleH[i+1]);
            }
            for (int i = 0; i < length; i += 2)
                newVector[i / 2] = downSampleL[i];
            for (int i = 1; i < length; i += 2)
                newVector[(length + i) / 2] = downSampleH[i];
            for (int c = 0; c < length; c++)
            {
                wavelet[line, c] = newVector[c];
                //Debug.WriteLine(newVector[i]);
            }
        }

        private void AnalysisV(int column, int length)
        {
            double[] originalVector = new double[length];
            for (int l = 0; l < length; l++)
                originalVector[l] = wavelet[l, column];
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = getValue(originalVector, i - 4) * analysisH[0] +
                    getValue(originalVector, i - 3) * analysisH[1] +
                    getValue(originalVector, i - 2) * analysisH[2] +
                    getValue(originalVector, i - 1) * analysisH[3] +
                    getValue(originalVector, i) * analysisH[4] +
                    getValue(originalVector, i + 1) * analysisH[5] +
                    getValue(originalVector, i + 2) * analysisH[6] +
                    getValue(originalVector, i + 3) * analysisH[7] +
                    getValue(originalVector, i + 4) * analysisH[8];
                low[i] = getValue(originalVector, i - 4) * analysisL[0] +
                    getValue(originalVector, i - 3) * analysisL[1] +
                    getValue(originalVector, i - 2) * analysisL[2] +
                    getValue(originalVector, i - 1) * analysisL[3] +
                    getValue(originalVector, i) * analysisL[4] +
                    getValue(originalVector, i + 1) * analysisL[5] +
                    getValue(originalVector, i + 2) * analysisL[6] +
                    getValue(originalVector, i + 3) * analysisL[7] +
                    getValue(originalVector, i + 4) * analysisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            double[] downSampleL = new double[length];
            double[] downSampleH = new double[length];
            for (int i = 0; i < length; i += 2)
            {
                downSampleL[i] = low[i];
                downSampleH[i + 1] = high[i + 1];
                //Debug.WriteLine(downSampleL[i+1] + " " + downSampleH[i+1]);
            }
            for (int i = 0; i < length; i += 2)
                newVector[i / 2] = downSampleL[i];
            for (int i = 1; i < length; i += 2)
                newVector[(length + i) / 2] = downSampleH[i];
            for (int l = 0; l < length; l++)
            {
                wavelet[l, column] = newVector[l];
                //Debug.WriteLine(newVector[i]);
            }

        }

        private void bAnH1_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 512; l++)
                AnalysisH(l, 512);
            lStatus.Text = "Stage 1 Horizontal Complete";
            tbY.Text = "256";
        }

        private void bAnV1_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 512; c++)
                AnalysisV(c, 512);
            lStatus.Text = "Stage 1 Vertical Complete";
            tbX.Text = "256";
        }

        private void bAnH2_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 256; l++)
                AnalysisH(l, 256);
            lStatus.Text = "Stage 2 Horizontal Complete";
            tbY.Text = "128";
        }

        private void bAnV2_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 256; c++)
                AnalysisV(c, 256);
            lStatus.Text = "Stage 2 Vertical Complete";
            tbX.Text = "128";
        }

        private void bAnH3_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 128; l++)
                AnalysisH(l, 128);
            lStatus.Text = "Stage 3 Horizontal Complete";
            tbY.Text = "64";
        }

        private void bAnV3_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 128; c++)
                AnalysisV(c, 128);
            lStatus.Text = "Stage 3 Vertical Complete";
            tbX.Text = "64";
        }

        private void bAnH4_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 64; l++)
                AnalysisH(l, 64);
            lStatus.Text = "Stage 4 Horizontal Complete";
            tbY.Text = "32";
        }

        private void bAnV4_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 64; c++)
                AnalysisV(c, 64);
            lStatus.Text = "Stage 4 Vertical Complete";
            tbX.Text = "32";
        }

        private void bAnH5_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 32; l++)
                AnalysisH(l, 32);
            lStatus.Text = "Stage 5 Horizontal Complete";
            tbY.Text = "16";
        }

        private void bAnV5_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 32; c++)
                AnalysisV(c, 32);
            lStatus.Text = "Stage 5 Vertical Complete";
            tbX.Text = "16";
        }

        private void tbX_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbX.Text))
                limitX = 512;
            else
                limitX = Convert.ToInt32(tbX.Text);
        }

        private void tbY_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbY.Text))
                limitY = 512;
            else
                limitY = Convert.ToInt32(tbY.Text);
        }
    }
}
