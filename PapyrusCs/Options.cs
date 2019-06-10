using System.Runtime.InteropServices;
using CommandLine;
using Maploader.Renderer;

namespace PapyrusCs
{
    public class Options
    {
        [Option('w', "world", Required = true, HelpText = "Sets the path the Minecraft Bedrock Edition Map")]
        public string MinecraftWorld { get; set; }

        [Option('o', "output", Required = false, HelpText = "Sets the output path for the generated map tiles", Default = ".")]
        public string OutputPath { get; set; }

        [Option("htmlfile", Required = false, HelpText = "Sets name of html map file", Default = "map.html")]
        public string MapHtml { get; set; }

        [Option('s', "strategy", Required = false, HelpText = "Sets the render strategy. Valid are Dataflow, SingleFor and ParallelFor (Multithreaded)", Default = Strategy.Dataflow)]
        public Strategy Strategy { get; set; }

        [Option("coords", Required = false, HelpText = "Render text coordinates in each chunk", Default = false)]
        public bool RenderCoords { get; set; }

        [Option("limitx", Required = false, HelpText = "Limits the chunk rendering in the x dimension (inclusive). Provide two values with comma separated, eg: -10,10")]
        public string LimitX { get; set; }

        [Option("limitz", Required = false, HelpText = "Limits the chunk rendering in the z dimension (inclusive). Provide two values with comma separated, eg: -10,10")]
        public string LimitZ { get; set; }

        [Option('y', "limity", Required = false, HelpText = "Limits the chunk rendering in the y dimension (inclusive). For y provide just one positive value, eg: 10. -1 means: all", Default = -1)]
        public int LimitY { get; set; }

        [Option("threads", Required = false, HelpText = "Set maximum of used threads", Default = 16)]
        public int MaxNumberOfThreads { get; set; }

        [Option('r', "rendermode", Required = false, HelpText = "RenderMode: Basic - Render without brightness adjustment. Heightmap - Render with brightness adjustment based on brillouin function and height of block", Default = RenderMode.Heightmap)]
        public RenderMode RenderMode { get; set; }

        [Option("brillouin_j", Required = false, HelpText = "Sets factor j for heightmap brightness formula brillouin: brightness = 1+brillouin(height / divider): See https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the function.", Default = 10000f)]
        public float BrillouinJ { get; set; }

        [Option("brillouin_divider", Required = false,
            HelpText =
                "Sets divider for heightmap brightness formula brillouin: brightness = 1+brillouin(height / divider). See https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the function.",
            Default = 20f)]
        public float BrillouinDivider { get; set; }

        [Option('f', Required = false, Default = "png", HelpText = "Sets the output file format")]
        public string FileFormat { get; set; }

        [Option('q', Required = false, Default = -1, HelpText = "Sets quality for jpg or web format (0-100, -1 for lossless webp)")]
        public int Quality { get; set; }

        [Option('d', "dim", Required = false, Default = 0, HelpText = "Selects the dimension. 0: Overworld, 1: Nether, 2: End")]
        public int Dimension { get; set; }

        [Option("trimceiling", Required = false, Default = false, HelpText = "Removes the ceiling starting from y-max. Automatically activated for nether")]
        public bool TrimCeiling { get; set; }


        // Derivative options
        public bool Loaded { get; set; }
        public int? LimitXLow { get; set; }
        public int? LimitXHigh { get; set; }
        public int? LimitZLow { get; set; }
        public int? LimitZHigh { get; set; }
    }
}