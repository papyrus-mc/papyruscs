using System;
using System.Collections.Generic;

namespace Maploader.Renderer
{
    public class RenderSettings
    {
        public bool RenderCoordinateStrings { get; set; } = false;
        public int MaxNumberOfThreads { get; set; } = 16;
        public int YMax { get; set; } = -1;
        public float BrillouinJ { get; set; } = 10000;
        public RenderMode RenderMode { get; set; }
        public float BrillouinDivider { get; set; }
        public bool TrimCeiling { get; set; } = true;
    }
}