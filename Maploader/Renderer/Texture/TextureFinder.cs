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
            {"minecraft:underwater_torch", true},
            {"minecraft:colored_torch_bp", true},
            {"minecraft:colored_torch_rg", true},
            {"minecraft:lantern", true},
            {"minecraft:lever", true},
            {"minecraft:redstone_torch", true},
            {"minecraft:tripWire", true},
            {"minecraft:tripwire_hook", true},
            {"minecraft:sapling", true},
            {"minecraft:bamboo_sapling", true},
            {"minecraft:double_plant", true },
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
            {"minecraft:hard_glass", true },
            {"minecraft:hard_stained_glass", true },
            {"minecraft:glass_pane", true },
            {"minecraft:stained_glass_pane", true },
            {"minecraft:hard_glass_pane", true },
            {"minecraft:hard_stained_glass_pane", true },
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
            {"minecraft:chain", true},
            {"minecraft:crimson_button", true},
            {"minecraft:crimson_fence", true},
            {"minecraft:crimson_fence_gate", true},
            {"minecraft:crimson_pressure_plate", true},
            {"minecraft:crimson_standing_sign", true},
            {"minecraft:warped_button", true},
            {"minecraft:warped_fence", true},
            {"minecraft:warped_fence_gate", true},
            {"minecraft:warped_pressure_plate", true},
            {"minecraft:warped_standing_sign", true},
            {"minecraft:weeping_vines", true},
            {"minecraft:warped_trapdoor", true},
            {"minecraft:crimson_trapdoor", true},

            // Fixes
            {"minecraft:chorus_plant", true},
            {"minecraft:cocoa", true},
            {"minecraft:polished_blackstone_button", true},
            {"minecraft:blackstone_wall", true},
            {"minecraft:polished_blackstone_wall", true},
            {"minecraft:polished_blackstone_brick_wall", true},
            {"minecraft:nether_brick_fence", true},
            {"minecraft:coral", true},
            {"minecraft:coral_fan", true},
            {"minecraft:coral_fan_dead", true},
            {"minecraft:coral_fan_hang", true},
            {"minecraft:coral_fan_hang2", true},
            {"minecraft:coral_fan_hang3", true},
            {"minecraft:sea_pickle", true},
            {"minecraft:wither_rose", true},

            // Caves & Cliffs Update: Part 1
            {"minecraft:lightning_rod", true},
            {"minecraft:small_dripleaf_block", true},
            {"minecraft:big_dripleaf", true},
            {"minecraft:pointed_dripstone", true},
            {"minecraft:hanging_roots", true},
            {"minecraft:amethyst_cluster", true},
            {"minecraft:budding_amethyst", true},
            {"minecraft:large_amethyst_bud", true},
            {"minecraft:medium_amethyst_bud", true},
            {"minecraft:small_amethyst_bud", true},
            {"minecraft:cobbled_deepslate_wall", true},
            {"minecraft:deepslate_tile_wall", true},
            {"minecraft:polished_deepslate_wall", true},
            {"minecraft:deepslate_brick_wall", true},
            {"minecraft:azalea_leaves", true},
            {"minecraft:azalea_leaves_flowered", true},
            // {"minecraft:tinted_glass", true},  // better effect without
            {"minecraft:glow_lichen", true},
            {"minecraft:glow_frame", true},
            {"minecraft:cave_vines", true},
            {"minecraft:cave_vines_head_with_berries", true},
            {"minecraft:cave_vines_body_with_berries", true},
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

        public TextureStack FindTexturePath(string name, Dictionary<string, Object> data, int x, int z, int y)
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



        private TextureStack GetSubstitution(string name, Dictionary<string, Object> data, int x, int z, int y)
        {
            // For debugging purposes

            if (Debug)
            {
                String datastring = string.Join(", ", data.Select(kvp => kvp.Key + ": " + kvp.Value.ToString()));
                Console.WriteLine($"{x} {z} {y}: {name}, {datastring}");
            }


            // oh look at all this mess...
            switch (name)
            {
                case "cobblestone_wall":
                    return RenderWall(data, "cobblestone_wall");

                case "bubble_column":
                case "water":
                case "flowing_water":
                    return "textures/blocks/water_placeholder";
                case "lava":
                case "flowing_lava":
                    return "textures/blocks/lava_placeholder";
                case "fire":
                    return "textures/blocks/fire_0_placeholder";
                case "packed_ice":
                    return GetTexture("ice_packed", data);
                case "lectern":
                    return "textures/blocks/lectern_top";
                case "cake":
                {
                    int bites = (int)data.GetValueOrDefault("bite_counter", 0);
                    return GetTexture("cake_top", 0).Translate(1 + 2*bites, 1, 14 - 2*bites, 14);
                }
                case "bed":
                {
                    // TODO: fix bed colours
                    int headBit = (int)data.GetValueOrDefault("head_piece_bit", 0);
                    RotateFlip rot = RotateFromDirection(((int)data["direction"] + 3) % 4);
                    return CreateTexture(headBit != 0
                                    ? "textures/blocks/bed_head_top"
                                    : "textures/blocks/bed_feet_top")
                        .Rotate(rot);
                }

                case "planks":
                case "wooden_slab":
                case "double_wooden_slab":
                {
                    int plankIndex = WoodIndexes[(string)data.GetValueOrDefault("wood_type")];
                    return GetTexture("planks", plankIndex);
                }

                case "prismarine_bricks_stairs":
                    return GetTexture("prismarine_bricks");

                case "oak_stairs":
                    return GetTexture("planks", 0); // data = direction
                case "brick_stairs":
                    return GetTexture("brick");
                case "dark_prismarine_stairs":
                    return GetTexture("dark_prismarine"); // data = direction
                case "prismarine_stairs":
                    return GetTexture("prismarine"); // data = direction
                case "spruce_stairs":
                    return GetTexture("spruce_planks");
                case "birch_stairs":
                    return GetTexture("birch_planks");
                case "acacia_stairs":
                    return GetTexture("acacia_planks");
                case "dark_oak_stairs":
                    return GetTexture("dark_oak_planks");
                case "nether_brick_stairs":
                    return GetTexture("nether_brick");
                case "sandstone_stairs":
                    return GetTexture("sandstone_top");
                case "normal_stone_stairs":
                    return GetTexture("stone", 0);
                case "jungle_stairs":
                    return GetTexture("jungle_planks");
                case "stone_brick_stairs":
                    return GetTexture("stonebrick");
                case "stone_stairs":
                    return GetTexture("cobblestone");
                case "mossy_cobblestone_stairs":
                    return GetTexture("cobblestone_mossy");
                case "quartz_stairs":
                    return GetTexture("quartz_block_top", data);
                case "mossy_stone_brick_stairs":
                    return GetTexture("mossy_stone_brick");
                case "smooth_sandstone_stairs":
                    return GetTexture("smooth_sandstone");

                case "red_sandstone_stairs":
                    return GetTexture("redsandstone_top");
                case "smooth_red_sandstone_stairs":
                    return GetTexture("smooth_red_sandstone");
                case "purpur_stairs":
                    return GetTexture("purpur_block_top");   
                case "red_nether_brick_stairs":
                    return GetTexture("red_nether_brick");   
                case "end_brick_stairs":
                    return GetTexture("end_bricks", data);   
                case "smooth_quartz_stairs":
                    return GetTexture("stair_smooth_quartz_block", data);

                case "cauldron":
                    return GetTexture("cauldron_inner", data)
                           + GetTexture("cauldron_top", data);
                case "carpet":
                    return GetTexture("wool", data);
                case "hay_block":
                    return RenderPillar("hayblock_top", "hayblock_side", data);
                case "hopper":
                    return GetTexture("hopper_inside", data)
                           + GetTexture("hopper_top", data);

                case "red_flower":
                    return GetTexture("red_flower", RedFlowerIndexes[(string)data.GetValueOrDefault("flower_type", "poppy")]);
                case "double_plant":
                {
                    int legacyData = DoublePlantIndexes[(string)data.GetValueOrDefault("double_plant_type", "sunflower")];
                    if ((int)data.GetValueOrDefault("upper_block_bit", 0) == 0)
                        return GetTexture("double_plant_bottom", legacyData & 0xF7);
                    else
                            return GetTexture("double_plant_carried", legacyData & 0xF7);
                    }
                case "waterlily":
                    return GetTexture("waterlily_carried");

                case "tnt":
                    return GetTexture("tnt_top");
                case "rail":
                {
                    switch ((int)data.GetValueOrDefault("rail_direction", 0))
                    {
                        case 6:
                            return GetTexture("rail_normal_turned");
                        case 7:
                            return GetTexture("rail_normal_turned").Rotate(RotateFlip.Rotate90FlipNone);
                        case 8:
                            return GetTexture("rail_normal_turned").Rotate(RotateFlip.Rotate180FlipNone);
                        case 9:
                            return GetTexture("rail_normal_turned").Rotate(RotateFlip.Rotate270FlipNone);
                    }
                    return RenderRail("rail_normal", data);
                }
                case "golden_rail":
                    return RenderRail("rail_golden", "rail_golden_powered", data);
                case "activator_rail":
                    return RenderRail("rail_activator", "rail_activator_powered", data);
                case "detector_rail":
                    return RenderRail("rail_detector", "rail_detector_powered", data);

                case "stonebrick":
                    return GetTexture("stonebrick", StoneBrickIndexes[(string)data["stone_brick_type"]]);
                case "monster_egg":
                    return GetTexture("monster_egg", MonsterEggIndexes[(string)data["monster_egg_stone_type"]]);

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
                case "hard_glass":
                    // Intentional fall-through
                case "hard_stained_glass":
                    return GetTexture("stained_glass", data);
                case "glass_pane":
                    // Intentional fall-through
                case "hard_glass_pane":
                    return GetTexture("glass_pane_top", data);
                case "stained_glass_pane":
                    // Intentional fall-through
                case "hard_stained_glass_pane":
                    return GetTexture("stained_glass_pane_top", data);
                case "redstone_torch":
                    return GetTexture("redstone_torch_on", data);
                case "unlit_redstone_torch":
                    return GetTexture("redstone_torch_off", data);
                case "redstone_wire":
                    return GetTexture("redstone_dust_cross", data);
                case "lit_redstone_ore":
                    return GetTexture("redstone_ore", data);
                case "unpowered_repeater":
                {
                    int dir = ((int)data.GetValueOrDefault("direction") + 2) % 4;
                    return GetTexture("repeater_up", 0, null, RotateFromDirection(dir));
                }
                case "powered_repeater":
                {
                    int dir = ((int)data.GetValueOrDefault("direction") + 2) % 4;
                    return GetTexture("repeater_up", 1, null, RotateFromDirection(dir));
                }
                case "daylight_detector":
                    return GetTexture("daylight_detector_top", 0);
                case "daylight_detector_inverted":
                    return GetTexture("daylight_detector_top", 1);
                case "dispenser":
                {
                    switch ((BlockFace) data["facing_direction"])
                    {
                        case BlockFace.Up:
                            return GetTexture("dispenser_front_vertical");
                        default:
                            return GetTexture("dispenser_top");
                    }
                }
                case "observer":
                {
                    int powered = (int)data.GetValueOrDefault("powered_bit", 0);
                    switch ((BlockFace) data["facing_direction"])
                    {
                        case BlockFace.Down:
                            return GetTexture("observer_south", powered);
                        case BlockFace.Up:
                            return GetTexture("observer_north", powered);
                        case BlockFace.North:
                            return GetTexture("observer_top", powered, null, RotateFlip.Rotate180FlipNone);
                        case BlockFace.South:
                            return GetTexture("observer_top", powered);
                        case BlockFace.West:
                            return GetTexture("observer_top", powered, null, RotateFlip.Rotate90FlipNone);
                        case BlockFace.East:
                            return GetTexture("observer_top", powered, null, RotateFlip.Rotate270FlipNone);
                    }
                    return GetTexture("observer_top");
                }
                case "dropper":
                {
                    switch ((BlockFace) data["facing_direction"])
                    {
                        case BlockFace.Up:
                            return GetTexture("dropper_front_vertical");
                        default:
                            return GetTexture("dropper_top");
                    }
                }

                case "smoker":
                case "lit_smoker":
                    return GetTexture("smoker_top");
                case "barrel":
                {
                    switch ((BlockFace)data["facing_direction"])
                    {
                        case BlockFace.Up:
                            return GetTexture("barrel_top");
                        case BlockFace.Down:
                            return GetTexture("barrel_bottom");
                        default:
                            return GetTexture("barrel_side", data);
                    }
                }
                case "bell":
                    return GetTexture("bell_top", 0).Translate(
                        new Rect(0, 0, 8, 8),
                        new Rect(4, 4, 8, 8));
                case "composter":
                    return GetTexture("composter_bottom");
                case "campfire":
                    return GetTexture("campfire_log_lit", 0).Translate(0, 0, 16, 16);
                case "ender_chest":
                    return GetTexture("ender_chest_inventory_top", data);
                case "anvil":
                {
                    int damage = 0;
                    switch ((string)data["damage"])
                    {
                        case "slightly_damaged":
                            damage = 1; break;
                        case "very_damaged":
                            damage = 2; break;
                    }
                    return GetTexture("anvil_top_damaged_x", damage, null, RotateFromDirection(data));
                }
                case "cactus":
                    return GetTexture("cactus_top", data).Translate(1, 1, 14, 14);

                case "dark_oak_pressure_plate":
                    return RenderPressurePlate("dark_oak_planks", data);
                case "birch_pressure_plate":
                    return RenderPressurePlate("birch_planks", data);
                case "jungle_pressure_plate":
                    return RenderPressurePlate("jungle_planks", data);
                case "acacia_pressure_plate":
                    return RenderPressurePlate("acacia_planks", data);
                case "wooden_pressure_plate":
                    return RenderPressurePlate("planks", data);
                case "spruce_pressure_plate":
                    return RenderPressurePlate("spruce_planks", data);
                case "light_weighted_pressure_plate":
                    return RenderPressurePlate("gold_block", data);
                case "heavy_weighted_pressure_plate":
                    return RenderPressurePlate("iron_block", data);
                case "stone_pressure_plate":
                    return RenderPressurePlate("stone", data);

                case "frame":
                    return RenderItemFrame(data, "itemframe_background");

                case "standing_sign":
                    return RenderSign(data, "sign");
                case "spruce_standing_sign":
                    return RenderSign(data, "spruce_sign");
                case "birch_standing_sign":
                    return RenderSign(data, "birch_sign");
                case "jungle_standing_sign":
                    return RenderSign(data, "jungle_sign");
                case "acacia_standing_sign":
                    return RenderSign(data, "acacia_sign");
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
                    return RenderFence("nether_brick", data);
                case "fence":
                    data["val"] = WoodIndexes[(string)data.GetValueOrDefault("wood_type")];
                    return RenderFence("planks", data);
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
                    // Intentional fall-through
                case "underwater_torch":
                    // Intentional fall-through
                case "colored_torch_bp":
                    // Intentional fall-through
                case "colored_torch_rg":
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
                {
                    RotateFlip rot = RotateFromDirection(data);
                    TextureStack t = GetTexture("endframe_top", 0, null, rot);
                    if ((int)data.GetValueOrDefault("end_portal_eye_bit", 0) != 0)
                        t += GetTexture("endframe_eye", 0).Translate(4, 4, 8, 8).Rotate(rot);
                    return t;
                    }

                case "wooden_door":
                    return GetTexture("door_upper");
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
                    return GetTexture("trip_wire", data);
                case "tripwire_hook":
                    return RenderTripwireHook(data, "trip_wire_source");

                case "lever":
                    return RenderLever(data);

                case "wall_sign":
                case "darkoak_wall_sign":
                case "spruce_wall_sign":
                case "birch_wall_sign":
                case "jungle_wall_sign":
                case "acacia_wall_sign":
                case "warped_wall_sign":
                case "crimson_wall_sign":
                    return RenderWallSign(data, name.Replace("wall_", ""));

                case "melon_block":
                    return GetTexture("melon_top", data);

                case "quartz_block":
                {
                    switch ((string)data.GetValueOrDefault("chisel_type", "default"))
                    {
                        case "chiseled":
                            data["val"] = 1; break;
                        case "lines":
                            data["val"] = 2; break;
                        case "smooth":
                            data["val"] = 3; break;
                    }
                    return RenderPillar("quartz_block_top", "quartz_block_side", data);
                }

                case "seaLantern":
                    return GetTexture("sea_lantern", data);

                case "purpur_block":
                {
                    switch ((string)data.GetValueOrDefault("chisel_type", "default"))
                    {
                        case "lines":
                            data["val"] = 2; break;
                    }
                    return RenderPillar("purpur_block_top", "purpur_block_side", data);
                }

                case "turtle_egg":
                        return GetTexture("turtle_egg", 0).Translate(
                            new Rect(0, 0, 4, 4),
                            new Rect(6, 6, 4, 4));


                case "powered_comparator":
                    return GetTexture("comparator_up", data);
                case "unpowered_comparator":
                    return GetTexture("comparator_up", data);

                case "pistonArmCollision":
                return GetTexture("piston_top_normal", data);
                case "piston":
                    return RenderPiston(data, "normal");
                case "stickyPistonArmCollision":
                    return GetTexture("piston_top_sticky", data);
                case "sticky_piston":
                    return RenderPiston(data, "sticky");
                case "jukebox":
                    return GetTexture("jukebox_top", data);
                case "stonecutter":
                    return GetTexture("stonecutter_top", data);
                case "stonecutter_block":
                    return GetTexture("stonecutter2_top", data);
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

                case "undyed_shulker_box":
                    return GetTexture("undyed_shulker_box_top", data);

                case "red_sandstone":
                    // sand_stone_type makes no difference to top
                    return GetTexture("redsandstone_top", data);
                case "sandstone":
                    // sand_stone_type makes no difference to top
                    return GetTexture("sandstone_top", data);

                case "stone_slab":
                case "double_stone_slab":
                    return GetTexture("stone_slab_top", StoneSlabIndexes[1][(string)data["stone_slab_type"]]);
                case "stone_slab2":
                case "double_stone_slab2":
                    return GetTexture("stone_slab_top_2", StoneSlabIndexes[2][(string)data["stone_slab_type_2"]]);
                case "stone_slab3":
                case "double_stone_slab3":
                    return GetTexture("stone_slab_top_3", StoneSlabIndexes[3][(string)data["stone_slab_type_3"]]);
                case "stone_slab4":
                case "double_stone_slab4":
                    return GetTexture("stone_slab_top_4", StoneSlabIndexes[4][(string)data["stone_slab_type_4"]]);

                case "bone_block":
                    return RenderPillar("bone_block_top", "bone_block_side", data);

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
                case "andesite_stairs":
                    return GetTexture("andesite", data);
                case "granite_stairs":
                    return GetTexture("granite", data);
                /* LEAVES */
                case "leaves":
                {
                    int val = WoodIndexes[(string)data.GetValueOrDefault("old_leaf_type", "oak")];
                    return GetTexture("leaves_carried", val);
                }
                case "leaves2":
                {
                    int val = WoodIndexes[(string)data.GetValueOrDefault("new_leaf_type", "acacia")] - 4;
                    return GetTexture("leaves_carried2", val);
                }


                /* WOOD */
                case "wood":
                {
                    int plankIndex = WoodIndexes[(string)data.GetValueOrDefault("wood_type", "oak")];
                    int strippedBit = (int)data.GetValueOrDefault("stripped_bit", 0);
                    data["val"] = plankIndex * 2 + strippedBit;
                    return RenderPillar("wood", data);
                }

                case "stripped_jungle_log":
                    return RenderPillar("stripped_jungle_log_top", "stripped_jungle_log_side", data);
                case "stripped_spruce_log":
                    return RenderPillar("stripped_spruce_log_top", "stripped_spruce_log_side", data);
                case "stripped_birch_log":
                    return RenderPillar("stripped_birch_log_top", "stripped_birch_log_side", data);
                case "stripped_dark_oak_log":
                    return RenderPillar("stripped_dark_oak_log_top", "stripped_dark_oak_log_side", data);
                case "stripped_oak_log":
                    return RenderPillar("stripped_oak_log_top", "stripped_oak_log_side", data);
                case "stripped_acacia_log":
                    return RenderPillar("stripped_acacia_log_top", "stripped_acacia_log_side", data);

                case "log":
                {
                    data["val"] = WoodIndexes[(string)data.GetValueOrDefault("old_log_type", "oak")];
                    return RenderPillar("log_top", "log_side", data);
                }
                case "log2":
                {
                    data["val"] = WoodIndexes[(string)data.GetValueOrDefault("new_log_type", "acacia")] - 4;
                    return RenderPillar("log_top2", "log_side2", data);
                }
              
                case "sapling":
                {
                    int val = WoodIndexes[(string)data.GetValueOrDefault("sapling_type")];
                    return GetTexture("sapling", val);
                }

                case "enchanting_table":
                    return GetTexture("enchanting_table_top", data);

                case "coral_block":
                {
                    int dead = (int)data.GetValueOrDefault("dead_bit", 0);
                    int coralIndex = CoralIndexes[(string)data["coral_color"]];
                    return GetTexture("coral_block", coralIndex + dead * 5);
                }
                case "coral":
                {
                    int dead = (int)data.GetValueOrDefault("dead_bit", 0);
                    int coralIndex = CoralIndexes[(string)data["coral_color"]];
                    return GetTexture("coral", coralIndex + dead * 5);
                }
                case "coral_fan":
                {
                    int coralIndex = CoralIndexes[(string)data["coral_color"]];
                    Rect srcRect = new Rect(0, 6, 16, 8);
                    Rect destRect = new Rect(0, 0, 16, 8);
                    return GetTexture("coral_fan", coralIndex).Translate(srcRect, destRect)
                        + GetTexture("coral_fan", coralIndex).Translate(srcRect, destRect).Rotate(RotateFlip.Rotate90FlipNone)
                        + GetTexture("coral_fan", coralIndex).Translate(srcRect, destRect).Rotate(RotateFlip.Rotate180FlipNone)
                        + GetTexture("coral_fan", coralIndex).Translate(srcRect, destRect).Rotate(RotateFlip.Rotate270FlipNone);
                }
                case "coral_fan_dead":
                {
                    int coralIndex = CoralIndexes[(string)data["coral_color"]];
                    Rect srcRect = new Rect(0, 6, 16, 8);
                    Rect destRect = new Rect(0, 0, 16, 8);
                    return GetTexture("coral_fan_dead", coralIndex).Translate(srcRect, destRect)
                        + GetTexture("coral_fan_dead", coralIndex).Translate(srcRect, destRect).Rotate(RotateFlip.Rotate90FlipNone)
                        + GetTexture("coral_fan_dead", coralIndex).Translate(srcRect, destRect).Rotate(RotateFlip.Rotate180FlipNone)
                        + GetTexture("coral_fan_dead", coralIndex).Translate(srcRect, destRect).Rotate(RotateFlip.Rotate270FlipNone);
                }
                case "coral_fan_hang":
                {
                    int dead = (int)data.GetValueOrDefault("dead_bit", 0);
                    int coralType = (int)data.GetValueOrDefault("coral_hang_type_bit", 0);
                    RotateFlip rot = CoralRotations[(int)data.GetValueOrDefault("coral_direction", 0)];
                    return GetTexture("coral_fan_hang_a", coralType + dead * 2, null, rot);
                }
                case "coral_fan_hang2":
                {
                    int dead = (int)data.GetValueOrDefault("dead_bit", 0);
                    int coralType = (int)data.GetValueOrDefault("coral_hang_type_bit", 0);
                    RotateFlip rot = CoralRotations[(int)data.GetValueOrDefault("coral_direction", 0)];
                    return GetTexture("coral_fan_hang_b", coralType + dead * 2, null, rot);
                }
                case "coral_fan_hang3":
                {
                    int dead = (int)data.GetValueOrDefault("dead_bit", 0);
                    int coralType = (int)data.GetValueOrDefault("coral_hang_type_bit", 0);
                    RotateFlip rot = CoralRotations[(int)data.GetValueOrDefault("coral_direction", 0)];
                    return GetTexture("coral_fan_hang_c", coralType + dead * 2, null, rot);
                }

                case "scaffolding":
                    return GetTexture("scaffolding_top", data);
                case "grindstone":
                    return GetTexture("grindstone_pivot", data);

                case "beetroot":
                {
                    int growth = (int)data.GetValueOrDefault("growth", 0) % 8;
                    return GetTexture("beetroot", GrowthEightToFour[growth]);
                }
                case "carrots":
                {
                    int growth = (int)data.GetValueOrDefault("growth", 0) % 8;
                    return GetTexture("carrots", GrowthEightToFour[growth]);
                }
                case "potatoes":
                {
                    int growth = (int)data.GetValueOrDefault("growth", 0) % 8;
                    return GetTexture("potatoes", GrowthEightToFour[growth]);
                }
                case "sweet_berry_bush":
                {
                    int growth = (int)data.GetValueOrDefault("growth", 0);
                    if (growth > 3) growth = 3;
                    return GetTexture($"sweet_berry_bush_{growth}");
                }
                case "wheat":
                {
                    int growth = (int)data.GetValueOrDefault("growth", 0);
                    return GetTexture("wheat", growth);
                }

                case "bee_nest":
                    return GetTexture("bee_nest_top", data);
                case "beehive":
                    return GetTexture("beehive_top", data);
                case "honey_block":
                    return GetTexture("honey_top", data);
                case "glowingobsidian":
                    return GetTexture("glowing_obsidian", data);
                case "netherreactor":
                    return GetTexture("reactor_core", data);
                case "chain_command_block":
                    return GetTexture("command_block_chain_front", data);
                case "repeating_command_block":
                    return GetTexture("command_block_repeating_front", data);

                // Nether update (1.16)
                case "crimson_nylium":
                    return GetTexture("crimson_nylium_top", data);
                case "warped_nylium":
                    return GetTexture("warped_nylium_top", data);
                case "crimson_stem":
                    return RenderPillar("crimson_log_top", "crimson_log_side", data);
                case "stripped_crimson_stem":
                    return RenderPillar("stripped_crimson_stem_top", "stripped_crimson_stem_side", data);
                case "warped_stem":
                    return RenderPillar("warped_stem_top", "warped_stem_side", data);
                case "stripped_warped_stem":
                    return RenderPillar("stripped_warped_stem_top", "stripped_warped_stem_side", data);
                case "warped_fungus":
                    return GetTexture("nether_shroom_blue", data);
                case "crimson_fungus":
                    return GetTexture("nether_shroom_red", data);
                case "twisting_vines":
                    return GetTexture("twisting_vines_base", data);
                case "soul_fire":
                    return GetTexture("soul_fire_0", data);

                case "basalt":
                    return RenderPillar("basalt_top", "basalt_side", data);
                case "polished_basalt":
                    return RenderPillar("polished_basalt_top", "polished_basalt_side", data);

                case "blackstone":
                    return GetTexture("blackstone_top", data);
                case "blackstone_slab":
                    return GetTexture("blackstone_top", data);
                case "blackstone_stairs":
                    return GetTexture("blackstone_top", data);
                case "blackstone_wall":
                    return RenderWall(data, "blackstone_top");

                case "polished_blackstone_slab":
                    return GetTexture("polished_blackstone", data);
                case "polished_blackstone_stairs":
                    return GetTexture("polished_blackstone", data);
                case "polished_blackstone_wall":
                    return RenderWall(data, "polished_blackstone");

                case "polished_blackstone_brick_slab":
                    return GetTexture("polished_blackstone_bricks", data);
                case "polished_blackstone_brick_stairs":
                    return GetTexture("polished_blackstone_bricks", data);
                case "polished_blackstone_brick_wall":
                    return RenderWall(data, "polished_blackstone_bricks");

                case "warped_door":
                    return GetTexture("warped_door_top", data);
                case "crimson_door":
                    return GetTexture("crimson_door_top", data);

                // some fixes
                case "ancient_debris":
                    return GetTexture("ancient_debris_top");
                case "beacon":
                    // TODO: add shell + base
                    return GetTexture("beacon");
                case "blackstone_double_slab":
                    return GetTexture("blackstone");
                case "blast_furnace":
                case "lit_blast_furnace":
                    return GetTexture("blast_furnace_top");
                case "bookshelf":
                    return GetTexture("planks");
                case "calcite":
                    return GetTexture("calcite");
                case "chorus_flower":
                    return GetTexture("chorus_flower", (int)data["age"] > 4 ? 1 : 0);
                case "crimson_button":
                    return RenderButton(data, "crimson_planks");
                case "crimson_double_slab":
                case "crimson_slab":
                case "crimson_stairs":
                    return GetTexture("crimson_planks");
                case "crimson_fence":
                    return RenderFence("crimson_planks", data);
                case "crimson_fence_gate":
                    return RenderFenceGate(data, "crimson_planks");
                case "crimson_hyphae":
                    return RenderPillar("crimson_log_side", data);
                case "crimson_pressure_plate":
                    return RenderPressurePlate("crimson_planks", data);
                case "crimson_standing_sign":
                    return RenderSign(data, "crimson_sign");
                case "lava_cauldron":
                    // TODO: add lava
                    return GetTexture("cauldron_top");
                case "lodestone":
                    return GetTexture("lodestone_top");
                case "polished_blackstone_button":
                    return RenderButton(data, "polished_blackstone");
                case "polished_blackstone_double_slab":
                    return GetTexture("polished_blackstone");
                case "polished_blackstone_pressure_plate":
                    return RenderPressurePlate("polished_blackstone", data);
                case "polished_blackstone_brick_double_slab":
                    return GetTexture("polished_blackstone_bricks");
                case "respawn_anchor":
                    return GetTexture("respawn_anchor_top", (int)data["respawn_anchor_charge"]).Translate(0, 0, 16, 16);
                case "soul_campfire":
                    return GetTexture("soul_campfire_log_lit", 0).Translate(0, 0, 16, 16);
                case "stripped_crimson_hyphae":
                    return RenderPillar("stripped_crimson_stem_side", data);
                case "stripped_warped_hyphae":
                    return RenderPillar("stripped_warped_stem_side", data);
                case "target":
                    return GetTexture("target_top");
                case "warped_button":
                    return RenderButton(data, "warped_planks");
                case "warped_double_slab":
                case "warped_slab":
                case "warped_stairs":
                    return GetTexture("warped_planks");
                case "warped_fence":
                    return RenderFence("warped_planks", data);
                case "warped_fence_gate":
                    return RenderFenceGate(data, "warped_planks");
                case "warped_hyphae":
                    return RenderPillar("warped_stem_side", data);
                case "warped_pressure_plate":
                    return RenderPressurePlate("warped_planks", data);
                case "warped_standing_sign":
                    return RenderSign(data, "warped_sign");
                case "weeping_vines":
                    return GetTexture("weeping_vines_base");
                case "lantern":
                    // TODO: support ceiling
                    return RenderLantern(data, "lantern");
                case "soul_lantern":
                    return RenderLantern(data, "soul_lantern");
                case "sea_pickle":
                    // TODO: number of pickles
                    return GetTexture("sea_pickle", data).Translate(
                        new Rect(0, 0, 4, 11), // 11 might not be the right number
                        new Rect(6, 5, 4, 11));
                case "sponge":
                    return GetTexture("sponge", (string)data["sponge_type"] == "wet" ? 1 : 0);
                case "stone":
                {
                    int index = StoneIndexes[(string)data.GetValueOrDefault("stone_type", "stone")];
                    return GetTexture("stone", index);
                }
                case "sand":
                {
                    string sand_type = (string)data.GetValueOrDefault("sand_type", "normal");
                    return GetTexture("sand", sand_type == "red" ? 1 : 0);
                }
                case "vine":
                    return GetTexture("vine_carried");
                // TODO: fix string textures

                case "chain":
                    return RenderChain(data);
                case "end_rod":
                    return RenderEndRod(data);

                // Caves & Cliffs Update: Part 1 (1.17)
                case "waxed_oxidized_cut_copper_stairs":
                case "oxidized_cut_copper_stairs":
                case "oxidized_cut_copper_slab":
                case "oxidized_double_cut_copper_slab":
                case "waxed_oxidized_cut_copper_slab":
                case "waxed_oxidized_double_cut_copper_slab":
                case "waxed_oxidized_cut_copper":
                    return GetTexture("oxidized_cut_copper", data);
                case "waxed_weathered_cut_copper_stairs":
                case "weathered_cut_copper_stairs":
                case "weathered_cut_copper_slab":
                case "weathered_double_cut_copper_slab":
                case "waxed_weathered_cut_copper_slab":
                case "waxed_weathered_double_cut_copper_slab":
                case "waxed_weathered_cut_copper":
                    return GetTexture("weathered_cut_copper", data);
                case "waxed_exposed_cut_copper_stairs":
                case "exposed_cut_copper_stairs":
                case "exposed_cut_copper_slab":
                case "exposed_double_cut_copper_slab":
                case "waxed_exposed_cut_copper_slab":
                case "waxed_exposed_double_cut_copper_slab":
                case "waxed_exposed_cut_copper":
                    return GetTexture("exposed_cut_copper", data);
                case "waxed_cut_copper_stairs":
                case "cut_copper_stairs":
                case "cut_copper_slab":
                case "double_cut_copper_slab":
                case "waxed_cut_copper_slab":
                case "waxed_double_cut_copper_slab":
                case "waxed_cut_copper":
                    return GetTexture("cut_copper", data);
                case "waxed_oxidized_copper":
                    return GetTexture("oxidized_copper", data);
                case "waxed_weathered_copper":
                    return GetTexture("weathered_copper", data);
                case "waxed_exposed_copper":
                    return GetTexture("exposed_copper", data);
                case "waxed_copper":
                    return GetTexture("copper_block", data);

                case "cobbled_deepslate_wall":
                    return RenderWall(data, "cobbled_deepslate");
                case "deepslate_tile_wall":
                    return RenderWall(data, "deepslate_tiles");
                case "polished_deepslate_wall":
                    return RenderWall(data, "polished_deepslate");
                case "deepslate_brick_wall":
                    return RenderWall(data, "deepslate_bricks");
                case "cobbled_deepslate_stairs":
                case "cobbled_deepslate_slab":
                case "cobbled_deepslate_double_slab":
                    return GetTexture("cobbled_deepslate", data);
                case "deepslate_tile_stairs":
                case "deepslate_tile_slab":
                case "deepslate_tile_double_slab":
                    return GetTexture("deepslate_tiles", data);
                case "polished_deepslate_stairs":
                case "polished_deepslate_slab":
                case "polished_deepslate_double_slab":
                    return GetTexture("polished_deepslate", data);
                case "deepslate_brick_slab":
                case "deepslate_brick_stairs":
                case "deepslate_brick_double_slab":
                    return GetTexture("deepslate_bricks", data);
                case "deepslate":
                case "infested_deepslate":
                    return RenderPillar("deepslate_top", "deepslate", data);

                case "lit_deepslate_redstone_ore":
                    return GetTexture("deepslate_redstone_ore", data);
                
                
                case "glow_frame":
                    return RenderItemFrame(data, "glow_item_frame");
                case "small_dripleaf_block":
                    return RenderSmallDripleaf(data);
                case "moss_carpet":
                    return GetTexture("moss_block", data);
                case "big_dripleaf":
                    return GetTexture("big_dripleaf_top", data);
                case "pointed_dripstone":
                    return RenderDripstone(data);
                case "azalea":
                    return GetTexture("azalea_top", data);
                case "flowering_azalea":
                    return GetTexture("flowering_azalea_top", data);

                case "lightning_rod":
                    return RenderLightningRod(data);

                case "glow_lichen":
                    return RenderGlowLichen(data);

                case "amethyst_cluster":
                case "large_amethyst_bud":
                case "medium_amethyst_bud":
                case "small_amethyst_bud":
                    return RenderAmethystCluster(name, data);

                case "cave_vines":
                    return GetTexture("cave_vines_head", data);
                case "cave_vines_head_with_berries":
                case "cave_vines_body_with_berries":
                    return GetTexture("cave_vines_head", 1);
            }

            return null;
        }

        private TextureStack RenderWallSign (Dictionary<string, Object> data, string texture)
        {
            return RenderWallSign(texture, data);
        }
        private TextureStack RenderWallSign (string texture, Dictionary<string, Object> data)
        {
            return GetTexture(texture, data).Translate(
                new Rect(0, 7, 14, 2),
                new Rect(1, 0, 14, 2)
            );
        }

        private TextureStack RenderChain(Dictionary<string, Object> data)
        {
            var t = GetTexture("chain1", 0).Translate(new Rect(0, 0, 3, 16), new Rect(6, 0, 4, 16));
            
             try
            {
                string dir = (string)data["pillar_axis"];
                switch (dir)
                {
                    case "x":
                        return t.Rotate(RotateFlip.Rotate90FlipNone);
                    case "y":
                        return GetTexture("chain2", data).Translate(new Rect(0, 6, 4, 3), new Rect(7, 7, 4, 3));
                }
            }
            catch 
            {
                Console.WriteLine("Invalid parameters for chain1");
            }

            return t;
        }

        private TextureStack RenderRail (string texture_off, string texture_on, Dictionary<string, Object> data)
        {
            string texture = (int)data.GetValueOrDefault("rail_data_bit", 0) == 0 ? texture_off : texture_on;
            return RenderRail(texture, data);
        }
        private TextureStack RenderRail (string texture, Dictionary<string, Object> data)
        {
            switch ((int)data.GetValueOrDefault("rail_direction", 0))
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

        private TextureStack RenderSign (Dictionary<string, Object> data, string texture)
        {
            return RenderSign(texture, data);
        }
        private TextureStack RenderSign(string texture, Dictionary<string, Object> data)
        {
            // TODO: rotation
            return GetTexture(texture, 0).Translate(
                new Rect(0, 7, 14, 2),
                new Rect(1, 7, 14, 2)
            );
        }

        private TextureStack RenderButton (Dictionary<string, Object> data, string texture)
        {
            return RenderButton(texture, data);
        }
        private TextureStack RenderButton(string texture, Dictionary<string, Object> data)
        {
            int direction = (int)data["facing_direction"];
            var t = GetTexture(texture, 0);
            int thickness = (int)data.GetValueOrDefault("button_pressed_bit", 0) == 0 ? 2 : 1;
            switch (direction)
            {
                case 0: // 0: Button on block bottom facing down. Assuming bottom = top
                case 1: // 1: Button on block top facing up
                    return t.Translate(new Rect(5, 6, 6, 4));
                case 2: // north
                    return t.Translate(new Rect(5, 16 - thickness, 6, thickness));
                case 3: // south
                    return t.Translate(new Rect(5, 0, 6, thickness));
                case 4: // east
                    return t.Translate(new Rect(16 - thickness, 5, thickness, 6));
                case 5: // west
                    return t.Translate(new Rect(0, 5, thickness, 6));
                default:
                    return null;
            }
        }

        private TextureStack RenderFence (Dictionary<string, Object> data, string texture)
        {
            return RenderFence(texture, data);
        }
        private TextureStack RenderFence (string texture, Dictionary<string, Object> data)
        {
            TextureStack full = GetTexture(texture, data).Translate(6, 6, 4, 4);
            // TODO: sides? 2px
            return full;
        }

        private TextureStack RenderFenceGate (Dictionary<string, Object> data, string texture)
        {
            return RenderFenceGate(texture, data);
        }
        private TextureStack RenderFenceGate (string texture, Dictionary<string, Object> data)
        {
            int direction = (int)data["direction"];
            int open_bit = (int)data["open_bit"];

            if (open_bit != 0)
            {
                switch (direction)
            {
                case 0:
                    return GetTexture(texture, 0)
                               .Translate(
                                    new Rect(0, 7, 2, 9))
                           + GetTexture(texture, 0)
                               .Translate(
                                    new Rect(14, 7, 2, 9));
                    case 2:
                    return GetTexture(texture, 0)
                               .Translate(
                                    new Rect(0, 0, 2, 9))
                           + GetTexture(texture, 0)
                               .Translate(
                                    new Rect(14, 0, 2, 9));
                    case 1:
                        return GetTexture(texture, 0).Translate(new Rect(0, 0, 9, 2))
                                + GetTexture(texture, 0).Translate(new Rect(0, 14, 9, 2));
                    case 3:
                        return GetTexture(texture, 0).Translate(new Rect(7, 0, 9, 2))
                            + GetTexture(texture, 0).Translate(new Rect(7, 14, 9, 2));
                }
            }
            else
            {
                switch (direction)
                {
                    case 0:
                    case 2:
                        return GetTexture(texture, 0).Translate(new Rect(0, 7, 16, 2));
                    case 1:
                    case 3:
                        return GetTexture(texture, 0).Translate(new Rect(7, 0, 2, 16));
                }
            }

            return null;
        }

        private TextureStack RenderTripwireHook(Dictionary<string, Object> data, string texture, Rect[] translate = null)
        {
            string[] directionKeys = 
            {
                "facing_direction",
                "direction",
                "val"
            };

            var t = GetTexture(texture);

            if(null != translate)
            {
                try
                {
                    t.Translate(translate[0], translate[1]);
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Invalid translate parameters for " + texture);
                }
            }

            int dir = 2;
            bool keyFound = false;

            foreach(string key in directionKeys)
            {
                if(data.ContainsKey(key))
                {
                    dir = (int)data[key];
                    keyFound = true;
                    break;
                }
            }
            
            if(keyFound == false)
            {
                Console.WriteLine("Invalid " + texture +" direction");
            }

            t.Rotate(RotateFromDirection(dir));

            return t;
        }

        private TextureStack RenderPiston (Dictionary<string, Object> data, string prefix)
        {
            return RenderPiston(prefix, data);
        }
        private TextureStack RenderPiston (string prefix, Dictionary<string, Object> data)
        {
            try
            {
                int dir = (int)data["facing_direction"];
                switch (dir)
                {
                    case 0:
                        return GetTexture("piston_bottom", data);
                    case 1:
                        return GetTexture("piston_top_" + prefix, data);
                }
            }
            catch {}

            return GetTexture("piston_side", data);
        }

        private TextureStack RenderLever (Dictionary<string, Object> data)
        {
            RotateFlip rot = RotateFlip.RotateNoneFlipNone;
            TextureTranslation trans = null;
            try
            {
                string dir = (string)data["lever_direction"];
                switch (dir)
                {
                    case "up_north_south":
                        // Intentional fall-through
                        trans = new TextureTranslation(new Rect(7, 3, 2, 9), new Rect(7, 6, 2, 9));
                        rot = RotateFlip.Rotate180FlipNone;
                        try
                        {
                            if((int)data["open_bit"] == 1)
                            {
                                rot = RotateFlip.RotateNoneFlipNone;
                            }
                        }
                        catch {}
                        break;
                    case "north":
                        rot = RotateFlip.RotateNoneFlipNone;
                        break;
                    case "east":
                        rot = RotateFlip.Rotate90FlipNone;
                        break;
                    case "south":
                        rot = RotateFlip.Rotate180FlipNone;
                        break;
                    case "up_east_west":
                        // Intentional fall-through
                        trans = new TextureTranslation(new Rect(7, 3, 2, 9), new Rect(7, 6, 2, 9));
                        rot = RotateFlip.Rotate90FlipNone;
                        try
                        {
                            if((int)data["open_bit"] == 1)
                            {
                                rot = RotateFlip.Rotate270FlipNone;
                            }
                        }
                        catch {}
                        break;
                    case "west":
                        rot = RotateFlip.Rotate270FlipNone;
                        break;
                }
            }
            catch {}

            return GetTexture("lever", data, trans, rot);
        }

        private TextureStack RenderWall (Dictionary<string, Object> data, string texture)
        {
            return RenderWall(texture, data);
        }
        private TextureStack RenderWall (string texture, Dictionary<string, Object> data)
        {
            TextureStack full = 
                (int)data.GetValueOrDefault("wall_post_bit", 1) != 0
                ? GetTexture(texture, data).Translate(4, 4, 8, 8)
                : GetTexture(texture, data).Translate(5, 5, 6, 6);

            if ((string)data.GetValueOrDefault("wall_connection_type_north", "none") != "none")
            {
                full = full + GetTexture(texture, data).Translate(5, 0, 6, 6);
            }
            if ((string)data.GetValueOrDefault("wall_connection_type_east", "none") != "none")
            {
                full = full + GetTexture(texture, data).Translate(10, 5, 6, 6);
            }
            if ((string)data.GetValueOrDefault("wall_connection_type_south", "none") != "none")
            {
                full = full + GetTexture(texture, data).Translate(5, 10, 6, 6);
            }
            if ((string)data.GetValueOrDefault("wall_connection_type_west", "none") != "none")
            {
                full = full + GetTexture(texture, data).Translate(0, 5, 6, 6);
            }

            return full;
        }

        private TextureStack RenderLantern (Dictionary<string, Object> data, string texture)
        {
            return RenderLantern(texture, data);
        }
        private TextureStack RenderLantern (string texture, Dictionary<string, Object> data)
        {
            return GetTexture(texture, data).Translate(
                        new Rect(0, 0, 6, 9),
                        new Rect(5, 7, 6, 9));
        }

        private TextureStack RenderDripstone (Dictionary<string, Object> data)
        {
            string filename = "pointed_dripstone_tip";
            try
            {
                int hanging = (int)data["hanging"];

                RotateFlip rot = RotateFlip.RotateNoneFlipNone;
                if(hanging != 0)
                {
                    rot = RotateFlip.Rotate180FlipNone;
                }
                
                TextureTranslation trans = new TextureTranslation(new Rect(4, 0, 7, 11), new Rect(4, 2, 7, 11));

                int usePointingUpTex = 1;
                return GetTexture(filename, usePointingUpTex, trans, rot);
            }
            catch {}

            int usingPointingDownTex = 0;
            return GetTexture("pointed_dripstone_tip", usingPointingDownTex);
        }

        private TextureStack RenderGlowLichen (Dictionary<string, Object> data)
        {
            // TODO fix render depending on orientation data.
            // For now not displaying when side facing and I'm fine with that.
            // Will still display when top/bottom facing.
            string filename = "glow_lichen";
            try
            {
                int dir = (int)data["multi_face_direction_bits"];
                TextureStack tex = GetTexture(filename, 0);
                TextureTranslation trans = null;
                RotateFlip rot = RotateFlip.RotateNoneFlipNone;

                // value of 1 is top-facing
                // value of 2 is bottom-facing
                if((dir != 1) && (dir != 2))
                {
                    tex.Translate(new Rect(11, 0, 16, 1), new Rect(0, 0, 0, 0));

                    // Orientation data is not necessarily true
                    // But case values are the right orientation bits for side-facing item
                    switch(dir)
                    {
                        case 4:
                            rot = RotateFlip.Rotate90FlipNone;
                        break;
                        case 8:
                            rot = RotateFlip.Rotate270FlipNone;
                        break;
                        case 16:
                            rot = RotateFlip.Rotate180FlipNone;
                        break;
                        case 32:

                        break;
                    }
                }

                return tex;
            }
            catch 
            {
                Console.WriteLine("Invalid " + filename + " direction");
            }

            return GetTexture(filename, 0);
        }

        private TextureStack RenderLightningRod (Dictionary<string, Object> data)
        {
            string filename = "lightning_rod";
            try
            {
                int dir = (int)data["facing_direction"];
                TextureStack head = GetTexture(filename, 0);

                switch(dir)
                {
                    case 0:
                        // intentional fall-through
                    case 1:
                        return head.Translate(new Rect(0,0,4,4), new Rect(6,6,4,4)).Rotate(RotateFlip.Rotate180FlipNone);
                }

                head = head.Translate(new Rect(0,0,4,4), new Rect(6,12,4,4));
                TextureStack body = GetTexture(filename, 0).Translate(new Rect(0,4,2,12), new Rect(7,4,2,12));
                switch(dir)
                {
                    case 2:
                        // head rotation is opposite to body
                        return body.Rotate(RotateFlip.RotateNoneFlipNone) + head.Rotate(RotateFlip.Rotate180FlipNone);
                    case 3:
                        return body.Rotate(RotateFlip.Rotate180FlipNone) + head.Rotate(RotateFlip.RotateNoneFlipNone);
                    case 4:
                        return body.Rotate(RotateFlip.Rotate270FlipNone) + head.Rotate(RotateFlip.Rotate90FlipNone);
                    case 5:
                        return body.Rotate(RotateFlip.Rotate90FlipNone) + head.Rotate(RotateFlip.Rotate270FlipNone);
                }
            }
            catch 
            {
                Console.WriteLine("Invalid " + filename + " direction");
            }

            return GetTexture(filename, data).Translate(new Rect(0,0,4,4), new Rect(6,6,4,4));
        }

        private TextureStack RenderEndRod (Dictionary<string, Object> data)
        {
            string filename = "end_rod";
            try
            {
                int dir = (int)data["facing_direction"];

                switch(dir)
                {
                    case 0:
                        // down -> base
                        return GetTexture(filename).Translate(new Rect(2,2,4,4), new Rect(6,6,4,4));
                    case 1:
                        // up -> base + tip
                        return GetTexture(filename).Translate(new Rect(2,2,4,4), new Rect(6,6,4,4))
                            + GetTexture(filename).Translate(new Rect(2,0,2,2), new Rect(7,7,2,2));
                }

                TextureStack head = GetTexture(filename).Translate(new Rect(2,6,4,1), new Rect(6,0,4,1));
                TextureStack body = GetTexture(filename).Translate(new Rect(0,0,2,15), new Rect(7,1,2,15));
                RotateFlip rot = RotateFlip.RotateNoneFlipNone;
                switch(dir)
                {
                    case 2:
                        rot = RotateFlip.RotateNoneFlipNone; break;
                    case 3:
                        rot = RotateFlip.Rotate180FlipNone; break;
                    case 4:
                        rot = RotateFlip.Rotate270FlipNone; break;
                    case 5:
                        rot = RotateFlip.Rotate90FlipNone; break;
                }
                return body.Rotate(rot) + head.Rotate(rot);
            }
            catch 
            {
                Console.WriteLine("Invalid " + filename + " direction");
            }

            return GetTexture(filename).Translate(new Rect(2,2,4,4), new Rect(6,6,4,4));
        }

        private TextureStack RenderItemFrame (Dictionary<string, Object> data, string texture)
        {
            return RenderItemFrame(texture, data);
        }
        private TextureStack RenderItemFrame (string texture, Dictionary<string, Object> data)
        {
            try
            {
                int dir = (int)data["facing_direction"];

                if((dir != 0) && (dir != 1))
                {
                    var t = GetTexture(texture, 0).Translate(
                        new Rect(0, 7, 14, 2),
                        new Rect(1, 0, 14, 2)
                    );
                    switch(dir)
                    {
                    case 2:
                        return t.Rotate(RotateFlip.Rotate180FlipNone);
                    case 3:
                        return t.Rotate(RotateFlip.RotateNoneFlipNone);
                    case 4:
                        return t.Rotate(RotateFlip.Rotate90FlipNone);
                    case 5:
                        return t.Rotate(RotateFlip.Rotate270FlipNone);
                    }
                }
            }
            catch 
            {
                Console.WriteLine("Invalid " + texture + " direction");
            }

            return GetTexture(texture, data);
        }

        private TextureStack RenderAmethystCluster (Dictionary<string, Object> data, string texture)
        {
            return RenderAmethystCluster(texture, data);
        }
        private TextureStack RenderAmethystCluster (string texture, Dictionary<string, Object> data)
        {
            TextureStack t = GetTexture(texture, data);
            try
            {
                int dir = (int)data["facing_direction"];

                switch(dir)
                {
                case 0:
                    t.Rotate(RotateFlip.Rotate180FlipNone);
                    goto case 1;
                case 1:
                    switch(texture)
                    {
                    case "small_amethyst_bud":
                        return t.Translate(new Rect(3,11, 10, 5), new Rect(3, 5, 10, 5));
                    case "medium_amethyst_bud":
                        return t.Translate(new Rect(3,9, 10, 7), new Rect(3, 4, 10, 5));
                    case "large_amethyst_bud":
                        return t.Translate(new Rect(2,6, 12, 10), new Rect(2, 2, 12, 10));
                    }
                    break;
                case 2:
                    return t.Rotate(RotateFlip.RotateNoneFlipNone);
                case 3:
                    return t.Rotate(RotateFlip.Rotate180FlipNone);
                case 4:
                    return t.Rotate(RotateFlip.Rotate270FlipNone);
                case 5:
                    return t.Rotate(RotateFlip.Rotate90FlipNone);
                }
            }
            catch 
            {
                Console.WriteLine("Invalid " + texture + " direction");
            }

            return t;
        }

        private TextureStack RenderSmallDripleaf (Dictionary<string, Object> data)
        {
            TextureStack t = GetTexture("small_dripleaf_top", data);
            t.Rotate(RotateFromDirection(data));
            return t;
        }

        private TextureStack RenderPressurePlate (Dictionary<string, Object> data, string texture)
        {
            return RenderPressurePlate(texture, data);
        }
        private TextureStack RenderPressurePlate (string texture, Dictionary<string, Object> data)
        {
            return GetTexture(texture).Translate(1, 1, 14, 14);
        }

        private TextureStack RenderPillar (string texture_both, Dictionary<string, Object> data)
        {
            return RenderPillar(texture_both, texture_both, data);
        }
        private TextureStack RenderPillar (string texture_top, string texture_side, Dictionary<string, Object> data)
        {
            string axis = (string)data.GetValueOrDefault("pillar_axis", "y");
            RotateFlip rotation =
                axis == "x" ? RotateFlip.Rotate90FlipNone : RotateFlip.RotateNoneFlipNone;
            return GetTexture(axis == "y" ? texture_top : texture_side, data, null, rotation);
        }

        private RotateFlip RotateFromDirection (Dictionary<string, Object> data)
        {
            return RotateFromDirection((int)data["direction"]);
        }
        private RotateFlip RotateFromDirection (int direction)
        {
            switch (Math.Abs(direction % 4))
            {
                case 0:
                    return RotateFlip.Rotate180FlipNone;
                case 1:
                    return RotateFlip.Rotate270FlipNone;
                case 2:
                    return RotateFlip.RotateNoneFlipNone;
                case 3:
                    return RotateFlip.Rotate90FlipNone;
            }
            return RotateFlip.RotateNoneFlipNone;
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

        static private readonly Dictionary<string, int> ColorIndexes = new Dictionary<string, int>()
        {
            {"white",      0},
            {"orange",     1},
            {"magenta",    2},
            {"light_blue", 3},
            {"yellow",     4},
            {"lime",       5},
            {"pink",       6},
            {"gray",       7},
            {"silver",     8},
            {"cyan",       9},
            {"purple",     10},
            {"blue",       11},
            {"brown",      12},
            {"green",      13},
            {"red",        14},
            {"black",      15},
            {"undyed",     16},
        };

        static private readonly Dictionary<string, int> CobblestoneWallIndexes = new Dictionary<string, int>()
        {
            {"cobblestone",       0},
            {"mossy_cobblestone", 1},
            {"granite",           2},
            {"diorite",           3},
            {"andesite",          4},
            {"sandstone",         5},
            {"brick",             6},
            {"stone_brick",       7},
            {"mossy_stone_brick", 8},
            {"nether_brick",      9},
            {"end_brick",        10},
            {"prismarine",       11},
            {"red_sandstone",    12},
            {"red_nether_brick", 13},
        };

        static private readonly Dictionary<string, int> WoodIndexes = new Dictionary<string, int>()
        {
            {"oak",      0},
            {"spruce",   1},
            {"birch",    2},
            {"jungle",   3},
            {"acacia",   4},
            {"dark_oak", 5},
        };

        static private readonly Dictionary<int, Dictionary<string, int>> StoneSlabIndexes = new Dictionary<int, Dictionary<string, int>>()
        {
            {1, new Dictionary<string, int>() {
                {"smooth_stone", 0},
                {"sandstone", 1},
                // {"planks"?, 2},
                {"cobblestone", 3},
                {"brick", 4},
                {"stone_brick", 5},
                {"quartz", 6},
                {"nether_brick", 7},
            }},
            {2, new Dictionary<string, int>() {
                {"red_sandstone", 0},
                {"purpur", 1},
                {"prismarine_rough", 2},
                {"prismarine_dark", 3},
                {"prismarine_brick", 4},
                {"mossy_cobblestone", 5},
                {"smooth_sandstone", 6},
                {"red_nether_brick", 7},
            }},
            {3, new Dictionary<string, int>() {
                {"end_stone_brick", 0},
                {"smooth_red_sandstone", 1},
                {"polished_andesite", 2},
                {"andesite", 3},
                {"diorite", 4},
                {"polished_diorite", 5},
                {"granite", 6},
                {"polished_granite", 7},
            }},
            {4, new Dictionary<string, int>() {
                {"mossy_stone_brick", 0},
                {"smooth_quartz", 1},
                {"stone", 2},
                {"cut_sandstone", 3},
                {"cut_red_sandstone", 4},
            }},
        };

        static private readonly Dictionary<string, int> StoneIndexes = new Dictionary<string, int>()
        {
            {"stone",           0},
            {"granite",         1},
            {"granite_smooth",  2},
            {"diorite",         3},
            {"diorite_smooth",  4},
            {"andesite",        5},
            {"andesite_smooth", 6},
        };

        static private readonly Dictionary<string, int> MonsterEggIndexes = new Dictionary<string, int>()
        {
            {"cobblestone",          0},
            {"stone_brick",          1},
            {"mossy_stone_brick",    2},
            {"cracked_stone_brick",  3},
            {"chiseled_stone_brick", 4},
            {"stone",                5},
        };

        static private readonly Dictionary<string, int> StoneBrickIndexes = new Dictionary<string, int>()
        {
            {"default",  0},
            {"mossy",    1},
            {"cracked",  2},
            {"chiseled", 3},
        };

        static private readonly Dictionary<int, int> GrowthEightToFour = new Dictionary<int, int>()
        {
            {0, 0},
            {1, 0},
            {2, 1},
            {3, 1},
            {4, 2},
            {5, 2},
            {6, 2},
            {7, 3},
        };

        static private readonly Dictionary<string, int> RedFlowerIndexes = new Dictionary<string, int>()
        {
            {"poppy",               0},
            {"orchid",              1},
            {"allium",              2},
            {"houstonia",           3},
            {"tulip_red",           4},
            {"tulip_orange",        5},
            {"tulip_white",         6},
            {"tulip_pink",          7},
            {"oxeye",               8},
            {"cornflower",          9},
            {"lily_of_the_valley", 10},
        };

        static private readonly Dictionary<string, int> DoublePlantIndexes = new Dictionary<string, int>()
        {
            {"sunflower", 0},
            {"syringa",   1},
            {"grass",     2},
            {"fern",      3},
            {"rose",      4},
            {"paeonia",   5},
        };

        static private readonly Dictionary<string, int> CoralIndexes = new Dictionary<string, int>()
        {
            {"blue",    0},
            {"pink",    1},
            {"purple",  2},
            {"red",     3},
            {"yellow",  4},
        };

        static private readonly Dictionary<int, RotateFlip> CoralRotations = new Dictionary<int, RotateFlip>()
        {
            {0, RotateFlip.Rotate270FlipNone},
            {1, RotateFlip.Rotate90FlipNone},
            {2, RotateFlip.RotateNoneFlipNone},
            {3, RotateFlip.Rotate180FlipNone},
        };

        private TextureStack GetTexture(string name, int data = 0, TextureTranslation translation = null, RotateFlip rot = RotateFlip.RotateNoneFlipNone)
        {
            var dictData = new Dictionary<string, Object>();
            dictData.Add("val", data);
            return GetTexture(name, dictData, translation, rot);
        }
        private TextureStack GetTexture(string name, Dictionary<string, Object> data, TextureTranslation translation = null, RotateFlip rot = RotateFlip.RotateNoneFlipNone)
        {
            string texturePath = null;
            if (texturesJson.ContainsKey(name))
            {
                var texture = texturesJson[name];
                texturePath = texture.Subtextures.First().Path;
                foreach(var blockProperties in data)
                {
                    if(blockProperties.Key == "val")
                    {
                        int intValue = (int)blockProperties.Value;
                        try
                        {
                            texturePath = texture.Subtextures[intValue].Path;
                        }
                        catch{}
                    }
                    if(blockProperties.Key == "color")
                    {

                        int colorIndex = ColorIndexes.First().Value;
                        try
                        {
                            string color = (string)data["color"];
                            colorIndex = ColorIndexes[color];
                            texturePath = texture.Subtextures[colorIndex].Path;
                        }
                        catch
                        {
                            Console.WriteLine("Cannot find color for " + name);
                        }
                    }
                    if(blockProperties.Key == "wall_block_type")
                    {
                        int wallBlockIndex = CobblestoneWallIndexes.First().Value;
                        try
                        {
                            string wallBlock = (string)data["wall_block_type"];
                            wallBlockIndex = CobblestoneWallIndexes[wallBlock];
                            texturePath = texture.Subtextures[wallBlockIndex].Path;
                        }
                        catch
                        {
                            Console.WriteLine("Cannot find wall type for " + name);
                        }
                    }
                    if(blockProperties.Key == "facing_direction")
                    {
                        int direction = (int)data["facing_direction"];
                        switch (direction)
                        {
                            case 2:
                                rot = RotateFlip.Rotate180FlipNone;
                                break;
                            case 3:
                                rot = RotateFlip.RotateNoneFlipNone;
                                break;
                            case 4:
                                rot = RotateFlip.Rotate90FlipNone;
                                break;
                            case 5:
                                rot = RotateFlip.Rotate270FlipNone;
                                break;
                        }
                    }
                }
            }

            if (texturePath == null)
                return null;
            return new TextureStack(texturePath, translation, rot);
        }

        private int LegacyGetOldDataValue (Dictionary<string, Object> data)
        {
            int result = 0;
            if(data.ContainsKey("val"))
            {
                try
                {
                    result = (int)data["val"];
                }
                catch {}
            }
            return result;
        }
    }
}