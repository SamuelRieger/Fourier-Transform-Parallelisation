using NAudio.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMusicAnalysis
{
    public class FourierTransform 
    {


        //  Cooley-Tukey FFT algorithm
        public static Complex[] ctfft(Complex[] x, Complex[] twiddles)
        {
            Complex[] Y = brp(x);
            int N = x.Length;

            for (int len = 2; len <= N; len <<= 1)
            {
                for (int i = 0; i < N; i += len)
                {
                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = Y[i + j], v = Y[i + j + (len / 2)];

                        Complex wFirstHalf = twiddles[j * (N / len)];
                        Complex wSecondHalf = twiddles[(j + (len / 2)) * (N / len)];

                        Y[i + j] = u + (v * wFirstHalf);
                        Y[i + j + (len / 2)] = u + (v * wSecondHalf);
                    }
                }
            }

            return Y;
        }

        // Calcualte bit reversal permutation
        private static Complex[] brp(Complex[] data)
        {
            int N = data.Length;
            int k = (int)Math.Log(N, 2);
            Complex[] Y = new Complex[N];

            Parallel.For(0, N, MainWindow.parallelOptions, i =>
            {
                int reverse = 0;
                for (int j = 0; j < k; j++)
                {
                    reverse |= (i & (1 << j)) != 0 ? 1 << (k - 1 - j) : 0;
                }
                Y[i] = data[reverse];
            });

            return Y;
        }
    }
}
