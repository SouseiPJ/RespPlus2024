using UnityEngine;

namespace SoundAnalysis
{

    public class ToneHeights
    {
        public double[] YIN(float[] d, int LenFFT, double Th)
        {
            int tau = 2;
            double[] ret = new double[2];
            double pitch;
            double clarity;

            while (tau < LenFFT - 1)
            {
                //Debug.Log($"d[{tau}]:{d[tau]}, {d[tau+1]}");
                if (d[tau] < Th)
                {
                    while (tau + 1 < LenFFT && d[tau + 1] < d[tau])
                        tau++;
                    break;
                }
                tau++;
            }
            if (tau == LenFFT || d[tau] >= Th)
                pitch = 0;
            else
                pitch = 1.0 / tau;
            clarity = 1.0 - Mathf.Log(1 + d[tau]) / Mathf.Log(2);
            ret[0] = pitch;
            ret[1] = clarity;
            return ret;
        }

        public float[] cmnd(float[] diff, int LenFFT)
        {
            diff[0] = 1.0f;
            float sum_value = 0.0f;
            for (int tau = 1; tau < LenFFT; tau++)
            {
                sum_value += diff[tau];
                diff[tau] /= (sum_value / tau);
            }
            return diff;
        }

        public float[] getAMDF(float[] x, int LenFFT)
        {
            float[] mad = new float[LenFFT];
            int ni;
            for (int i = 0; i < LenFFT; i++)
            {
                mad[i] = 0;
                ni = 0;
                for (int j = 0; j < LenFFT; j++)
                {
                    if (j - i >= 0)
                    {
                        mad[i] += Mathf.Abs(x[j - i] - x[j]);
                        ni++;
                    }
                }
                mad[i] /= ni;
            }
            return mad;
        }

        public float Hz2Scale(float Hz)
        {
            if (Hz == 0) return 0.0f;
            else return (12.0f * Mathf.Log(Hz / 110.0f) / Mathf.Log(2.0f));
        }

        public int[] Scale2Chroma(float scale)
        {
            int[] ret = new int[2];
            while (scale < 0.0f) scale += 12.0f;
            int scaleI = (int)scale;
            if (scale - scaleI > 0.5) scaleI++;
            else if (scale - scaleI == 0.5) scaleI = (scaleI % 2 == 0) ? scaleI : scaleI++;

            ret[0] = scaleI % 12; // chroma
            ret[1] = scaleI / 12; // octave

            return ret;
        }
    }

}