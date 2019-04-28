using System;
using System.Collections.Generic;

namespace Maploader.Renderer
{
    public class RenderSettings
    {
        public bool RenderCoords { get; set; } = false;
        public int MaxNumberOfThreads { get; set; } = 16;
        public HashSet<UInt64> Keys { get; set; }
    }
}