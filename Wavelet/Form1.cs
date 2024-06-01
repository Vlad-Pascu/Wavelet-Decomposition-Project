using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Wavelet
{
    public partial class fWavelet : Form
    {
        byte[,] original = new byte[512, 512];
        double[,] wavelet = new double[512, 512];
        string fileName;
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
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Bitmap Image|*.bmp";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFile.FileName;
                Bitmap bmp = new Bitmap(filePath);
                fileName = openFile.FileName;
                Debug.WriteLine(fileName);
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
                Debug.WriteLine("Image loaded successfully!");
            }
            else
            {
                Debug.WriteLine("Could not load image");
            }
        }

        private void bRefresh_Click(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void RefreshImage()
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

        private double GetValue(double[] vector, int index)
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
            for (int i = 0; i < length; i++)
                originalVector[i] = wavelet[i, line];
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = GetValue(originalVector, i - 4) * analysisH[0] +
                    GetValue(originalVector, i - 3) * analysisH[1] +
                    GetValue(originalVector, i - 2) * analysisH[2] +
                    GetValue(originalVector, i - 1) * analysisH[3] +
                    GetValue(originalVector, i) * analysisH[4] +
                    GetValue(originalVector, i + 1) * analysisH[5] +
                    GetValue(originalVector, i + 2) * analysisH[6] +
                    GetValue(originalVector, i + 3) * analysisH[7] +
                    GetValue(originalVector, i + 4) * analysisH[8];
                low[i] = GetValue(originalVector, i - 4) * analysisL[0] +
                    GetValue(originalVector, i - 3) * analysisL[1] +
                    GetValue(originalVector, i - 2) * analysisL[2] +
                    GetValue(originalVector, i - 1) * analysisL[3] +
                    GetValue(originalVector, i) * analysisL[4] +
                    GetValue(originalVector, i + 1) * analysisL[5] +
                    GetValue(originalVector, i + 2) * analysisL[6] +
                    GetValue(originalVector, i + 3) * analysisL[7] +
                    GetValue(originalVector, i + 4) * analysisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            double[] downSampleL = new double[length];
            double[] downSampleH = new double[length];
            for (int i = 0; i < length; i++)
            {
                if (i % 2 == 0)
                    downSampleL[i] = low[i];
                else
                    downSampleH[i] = high[i];
                //Debug.WriteLine(downSampleL[i+1] + " " + downSampleH[i+1]);
            }
            for (int i = 0; i < length / 2; i++)
            {
                newVector[i] = downSampleL[2 * i];
                newVector[i + length / 2] = downSampleH[2 * i + 1];
            }
            for (int i = 0; i < length; i++)
            {
                wavelet[i, line] = newVector[i];
                //Debug.WriteLine(newVector[i]);
            }
        }

        private void AnalysisV(int column, int length)
        {
            double[] originalVector = new double[length];
            for (int i = 0; i < length; i++)
                originalVector[i] = wavelet[column, i];
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = GetValue(originalVector, i - 4) * analysisH[0] +
                    GetValue(originalVector, i - 3) * analysisH[1] +
                    GetValue(originalVector, i - 2) * analysisH[2] +
                    GetValue(originalVector, i - 1) * analysisH[3] +
                    GetValue(originalVector, i) * analysisH[4] +
                    GetValue(originalVector, i + 1) * analysisH[5] +
                    GetValue(originalVector, i + 2) * analysisH[6] +
                    GetValue(originalVector, i + 3) * analysisH[7] +
                    GetValue(originalVector, i + 4) * analysisH[8];
                low[i] = GetValue(originalVector, i - 4) * analysisL[0] +
                    GetValue(originalVector, i - 3) * analysisL[1] +
                    GetValue(originalVector, i - 2) * analysisL[2] +
                    GetValue(originalVector, i - 1) * analysisL[3] +
                    GetValue(originalVector, i) * analysisL[4] +
                    GetValue(originalVector, i + 1) * analysisL[5] +
                    GetValue(originalVector, i + 2) * analysisL[6] +
                    GetValue(originalVector, i + 3) * analysisL[7] +
                    GetValue(originalVector, i + 4) * analysisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            double[] downSampleL = new double[length];
            double[] downSampleH = new double[length];
            for (int i = 0; i < length; i++)
            {
                if (i % 2 == 0)
                    downSampleL[i] = low[i];
                else
                    downSampleH[i] = high[i];
                //Debug.WriteLine(downSampleL[i+1] + " " + downSampleH[i+1]);
            }
            for (int i = 0; i < length / 2; i++)
            {
                newVector[i] = downSampleL[2 * i];
                newVector[i + length / 2] = downSampleH[2 * i + 1];
            }
            for (int i = 0; i < length; i++)
            {
                wavelet[column, i] = newVector[i];
                //Debug.WriteLine(newVector[i]);
            }
        }

        private void SynthesisH(int line, int length)
        {
            double[] originalVector = new double[length];
            for (int i = 0; i < length; i++)
                originalVector[i] = wavelet[i, line];
            double[] upSampleL = new double[length];
            double[] upSampleH = new double[length];
            for (var i = 0; i < length / 2; i++)
            {
                upSampleL[2 * i] = originalVector[i];
                upSampleH[2 * i + 1] = originalVector[i + length / 2];
            }
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = GetValue(upSampleH, i - 4) * synthesisH[0] +
                    GetValue(upSampleH, i - 3) * synthesisH[1] +
                    GetValue(upSampleH, i - 2) * synthesisH[2] +
                    GetValue(upSampleH, i - 1) * synthesisH[3] +
                    GetValue(upSampleH, i) * synthesisH[4] +
                    GetValue(upSampleH, i + 1) * synthesisH[5] +
                    GetValue(upSampleH, i + 2) * synthesisH[6] +
                    GetValue(upSampleH, i + 3) * synthesisH[7] +
                    GetValue(upSampleH, i + 4) * synthesisH[8];
                low[i] = GetValue(upSampleL, i - 4) * synthesisL[0] +
                    GetValue(upSampleL, i - 3) * synthesisL[1] +
                    GetValue(upSampleL, i - 2) * synthesisL[2] +
                    GetValue(upSampleL, i - 1) * synthesisL[3] +
                    GetValue(upSampleL, i) * synthesisL[4] +
                    GetValue(upSampleL, i + 1) * synthesisL[5] +
                    GetValue(upSampleL, i + 2) * synthesisL[6] +
                    GetValue(upSampleL, i + 3) * synthesisL[7] +
                    GetValue(upSampleL, i + 4) * synthesisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            for (int i = 0; i < length; i++)
                newVector[i] = low[i] + high[i];
            for (int i = 0; i < length; i++)
            {
                wavelet[i, line] = newVector[i];
                //Debug.WriteLine(wavelet[line,c]);
            }
        }

        private void SynthesisV(int column, int length)
        {
            double[] originalVector = new double[length];
            for (int i = 0; i < length; i++)
                originalVector[i] = wavelet[column, i];
            double[] upSampleL = new double[length];
            double[] upSampleH = new double[length];
            for (var i = 0; i < length / 2; i++)
            {
                upSampleL[2 * i] = originalVector[i];
                upSampleH[2 * i + 1] = originalVector[i + length / 2];
            }
            double[] low = new double[length];
            double[] high = new double[length];
            double[] newVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                high[i] = GetValue(upSampleH, i - 4) * synthesisH[0] +
                    GetValue(upSampleH, i - 3) * synthesisH[1] +
                    GetValue(upSampleH, i - 2) * synthesisH[2] +
                    GetValue(upSampleH, i - 1) * synthesisH[3] +
                    GetValue(upSampleH, i) * synthesisH[4] +
                    GetValue(upSampleH, i + 1) * synthesisH[5] +
                    GetValue(upSampleH, i + 2) * synthesisH[6] +
                    GetValue(upSampleH, i + 3) * synthesisH[7] +
                    GetValue(upSampleH, i + 4) * synthesisH[8];
                low[i] = GetValue(upSampleL, i - 4) * synthesisL[0] +
                    GetValue(upSampleL, i - 3) * synthesisL[1] +
                    GetValue(upSampleL, i - 2) * synthesisL[2] +
                    GetValue(upSampleL, i - 1) * synthesisL[3] +
                    GetValue(upSampleL, i) * synthesisL[4] +
                    GetValue(upSampleL, i + 1) * synthesisL[5] +
                    GetValue(upSampleL, i + 2) * synthesisL[6] +
                    GetValue(upSampleL, i + 3) * synthesisL[7] +
                    GetValue(upSampleL, i + 4) * synthesisL[8];
                //Debug.WriteLine(low[i] + " " + high[i]);
            }
            for (int i = 0; i < length; i++)
                newVector[i] = low[i] + high[i];
            for (int i = 0; i < length; i++)
            {
                //wavelet[l, column] = Math.Round(newVector[l]);
                wavelet[column, i] = newVector[i];
                //Debug.WriteLine(wavelet[l, column]);
            }
        }

        private void bAnH1_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 512; l++)
                AnalysisH(l, 512);
            lStatus.Text = "Stage 1 Horizontal Analysis";
            tbX.Text = "256";
            RefreshImage();
        }

        private void bAnV1_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 512; c++)
                AnalysisV(c, 512);
            lStatus.Text = "Stage 1 Vertical Analysis";
            tbY.Text = "256";
            RefreshImage();
        }

        private void bAnH2_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 256; l++)
                AnalysisH(l, 256);
            lStatus.Text = "Stage 2 Horizontal Analysis";
            tbX.Text = "128";
            RefreshImage();
        }

        private void bAnV2_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 256; c++)
                AnalysisV(c, 256);
            lStatus.Text = "Stage 2 Vertical Analysis";
            tbY.Text = "128";
            RefreshImage();
        }

        private void bAnH3_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 128; l++)
                AnalysisH(l, 128);
            lStatus.Text = "Stage 3 Horizontal Analysis";
            tbX.Text = "64";
            RefreshImage();
        }

        private void bAnV3_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 128; c++)
                AnalysisV(c, 128);
            lStatus.Text = "Stage 3 Vertical Analysis";
            tbY.Text = "64";
            RefreshImage();
        }

        private void bAnH4_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 64; l++)
                AnalysisH(l, 64);
            lStatus.Text = "Stage 4 Horizontal Analysis";
            tbX.Text = "32";
            RefreshImage();
        }

        private void bAnV4_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 64; c++)
                AnalysisV(c, 64);
            lStatus.Text = "Stage 4 Vertical Analysis";
            tbY.Text = "32";
            RefreshImage();
        }

        private void bAnH5_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 32; l++)
                AnalysisH(l, 32);
            lStatus.Text = "Stage 5 Horizontal Analysis";
            tbX.Text = "16";
            RefreshImage();
        }

        private void bAnV5_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 32; c++)
                AnalysisV(c, 32);
            lStatus.Text = "Stage 5 Vertical Analysis";
            tbY.Text = "16";
            RefreshImage();
        }

        private void bSyH1_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 512; l++)
                SynthesisH(l, 512);
            lStatus.Text = "Stage 1 Horizontal Synthesis";
            tbX.Text = "512";
            RefreshImage();
        }

        private void bSyV1_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 512; c++)
                SynthesisV(c, 512);
            lStatus.Text = "Stage 1 Vertical Synthesis";
            tbY.Text = "512";
            RefreshImage();
        }

        private void bSyH2_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 256; l++)
                SynthesisH(l, 256);
            lStatus.Text = "Stage 2 Horizontal Synthesis";
            tbX.Text = "256";
            RefreshImage();
        }

        private void bSyV2_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 256; c++)
                SynthesisV(c, 256);
            lStatus.Text = "Stage 2 Vertical Synthesis";
            tbY.Text = "256";
            RefreshImage();
        }

        private void bSyH3_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 128; l++)
                SynthesisH(l, 128);
            lStatus.Text = "Stage 3 Horizontal Synthesis";
            tbX.Text = "128";
            RefreshImage();
        }

        private void bSyV3_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 128; c++)
                SynthesisV(c, 128);
            lStatus.Text = "Stage 3 Vertical Synthesis";
            tbY.Text = "128";
            RefreshImage();
        }

        private void bSyH4_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 64; l++)
                SynthesisH(l, 64);
            lStatus.Text = "Stage 4 Horizontal Synthesis";
            tbX.Text = "64";
            RefreshImage();
        }

        private void bSyV4_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 64; c++)
                SynthesisV(c, 64);
            lStatus.Text = "Stage 4 Vertical Synthesis";
            tbY.Text = "64";
            RefreshImage();
        }

        private void bSyH5_Click(object sender, EventArgs e)
        {
            for (int l = 0; l < 32; l++)
                SynthesisH(l, 32);
            lStatus.Text = "Stage 5 Horizontal Synthesis";
            tbX.Text = "32";
            RefreshImage();
        }

        private void bSyV5_Click(object sender, EventArgs e)
        {
            for (int c = 0; c < 32; c++)
                SynthesisV(c, 32);
            lStatus.Text = "Stage 5 Vertical Synthesis";
            tbY.Text = "32";
            RefreshImage();
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
            double min = 256.00, max = -256.00;
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    double error = Math.Abs(original[i, j] - Math.Round(wavelet[i, j]));
                    if (error < min)
                        min = error;
                    if (error > max)
                        max = error;
                }
            }
            tbMaxError.Text = max.ToString();
            tbMinError.Text = min.ToString();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Wavelet files (*.wvl)|";
            saveFile.RestoreDirectory = true;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFile.FileName;

                StreamWriter writer = new StreamWriter(filePath);
                for (int y = 0; y < 512; y++)
                {
                    for (int x = 0; x < 512; x++)
                        writer.WriteLine(wavelet[x, y].ToString("F6") + " ");
                }
                Debug.WriteLine("File saved successfully.");
                writer.Close();

            }
            else
                Debug.WriteLine("Could not save");
        }

        private void bLoadWavelet_Click(object sender, EventArgs e)
        {
            StreamReader reader;
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Wavelet (*.wvl)|";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFile.FileName;
                fileName = openFile.FileName;
                Debug.WriteLine(fileName);
                reader = new StreamReader(fileName);
                for (int y = 0; y < 512; y++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        wavelet[x, y] = double.Parse(reader.ReadLine());
                    }
                }
                Debug.WriteLine("Wavelet Loaded");
            }
            else
            {
                Debug.WriteLine("Could not load wavelet");
            }
        }
    }
}
