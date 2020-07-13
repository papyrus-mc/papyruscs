Chat: [![Discord](https://img.shields.io/discord/569841820092203011.svg?logo=discord&logoColor=white)](https://discord.gg/J2sBaXa) <br>
Windows: [![Build status](https://ci.appveyor.com/api/projects/status/tfspbbi72bx73qg8?svg=true)](https://ci.appveyor.com/project/mjungnickel18/papyruscs) <br>
Linux: [![Build status](https://ci.appveyor.com/api/projects/status/xo9ew31l49hayjcm?svg=true)](https://ci.appveyor.com/project/mjungnickel18/papyruscs-ytqjm) <br>

# Papyrus

A Minecraft: Bedrock Edition map renderer.

Papayrus renders a top-down map of every already explored chunk.
It works on Windows and on Linux, and it outputs HTML and images suitable for displaying on the web.

* [Intro](#intro)
* [Setup](#setup)
* [Usage](#usage)
* [Building](#building)
* [Contributing](#contribute-xor-support)
* [What's new - the changelog](#changelog)
* [Other notes](#other-notes)

## Intro
Papyrus is a tool to render Minecraft: Bedrock Edition (MCBE) worlds on the web.
It's written in C# and powered by .NET Core.
You can view an example [here](http://papyrus.gwsa.de/).

There are several tools which work for Java Edition.
MCBE worlds don't use the Anvil format like in the Java Edition.
Mojang chose a [modified version](https://github.com/Mojang/leveldb-mcpe) of Google's [LevelDB](http://leveldb.org/) to save MCBE maps.
Papyrus reads these worlds and assembles a render of every pre-generated chunk.


## Contribute xor support
If you want to help improving Papyrus please consider forking the repository, making your changes, and proposing a PR.

Want to buy me a coffee (I love coffee)? [Donate via PayPal ♥](https://paypal.me/mjungnickelpapyrus)

## Setup

### Easy mode - Windows
- Grab the Windows [pre-built binaries](https://github.com/mjungnickel18/papyruscs/releases).
- Unpack the zip file.
- Optional: [update the textures](#updating-the-textures)
- Then follow the [usage instructions](#usage).

### Easy mode - Linux
- Grab the Linux [pre-built binaries](https://github.com/mjungnickel18/papyruscs/releases).
- Make sure that your graphics libs are up-to-date.
On Ubuntu/Debian, that looks like:
```
sudo apt-get update
sudo apt-get install libgdiplus
sudo apt-get install libc6-dev
```
- Make the `PapyrusCs` binary executable:
```chmod +x PapyrusCs```
- Optional: [update the textures](#updating-the-textures)
- Then follow the [usage instructions](#usage).

### Hard mode - build it yourself

See the [building](#building) instructions.

### Updating the textures
Papyrus comes with some of the required textures, but recent MCBE releases may have added more.
You can update with a texture pack in order to generate everything just right.

The vanilla resource pack with the default textures can be downloaded from [here](https://aka.ms/resourcepacktemplate).
Extract `textures/terrain_texture.json` and `textures/blocks/*` from that zip.
Wherever you unzipped Papyrus, replace its `textures/` directory with what you got from the resource pack.

## Usage

PapyrusCs is a command-line tool.
Supported flags are shown here.
`--world` and `--output` are required; the rest are optional.

```papyruscs --world "My World/db" --output "C:\papyrus"```

On Windows, you can now start `papyruscs.exe` in interactive mode.
Run without any arguments, and it will try to find your local minecraft maps.

The rest of the parameters are listed if you run with `--help`:

```

PapyrusCs 0.5.0
Copyright (C) 2020 Markus Jungnickel

  -w, --world                     Sets the path the Minecraft Bedrock Edition Map

  -o, --output                    Sets the output path for the generated map tiles

  --htmlfile                      (Default: map.html) Sets name of html map file

  -s, --strategy                  (Default: Dataflow) Sets the render strategy. Valid is only Dataflow

  --coords                        (Default: false) Render text coordinates in each chunk

  --limitx                        Limits the chunk rendering in the x dimension (inclusive). Provide two
                                  values with comma separated, eg: -10,10

  --limitz                        Limits the chunk rendering in the z dimension (inclusive). Provide two
                                  values with comma separated, eg: -10,10

  -y, --limity                    (Default: -1) Limits the chunk rendering in the y dimension
                                  (inclusive). For y provide just one positive value, eg: 10. -1 means:
                                  all

  --threads                       (Default: 16) Set maximum of used threads

  --maxqueue                      (Default: 128) Set maximum queue length for the pipeline stages

  -r, --rendermode                (Default: Heightmap) RenderMode: Basic - Render without brightness
                                  adjustment. Heightmap - Render with brightness adjustment based on
                                  brillouin function and height of block

  --brillouin_j                   (Default: 10000) Sets factor j for heightmap brightness formula
                                  brillouin: brightness = 1+brillouin((height-offset) / divider): See
                                  https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the
                                  function.

  --brillouin_divider             (Default: 20) Sets divider for heightmap brightness formula brillouin:
                                  brightness = 1+brillouin((height-offset) / divider). See
                                  https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the
                                  function.

  --brillouin_offset              (Default: 64) Sets the offset for heightmap brightness formula
                                  brillouin: brightness = 1+brillouin((height-offset) / divider). See
                                  https://de.wikipedia.org/wiki/Brillouin-Funktion for a diagram of the
                                  function.

  -f                              (Default: png) Sets the output file format

  -q                              (Default: -1) Sets quality for jpg or web format (0-100, -1 for
                                  lossless webp)

  --forceoverwrite                (Default: false) Forces PapyrusCs to render every chunk again

  -d, --dim                       (Default: 0) Selects the dimension. 0: Overworld, 1: Nether, 2: End

  -p, --profile                   (Default: ) Optional profile: 'underground', 'aquatic', 'ore',
                                  'stronghold'

  --trimceiling                   (Default: false) Removes the ceiling starting from y-max.
                                  Automatically activated for nether

  --notrimceiling                 (Default: false) Disables auto ceiling removal for Nether

  --deleteexistingupdatefolder    (Default: false) Delete an existing update/dimX folder right before
                                  creating a possible new update

  --chunksperdimension            (Default: 2) Sets the chunks per X and Y dimension for the generated
                                  tiles. 1 => 1 chunk per tile, 2 => 4 chunks per tile and so on

  --playericons                   (Default: false) Renders player markers on the map. Player names must
                                  be manually entered. After running, edit '/map/playersData.js' text
                                  file to modify player names and colors.

  --render_map                    (Default: true) Renders the map. This is the main feature of this
                                  program. Only disable this in special circumstances, such as if you
                                  want to quickly update player markers without updating the map.

  --help                          Display this help screen.

  --version                       Display version information.


```


## Building
Papyrus runs on .NET Core 3.0+.
First, make sure you have [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0) on your development machine.

To compile for Windows, either use a recent Visual Studio or run:
```dotnet publish PapyrusCs -c Release --self-contained --runtime win-x64```

To compile for Linux run:
```dotnet publish PapyrusCs -c Debug --self-contained --runtime linux-x64```

### Additional dependencies
PapyrusCs depends on a few external libraries with some custom changes.
The source for the modified versions is included in this repository.
All needed dependencies are already included in this repository or automatically downloaded via NuGet.

- [level-db-sharp](https://github.com/meebey/leveldb-sharp): This project used a modified version of leveldb-sharp. [license](https://github.com/mjungnickel18/papyruscs/blob/master/leveldb-sharp-std/license.md)
- [imageformats](https://github.com/dbrant/imageformats): This project uses a modified version of image formats TgaReader
[license](https://github.com/mjungnickel18/papyruscs/blob/master/Imports/ImageFormats-std/license.md)
- [leveldb-mcpe](https://github.com/Mojang/leveldb-mcpe): This project uses the leveldb native library for windows64 and linux64. [license](https://github.com/Mojang/leveldb-mcpe/blob/master/LICENSE)


## Changelog

## Version 0.5.0 - So many things
Update to .NET Core 3.1
Updated textures
- Fixed null ref exception with bee hives
- Interactive mode (Thx to aaronaxvig)
- Crc32 Fix (Thx to bennydiamond)
- Updated Readme (Thx to PaulWalkerUK)
- Player Markser (thx to barrett777)


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

## Other notes

### Special thanks to...
... [clarkx86](https://github.com/clarkx86) for his team work and idea giving and his port in node.js.

### Disclaimer
Papyrus is in no way affiliated with Mojang or Minecraft.

Contact: [papyrus@gwsa.de](mailto:papyrus@gwsa.de?subject=GitHub%20Papyrus)
