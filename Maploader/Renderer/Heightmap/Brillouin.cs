using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Maploader.Renderer.Heightmap
{
    public class Brillouin
    {
        private readonly float j;

        private float Compute(float x)
        {
            if (Math.Abs(x) < 0.01)
                return 0;
            return (2 * j + 1) / (2 * j) * Coth((2 * j + 1) / (2 * j) * x) 
                   - 1 / (2 * j) * Coth(1 / (2 * j) * x);
        }

        private static float Coth(float x) => (float) (Math.Cosh(x) / Math.Sinh(x));

        private Dictionary<int, float> Cache { get; } = new Dictionary<int, float>();
        public float GetBrightness(int height)
        {
            if (!Cache.ContainsKey(height))
                Cache[height] = 1 + Compute(1 / 20f * height);

            return Cache[height];
        }
        public Brillouin(float j)
        {
            this.j = j;
        }
    }
}
