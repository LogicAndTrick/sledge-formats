# Half-Life Formats Library

These C# libraries parse the formats you'll find in your Half-Life install directory. These are low-level libraries intended for developers. Some additional formats are included to include other related formats, such as those found in the Source and Quake engines.

## Install using NuGet

Coming soon™

## Currently Supported Formats

- Sledge.Formats - Small formats, or formats that are shared
    - Valve
        - **Liblist** - The format used for the liblist.gam file.
        - **SerialisedObject** - The format used for many Valve config files used in Steam HL, Source games, and Steam. Some file types that use this format are VMT, VMF, RES, VDF, gameinfo.txt, and many text files found in Source game directories.
- Sledge.Formats.Map - Map source files used by level editors
    - Formats
        - **HammerVmfFormat** - The format used by Valve Hammer Editor 4 for map source files.
        - **QuakeMapFormat** - The format used by most Quake engines for map source files.
            - Supports the formats used in Quake 1 (idTech2) and Half-Life 1 .map files.
            - idTech3 and idTech4 .map files are not currently supported.
        - **WorldcraftRmfFormat** - The format used by Valve Hammer Editor 3 for map source files.
            - Only RMF version 2.2 (Worldcraft 3.3 and up) is currently supported.
- Sledge.Formats.Bsp - Compiled map files used by the engine
    - **BspFile** - A format used by Quake based engines for compiled maps.
        - Currently supports Quake 1 (v29), Quake 2 (IBSP v38), and Half-Life 1 (v30) bsp formats.
        - Not currently supported: BSP2 (DarkPlaces engine), Quake 3 (IBSP v46), Source (VBSP v17-21)
        - Currently, visibility data is not parsed, it is kept as a binary blob.
        - Editing of lightmap data is currently not well supported and must be done manually.
        - The library does no checking to ensure that the indexes and offsets are correct. Possibly a higher-level library could wrap around this format to provide developers with a more flexible BSP creation experience.
- Sledge.Formats.Packages - File package formats used by Quake/HL1/Source
    - **PakPackage** - The PAK format used in Quake 1/2 and non-Steam Half-Life.
    - **VpkPackage** - The VPK format used by post-SteamPipe Source engine games.
- Sledge.Formats.Texture - Texture formats used by Quake and Half-Life
    - Wad
        - **WadFile** - The WAD format used by Quake 1 and Goldsource to store textures
            - Currently supports Quake 1 (WAD2) and Goldsource (WAD3) formats
            - The ColorMap and ColorMap2 will load, but won't contain any data. These lump types aren't used anywhere.
            - Quake 1's gfx.wad contains a lump called "CONCHARS", which has an invalid type. There's special logic to handle this lump.
    - Vtf
        - **VtfFile** - The VTF format used by the Source engine.
            - Currently supports all formats that VtfLib supports (read only, not write)

## Unsupported formats (may be added in the future)

- **FGD**, used for entity definitions in Worldcraft and Valve Hammer Editor.
- **JMF**, used for Jackhammer/JACK editor for map source files.
- **MDL**, used for models in many Quake-based games.
    - **MDL v10** (IDSQ/IDST), used in Half-Life, which adds skeletal animation.
    - **MDL v44-49** (IDSQ/IDST), used in Source, along with VTX, VVD, ANI, and PHY files. This is a very complex format, so it's not high priority.

## Unsupported formats (probably won't be added)

- **GCF**, used by pre-SteamPipe Steam Half-Life - this format is no longer in use, so it's not really useful to create a library for it.
- **PK3**, used in Quake 3 games - this format is just a zip file with a different extension. Other libraries (including .NET itself) already have good support for zip files.
- **WAD1**, used in the Doom engine - this is a bit too far out of scope for this project, which is focused mostly on Half-Life 1.
- **MDL v6** (IDPO), **MD2** and **MD3**, used for models in non-Valve Quake engines - MDL formats are extremely complex and not very well documented, so these are considered out of scope for this project for now.
- Anything introduced in Doom 3, Source 2 or newer engines.