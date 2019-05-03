using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Maploader.Renderer.Heightmap
{
    public class Brillouin
    {
        private static double Compute(double j, double x)
        {
            if (Math.Abs(x) < 0.01)
                return 0;
            return (2 * j + 1) / (2 * j) * Coth((2 * j + 1) / (2 * j) * x) 
                   - 1 / (2 * j) * Coth(1 / (2 * j) * x);
        }

        private static double Coth(double x) => Math.Cosh(x) / Math.Sinh(x);

        private Dictionary<int, double> Cache { get; } = new Dictionary<int, double>();
        public double GetBrightness(int height)
        {
            if (!Cache.ContainsKey(height))
                Cache[height] = 1 + Compute(10000, 1 / 20d * height);

            return Cache[height];
        }
        public Brillouin()
        {
        }
    }
}
