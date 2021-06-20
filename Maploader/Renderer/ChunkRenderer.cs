using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Maploader.Extensions;
using Maploader.Renderer.Heightmap;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace Maploader.Renderer
{
    public class ChunkRenderer<TImage> where TImage : class
    {
        private readonly TextureFinder<TImage> textureFinder;
        private readonly IGraphicsApi<TImage> graphics;
        private readonly RenderSettings renderSettings;

        public ChunkRenderer([NotNull] TextureFinder<TImage> textureFinder, IGraphicsApi<TImage> graphics, RenderSettings settings = null)
        {
            this.textureFinder = textureFinder ?? throw new ArgumentNullException(nameof(textureFinder));
            this.graphics = graphics;
            this.renderSettings = settings ?? new RenderSettings();

            b = new Brillouin(renderSettings.BrillouinJ, renderSettings.BrillouinDivider);
        }

        public List<string> MissingTextures { get; } = new List<string>();

        private Brillouin b;



        public void RenderChunk(TImage dest, Chunk c, int xOffset, int zOffset)
        {
            var xzColumns = c.Blocks.GroupBy(x => x.Value.XZ);
            var blocksOrderedByXZ = xzColumns.OrderBy(x => x.Key.GetLeByte(0)).ThenBy(x => x.Key.GetLeByte(1));
            var brightnessOffset = Math.Min(renderSettings.BrillouinOffset, renderSettings.YMax);
            if (brightnessOffset < 0)
                brightnessOffset = renderSettings.BrillouinOffset;


            foreach (var blocks in blocksOrderedByXZ)
            {
                var blocksToRender = new Stack<BlockCoord>();

                List<KeyValuePair<uint, BlockCoord>> blocksFromSkyToBedrock = blocks.Where(x => x.Value.Block.Id != "minecraft:air").OrderByDescending(x => x.Value.Y).ToList();
                if (renderSettings.YMax > 0)
                    blocksFromSkyToBedrock = blocksFromSkyToBedrock.Where(x => x.Value.Y <= renderSettings.YMax).ToList();

                if (renderSettings.TrimCeiling)
                {
                    int start = -1;
                    for (int i = 1; i < blocksFromSkyToBedrock.Count(); i++)
                    {
                        if (Math.Abs(blocksFromSkyToBedrock[i].Value.Y - blocksFromSkyToBedrock[i - 1].Value.Y) > 4)
                        {
                            start = i;
                            break;
                        }
                    }

                    if (start != -1)
                    {
                        blocksFromSkyToBedrock.RemoveRange(0, start);
                    }

                }

                switch (renderSettings.Profile)
                {
                    case "underground":
                        {
                            var lastYValue = 300;
                            var isRendering = false;
                            var state = "goingthroughtoplevelsky";

                            foreach (var blockColumn in blocksFromSkyToBedrock)
                            {
                                var block = blockColumn.Value;

                                if (!isRendering)
                                {
                                    var skyBlocksSkipped = lastYValue - block.Y - 1;

                                    switch (state)
                                    {
                                        case "goingthroughtoplevelsky":
                                            if (textureFinder.TransparentBlocks.ContainsKey(block.Block.Id) || block.Block.Id.Contains("water") || block.Block.Id.Contains("kelp"))
                                            {
                                                continue;
                                            }

                                            lastYValue = block.Y;

                                            if (skyBlocksSkipped > 0)
                                            {
                                                state = "goingthroughground";
                                            }
                                            break;
                                        case "goingthroughground":
                                            lastYValue = block.Y;

                                            if (textureFinder.TransparentBlocks.ContainsKey(block.Block.Id) || block.Block.Id.Contains("water") || block.Block.Id.Contains("kelp"))
                                            {
                                                isRendering = true;
                                            }

                                            if (skyBlocksSkipped > 0)
                                            {
                                                isRendering = true;
                                            }
                                            break;
                                    }
                                }
                                
                                if (isRendering)
                                {
                                    blocksToRender.Push(block);
                                    if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case "aquatic":
                        {
                            bool isWater = false;

                            foreach (var blockColumn in blocksFromSkyToBedrock)
                            {
                                var block = blockColumn.Value;

                                if (block.Block.Id.Contains("water"))
                                {
                                    isWater = true;
                                    continue;
                                }

                                if (!isWater)
                                {
                                    // stop if we hit a solid block first
                                    if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id))
                                    {
                                        break;
                                    }

                                    continue;
                                }

                                blocksToRender.Push(block);
                                if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id))
                                {
                                    break;
                                }
                            }
                        }
                        break;
                    case "ore":
                        {
                            SearchForOres(blocksToRender, blocksFromSkyToBedrock);
                        }
                        break;
                    case "stronghold":
                        {
                            var lastYValue = 300;
                            var isRendering = false;
                            var state = "goingthroughtoplevelsky";

                            foreach (var blockColumn in blocksFromSkyToBedrock)
                            {
                                var block = blockColumn.Value;

                                if (!isRendering)
                                {
                                    var skyBlocksSkipped = lastYValue - block.Y - 1;

                                    switch (state)
                                    {
                                        case "goingthroughtoplevelsky":
                                            if (textureFinder.TransparentBlocks.ContainsKey(block.Block.Id) || block.Block.Id.Contains("water") || block.Block.Id.Contains("kelp"))
                                            {
                                                continue;
                                            }

                                            lastYValue = block.Y;

                                            if (skyBlocksSkipped > 0)
                                            {
                                                state = "goingthroughground";
                                            }
                                            break;
                                        case "goingthroughground":
                                            lastYValue = block.Y;

                                            if (textureFinder.TransparentBlocks.ContainsKey(block.Block.Id) || block.Block.Id.Contains("water") || block.Block.Id.Contains("kelp"))
                                            {
                                                isRendering = true;
                                            }

                                            if (skyBlocksSkipped > 0)
                                            {
                                                isRendering = true;
                                            }
                                            break;
                                    }
                                }

                                if (isRendering)
                                {
                                    if (block.Block.Id.Contains("cobblestone") ||
                                        block.Block.Id.Contains("brick") ||
                                        block.Block.Id.Contains("end") ||
                                        block.Block.Id.Contains("iron_bars") ||
                                        block.Block.Id.Contains("spawn") ||
                                        block.Block.Id.Contains("egg") ||
                                        block.Block.Id.Contains("bookshelf") ||
                                        block.Block.Id.Contains("cobweb") ||
                                        block.Block.Id.Contains("oak_planks") ||
                                        block.Block.Id.Contains("chest") ||
                                        block.Block.Id.Contains("door"))
                                    {
                                        blocksToRender.Push(block);
                                        if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        {
                            foreach (var blockColumn in blocksFromSkyToBedrock) // Look for transparent blocks in single y column
                            {
                                var block = blockColumn.Value;

                                blocksToRender.Push(block);
                                if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id))
                                {
                                    break;
                                }
                            }
                        }
                        break;
                }

                foreach (var block in blocksToRender)
                {
                    if(SkipSpecialBlockRender(block.Block))
                    {
                        continue;
                    }
                    var textures =
                        textureFinder.FindTexturePath(block.Block.Id, block.Block.Data, block.X, block.Z, block.Y);
                    if (textures == null)
                    {
                        Console.WriteLine($"Missing Texture(2): {block.ToString().PadRight(30)}");
                        MissingTextures.Add($"ID: {block.Block.Id}");
                        continue;
                    }

                    foreach (var texture in textures.Infos)
                    {
                        var bitmapTile = textureFinder.GetTextureImage(texture);
                        if (bitmapTile != null)
                        {
                            var x = xOffset + block.X * 16;
                            var z = zOffset + block.Z * 16;

                            if (renderSettings.RenderMode == RenderMode.Heightmap)
                            {
                                graphics.DrawImageWithBrightness(dest, bitmapTile, x, z, b.GetBrightness(block.Y - brightnessOffset));
                            }
                            else
                            {
                                graphics.DrawImageWithBrightness(dest, bitmapTile, x, z, 1);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"\nMissing Texture(1): {block.ToString().PadRight(30)} -- {texture.Filename}");
                            MissingTextures.Add($"ID: {block.Block.Id}, {texture.Filename}");
                        }
                    }
                }
            }

            if (renderSettings.RenderCoordinateStrings)
            {
                graphics.DrawString(dest, $"{c.X * 1}, {c.Z * 1}", new Font(FontFamily.GenericSansSerif, 10), Brushes.Black, xOffset, zOffset);
            }
        }

        private static void SearchForOres(Stack<BlockCoord> blocksToRender, List<KeyValuePair<uint, BlockCoord>> blocksFromSkyToBedrock)
        {
            var orePriority = new[]
            {
                "minecraft:diamond_ore",
                "minecraft:emerald_ore",
                "minecraft:redstone_ore",
                "minecraft:gold_ore",
                "minecraft:iron_ore",
                "minecraft:lapis_ore",
                "minecraft:coal_ore",
            };

            foreach (var target in orePriority)
            {
                bool foundOre = false;

                foreach (var blockColumn in blocksFromSkyToBedrock)
                {
                    var block = blockColumn.Value;

                    if (block.Block.Id == target)
                    {
                        blocksToRender.Push(block);
                        foundOre = true;
                        break;
                    }
                }

                if (foundOre)
                {
                    break;
                }
            }
        }

        private static void SearchForEndBlocks(Stack<BlockCoord> blocksToRender, List<KeyValuePair<uint, BlockCoord>> blocksFromSkyToBedrock)
        {
            foreach (var blockColumn in blocksFromSkyToBedrock)
            {
                var block = blockColumn.Value;

                if (block.Block.Id.Contains("end"))
                {
                    blocksToRender.Push(block);
                    break;
                }
            }
        }

        private static readonly List<string> SpecialBlockList = new List<string>
        {
            "minecraft:light_block"
        };
        private static bool SkipSpecialBlockRender (BlockData block)
        {
            return SpecialBlockList.Contains(block.Id);
        }
    }
}
