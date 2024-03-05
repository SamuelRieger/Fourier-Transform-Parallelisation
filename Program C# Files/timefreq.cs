using System;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Shapes;
using System.Windows.Forms;

using System.Threading;
using System.Linq;

namespace DigitalMusicAnalysis
{
    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;

        private float fftMax;
        private int N;
        private int numFramesPerThread;
        private Complex[] xThread;
        private float[][] Y;

        public timefreq(float[] x, int windowSamp)
        {
            int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];

            fftMax = 0;
            N = 0;
            numFramesPerThread = 0;

            for (ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }

            timeFreqData = new float[wSamp/2][];

            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];
            for (int kk = 0; kk < nearest; kk++)
            {
                if (kk < x.Length)
                {
                    compX[kk] = x[kk];
                }
                else
                {
                    compX[kk] = Complex.Zero;
                }
            }

            int cols = 2 * nearest /wSamp;

            for (int jj = 0; jj < wSamp / 2; jj++)
            {
                timeFreqData[jj] = new float[cols];
            }

            timeFreqData = distributeSTFT(compX, wSamp);

            // Save timeFreqData as into a binary file.
            //using (Stream stream = File.Open("TimeFreqData.bin", FileMode.Create))
            //{
            //    BinaryFormatter bformatter = new BinaryFormatter();
            //    bformatter.Serialize(stream, timeFreqData);
            //}

            // Check that the generated timeFreqData matches the benchmark one.
            //float[][] checkTimeFreq;

            //using (Stream stream = File.Open("..\\..\\..\\..\\..\\TimeFreqData.bin", FileMode.Open))
            //{
            //    BinaryFormatter bformatter = new BinaryFormatter();
            //    checkTimeFreq = (float[][])bformatter.Deserialize(stream);
            //    for (int qq = 0; qq < checkTimeFreq.Length; qq++)
            //    {
            //        for (int ww = 0; ww < checkTimeFreq[qq].Length; ww++)
            //        {
            //            Debug.Assert(checkTimeFreq[qq][ww] == timeFreqData[qq][ww], "Incorrect timeFreqData produced.");
            //        }
            //    }
            //}
        }

        float[][] distributeSTFT(Complex[] x, int wSamp)
        {
            Stopwatch sw = new Stopwatch();
            double timeElapsed;
            sw.Start();

            N = x.Length;
            fftMax = 0;
            Y = new float[wSamp / 2][];
            xThread = x;

            for (int ll = 0; ll < wSamp / 2; ll++)
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            }

            Thread[] stftThreads = new Thread[MainWindow.availableThreadNum];
            numFramesPerThread = ((int)Math.Ceiling(2 * (double)N / (double)wSamp) + MainWindow.availableThreadNum) / (MainWindow.availableThreadNum);

            for (int j = 0; j < stftThreads.Length; j++)
            {
                stftThreads[j] = new Thread(stft);
                stftThreads[j].Start(j);
            }

            for (int j = 0; j < stftThreads.Length; j++)
            {
                stftThreads[j].Join();
            }

            for (int ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {
                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            }

            sw.Stop();
            timeElapsed = sw.Elapsed.TotalMilliseconds;
            Debug.WriteLine("Time elapsed distributeSTFT() = {0}ms", timeElapsed);

            return Y;
        }

        public void stft(object? threadId)
        {
            int id = (int)threadId;
            int startIndex = id * numFramesPerThread;
            int endIndex = startIndex + numFramesPerThread;
            if (endIndex > 2 * Math.Floor((double)N / (double)wSamp) - 1)
            {
                endIndex = 2 * (int)Math.Floor((double)N / (double)wSamp) - 1;
            }

            Complex[] temp = new Complex[wSamp];
            Complex[] tempFFT;

            for (int k = startIndex; k < endIndex; k++)
            {
                for (int j = 0; j < wSamp; j++)
                {
                    temp[j] = xThread[k * (wSamp / 2) + j];
                }

                tempFFT = FourierTransform.ctfft(temp, twiddles);

                for (int j = 0; j < wSamp / 2; j++)
                {
                    Y[j][k] = (float)Complex.Abs(tempFFT[j]);

                    if (Y[j][k] > fftMax)
                    {
                        fftMax = Y[j][k];
                    }
                }
            }
        }
    }
}
