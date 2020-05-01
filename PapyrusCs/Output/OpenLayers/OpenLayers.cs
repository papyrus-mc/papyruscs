using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Maploader.World;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PapyrusAlgorithms.Database;

namespace PapyrusCs.Output.OpenLayers
{
    public class OpenLayers
    {
        private static readonly string[] randomPlayerMapIconColors = {
            // You can also use hex colors "#FFFFFF" and rgb "rgb(128, 128, 128)"
            "DeepPink",
            "DarkRed",
            "DarkOrange",
            "Gold",
            "SaddleBrown",
            "DarkGreen",
            "Teal",
            "DarkBlue",
            "Purple",
            "SlateGray"
        };

        public void OutputMap(int tileSize, string outputPath, string mapHtmlFile, Settings[] settings,
            bool isUpdate, bool useLegacyLeaflet, bool showPlayerIcons, World world)
        {
            WriteMapHtml(tileSize, outputPath, mapHtmlFile, settings, isUpdate, useLegacyLeaflet);
            AddPlayerIcons(outputPath, showPlayerIcons, world);
        }

        public void AddPlayerIcons(string outputPath, bool showPlayerIcons, World world)
        {
            // Note: Use a .js file instead of .json because there are CORS issues if the user wants to view the .html file from their local file system instead of a web server
            var playersDataJsonFile = Path.Combine(outputPath, "map", "playersData.js");
            var playersDataJsonFileForUpdate = Path.Combine(outputPath, "update", "playersData.js");

            if (showPlayerIcons)
            {
                Console.WriteLine("Retrieving player data and writing to json file");

                const string playersDataJsonFileTemplate =
                    @"// NOTE: Please only modify player attributes below such as name, color, and visible
// To hide a player's marker, change their 'visible' variable from 'true' to 'false'
// This file is automatically updated, and if other parts are changed, it will fail to update
// Changes will only be read from the /map/playersData.js file - /update/playersData.js will be overwritten, so don't make changes there
var playersData = // # INJECT DATA HERE;";

                if (!File.Exists(playersDataJsonFile))
                {
                    File.WriteAllText(playersDataJsonFile,
                        playersDataJsonFileTemplate.Replace(
                            "// # INJECT DATA HERE",
                            "{ players: [] }"));
                }

                var existingPlayersDataRaw = File.ReadAllText(playersDataJsonFile);
                var existingPlayersData = JObject.Parse(existingPlayersDataRaw.Substring(existingPlayersDataRaw.IndexOf('=') + 1).Trim().TrimEnd(';'));

                var random = new Random();

                foreach (var (uuid, name, dimensionId, position) in world.GetPlayerData())
                {
                    var existingPlayerData = existingPlayersData["players"].FirstOrDefault(player => (Guid)player["uuid"] == uuid);

                    if (existingPlayerData == null)
                    {
                        // First time we have seen this player
                        // Add their record to the list, and pick a random color for their icon on the map
                        ((JArray)existingPlayersData["players"]).Add(JObject.FromObject(new
                        {
                            uuid,
                            name,
                            dimensionId,
                            position,
                            color = randomPlayerMapIconColors[random.Next(0, randomPlayerMapIconColors.Length)],
                            visible = true
                        }));
                    }
                    else
                    {
                        // This player has already been added to the list - just update properties that may have changed
                        existingPlayerData["dimensionId"] = dimensionId;
                        existingPlayerData["position"] = new JArray { position[0], position[1], position[2] };
                    }
                }

                File.WriteAllText(playersDataJsonFile,
                    playersDataJsonFileTemplate.Replace(
                        "// # INJECT DATA HERE",
                        JsonConvert.SerializeObject(existingPlayersData, Formatting.Indented)));

                if (Directory.Exists(Path.Combine(outputPath, "update")))
                {
                    File.Copy(playersDataJsonFile, playersDataJsonFileForUpdate, overwrite: true);
                }
            }
            else
            {
                // Player icons disabled
                if (File.Exists(playersDataJsonFile))
                {
                    File.Delete(playersDataJsonFile);
                }

                if (File.Exists(playersDataJsonFileForUpdate))
                {
                    File.Delete(playersDataJsonFileForUpdate);
                }
            }
        }

        public void WriteMapHtml(int tileSize, string outputPath, string mapHtmlFile, Settings[] settings,
            bool isUpdate, bool useLegacyLeaflet)
        {
            try
            {
                var layernames = new Dictionary<string, string>
                {
                    { "dim0", "Overworld" },
                    { "dim0_underground", "Underground" },
                    { "dim0_aquatic", "Aquatic" },
                    { "dim0_ore", "Ores" },
                    { "dim0_stronghold", "Strongholds" },
                    { "dim1", "Nether" },
                    { "dim2", "The End" },
                };

                var mapHtmlContext = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, useLegacyLeaflet ? "map.leaflet.thtml" : "map.thtml"));

                Func<Settings, string> getDimWithProfile = (Settings setting) => "dim" + setting.Dimension + (string.IsNullOrEmpty(setting.Profile) ? "" : $"_{setting.Profile}");

                var layersdef = settings.ToDictionary(
                    getDimWithProfile,
                    setting => new LayerDef
                    {
                        name = layernames.ContainsKey(getDimWithProfile(setting)) ? layernames[getDimWithProfile(setting)] : $"Dimension{setting.Dimension}_{setting.Profile}",
                        attribution = "Generated by <a href=\"https://github.com/mjungnickel18/papyruscs\">PapyrusCS</a>",
                        minNativeZoom = setting.MinZoom,
                        maxNativeZoom = setting.MaxZoom,
                        noWrap = true,
                        tileSize = tileSize,
                        folder = "dim" + setting.Dimension + (string.IsNullOrEmpty(setting.Profile) ? "" : $"_{setting.Profile}"),
                        fileExtension = setting.Format,
                    }
                );

                var globalconfig = new GlobalConfig
                {
                    factor = (Math.Pow(2, settings.First().MaxZoom - 4)),
                    globalMaxZoom = settings.First(x => x.Dimension == settings.Min(y => y.Dimension)).MaxZoom,
                    globalMinZoom = settings.First(x => x.Dimension == settings.Min(y => y.Dimension)).MinZoom,
                    tileSize = tileSize,
                    blocksPerTile = tileSize / 16
                };

                mapHtmlContext = mapHtmlContext.Replace(
                    "// # INJECT DATA HERE",
                    "layers = " + JsonConvert.SerializeObject(layersdef) + "; \r\n" +
                    "config = " + JsonConvert.SerializeObject(globalconfig) + ";");

                Directory.CreateDirectory(Path.Combine(outputPath, "map"));
                File.WriteAllText(Path.Combine(outputPath, "map", mapHtmlFile), mapHtmlContext);
                if (isUpdate)
                {
                    Directory.CreateDirectory(Path.Combine(outputPath, "update"));
                    File.WriteAllText(Path.Combine(outputPath, "update", mapHtmlFile), mapHtmlContext);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write map.html");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
