using System.Data.Common;
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
            scale = Convert.ToDouble(tbScale.Text);
            offset = Convert.ToInt32(tbOffset.Text);
            limitX = Convert.ToInt32(tbX.Text);
            limitY = Convert.ToInt32(tbY.Text);
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
                    if (x < limitX && y < limitY)
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

        private void SynthesisH(int line, int length)
        {
            double[] originalVector = new double[length];
            for (int c = 0; c < length; c++)
                originalVector[c] = wavelet[line, c];
            double[] upSampleL = new double[length];
            double[] upSampleH = new double[length];
            for (int i = 0; i < length; i += 2)
                upSampleL[i] = originalVector[i / 2];
            for (int i = 1; i < length; i += 2)
                upSampleH[i] = originalVector[(length + i) / 2];
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = getValue(upSampleH, i - 4) * synthesisH[0] +
                    getValue(upSampleH, i - 3) * synthesisH[1] +
                    getValue(upSampleH, i - 2) * synthesisH[2] +
                    getValue(upSampleH, i - 1) * synthesisH[3] +
                    getValue(upSampleH, i) * synthesisH[4] +
                    getValue(upSampleH, i + 1) * synthesisH[5] +
                    getValue(upSampleH, i + 2) * synthesisH[6] +
                    getValue(upSampleH, i + 3) * synthesisH[7] +
                    getValue(upSampleH, i + 4) * synthesisH[8];
                low[i] = getValue(upSampleL, i - 4) * synthesisL[0] +
                    getValue(upSampleL, i - 3) * synthesisL[1] +
                    getValue(upSampleL, i - 2) * synthesisL[2] +
                    getValue(upSampleL, i - 1) * synthesisL[3] +
                    getValue(upSampleL, i) * synthesisL[4] +
                    getValue(upSampleL, i + 1) * synthesisL[5] +
                    getValue(upSampleL, i + 2) * synthesisL[6] +
                    getValue(upSampleL, i + 3) * synthesisL[7] +
                    getValue(upSampleL, i + 4) * synthesisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            for (int i = 0; i < length; i++)
                newVector[i] = low[i] + high[i];
            for (int c = 0; c < length; c++)
            {
                wavelet[line, c] = Math.Round(newVector[c]);
                //Debug.WriteLine(wavelet[line,c]);
            }
        }

        private void SynthesisV(int column, int length)
        {
            double[] originalVector = new double[length];
            for (int l = 0; l < length; l++)
                originalVector[l] = wavelet[l, column];
            double[] upSampleL = new double[length];
            double[] upSampleH = new double[length];
            for (int i = 0; i < length; i += 2)
                upSampleL[i] = originalVector[i / 2];
            for (int i = 1; i < length; i += 2)
                upSampleH[i] = originalVector[(length + i) / 2];
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = getValue(upSampleH, i - 4) * synthesisH[0] +
                    getValue(upSampleH, i - 3) * synthesisH[1] +
                    getValue(upSampleH, i - 2) * synthesisH[2] +
                    getValue(upSampleH, i - 1) * synthesisH[3] +
                    getValue(upSampleH, i) * synthesisH[4] +
                    getValue(upSampleH, i + 1) * synthesisH[5] +
                    getValue(upSampleH, i + 2) * synthesisH[6] +
                    getValue(upSampleH, i + 3) * synthesisH[7] +
                    getValue(upSampleH, i + 4) * synthesisH[8];
                low[i] = getValue(upSampleL, i - 4) * synthesisL[0] +
                    getValue(upSampleL, i - 3) * synthesisL[1] +
                    getValue(upSampleL, i - 2) * synthesisL[2] +
                    getValue(upSampleL, i - 1) * synthesisL[3] +
                    getValue(upSampleL, i) * synthesisL[4] +
                    getValue(upSampleL, i + 1) * synthesisL[5] +
                    getValue(upSampleL, i + 2) * synthesisL[6] +
                    getValue(upSampleL, i + 3) * synthesisL[7] +
                    getValue(upSampleL, i + 4) * synthesisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            for (int i = 0; i < length; i++)
                newVector[i] = low[i] + high[i];
            for (int l = 0; l < length; l++)
            {
                wavelet[l, column] = Math.Round(newVector[l]);
                //Debug.WriteLine(wavelet[l, column]);
            }
        }

        private void bAnH1_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 512; l++)
                AnalysisH(l, 512);
            lStatus.Text = "Stage 1 Horizontal Analysis";
            tbY.Text = "256";
        }

        private void bAnV1_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 512; c++)
                AnalysisV(c, 512);
            lStatus.Text = "Stage 1 Vertical Analysis";
            tbX.Text = "256";
        }

        private void bAnH2_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 256; l++)
                AnalysisH(l, 256);
            lStatus.Text = "Stage 2 Horizontal Analysis";
            tbY.Text = "128";
        }

        private void bAnV2_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 256; c++)
                AnalysisV(c, 256);
            lStatus.Text = "Stage 2 Vertical Analysis";
            tbX.Text = "128";
        }

        private void bAnH3_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 128; l++)
                AnalysisH(l, 128);
            lStatus.Text = "Stage 3 Horizontal Analysis";
            tbY.Text = "64";
        }

        private void bAnV3_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 128; c++)
                AnalysisV(c, 128);
            lStatus.Text = "Stage 3 Vertical Analysis";
            tbX.Text = "64";
        }

        private void bAnH4_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 64; l++)
                AnalysisH(l, 64);
            lStatus.Text = "Stage 4 Horizontal Analysis";
            tbY.Text = "32";
        }

        private void bAnV4_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 64; c++)
                AnalysisV(c, 64);
            lStatus.Text = "Stage 4 Vertical Analysis";
            tbX.Text = "32";
        }

        private void bAnH5_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 32; l++)
                AnalysisH(l, 32);
            lStatus.Text = "Stage 5 Horizontal Analysis";
            tbY.Text = "16";
        }

        private void bAnV5_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 32; c++)
                AnalysisV(c, 32);
            lStatus.Text = "Stage 5 Vertical Analysis";
            tbX.Text = "16";
        }

        private void bSyH1_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 512; l++)
                SynthesisH(l, 512);
            lStatus.Text = "Stage 1 Horizontal Synthesis";
            tbY.Text = "512";
        }

        private void bSyV1_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 512; c++)
                SynthesisV(c, 512);
            lStatus.Text = "Stage 1 Vertical Synthesis";
            tbX.Text = "512";
        }

        private void bSyH2_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 256; l++)
                SynthesisH(l, 256);
            lStatus.Text = "Stage 2 Horizontal Synthesis";
            tbY.Text = "256";
        }

        private void bSyV2_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 256; c++)
                SynthesisV(c, 256);
            lStatus.Text = "Stage 2 Vertical Synthesis";
            tbX.Text = "256";
        }

        private void bSyH3_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 128; l++)
                SynthesisH(l, 128);
            lStatus.Text = "Stage 3 Horizontal Synthesis";
            tbY.Text = "128";
        }

        private void bSyV3_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 128; c++)
                SynthesisV(c, 128);
            lStatus.Text = "Stage 3 Vertical Synthesis";
            tbX.Text = "128";
        }

        private void bSyH4_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 64; l++)
                SynthesisH(l, 64);
            lStatus.Text = "Stage 4 Horizontal Synthesis";
            tbY.Text = "64";
        }

        private void bSyV4_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 64; c++)
                SynthesisV(c, 64);
            lStatus.Text = "Stage 4 Vertical Synthesis";
            tbX.Text = "64";
        }

        private void bSyH5_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 32; l++)
                SynthesisH(l, 32);
            lStatus.Text = "Stage 5 Horizontal Synthesis";
            tbY.Text = "32";
        }

        private void bSyV5_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 32; c++)
                SynthesisV(c, 32);
            lStatus.Text = "Stage 5 Vertical Synthesis";
            tbX.Text = "32";
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

        private void bError_Click(object sender, EventArgs e)
        {
            double min = 512.00, max = -100.00;
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    double error = Math.Abs(original[i, j] - wavelet[i, j]);
                    if (error < min)
                        min = error;
                    if (error > max)
                        max = error;
                }
            }
            tbMaxError.Text= max.ToString();
            tbMinError.Text= min.ToString();
        }
    }
}
