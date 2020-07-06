using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DmitryBrant.ImageFormats;
using Maploader.Renderer.Imaging;

namespace Maploader.Renderer.Texture
{
    public class TextureFinder<TImage> where TImage : class
    {
        /// <summary>
        /// Returns if a texture is transparent
        /// </summary>
        public Dictionary<string, bool> TransparentBlocks { get; } = new Dictionary<string, bool>()
        {
            {"minecraft:tallgrass", true},
            {"minecraft:waterlily", true},
            {"minecraft:torch", true},
            {"minecraft:lantern", true},
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
            {"minecraft:red_mushroom", true },
            {"minecraft:brown_mushroom", true },
            {"minecraft:fire", true },
            {"minecraft:pumpkin_stem", true },
            {"minecraft:melon_stem", true },
            {"minecraft:carrots", true },
            {"minecraft:potatoes", true },
            {"minecraft:anvil", true },
            {"minecraft:cauldron", true },
            {"minecraft:brewing_stand", true },
            {"minecraft:frame", true },
            {"minecraft:deadbush", true },
            {"minecraft:sweet_berry_bush", true },
            {"minecraft:web", true },
            {"minecraft:conduit", true },
            {"minecraft:flower_pot", true },
            {"minecraft:bamboo", true },
            {"minecraft:leaves", true },
            {"minecraft:leaves2", true },

            {"minecraft:wooden_door", true },
            {"minecraft:spruce_door", true },
            {"minecraft:birch_door", true },
            {"minecraft:jungle_door", true },
            {"minecraft:acacia_door", true },
            {"minecraft:dark_oak_door", true },
            {"minecraft:iron_door", true },

            {"minecraft:trapdoor", true },
            {"minecraft:spruce_trapdoor", true },
            {"minecraft:birch_trapdoor", true },
            {"minecraft:jungle_trapdoor", true },
            {"minecraft:acacia_trapdoor", true },
            {"minecraft:dark_oak_trapdoor", true },
            {"minecraft:iron_trapdoor", true },

            {"minecraft:standing_sign", true},
            {"minecraft:spruce_standing_sign", true},
            {"minecraft:birch_standing_sign", true},
            {"minecraft:jungle_standing_sign", true},
            {"minecraft:acacia_standing_sign", true},
            {"minecraft:darkoak_standing_sign", true},
            {"minecraft:birch_pressure_plate", true},
            {"minecraft:jungle_pressure_plate", true},
            {"minecraft:acacia_pressure_plate", true},
            {"minecraft:dark_oak_pressure_plate", true},
            {"minecraft:oak_pressure_plate", true},
            {"minecraft:stone_pressure_plate", true},
            {"minecraft:spruce_pressure_plate", true},
            {"minecraft:wooden_pressure_plate", true},
            {"minecraft:heavy_weighted_pressure_plate", true},
            {"minecraft:light_weighted_pressure_plate", true},
            {"minecraft:cactus", true},
            {"minecraft:beetroot", true},
            {"minecraft:bell", true},
            {"minecraft:standing_banner", true},

            {"minecraft:wooden_button", true},
            {"minecraft:spruce_button", true},
            {"minecraft:birch_button", true},
            {"minecraft:jungle_button", true},
            {"minecraft:acacia_button", true},
            {"minecraft:dark_oak_button", true},

            {"minecraft:spruce_wall_sign", true},
            {"minecraft:birch_wall_sign", true},
            {"minecraft:jungle_wall_sign", true},
            {"minecraft:acacia_wall_sign", true},
            {"minecraft:darkoak_wall_sign", true},
            {"minecraft:turtle_egg", true},
            {"minecraft:cake", true},
            {"minecraft:scaffolding", true},
            {"minecraft:mob_spawner", true},
            {"minecraft:slime", true},
            {"minecraft:reeds", true},

            // Nether
            {"minecraft:nether_wart", true},
            {"minecraft:soul_fire", true},
            {"minecraft:soul_torch", true},
            {"minecraft:soul_lantern", true},
            {"minecraft:crimson_fungus", true},
            {"minecraft:warped_fungus", true},
            {"minecraft:weeping_vines_base", true},
            {"minecraft:weeping_vines_bottom", true},
            {"minecraft:twisting_vines_base", true},
            {"minecraft:twisting_vines_bottom", true},
            {"minecraft:crimson_roots", true},
            {"minecraft:warped_roots", true},
            {"minecraft:crimson_roots_pot", true},
            {"minecraft:warped_roots_pot", true},
            {"minecraft:nether_sprouts", true},
            {"minecraft:chain1", true},
            {"minecraft:chain2", true},
            {"minecraft:warped_door", true},
            {"minecraft:crimson_door", true},
            {"minecraft:warped_wall_sign", true},
            {"minecraft:crimson_wall_sign", true},
        };

        private readonly Dictionary<string, Texture> texturesJson;
        private readonly string path;
        private readonly IGraphicsApi<TImage> graphics;

        public TextureFinder(Dictionary<string, Texture> texturesJson, string path, IGraphicsApi<TImage> graphics)
        {
            this.texturesJson = texturesJson;
            this.path = path;
            this.graphics = graphics;
        }

        public TextureStack FindTexturePath(string name, long data, int x, int z, int y)
        {
            name = name.Replace("minecraft:", "");

            var newTexture = GetSubstitution(name, data, x, z, y);

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



        private TextureStack GetSubstitution(string name, long data, int x, int z, int y)
        {
            // For debugging purposes

            if (Debug)
            {
                Console.WriteLine($"{x} {z} {y}: {name},{data}");
            }


            // oh look at all this mess...
            switch (name)
            {
                case "cobblestone_wall":
                    return GetTexture("cobblestone_wall", data).Translate(5, 5, 6, 6);

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
                case "cake":
                    return GetTexture("cake_top",0);
                case "bed":
                    switch (data & 0xF7)
                    {
                        case 0:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlip.Rotate90FlipNone);
                        case 1:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlip.Rotate180FlipNone);
                        case 2:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlip.Rotate270FlipNone);
                        case 3:
                            return CreateTexture((data & 8) == 8
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                                .Rotate(RotateFlip.RotateNoneFlipNone);
                    }

                    return null;


                case "wooden_slab":
                    return GetTexture("planks", data);
                case "double_wooden_slab":
                    return GetTexture("planks", data);

                case "prismarine_bricks_stairs":
                    return GetTexture("prismarine_bricks", 0);

                case "oak_stairs":
                    return GetTexture("planks", 0); // data = direction
                case "brick_stairs":
                    return GetTexture("brick", 0);
                case "dark_prismarine_stairs":
                    return GetTexture("dark_prismarine", 0); // data = direction
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
                case "nether_brick_stairs":
                    return GetTexture("nether_brick", data);
                case "sandstone_stairs":
                    return GetTexture("sandstone_top", data);
                case "normal_stone_stairs":
                    return GetTexture("stone", data);
                case "jungle_stairs":
                    return GetTexture("planks", data);
                case "stone_brick_stairs":
                    return GetTexture("stonebrick", 0);
                case "stone_stairs":
                    return GetTexture("cobblestone", 0);
                case "mossy_cobblestone_stairs":
                    return GetTexture("cobblestone_mossy", 0);
                case "quartz_stairs":
                    return GetTexture("quartz_block_top", data);
                case "mossy_stone_brick_stairs":
                    return GetTexture("mossy_stone_brick", 0);
                case "smooth_sandstone_stairs":
                    return GetTexture("smooth_sandstone", 0);

                case "red_sandstone_stairs":
                    return GetTexture("smooth_red_sandstone", 0);

                case "purpur_stairs":
                    return GetTexture("purpur_block_top", 0);

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
                    switch (data & 0x8)
                    {
                        case 8:
                            return GetTexture("double_plant_carried", data & 0xF7);
                        case 0:
                            return GetTexture("double_plant_bottom", data & 0xF7);
                    }

                    return null;

                case "tnt":
                    return GetTexture("tnt_top", 0);
                case "rail":
                    switch (data)
                    {
                        case 0:
                            return GetTexture("rail_normal", data);
                        case 1:
                            return GetTexture("rail_normal", data).Rotate(RotateFlip.Rotate90FlipNone);
                        case 2:
                            return GetTexture("rail_normal", data).Rotate(RotateFlip.Rotate90FlipNone);
                        case 3:
                            return GetTexture("rail_normal", data).Rotate(RotateFlip.Rotate90FlipNone);
                        case 4:
                            return GetTexture("rail_normal", data);
                        case 5:
                            return GetTexture("rail_normal", data);
                        case 6:
                            return GetTexture("rail_normal_turned", data);
                        case 7:
                            return GetTexture("rail_normal_turned", data).Rotate(RotateFlip.Rotate90FlipNone);
                        case 8:
                            return GetTexture("rail_normal_turned", data).Rotate(RotateFlip.Rotate180FlipNone);
                        case 9:
                            return GetTexture("rail_normal_turned", data).Rotate(RotateFlip.Rotate270FlipNone);
                    }

                    return null;

                case "golden_rail":
                    if ((data & 8) == 8)
                        return RenderRail(data & 0xF7, "rail_golden_powered");
                    else
                        return RenderRail(data, "rail_golden");

                case "activator_rail":
                    if ((data & 8) == 8)
                        return RenderRail(data & 0xF7, "rail_activator");
                    else
                        return RenderRail(data, "rail_activator");



                case "detector_rail":
                    if ((data & 8) == 8)
                        return RenderRail(data & 0xF7, "rail_detector_powered");
                    else
                        return RenderRail(data, "rail_detector");

                case "monster_egg":
                    return GetTexture("monster_egg", Math.Max(0, data - 1));

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
                case "lit_redstone_ore":
                    return GetTexture("redstone_ore", data);
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
                    switch ((BlockFace) data)
                    {
                        case BlockFace.Down:
                            return GetTexture("observer_south", 0);
                        case BlockFace.Up:
                            return GetTexture("observer_north", 0);
                        case BlockFace.North:
                            return GetTexture("observer_top", 0, null, RotateFlip.Rotate180FlipNone);
                        case BlockFace.South:
                            return GetTexture("observer_top", 0);
                        case BlockFace.West:
                            return GetTexture("observer_top", 0, null, RotateFlip.Rotate90FlipNone);
                        case BlockFace.East:
                            return GetTexture("observer_top", 0, null, RotateFlip.Rotate270FlipNone);
                    }

                    return GetTexture("observer_top", data);

                case "dropper":
                    switch ((BlockFace) data)
                    {
                        case BlockFace.Up:
                            return GetTexture("dropper_front_vertical", 0);
                        default:
                            return GetTexture("dropper_top", data);
                    }

                case "smoker":
                    return GetTexture("smoker_top", 0);
                case "barrel":
                    switch ((BlockFace)data)
                    {
                        case BlockFace.Up:
                            return GetTexture("barrel_top", 0);
                        case BlockFace.Down:
                            return GetTexture("barrel_bottom", 0);
                        default:
                            return GetTexture("barrel_side", data);
                    }
                case "bell":
                    return GetTexture("bell_top", 0).Translate(
                        new Rect(0, 0, 8, 8),
                        new Rect(4, 4, 8, 8));
                case "composter":
                    return GetTexture("composter_bottom", 0);
                case "campfire":
                    return GetTexture("campfire_log_lit", 0).Translate(0, 0, 16, 16);
                case "ender_chest":
                    return GetTexture("ender_chest_inventory_top", data);
                case "anvil":
                    return GetTexture("anvil_top_damaged_x", data);
                case "cactus":
                    return GetTexture("cactus_top", data).Translate(1, 1, 14, 14);

                case "dark_oak_pressure_plate":
                    return GetTexture("dark_oak_planks", 0).Translate(1, 1, 14, 14);
                case "birch_pressure_plate":
                    return GetTexture("birch_planks", 0).Translate(1, 1, 14, 14);
                case "jungle_pressure_plate":
                    return GetTexture("jungle_planks", 0).Translate(1, 1, 14, 14);
                case "acacia_pressure_plate":
                    return GetTexture("acacia_planks", 0).Translate(1, 1, 14, 14);
                case "wooden_pressure_plate":
                    return GetTexture("planks", data).Translate(1, 1, 14, 14);
                case "spruce_pressure_plate":
                    return GetTexture("spruce_planks", data).Translate(1, 1, 14, 14);
                case "light_weighted_pressure_plate":
                    return GetTexture("gold_block", data).Translate(1, 1, 14, 14);
                case "heavy_weighted_pressure_plate":
                    return GetTexture("iron_block", data).Translate(1, 1, 14, 14);
                case "stone_pressure_plate":
                    return GetTexture("stone", data).Translate(1, 1, 14, 14);
                    ;

                case "frame":
                    return RenderFrame(data, "sign");

                case "standing_sign":
                    return RenderSign(data, "sign");
                case "spruce_standing_sign":
                    return RenderSign(data, "spruce_sign");
                case "birch_standing_sign":
                    return RenderSign(data, "birch_sign");
                case "jungle_standing_sign":
                    return RenderSign(data, "acacia_sign");
                case "acacia_standing_sign":
                    return RenderSign(data, "jungle_sign");
                case "darkoak_standing_sign":
                    return RenderSign(data, "darkoak_sign");

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

                case "nether_brick_fence":
                    return GetTexture("nether_brick", 0, new TextureTranslation(
                        new Rect(5, 5, 6, 6),
                        new Rect(0, 0, 16, 16)));
                case "fence":
                    return GetTexture("planks", data, new TextureTranslation(
                        new Rect(5, 5, 6, 6),
                        new Rect(0, 0, 16, 16)));
                case "podzol":
                    return GetTexture("dirt_podzol_top", data);
                case "grass":
                    return GetTexture("grass_carried_top", data);
                case "seagrass":
                    return GetTexture("seagrass_carried", data);
                case "tallgrass":
                    return GetTexture("tallgrass_carried", data);

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
                        new Rect(5, 5, 6, 6),
                        new Rect(5, 10, 6, 6)));

                case "conduit":
                    return GetTexture("conduit", data,
                        new TextureTranslation(
                            new Rect(5, 5, 6, 6),
                            new Rect(12, 0, 6, 6)));

                case "kelp":
                    return GetTexture("kelp_top", data);
                case "dried_kelp_block":
                    return GetTexture("dried_kelp_block_top", data);
                case "stained_hardened_clay":
                    return GetTexture("stained_clay", data);




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

                case "spruce_button":
                    return RenderButton(data, "spruce_planks");
                case "wooden_button":
                    return RenderButton(data, "planks");
                case "stone_button":
                    return RenderButton(data, "stone");
                case "dark_oak_button":
                    return RenderButton(data, "dark_oak_planks");
                case "acacia_button":
                    return RenderButton(data, "acacia_planks");
                case "birch_button":
                    return RenderButton(data, "birch_planks");
                case "jungle_button":
                    return RenderButton(data, "jungle_planks");
                case "wall_banner":
                case "standing_banner":
                    return RenderSign(data, "sign");
                case "tripWire":
                case "tripwire_hook":
                    return null;

                case "wall_sign":
                case "darkoak_wall_sign":
                case "spruce_wall_sign":
                case "birch_wall_sign":
                case "jungle_wall_sign":
                case "acacia_wall_sign":
                    return RenderWallSign(data, name.Replace("wall_", ""));

                case "melon_block":
                    return GetTexture("melon_top", data);

                case "quartz_block":
                    return GetTexture("quartz_block_top", data);

                case "seaLantern":
                    return GetTexture("sea_lantern", data);

                case "purpur_block":
                    return GetTexture("purpur_block_side", data);

                case "turtle_egg":
                        return GetTexture("turtle_egg", 0).Translate(
                            new Rect(0, 0, 4, 4),
                            new Rect(6, 6, 4, 4));


                case "powered_comparator":
                    return GetTexture("comparator_up", data);
                case "unpowered_comparator":
                    return GetTexture("comparator_up", data);

                case "pistonArmCollision":
                case "piston":
                    return GetTexture("piston_top", data);
                case "jukebox":
                    return GetTexture("jukebox_top", data);
                case "stonecutter_block":
                case "stonecutter":
                    return GetTexture("stonecutter_top", data);
                case "loom":
                    return GetTexture("loom_top", data);
                case "smithing_table":
                    return GetTexture("smithing_table_top", data);
                case "cartography_table":
                    return GetTexture("cartography_table_top", data);
                case "fletching_table":
                    return GetTexture("fletching_table_top", data);
                case "redstone_lamp":
                    return GetTexture("redstone_lamp_off", data);
                case "lit_redstone_lamp":
                    return GetTexture("redstone_lamp_on", data);

                case "sticky_piston":
                    return GetTexture("piston_top_sticky", data);


                case "undyed_shulker_box":
                    return GetTexture("undyed_shulker_box_top", data);

                case "red_sandstone":
                    return GetTexture("redsandstone_top", data);
                case "sandstone":
                    return GetTexture("sandstone_top", data);

                case "stone_slab":
                    return GetTexture("stone_slab_top", data);

                case "stone_slab2":
                    return GetTexture("stone_slab_top_2", data);
                case "double_stone_slab2":
                    return GetTexture("stone_slab_top_2", data);
                case "stone_slab3":

                    return GetTexture("stone_slab_top_3", data);
                case "double_stone_slab3":
                    return GetTexture("stone_slab_top_3", data);

                case "stone_slab4":
                    return GetTexture("stone_slab_top_4", data);
                case "double_stone_slab4":
                    return GetTexture("stone_slab_top_4", data);


                case "double_stone_slab":
                    return GetTexture("stone_slab_top", data);


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
                        new Rect(6, 0, 4, 16),
                        new Rect(0, 0, 4, 16)
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
                case "polished_diorite_stairs":
                    return GetTexture("polished_diorite", data);

                case "polished_andesite_stairs":
                    return GetTexture("polished_andesite", data);
                case "polished_granite_stairs":
                    return GetTexture("polished_granite", data);
                case "diorite_stairs":
                    return GetTexture("diorite", data);
                case "granite_stairs":
                    return GetTexture("granite", data);
                /* LEAVES */
                case "leaves":
                    return GetTexture("leaves_carried", data & 0xF7);
                case "leaves2":
                    return GetTexture("leaves_carried2", data & 0xF7);


                /* WOOD */
                case "wood":
                    return GetTexture("wood", (data & 0xFFF7) * 2 + ((data & 0x8) / 8));

                case "stripped_jungle_log":
                    return GetTexture((data & 2) == 0 ? "stripped_jungle_log_top" : "stripped_jungle_log_side", data);
                case "stripped_spruce_log":
                    return GetTexture((data & 2) == 0 ? "stripped_spruce_log_top" : "stripped_spruce_log_side", data);
                case "stripped_birch_log":
                    return GetTexture((data & 2) == 0 ? "stripped_birch_log_top" : "stripped_birch_log_side", data);
                case "stripped_dark_oak_log":
                    return GetTexture((data & 2) == 0 ? "stripped_dark_oak_log_top" : "stripped_dark_oak_log_side",
                        data);
                case "stripped_oak_log":
                    return GetTexture((data & 2) == 0 ? "stripped_oak_log_top" : "stripped_oak_log_side", data);
                case "stripped_acacia_log":
                    return GetTexture((data & 2) == 0 ? "stripped_acacia_log_top" : "stripped_acacia_log_side", data);

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
                case "grindstone":
                    return GetTexture("grindstone_pivot", data);
                case "sweet_berry_bush":
                    return GetTexture($"sweet_berry_bush_{data%4}", 0);
                case "bee_nest":
                    return GetTexture("bee_nest_top", data);

                // Nether update (1.16)
                case "crimson_nylium":
                    return GetTexture("crimson_nylium_top", data);
                case "warped_nylium":
                    return GetTexture("warped_nylium_top", data);
                case "crimson_stem":
                    return GetTexture("stripped_crimson_stem_top", data);
                case "warped_stem":
                    return GetTexture("stripped_warped_stem_top", data);
                case "warped_fungus":
                    return GetTexture("nether_shroom_blue", data);
                case "crimson_fungus":
                    return GetTexture("nether_shroom_red", data);
                case "twisting_vines":
                    return GetTexture("twisting_vines_base", data);
                case "soul_fire":
                    return GetTexture("soul_fire_0", data);

                case "basalt":
                    return GetTexture("basalt_top", data);
                case "polished_basalt":
                    return GetTexture("polished_basalt_top", data);

                case "blackstone":
                    return GetTexture("blackstone_top", data);
                case "blackstone_slab":
                    return GetTexture("blackstone_top", data);
                case "blackstone_stairs":
                    return GetTexture("blackstone_top", data);
                case "blackstone_wall":
                    return GetTexture("blackstone_top", data);

                case "polished_blackstone_slab":
                    return GetTexture("polished_blackstone", data);
                case "polished_blackstone_stairs":
                    return GetTexture("polished_blackstone", data);
                case "polished_blackstone_wall":
                    return GetTexture("polished_blackstone", data);

                case "polished_blackstone_brick_slab":
                    return GetTexture("polished_blackstone_bricks", data);
                case "polished_blackstone_brick_stairs":
                    return GetTexture("polished_blackstone_bricks", data);
                case "polished_blackstone_brick_wall":
                    return GetTexture("polished_blackstone_bricks", data);

                case "warped_door":
                    return GetTexture("warped_door_top", data);
                case "crimson_door":
                    return GetTexture("crimson_door_top", data);
                case "warped_wall_sign":
                    return RenderWallSign(data, name.Replace("wall_", ""));
                case "crimson_wall_sign":
                    return RenderWallSign(data, name.Replace("wall_", ""));
            }

            return null;
        }

        private TextureStack RenderWallSign(long data, string texture)
        {
            var t = GetTexture(texture, 0).Translate(
                new Rect(0, 7, 14, 2),
                new Rect(1, 0, 14, 2)
            );
            switch (data)
            {
                case 0:
                    return t.Rotate(RotateFlip.Rotate270FlipNone);
                case 1:
                    return t.Rotate(RotateFlip.Rotate90FlipNone);
                case 2:
                    return t.Rotate(RotateFlip.Rotate180FlipNone);
                case 3:
                    return t.Rotate(RotateFlip.RotateNoneFlipNone);
                case 4:
                    return t.Rotate(RotateFlip.Rotate90FlipNone);
                case 5:
                    return t.Rotate(RotateFlip.Rotate270FlipNone);
            }

            return null;
        }


        private TextureStack RenderRail(long data, string texture)
        {
            switch (data)
            {
                case 0:
                    return GetTexture(texture, data);
                case 1:
                    return GetTexture(texture, data).Rotate(RotateFlip.Rotate90FlipNone);
                case 2:
                    return GetTexture(texture, data).Rotate(RotateFlip.Rotate90FlipNone);
                case 3:
                    return GetTexture(texture, data).Rotate(RotateFlip.Rotate90FlipNone);
                case 4:
                    return GetTexture(texture, data);
                case 5:
                    return GetTexture(texture, data);
            }

            return null;
        }

        private TextureStack RenderSign(long data, string texture)
        {
            return GetTexture(texture, 0).Translate(
                new Rect(0, 7, 14, 2),
                new Rect(1, 7, 14, 2)
            );
        }

        private TextureStack RenderFrame(long data, string texture)
        {
            var t = GetTexture(texture, 0).Translate(
                new Rect(0, 7, 14, 2),
                new Rect(1, 0, 14, 2)
            );
            switch (data)
            {
                case 0:
                    return t.Rotate(RotateFlip.Rotate270FlipNone);
                case 1:
                    return t.Rotate(RotateFlip.Rotate90FlipNone);
                case 3:
                    return t.Rotate(RotateFlip.Rotate180FlipNone);
                case 2:
                    return t.Rotate(RotateFlip.RotateNoneFlipNone);
            }

            return t;
        }

        private TextureStack RenderButton(long data, string texture)
        {
            var t = GetTexture(texture, 0).Translate(
                new Rect(6, 6, 4, 3),
                new Rect(6, 0, 4, 3)
            );
            if ((data & 8) == 8)
            {
                // Per https://minecraft.gamepedia.com/Button : 0x8	If this bit is set, the button is currently active
                //  Active/Unactive, on the scale rendering, doesn't matter, so remove that bit
                data = data ^ 8;
            }
            switch (data)
            {
                
                case 1:
                    return t.Translate(
                        new Rect(6, 6, 4, 4),
                        new Rect(6, 6, 4, 4));
                case 2:
                    return t.Rotate(RotateFlip.Rotate180FlipNone);
              
                case 3:
                    return t.Rotate(RotateFlip.RotateNoneFlipNone);
                case 4:
                    return t.Rotate(RotateFlip.Rotate90FlipNone);
                case 0: // 0: Button on block bottom facing down. Assuming bottom = top
                case 5: // 5: Button on block top facing up
                    return t.Rotate(RotateFlip.Rotate270FlipNone);
                default:
                    return null;
            }
        }

        private TextureStack RenderFenceGate(long data, string texture)
        {
            if ((data & 8) == 8)
            {
                // Per https://minecraft.gamepedia.com/Fence_Gate : 0x8	If 1, the gate is lowered by three pixels, to accommodate attaching more cleanly with normal and mossy Cobblestone Walls
                //  3 pixels, on the scale rendering, doesn't matter, so remove that bit
                data = data ^ 8;
            }
            switch (data)
            {
                case 0:
                case 2:
                    return GetTexture(texture, 0).Translate(new Rect(0, 6, 16, 4));
                case 1:
                case 3:
                    return GetTexture(texture, 0).Translate(new Rect(0, 6, 16, 4)).Rotate(RotateFlip.Rotate90FlipNone);

                case 4:
                    return GetTexture(texture, 0)
                               .Translate(
                                   new Rect(0, 6, 10, 4),
                                   new Rect(6, 0, 10, 4))
                               .Rotate(RotateFlip.Rotate90FlipNone)

                           + GetTexture(texture, 0)
                               .Translate(
                                   new Rect(0, 6, 10, 4),
                                   new Rect(6, 12, 10, 4))
                               .Rotate(RotateFlip.Rotate90FlipNone);
                case 6:
                    return GetTexture(texture, 0)
                               .Translate(
                                   new Rect(0, 6, 10, 4),
                                   new Rect(6, 0, 10, 4))
                               .Rotate(RotateFlip.Rotate270FlipNone)

                           + GetTexture(texture, 0)
                               .Translate(
                                   new Rect(0, 6, 10, 4),
                                   new Rect(6, 12, 10, 4))
                               .Rotate(RotateFlip.Rotate270FlipNone);
                case 5:
                    return GetTexture(texture, 0)
                               .Translate(
                                new Rect(0, 6, 10, 4),
                                new Rect(6, 0, 10, 4))
                               .Rotate(RotateFlip.Rotate180FlipNone)

                            + GetTexture(texture, 0)
                                .Translate(
                                new Rect(0, 6, 10, 4),
                                new Rect(6, 12, 10, 4))
                                .Rotate(RotateFlip.Rotate180FlipNone);
                        
                case 7:
                    return GetTexture(texture, 0).Translate(
                               new Rect(0, 6, 10, 4),
                               new Rect(6, 0, 10, 4))
                           + GetTexture(texture, 0).Translate(
                               new Rect(0, 6, 10, 4),
                               new Rect(6, 12, 10, 4));

            }

            return null;
        }

        public Dictionary<TextureInfo, TImage> Cache { get; } = new Dictionary<TextureInfo, TImage>();
        public bool Debug { get; set; }

        /// <summary>
        /// Not thread safe
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public TImage GetTextureImage(TextureInfo info)
        {
            if (info == null)
                return null;
            var localPath = info.Filename;

            localPath = localPath.Replace("textures/", "");
            localPath = localPath.Replace('/', Path.DirectorySeparatorChar);

            if (Cache.ContainsKey(info))
            {
                return Cache[info];
            }

            try
            {
                string filepath = Path.Combine(path, localPath);
                TImage b = null;
                if (File.Exists(filepath + ".png"))
                {
                    b = graphics.LoadImage(filepath + ".png");
                }
                else if (File.Exists(filepath + ".tga"))
                {
                    b = graphics.LoadImage(filepath + ".tga");
                }

                if (b == null)
                {
                    Console.WriteLine("Could not find {0}", filepath);
                }

                if (info.Translation != null)
                {
                    var bnew = graphics.CreateEmptyImage(16, 16);
                    graphics.DrawImage(bnew, b, info.Translation.Dest, info.Translation.Source);
                    b = bnew;
                }

                if (info.Rotation != RotateFlip.RotateNoneFlipNone)
                {
                    graphics.RotateFlip(b, info.Rotation);
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
            return new TextureStack(texturePath, null, RotateFlip.RotateNoneFlipNone);
        }

        private TextureStack GetTexture(string name, long data, TextureTranslation translation = null, RotateFlip rot = RotateFlip.RotateNoneFlipNone)
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