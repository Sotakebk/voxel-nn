using NoiseTest;

namespace RealMode.Generation
{
    public static class OpenSimplexNoiseExtensions
    {
        public static double EvaluateFBM(this OpenSimplexNoise osn, double x, double y,
            double frequency, int octaveCount, double persistence, double lacunarity)
        {
            double value = 0;
            double amplitude = 1;
            double totalAmplitude = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                value += amplitude * osn.Evaluate(x * frequency, y * frequency);
                totalAmplitude += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return value / totalAmplitude;
        }

        public static double EvaluateFBM(this OpenSimplexNoise osn, double x, double y, double z,
            double frequency, int octaveCount, double persistence, double lacunarity)
        {
            double value = 0;
            double amplitude = 1;
            double totalAmplitude = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                value += amplitude * osn.Evaluate(x * frequency, y * frequency, z * frequency);
                totalAmplitude += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return value / totalAmplitude;
        }

        public static double EvaluateFBM(this OpenSimplexNoise osn, double x, double y, double z, double w,
            double frequency, int octaveCount, double persistence, double lacunarity)
        {
            double value = 0;
            double amplitude = 1;
            double totalAmplitude = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                value += amplitude * osn.Evaluate(x * frequency, y * frequency, z * frequency, w * frequency);
                totalAmplitude += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return value / totalAmplitude;
        }
    }
}