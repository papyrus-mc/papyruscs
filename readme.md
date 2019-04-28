## papyrus.cs
Papyrus is a tool to render Minecraft: Bedrock Edition (from now on referenced as "MCBE") worlds using Leaflet. It is written in C# and powered by .NET Core 2.2.
It currently runs only under windows, but support for linux is planned.

You can view an example [here](http://gwsa.de/minecraft/map.html).

## Introduction
Since MCBE worlds don't use the Anvil format like in the Java Edition, but rather a by Mojang [modified version](https://github.com/Mojang/leveldb-mcpe) of Google's [LevelDB](http://leveldb.org/) to save, the goal of Papyrus is to read these worlds and assemble a render of every pre-generated chunk.

## Features
- Render a top-down map of every already explored chunk
- Windows and Linux Support

#### Planned

- Nether/ The End support
- Isometric renders
- Auto-Updating renders

## Installation
Just unpack the zip-file.

Requires .NET Core 2.2. You may need to install [additional dependencies](https://github.com/mjungnickel18/papyruscs#additional-dependencies).

Otherwise, just grab one of the [pre-built binaries](https://github.com/mjungnickel18/papyruscs/releases).

## Usage
```papyrus --world "My World/db" --output "C:\papyrus"```

```
  -w, --world       Required. Sets the path the Minecraft Bedrock Edition Map

  -o, --output      (Default: .) Sets the output path for the generated map tiles

  -s, --strategy    (Default: ParallelFor) Sets the render strategy. Valid are SingleFor and ParallelFor (Multithreaded)

  --coords          (Default: true) Render text coordinates in each chunk

  --threads         (Default: 16) Set maximum of used threads

  --help            Display this help screen.

  --version         Display version information.
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

## Contribute xor support
If you want to help improving Papyrus please consider forking the repository.

Want to buy me a coffee (I love coffee)? [Donate via PayPal ♥](https://paypal.me/mjungnickelpapyrus)

## Special thanks to...
... [clarkx86](https://github.com/clarkx86).

## Disclaimer
Papyrus is in no way affiliated with Mojang or Minecraft.

Contact: [papyrus@gwsa.de](mailto:papyrus@gwsa.de?subject=GitHub%20Papyrus)
