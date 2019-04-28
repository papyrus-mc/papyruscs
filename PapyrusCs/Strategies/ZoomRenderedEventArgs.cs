using System;

namespace PapyrusCs.Strategies
{
    public class ZoomRenderedEventArgs : EventArgs
    {
        public int LinesRendered { get; }
        public int TotalLines { get; }
        public int ZoomLevel { get; }

        public ZoomRenderedEventArgs(int linesRendered, int totalLines, int zoomLevel)
        {
            this.LinesRendered = linesRendered;
            this.TotalLines = totalLines;
            this.ZoomLevel = zoomLevel;
        }

    }
}