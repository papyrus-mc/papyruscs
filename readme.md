# papyrus.cs
Papyrus is a tool to render Minecraft: Bedrock Edition (from now on referenced as "MCBE") worlds using Leaflet. It is written in C# and powered by .NET Core 2.2

You can view an example [here](http://gwsa.de/minecraft/map.html).

## Introduction
Since MCBE worlds don't use the Anvil format like in the Java Edition, but rather a by Mojang [modified version](https://github.com/Mojang/leveldb-mcpe) of Google's [LevelDB](http://leveldb.org/) to save, the goal of Papyrus is to read these worlds and assemble a render of every pre-generated chunk.

## Features
- Render a top-down map of every already explored chunk
#### Planned
- Isometric renders
- Auto-Updating renders
- Live-View of currently online players on map (and their respective statistics)
- Nether/ The End support


## Installation
Just unpack the zip-file.

Requires .NET Core 2.2. You may need to install [additional dependencies](https://github.com/mjungnickel18/papyruscs#additional-dependencies).

Otherwise, just grab one of the [pre-built binaries](https://github.com/clarkx86/papyruscs/releases).

## Usage
```papyrus --path "My World" --textures "Vanilla_Resource_Pack.zip" --output "C:\papyrus" --mode papyrus```

You'll have to define a path to the texture pack you want to use for the final render. The vanilla resource pack with the default textures can be downloaded from [here](https://aka.ms/resourcepacktemplate).

## Additional dependencies
- [level-db-sharp](https://github.com/meebey/leveldb-sharp): This project requires a modified version of leveldb-sharp.

All needed depencies are already included in this repository or automatically downloaded via NuGet

## Contribute xor support
If you want to help improving Papyrus please consider forking the repository.

Want to buy me a coffee (I love coffee)? [Donate via PayPal ♥](https://paypal.me/mjungnickelpapyruscs)

## Special thanks to...
... [clarkx86](https://github.com/clarkx86).

## Disclaimer
Papyrus is in no way affiliated with Mojang or Minecraft.

Contact: [r@gwsa.de](mailto:r@gwsa.de?subject=GitHub%20Papyrus)
