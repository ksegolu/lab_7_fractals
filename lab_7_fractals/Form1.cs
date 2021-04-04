using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;
using System.IO;


namespace lab_7_fractals
{
    public partial class Form1 : Form
    {
        string filename;
        Bitmap img_2;
        //int cellSize;
        int celllInLine;
        int cellCount;
        float[,,] u0, u, b;
        int w, h;
        const int delta = 4;
        double[] xi = new double[delta];
        double[] yi = new double[delta];
        float[,] vol;
        int q;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = "unh_4.jpg";
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(path);
            Image<Bgr, Byte> image2 = image.Convert<Bgr, Byte>().Resize(1024, 1024, Emgu.CV.CvEnum.Inter.Linear);
            image2.Save("edited.png");
            Bitmap img_2 = new Bitmap("edited.png");
            pictureBox1.Image = img_2;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            h = img_2.Height;
            w = img_2.Width;
            q = 0;
            double[] res = new double[9];
            for (int cellSize = 4; cellSize <= Math.Pow(2, 10); cellSize *= 2)
            {
                celllInLine = 1024 / cellSize;
                cellCount = celllInLine * celllInLine;
                vol = new float[cellCount, delta];
                u0 = new float[cellCount, cellSize, cellSize];
                u = new float[cellCount, cellSize, cellSize];
                b = new float[cellCount, cellSize, cellSize];
                setU0(img_2, cellSize);
                float s;
                //calc u and b 2 times
                for (int d = 1; d <= delta; d++)
                {
                    calcU(d, cellSize);
                    calcB(d, cellSize);
                    for (int k = 0; k < cellCount; k++)
                    {
                        s = 0;
                        for (int i = cellSize - 1; i >= 0; i--)
                        {
                            for (int j = cellSize - 1; j >= 0; j--)
                            {
                                s += u[k, i, j] - b[k, i, j];
                            }
                        }
                        vol[k, d - 1] = s;
                    }

                }
                s = 0;
                for (int k = 0; k < cellCount; k++)
                {
                    s += vol[k, 0] / 2;
                }
                yi[0] = Math.Log(s);
                xi[0] = Math.Log(1);
                for (int d = 1; d < delta; d++)
                {
                    s = 0;
                    for (int k = 0; k < cellCount; k++)
                    {
                        s += (vol[k, d] - vol[k, d - 1]) / 2;
                    }
                    yi[d] = Math.Log(s);
                    xi[d] = Math.Log(d + 1);
                }
                res[q] = 2.0 + MNK(delta);
                q++;
            }

        }



        private void setU0(Bitmap img, int cellS)
        {
            for (int k = 0; k < cellCount; k++)
            {
                for (int i = cellS - 1; i >= 0; i--)
                {
                    for (int j = cellS - 1; j >= 0; j--)
                    {
                        u0[k, i, j] = getIntense(img.GetPixel(i + (cellS * (k % celllInLine)), j + (cellS * (k / celllInLine))));
                    }
                }
            }

            return;
        }
        //верхняя поверхность uδ(i,j)
        private void calcU(int delta, int cellS)
        {
            if (delta > 1)
            {
                for (int k = 0; k < cellCount; k++)
                {
                    for (int i = cellS - 1; i >= 0; i--)
                    {
                        for (int j = cellS - 1; j >= 0; j--)
                        {
                            u0[k, i, j] = u[k, i, j];
                        }
                    }
                }
            }
            for (int k = 0; k < cellCount; k++)
            {
                for (int i = cellS - 1; i >= 0; i--)
                {
                    for (int j = cellS - 1; j >= 0; j--)
                    {
                        u[k, i, j] = Math.Max(
                            u0[k, i, j] + 1,
                            Math.Max(
                                Math.Max(
                                    i > 0 ? u0[k, i - 1, j] : 0,
                                    j > 0 ? u0[k, i, j - 1] : 0
                                    ),
                                Math.Max(
                                    i < cellS - 1 ? u0[k, i + 1, j] : 0,
                                    j < cellS - 1 ? u0[k, i, j + 1] : 0
                                    )
                                )
                            );
                    }
                }
            }
            return;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void calcB(int delta, int cellS)
        {
            if (delta > 1)
            {
                for (int k = 0; k < cellCount; k++)
                {
                    for (int i = cellS - 1; i >= 0; i--)
                    {
                        for (int j = cellS - 1; j >= 0; j--)
                        {
                            u0[k, i, j] = b[k, i, j];
                        }
                    }
                }
            }
            for (int k = 0; k < cellCount; k++)
            {
                for (int i = cellS - 1; i >= 0; i--)
                {
                    for (int j = cellS - 1; j >= 0; j--)
                    {
                        b[k, i, j] = Math.Min(
                            u0[k, i, j] - 1,
                            Math.Min(
                                Math.Min(
                                    i > 0 ? u0[k, i - 1, j] : 256,
                                    j > 0 ? u0[k, i, j - 1] : 256
                                    ),
                                Math.Min(
                                    i < cellS - 1 ? u0[k, i + 1, j] : 256,
                                    j < cellS - 1 ? u0[k, i, j + 1] : 256
                                    )
                                )
                            );
                    }
                }
            }
            return;
        }
        


        private double MNK(int n)
        {
            double sx = 0,
                sx2 = 0,
                sxy = 0,
                sy = 0;
            for (int i = 0; i < n; i++)
            {
                sx += xi[i];
                sx2 += xi[i] * xi[i];
                sxy += xi[i] * yi[i];
                sy += yi[i];
            }
            sx /= n;
            sx2 /= n;
            sxy /= n;
            sy /= n;
            return (sxy - sx * sy) / (sx2 - sx * sx);
        }
        private float getIntense(Color color)
        {
            return 255 - (color.R + color.G + color.B) / 3;
        }
    }
}

