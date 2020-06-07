Chat: [![Discord](https://img.shields.io/discord/569841820092203011.svg?logo=discord&logoColor=white)](https://discord.gg/J2sBaXa) <br>
Windows: [![Build status](https://ci.appveyor.com/api/projects/status/tfspbbi72bx73qg8?svg=true)](https://ci.appveyor.com/project/mjungnickel18/papyruscs) <br>
Linux: [![Build status](https://ci.appveyor.com/api/projects/status/xo9ew31l49hayjcm?svg=true)](https://ci.appveyor.com/project/mjungnickel18/papyruscs-ytqjm) <br>

## Contribute xor support
If you want to help improving Papyrus please consider forking the repository.

Want to buy me a coffee (I love coffee)? [Donate via PayPal ♥](https://paypal.me/mjungnickelpapyrus)

## Version 0.4.0 - Memory usage and textures improvements
A few tweaks to improve memory usage:

use parameter --threads 1 --maxqueue 1 to limit memory usage.

This version also includes two pull requests:
 * Significantly improve memory usage when rendering zoom levels (#21 by barrett777)
 * Fixed bunch of annoyances (#19 by jflin404)

Thx for the help!
Thx to Mecu for updating the docs!


## Version 0.3.7 - .NET Core 3 Update
Update to .NET Core 3
Changed link to openlayers
Some other optimizations...

## Version 0.3.6
Less memory usage and faster!
More robust!

## Version 0.3.5
Thx to @hach-que!
```
--profile underground|aquatic|ore|stronghold
```
add underground, aquatic, ore OR stronghold after `--profile` for the specific profile view.

## Version 0.3.3
OpenLayers as new default mapping library! Thanks for the contribution!
Use
```
--use_leaflet_legacy
```
options if you want to use the old leaflet instead.

## Version 0.3.2
Texture fixes\
--brillouin_offset x parameter for worlds with low/high ground level\
--forceoverwrite parameter to force reender everything\

## Version 0.3
### What's new:
#### New folder structure:
The new folder structure is:
```
outputdir
|-map
|   |-dim0
|   |-dim1
|   |-dim2
|   |-map.html
|   |-playersData.js
|-update
|   |-dim0
|   |-dim1
|   |-dim2
|   |-map.html
|   |-playersData.js
|-chunks.sqlite
|-chunks-backup.sqlite
```
#### Incremental Rendering:

With strategy Dataflow (default) PapyrusCs creates a sqlite database with chunk CRCs to check if a update rendering is necessary.
PapyrusCs creates an update folder, which contains only the files that were updated (it will be copied to the map folder as well)
So you just have to upload the content of the update folder after an update of the map.

With the option 
```
--deleteexistingupdatefolder
```
the current dimension update folder to be rendered (dim0/1/2) will be deleted before the next update will be rendered.
This is useful if you don't want to upload previous updated files again. Of course you can delete this folder by hand yourself.

#### Dimensions
PapyrusCs supports dimensions now: Overworld, Nether, The End\
To render another dimension, add the --dim <dimension> parameter (default --dim 0):\
```
--dim 0 (=Overworld)
--dim 1 (=Nether)
--dim 2 (=The End)
```
To render all dimensions in one map with layers [See papyrus bedrock server](http://papyrus.gwsa.de/), you have to call PapyrusCs three times. Example:\
```
PapyrusCs -w <yourworld> -o <youroutputfolder> -dim 0
PapyrusCs -w <yourworld> -o <youroutputfolder> -dim 1
PapyrusCs -w <yourworld> -o <youroutputfolder> -dim 2
```  
This will generate all dimensions as maps.\

#### Outputformat
Multiple formats are now supported: jpg, png, webp\
You now can select the output format with -f jpg or -f png or -f webp\
For jpg and webp, you can select the quality:\

jpg: 0-100 (100 best)\
webp: 0-100 (100 best but lossy. Value -1 is lossless and usually smaller than 100, -1 is also default)\

You can set that value with the parameter -q

Example
```
-f webp -q -1
-f png
-f jpg -q 20
```

## papyrus.cs
Papyrus is a tool to render Minecraft: Bedrock Edition (from now on referenced as "MCBE") worlds using Leaflet. It is written in C# and powered by .NET Core 2.2.
It currently runs only under windows, but support for linux is planned.

You can view an example [here](http://papyrus.gwsa.de/).

## Introduction
Since MCBE worlds don't use the Anvil format like in the Java Edition, but rather a by Mojang [modified version](https://github.com/Mojang/leveldb-mcpe) of Google's [LevelDB](http://leveldb.org/) to save, the goal of Papyrus is to read these worlds and assemble a render of every pre-generated chunk.

## Features
- Render a top-down map of every already explored chunk
- Windows and Linux Support

#### Planned

- Isometric renders

## Installation
Otherwise, just grab one of the [pre-built binaries](https://github.com/mjungnickel18/papyruscs/releases).
Just unpack the zip-file.

### Linux
Make sure that your graphics libs are up-to-date: (Syntax may vary in other distros) 
```
sudo apt-get update
sudo apt-get install libgdiplus
sudo apt-get install libc6-dev
```

And give PapyrusCs execution rights:
```chmod +x PapyrusCs```

For own compilation:
Requires [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0). You may need to install [additional dependencies](https://github.com/mjungnickel18/papyruscs#additional-dependencies).


## Usage
For Linux: give the extracted PapyrusCs file execution rights! See installation notes above. 

```papyruscs --world "My World/db" --output "C:\papyrus"```

```

  -w, --world                     Sets the path the Minecraft Bedrock Edition Map.  When not specified, searches 
                                  Bedrock Edition worlds folder and lets you chose from found worlds.

  -o, --output                    (Default: generatedmaps/<world name>) Sets the output path for the generated map tiles

  --htmlfile                      (Default: map.html) Sets name of html map file

  -s, --strategy                  (Default: Dataflow) Sets the render strategy. Valid are Dataflow, SingleFor and
                                  ParallelFor (Multithreaded)

  --coords                        (Default: false) Render text coordinates in each chunk

  --limitx                        Limits the chunk rendering in the x dimension (inclusive). Provide two values with
                                  comma separated, eg: -10,10

  --limitz                        Limits the chunk rendering in the z dimension (inclusive). Provide two values with
                                  comma separated, eg: -10,10

  -y, --limity                    (Default: -1) Limits the chunk rendering in the y dimension (inclusive). For y
                                  provide just one positive value, eg: 10. -1 means: all

  --threads                       (Default: 16) Set maximum of used threads

  --maxqueue                      (Default: 128) Set maximum queue length for the pipeline stages

  -r, --rendermode                (Default: Heightmap) RenderMode: Basic - Render without brightness adjustment.
                                  Heightmap - Render with brightness adjustment based on brillouin function and height
                                  of block

  --brillouin_j                   (Default: 10000) Sets factor j for heightmap brightness formula brillouin:
                                  brightness = 1+brillouin((height-offset) / divider): See
                                  https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the function.

  --brillouin_divider             (Default: 20) Sets divider for heightmap brightness formula brillouin: brightness =
                                  1+brillouin((height-offset) / divider). See
                                  https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the function.

  --brillouin_offset              (Default: 64) Sets the offset for heightmap brightness formula brillouin: brightness
                                  = 1+brillouin((height-offset) / divider). See
                                  https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the function.

  -f                              (Default: png) Sets the output file format

  -q                              (Default: -1) Sets quality for jpg or web format (0-100, -1 for lossless webp)

  --forceoverwrite                (Default: false) Forces PapyrusCs to render every chunk again

  -d, --dim                       (Default: 0) Selects the dimension. 0: Overworld, 1: Nether, 2: End

  -p, --profile                   (Default: ) Optional profile: 'underground', 'aquatic', 'ore', 'stronghold'
  
  --trimceiling                   (Default: false) Removes the ceiling starting from y-max. Automatically activated
                                  for nether

  --notrimceiling                 (Default: false) Disables auto ceiling removal for Nether

  --deleteexistingupdatefolder    (Default: false) Delete an existing update/dimX folder right before creating a
                                  possible new update

  --use_leaflet_legacy            (Default: false) Use the legacy leaflet.js map renderer instead of the new
                                  OpenLayers version
 
  --chunksperdimension            (Default: 2) Sets the chunks per X and Y dimension for the generated tiles. 1 => 1 chunk per tile, 2
                                  => 4 chunks per tile and so on

  --playericons                   (Default: true) Renders player markers on the map. Player names must be manually entered.
                                  After running, edit '/map/playersData.js' text file to modify player names and colors.
                                  Updated player names and colors will be preserved when re-running
								  Currently, only players on the Overworld are supported
                                  Note: Not compatible with 'use_leaflet_legacy'

  --render_map                    (Default: true) Renders the map. This is the main feature of this program.
                                  Only disable this in special circumstances, such as if you want to
                                  quickly update player markers without updating the map.
  --help                          Display this help screen.

  --version                       Display version information.
```

The vanilla resource pack with the default textures can be downloaded from [here](https://aka.ms/resourcepacktemplate).

## Compilation
To compile for windows use either Visual Studio or run:
```dotnet publish PapyrusCs -c Release --self-contained --runtime win-x64```

To compile for Linux run:
```dotnet publish PapyrusCs -c Debug --self-contained --runtime linux-x64```

## Additional dependencies
- [level-db-sharp](https://github.com/meebey/leveldb-sharp): This project used a modified version of leveldb-sharp. [license](https://github.com/mjungnickel18/papyruscs/blob/master/leveldb-sharp-std/license.md)
- [imageformats](https://github.com/dbrant/imageformats): This project uses a modified version of image formats TgaReader
[license](https://github.com/mjungnickel18/papyruscs/blob/master/Imports/ImageFormats-std/license.md)
- [leveldb-mcpe](https://github.com/Mojang/leveldb-mcpe): This project uses the leveldb native library for windows64 and linux64. [license](https://github.com/Mojang/leveldb-mcpe/blob/master/LICENSE)

The source for the modified versions is included in this repository.
All needed depencies are already included in this repository or automatically downloaded via NuGet.

## Special thanks to...
... [clarkx86](https://github.com/clarkx86) for his team work and idea giving and his port in node.js.

## Disclaimer
Papyrus is in no way affiliated with Mojang or Minecraft.

Contact: [papyrus@gwsa.de](mailto:papyrus@gwsa.de?subject=GitHub%20Papyrus)
