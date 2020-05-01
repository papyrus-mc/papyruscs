using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PapyrusCs
{
    public partial class Program
    {
        private static bool InteractiveMode(Options options)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("World not specified.  Please specify one using the --world parameter.");
            }
            else
            {
                Console.WriteLine("World not specified.  Looking for worlds in the default Bedrock Edition worlds folder.");

                try
                {
                    // Get all of the world folders that exist
                    string worldsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Properties.Resources.DefaultBedrockWorldsLocation);
                    string[] worldDirectories = { };

                    try
                    {
                        worldDirectories = Directory.GetDirectories(worldsFolder);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Console.WriteLine("Bedrock Edition worlds folder not found.  Please specify a world manually using the --world parameter.");
                        {
                            return false;
                        }
                    }

                    if (worldDirectories.Length == 0)
                    {
                        Console.WriteLine("No worlds found.  Please specify one using the --world parameter.");
                        {
                            return false;
                        }
                    }

                    Console.WriteLine($"Found {worldDirectories.Length} worlds:");

                    // Print out the list of worlds
                    for (int i = 0; i < worldDirectories.Length; i++)
                    {
                        string worldNameFilePath = Path.Combine(worldDirectories[i], Properties.Resources.WorldNameFile);
                        string worldName = File.ReadLines(worldNameFilePath).First();
                        Console.WriteLine($"{i} - {worldName}");
                    }

                    // Make the user choose one of the worlds
                    while (String.IsNullOrEmpty(options.MinecraftWorld))
                    {
                        Console.Write("Type a world number and press Enter: ");
                        string userInput = Console.ReadLine();
                        bool parseSuccess = Int32.TryParse(userInput, out int worldNumber);

                        if (!parseSuccess)
                        {
                            Console.WriteLine($"'{userInput}' was not recognized as a number.");
                        }
                        else if ((worldNumber < 0) || (worldNumber >= worldDirectories.Length))
                        {
                            Console.WriteLine($"There is no world #{worldNumber}");
                        }
                        else
                        {
                            options.MinecraftWorld = Path.Combine(worldDirectories[worldNumber], Properties.Resources.WorldDatabaseFolder);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
