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
        /// <summary>
        /// Returns if a texture is transparent
        /// </summary>
        public Dictionary<string, bool> TransparentBlocks { get; } = new Dictionary<string, bool>()
        {
            {"minecraft:tallgrass", true},
            {"minecraft:waterlily", true},
            {"minecraft:torch", true},
            {"minecraft:lever", true},
            {"minecraft:redstone_torch", true},
            {"minecraft:sapling", true},
            {"minecraft:bamboo_sapling", true},
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
            {"minecraft:spruce_fence_gate", true },
            {"minecraft:birch_fence_gate", true },
            {"minecraft:acacia_fence_gate", true },
            {"minecraft:jungle_fence_gate", true },
            {"minecraft:dark_oak_fence_gate", true },
            {"minecraft:cobblestone_wall", true },

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
            {"minecraft:detector_rail", true },
            {"minecraft:activator_rail", true },
            {"minecraft:iron_bars", true },
            {"minecraft:end_rod", true },
            {"minecraft:wall_sign", true },
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
            {"minecraft:web", true },
            {"minecraft:conduit", true },
            {"minecraft:flower_pot", true },
            {"minecraft:bamboo", true },
            {"minecraft:wooden_door", true },



        };

        private readonly Dictionary<string, Texture> texturesJson;
        private readonly string path;

        public TextureFinder(Dictionary<string, Texture> texturesJson, string path)
        {
            this.texturesJson = texturesJson;
            this.path = path;
        }

        public TextureStack FindTexturePath(string name, long data, int x, int z)
        {
            name = name.Replace("minecraft:", "");

            var newTexture = GetSubstitution(name, data, x, z);

            if (newTexture != null)
            {
                return newTexture;
            }
            if (texturesJson.ContainsKey(name))
            {
                return GetTexture(name, data);
            }
            return null;

        }

       

        private TextureStack GetSubstitution(string name, long data, int x, int z)
        {
            // For debugging purposes
            if (name.Contains("bed"))
            {
                Console.WriteLine($"{x} {z}: {name},{data}");
            }

            switch (name)
            {
                case "cobblestone_wall":
                    return GetTexture("cobblestone_wall", data).Translate(5,5,6,6);

                case "bubble_column":
                    return "textures/blocks/water_placeholder";
                case "water":
                    return "textures/blocks/water_placeholder";
                case "lava":
                    return "textures/blocks/lava_placeholder";
                case "fire":
                    return "textures/blocks/fire_0_placeholder";
                case "packed_ice":
                    return GetTexture("ice_packed", data);
                case "lectern":
                    return "textures/blocks/lectern_top";
                case "bed":
                    switch (data & 0xF7)
                    {
                        case 0:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlipType.Rotate90FlipNone);
                        case 1:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlipType.Rotate180FlipNone);
                        case 2:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlipType.Rotate270FlipNone);
                        case 3:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlipType.RotateNoneFlipNone);
                    }

                    return null;
                    

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
                    return GetTexture("brick", 0);
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

                case "cauldron":
                    return GetTexture("cauldron_inner", data)
                        + GetTexture("cauldron_top", data);
                case "carpet":
                    return GetTexture("wool", data);
                case "hay_block":
                    return GetTexture("hayblock_top", data);
                case "hopper":
                    return GetTexture("hopper_inside", data)
                        + GetTexture("hopper_top", data);
                case "double_plant":
                    return GetTexture("double_plant_carried", 0); // todo fixme
                case "tnt":
                    return GetTexture("tnt_top", 0); 
                case "rail":
                    switch (data)
                    {
                        case 0:
                            return GetTexture("rail_normal", data);
                        case 1:
                            return GetTexture("rail_normal", data, null, RotateFlipType.Rotate90FlipNone);
                        case 2:
                            return GetTexture("rail_normal", data, null, RotateFlipType.Rotate90FlipNone);
                        case 3:
                            return GetTexture("rail_normal", data, null, RotateFlipType.Rotate90FlipNone);
                        case 4:
                            return GetTexture("rail_normal", data, null);
                        case 5:
                            return GetTexture("rail_normal", data, null);
                        case 6:
                            return GetTexture("rail_normal_turned", data, null);
                        case 7:
                            return GetTexture("rail_normal_turned", data, null, RotateFlipType.Rotate90FlipNone);
                        case 8:
                            return GetTexture("rail_normal_turned", data, null, RotateFlipType.Rotate180FlipNone);
                        case 9:
                            return GetTexture("rail_normal_turned", data, null, RotateFlipType.Rotate270FlipNone);
                    }

                    return null;

                case "golden_rail":
                    switch (data)
                    {
                        case 0:
                            return GetTexture("rail_golden_powered", data);
                        case 1:
                            return GetTexture("rail_golden_powered", data, null, RotateFlipType.Rotate90FlipNone);
                        case 2:
                            return GetTexture("rail_golden_powered", data, null, RotateFlipType.Rotate90FlipNone);
                        case 3:
                            return GetTexture("rail_golden_powered", data, null, RotateFlipType.Rotate90FlipNone);
                        case 4:
                            return GetTexture("rail_golden_powered", data, null);
                        case 5:
                            return GetTexture("rail_golden_powered", data, null);
                    }
                    return null;

                case "activator_rail":
                    switch (data)
                    {
                        case 0:
                            return GetTexture("rail_activator", data);
                        case 1:
                            return GetTexture("rail_activator", data, null, RotateFlipType.Rotate90FlipNone);
                        case 2:
                            return GetTexture("rail_activator", data, null, RotateFlipType.Rotate90FlipNone);
                        case 3:
                            return GetTexture("rail_activator", data, null, RotateFlipType.Rotate90FlipNone);
                        case 4:
                            return GetTexture("rail_activator", data, null);
                        case 5:
                            return GetTexture("rail_activator", data, null);
                    }
                    return null;


                case "detector_rail":
                    switch (data)
                    {
                        case 0:
                            return GetTexture("rail_detector", data);
                        case 1:
                            return GetTexture("rail_detector", data, null, RotateFlipType.Rotate90FlipNone);
                        case 2:
                            return GetTexture("rail_detector", data, null, RotateFlipType.Rotate90FlipNone);
                        case 3:
                            return GetTexture("rail_detector", data, null, RotateFlipType.Rotate90FlipNone);
                        case 4:
                            return GetTexture("rail_detector", data, null);
                        case 5:
                            return GetTexture("rail_detector", data, null);
                    }
                    return null;
                case "monster_egg":
                    return GetTexture("monster_egg", Math.Max(0,data-1));

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
                    switch ((BlockFace) data)
                    {
                        case BlockFace.Up:
                            return GetTexture("dispenser_front_vertical", 0);
                        default:
                            return GetTexture("dispenser_top", data);
                    }

                case "observer":
                    switch ((BlockFace)data)
                    {
                        case BlockFace.Down:
                            return GetTexture("observer_south", 0);
                        case BlockFace.Up:
                            return GetTexture("observer_north", 0);
                        case BlockFace.North:
                            return GetTexture("observer_top", 0, null, RotateFlipType.Rotate180FlipNone);
                        case BlockFace.South:
                            return GetTexture("observer_top", 0);
                        case BlockFace.West:
                            return GetTexture("observer_top", 0, null, RotateFlipType.Rotate90FlipNone);
                        case BlockFace.East:
                            return GetTexture("observer_top", 0, null, RotateFlipType.Rotate270FlipNone);
                    }

                    return GetTexture("observer_top", data);

                case "dropper":
                    switch ((BlockFace)data)
                    {
                        case BlockFace.Up:
                            return GetTexture("dropper_front_vertical", 0);
                        default:
                            return GetTexture("dropper_top", data);
                    }

                case "ender_chest":
                    return GetTexture("ender_chest_inventory_top", data);
                case "anvil":
                    return GetTexture("anvil_top_damaged_x", data);
                case "cactus":
                    return GetTexture("cactus_top", data);

                case "fence_gate":
                    return RenderFenceGate(data, "planks");
                case "spruce_fence_gate":
                    return RenderFenceGate(data, "spruce_planks");
                case "birch_fence_gate":
                    return RenderFenceGate(data, "birch_planks");
                case "acacia_fence_gate":
                    return RenderFenceGate(data, "acacia_planks");
                case "jungle_fence_gate":
                    return RenderFenceGate(data, "jungle_planks");
                case "dark_oak_fence_gate":
                    return RenderFenceGate(data, "dark_oak_planks");

                case "fence":
                    return GetTexture("planks", data, new TextureTranslation(
                        new Rectangle(5,5,6,6), 
                        new Rectangle(0,0,16,16)));
                case "podzol":
                    return GetTexture("dirt_podzol_top", data);
                case "grass":
                    return GetTexture("grass_carried_top", data);
                case "seagrass":
                    return GetTexture("seagrass_carried", data);
                case "tallgrass":
                    return GetTexture("tallgrass_carried", data);
                case "red_sandstone":
                    return GetTexture("redsandstone_top", data);
                case "sandstone":
                    return GetTexture("sandstone_top", data);
                case "grass_path":
                    return GetTexture("grass_path_top", data);
                case "lit_pumpkin":
                    return GetTexture("pumpkin_top", data);
                case "pumpkin":
                    return GetTexture("pumpkin_top", data);
                case "carved_pumpkin":
                    return GetTexture("pumpkin_top", data);
                case "torch":
                    return GetTexture("torch_on", data);
                case "nether_brick_stairs":
                    return GetTexture("nether_brick", data);

                case "crafting_table":
                    return GetTexture("crafting_table_top", data);
                case "brick_block":
                    return GetTexture("brick", data);

                    
                case "chest":
                    return GetTexture("chest_inventory_top", data);
                case "snow_layer":
                    return GetTexture("snow", data);

                case "flower_pot":
                    return GetTexture("flower_pot", data, new TextureTranslation(
                        new Rectangle(5, 5, 6, 6),
                        new Rectangle(5, 10, 6, 6)));

                case "conduit":
                    return GetTexture("conduit", data, 
                        new TextureTranslation(
                            new Rectangle(5,5,6,6), 
                            new Rectangle(12, 0, 6, 6)));

                case "kelp":
                    return GetTexture("kelp_top", data);
                case "dried_kelp_block":
                    return GetTexture("dried_kelp_block_top", data);
                case "stained_hardened_clay":
                    return GetTexture("stained_clay", data);
                case "sandstone_stairs":
                    return GetTexture("sandstone_top", data);

                case "wall_sign":
                case "darkoak_wall_sign":
                case "spruce_wall_sign":
                case "birch_wall_sign":
   
                case "frame":
                case "standing_sign":
                case "wooden_button":
                case "stone_button":
                    return null;


                case "end_portal_frame":
                    return GetTexture("endframe_top", 0);
              
                case "wooden_door":
                    return GetTexture("door_upper", 0);
                case "spruce_door":
                    return GetTexture("door_upper", 1);
                case "birch_door":
                    return GetTexture("door_upper", 2);
                case "jungle_door":
                    return GetTexture("door_upper", 3);
                case "acacia_door":
                    return GetTexture("door_upper", 4);
                case "dark_oak_door":
                    return GetTexture("door_upper", 5);
                case "iron_door":
                    return GetTexture("door_upper", 6);

                case "standing_banner":
                case "tripWire":
                case "spruce_button":
                case "dark_oak_button":
                case "jungle_standing_sign":
                case "tripwire_hook":
                case "dark_oak_pressure_plate":

                    return null;

                case "melon_block":
                    return GetTexture("melon_top", data);

                case "quartz_block":
                    return GetTexture("quartz_block_top", data);

                case "seaLantern":
                    return GetTexture("sea_lantern", data);

                case "purpur_block":
                    return GetTexture("purpur_block_top", data);

                case "normal_stone_stairs":
                    return GetTexture("stone", data);

                case "powered_comparator":
                    return GetTexture("comparator_up", data);
                case "unpowered_comparator":
                    return GetTexture("comparator_up", data);

                case "pistonArmCollision":
                case "piston":
                    return GetTexture("piston_top", data);
                case "jukebox":
                    return GetTexture("jukebox_top", data);
                case "stonecutter":
                    return GetTexture("stonecutter_top", data);
                case "loom":
                    return GetTexture("loom_top", data);

                case "redstone_lamp":
                    return GetTexture("redstone_lamp_off", data);
                case "lit_redstone_lamp":
                    return GetTexture("redstone_lamp_on", data);

                case "sticky_piston":
                    return GetTexture("piston_top_sticky", data);

                case "jungle_stairs":
                    return GetTexture("planks", data);

                case "undyed_shulker_box":
                    return GetTexture("undyed_shulker_box_top", data);


                case "double_stone_slab3":
                    return GetTexture("stone_slab_top_3", data);
                case "stone_slab":
                    return GetTexture("stone_slab_top", data);
                case "stone_slab2":
                    return GetTexture("stone_slab_top_2", data);
                case "stone_slab3":
                    return GetTexture("stone_slab_top_3", data);
                case "stone_slab4":
                    return GetTexture("stone_slab_top_4", data);

                case "stone_brick_stairs":
                    return GetTexture("stonebrick", 0);
                case "mossy_stone_brick_stairs":
                    return GetTexture("mossy_stone_brick", 0);

                case "double_stone_slab":
                    return GetTexture("stone_slab_top", data);

                case "stone_stairs":
                    return GetTexture("cobblestone", 0);

                case "mossy_cobblestone_stairs":
                    return GetTexture("cobblestone_mossy", 0);
                case "quartz_stairs":
                    return GetTexture("quartz_block_top", data);


                case "bone_block":
                    return GetTexture("bone_block_top", data);


                case "mycelium":
                    return GetTexture("mycelium_top", data);

                case "shulker_box":
                    return GetTexture("shulker_box_top", data);

                case "slime":
                    return GetTexture("slime_block", data);
                case "bamboo":
                    return GetTexture("bamboo_stem", data, new TextureTranslation(
                        new Rectangle(6,0,4,16), 
                        new Rectangle(0,0,4,16)
                        ));

                case "powered_repeater":
                    return GetTexture("repeater_up", data);

                case "unlit_redstone_torch":
                    return GetTexture("redstone_torch_off", data);

                case "lit_furnace":
                    return GetTexture("redstone_torch_off", data);
           
                case "concrete":
                    return GetTexture("concrete", data);

                case "trapped_chest":
                    return GetTexture("chest_inventory_top", data);

                case "polished_andesite_stairs":
                    return GetTexture("polished_andesite", data);
                case "polished_granite_stairs":
                    return GetTexture("polished_granite", data);

                /* LEAVES */
                case "leaves":
                    return GetTexture("leaves_carried", data & 0xF7);
                case "leaves2":
                    return GetTexture("leaves_carried2", data & 0xF7);


                /* WOOD */
                case "wood":
                    return GetTexture("wood", (data & 0xFFF7)*2 + ((data & 0x8)/8));

                case "stripped_jungle_log":
                    return GetTexture((data & 2) == 0 ? "stripped_jungle_log_top" : "stripped_jungle_log_side", data);
                case "stripped_spruce_log":
                    return GetTexture((data & 2) == 0 ? "stripped_spruce_log_top" : "stripped_spruce_log_side", data);
                case "stripped_birch_log":
                    return GetTexture((data & 2) == 0 ? "stripped_birch_log_top" : "stripped_birch_log_side", data);
                case "stripped_dark_oak_log":
                    return GetTexture((data & 2) == 0 ? "stripped_dark_oak_log_top" : "stripped_dark_oak_log_side", data);
                case "stripped_oak_log":
                    return GetTexture((data & 2) == 0 ? "stripped_oak_log_top" : "stripped_oak_log_side", data);

                case "enchanting_table":
                    return GetTexture("enchanting_table_top", data);
                case "log":
                    return GetTexture((data & 8) == 0 ? "log_top" : "log_side", data & 3);
                case "log2":
                    return GetTexture((data & 8) == 0 ? "log_top2" : "log_side2", data & 3);

                case "coral_fan_hang":
                    return GetTexture("coral_fan_hang_a", data);
                case "scaffolding":
                    return GetTexture("scaffolding_top", data);

                    break;
            }

            return null;
        }

        private TextureStack RenderFenceGate(long data, string texture)
        {
            switch (data)
            {
                case 0:
                case 2:
                    return GetTexture(texture, 0).Translate(new Rectangle(0, 6, 16, 4));
                case 1:
                case 3:
                    return GetTexture(texture, 0).Translate(new Rectangle(0, 6, 16, 4)).Rotate(RotateFlipType.Rotate90FlipNone);

                case 4:
                    return GetTexture(texture, 0)
                               .Translate(
                                   new Rectangle(0, 6, 10, 4),
                                   new Rectangle(6, 0, 10, 4))
                               .Rotate(RotateFlipType.Rotate90FlipNone)

                           + GetTexture(texture, 0)
                               .Translate(
                                   new Rectangle(0, 6, 10, 4),
                                   new Rectangle(6, 12, 10, 4))
                               .Rotate(RotateFlipType.Rotate90FlipNone);
                case 6:
                    return GetTexture(texture, 0)
                               .Translate(
                                   new Rectangle(0, 6, 10, 4),
                                   new Rectangle(6, 0, 10, 4))
                               .Rotate(RotateFlipType.Rotate270FlipNone)

                           + GetTexture(texture, 0)
                               .Translate(
                                   new Rectangle(0, 6, 10, 4),
                                   new Rectangle(6, 12, 10, 4))
                               .Rotate(RotateFlipType.Rotate270FlipNone);
                case 5:
                    return GetTexture(texture, 0)
                               .Translate(
                                new Rectangle(0, 6, 10, 4),
                                new Rectangle(6, 0, 10, 4))
                               .Rotate(RotateFlipType.Rotate180FlipNone)

                            + GetTexture(texture, 0)
                                .Translate(
                                new Rectangle(0, 6, 10, 4),
                                new Rectangle(6, 12, 10, 4))
                                .Rotate(RotateFlipType.Rotate180FlipNone);
                        
                case 7:
                    return GetTexture(texture, 0).Translate(
                               new Rectangle(0, 6, 10, 4),
                               new Rectangle(6, 0, 10, 4))
                           + GetTexture(texture, 0).Translate(
                               new Rectangle(0, 6, 10, 4),
                               new Rectangle(6, 12, 10, 4));

            }

            return null;
        }

        public Dictionary<TextureInfo, Bitmap> Cache { get; } = new Dictionary<TextureInfo, Bitmap>();

        /// <summary>
        /// Not thread safe
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public Bitmap GetTextureImage(TextureInfo info)
        {
            if (info == null)
                return null;
            var localPath = info.Filename;

            localPath = localPath.Replace("textures/", "");
            localPath = localPath.Replace("/", "\\");

            if (Cache.ContainsKey(info))
            {
                return Cache[info];
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

                if (info.Translation != null)
                {
                    var bnew = new Bitmap(16,16);
                    using (var gnew = Graphics.FromImage(bnew))
                    {
                        gnew.DrawImage(b, info.Translation.Dest, info.Translation.Source, GraphicsUnit.Pixel);
                    }

                    b = bnew;
                }

                if (info.Rotation != RotateFlipType.RotateNoneFlipNone)
                {
                    b.RotateFlip(info.Rotation);
                }
                
                if (!Cache.ContainsKey(info))
                {
                    Cache.Add(info, b);
                }

                return b;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private TextureStack CreateTexture(string texturePath)
        {
            return new TextureStack(texturePath, null, RotateFlipType.RotateNoneFlipNone);
        }

        private TextureStack GetTexture(string name, long data, TextureTranslation translation = null, RotateFlipType rot = RotateFlipType.RotateNoneFlipNone)
        {
            string texturePath = null;
            if (texturesJson.ContainsKey(name))
            {
                var texture = texturesJson[name];

                if (texture.Subtextures.Count <= data || data < 0)
                {
                    //Console.WriteLine("Index out of bounds during GetTexture for {0}", name);
                    texturePath = texture.Subtextures.First().Path;
                }
                else
                {
                        texturePath = texture.Subtextures[(int) data].Path;
                }
            }

            if (texturePath == null)
                return null;
            return new TextureStack(texturePath, translation, rot);
        }
    }
}