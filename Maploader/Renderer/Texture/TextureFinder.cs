using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DmitryBrant.ImageFormats;

namespace Maploader.Renderer.Texture
{
    public class TextureFinder
    {
        public Dictionary<string, bool> TransparentBlocks { get; } = new Dictionary<string, bool>()
        {
            {"minecraft:tallgrass", true},
            {"minecraft:waterlily", true},
            {"minecraft:torch", true},
            {"minecraft:lever", true},
            {"minecraft:redstone_torch", true},
            {"minecraft:sapling", true},
            {"minecraft:double_plant", true },
            {"minecraft:double_plant_grass", true },
            {"minecraft:double_plant_bottom", true },
            {"minecraft:double_plant_top", true },
            {"minecraft:hopper", true },
            {"minecraft:vine", true },
            {"minecraft:yellow_flower", true },
            {"minecraft:red_flower", true },
            {"minecraft:fence", true },
            {"minecraft:fence_gate", true },
            {"minecraft:stone_button", true },
            {"minecraft:redstone_wire", true },
            {"minecraft:kelp", true },
            {"minecraft:seagrass", true },
            {"minecraft:wheat", true },
            {"minecraft:ladder", true },
            {"minecraft:glass", true },
            {"minecraft:stained_glass", true },
            {"minecraft:glass_pane", true },
            {"minecraft:stained_glass_pane", true },
            {"minecraft:rail", true },
            {"minecraft:golden_rail", true },
            {"minecraft:iron_bars", true },
            {"minecraft:end_rod", true },
            {"minecraft:wall_sign", true },
            {"minecraft:spruce_fence_gate", true },
            {"minecraft:standing_sign", true },
            {"minecraft:mushroom_red", true },
            {"minecraft:mushroom_brown", true },
            {"minecraft:fire", true },
            {"minecraft:pumpkin_stem", true },
            {"minecraft:melon_stem", true },
            {"minecraft:carrots", true },
            {"minecraft:potatoes", true },
            {"minecraft:anvil", true },
            {"minecraft:cauldron", true },
            {"minecraft:brewing_stand", true },
            {"minecraft:frame", true },
            {"minecraft:trapdoor", true },
            {"minecraft:deadbush", true },


        };

        private readonly Dictionary<string, Texture> textures;
        private readonly string path;

        public TextureFinder(Dictionary<string, Texture> textures, string path)
        {
            this.textures = textures;
            this.path = path;
        }

        public string FindTexturePath(string name, long data)
        {
            name = name.Replace("minecraft:", "");

            if (name.StartsWith("leaves2"))
                return "textures/blocks/leaves_oak_carried";
            else if (name.StartsWith("leaves"))
                return "textures/blocks/" + name + "_oak_carried";

            string newTexture = GetSubstraction(name, data);

            if (newTexture != null)
            {
                return newTexture;
            }
            if (textures.ContainsKey(name))
            {
                return GetTexture(name, data);
            }
            return null;

        }

        private string GetSubstraction(string name, long data)
        {
            switch (name)
            {
                case "bubble_column":
                case "water":
                    return "textures/blocks/water_placeholder";
                case "lava":
                    return "textures/blocks/lava_placeholder";
                case "fire":
                    return "textures/blocks/fire_0_placeholder";
                case "packed_ice":
                    return GetTexture("ice_packed", data);
                case "bed":
                    if ((data & 8) == 8)
                    {
                        return "textures/blocks/bed_head_top";
                    }
                    else
                    {
                        return "textures/blocks/bed_feet_top";
                    }
                case "wooden_slab":
                    return GetTexture("planks", data);
                case "double_wooden_slab":
                    return GetTexture("planks", data);
                case "wooden_pressure_plate":
                    return GetTexture("planks", data);
                case "spruce_pressure_plate":
                    return GetTexture("spruce_planks", data);
                case "stone_pressure_plate":
                    return GetTexture("stone", data);
                case "oak_stairs":
                    return GetTexture("planks", 0); // data = direction
                case "brick_stairs":
                    return GetTexture("brick", data);
                case "prismarine_stairs":
                    return GetTexture("prismarine", 0); // data = direction
                case "spruce_stairs":
                    return GetTexture("spruce_planks", data);
                case "birch_stairs":
                    return GetTexture("birch_planks", data);
                case "acacia_stairs":
                    return GetTexture("acacia_planks", data);
                case "dark_oak_stairs":
                    return GetTexture("dark_oak_planks", data);
                case "stripped_dark_oak_log":
                    return GetTexture("stripped_dark_oak_log_top", data);
                case "stripped_oak_log":
                    return GetTexture("stripped_oak_log_top", data);
                case "enchanting_table":
                    return GetTexture("enchanting_table_top", data);
                case "log":
                    return GetTexture("log_top", data);
                case "cauldron":
                    return GetTexture("cauldron_top", data);
                case "carpet":
                    return GetTexture("wool", data);
                case "hay_block":
                    return GetTexture("hayblock_top", data);
                case "hopper":
                    return GetTexture("hopper_top", data);
                case "double_plant":
                    return GetTexture("double_plant_carried", 0); // todo fixme
                case "rail":
                    return GetTexture("rail_normal", data);
                case "golden_rail":
                    return GetTexture("rail_golden", data);
                case "red_mushroom_block":
                    return GetTexture("mushroom_red_top", data);
                case "brown_mushroom_block":
                    return GetTexture("mushroom_brown_top", data);
                case "brown_mushroom":
                    return GetTexture("mushroom_red", data);
                case "red_mushroom":
                    return GetTexture("mushroom_brown", data);
                case "mossy_cobblestone":
                    return GetTexture("cobblestone_mossy", data);
                case "glass_pane":
                    return GetTexture("glass_pane_top", data);
                case "stained_glass_pane":
                    return GetTexture("stained_glass_pane_top", data);
                case "redstone_torch":
                    return GetTexture("redstone_torch_on", data);
                case "redstone_wire":
                    return GetTexture("redstone_dust_cross", data);
                case "unpowered_repeater":
                    return GetTexture("repeater_up", data);
                case "daylight_detector":
                    return GetTexture("daylight_detector_top", data);
                case "daylight_detector_inverted":
                    return GetTexture("daylight_detector_top", data);
                case "dispenser":
                    return GetTexture("dispenser_top", data);
                case "observer":
                    return GetTexture("observer_top", data);
                case "ender_chest":
                    return GetTexture("ender_chest_inventory_top", data);
                case "anvil":
                    return GetTexture("anvil_top_damaged_x", data);
                case "cactus":
                    return GetTexture("cactus_top", data);

                /*  case "fence_gate":
                        return GetTexture("grass_carried_top");
                    case "fence":
                        return GetTexture("grass_carried_top");
                        */
                case "podzol":
                    return GetTexture("dirt_podzol_top", data);
                case "grass":
                    return GetTexture("grass_carried_top", data);
                case "seagrass":
                    return GetTexture("seagrass_carried", data);
                case "tallgrass":
                    return GetTexture("tallgrass_carried", data);
                case "sandstone":
                    return GetTexture("sandstone_top", data);
                case "grass_path":
                    return GetTexture("grass_path_top", data);
                case "lit_pumpkin":
                    return GetTexture("pumpkin_top", data);
                case "pumpkin":
                    return GetTexture("pumpkin_top", data);
                case "torch":
                    return GetTexture("torch_on", data);
                case "nether_brick_stairs":
                    return GetTexture("nether_brick", data);
                case "stone_brick_stairs":
                    return GetTexture("stonebrick", data);
                case "crafting_table":
                    return GetTexture("crafting_table_top", data);
                case "double_stone_slab":
                    return GetTexture("stone", data);
                case "brick_block":
                    return GetTexture("brick", data);
                case "stone_stairs":
                    return GetTexture("stone", data);
                case "chest":
                    return GetTexture("chest_inventory_top", data);
                case "snow_layer":
                    return GetTexture("snow", data);
            }

            return null;
        }

        public Dictionary<string, Bitmap> Cache { get; } = new Dictionary<string, Bitmap>();

        public Bitmap GetTextureImage(string localPath)
        {
            if (localPath == null)
                return null;



            localPath = localPath.Replace("textures/", "");
            localPath = localPath.Replace("/", "\\");

            if (Cache.ContainsKey(localPath))
            {
                return Cache[localPath];
            }

            try
            {
                string extension = ".jpg";
                string filepath = Path.Combine(path, localPath);
                Bitmap b = null;
                if (File.Exists(filepath + ".png"))
                {
                    extension = ".png";
                    b = new Bitmap(filepath + extension);
                }
                else if (File.Exists(filepath + ".tga"))
                {
                    extension = ".tga";
                    b = TgaReader.Load(filepath + extension);
                }

                if (!Cache.ContainsKey(localPath))
                {
                    Cache.Add(localPath, b);
                }

                return b;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private string GetTexture(string name, long data)
        {
            if (textures.ContainsKey(name))
            {
                var texture = textures[name];

                if (texture.Subtextures.Count <= data)
                {
                    return textures[name].Subtextures.First().Path;
                }
                else
                {
                    return textures[name].Subtextures[(int) data].Path;
                }
            }

            return null;
        }
    }
}