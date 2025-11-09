Hex of Steel – Tile Ping Mod
====================================================================

Hos Ping Mod adds quick tile pings to highlight tiles using Alt+Left Click.

====================================================================

Features
--------
- Alt+LeftClick any tile on the map to create a ping highlight.
- Built-in 0.4 s cooldown prevents ping spam.
- Different color for each player to differenciate pings.

Build
-----
1. Install the .NET 8 SDK and your preferred C# IDE (Visual Studio, Rider, or VS Code).
2. Open hos_ping_mod.sln.
3. Build the project; HosPingMod.dll is emitted to output/net48/.
4. Run deploy.py to create a mod folder.

Installation
------------
1. Drop the package (mod folder) in the hex of steel MOD directory
2. Launch Hex of Steel, enable the ping mod in the Mods menu (and under Harmony), and start a multiplayer game.

Troubleshooting
---------------
- Check Player.log if pings stop appearing or the mod fails to load.
  - Windows: %USERPROFILE%/AppData/LocalLow/War Frogs Studio/Hex of Steel/Player.log
  - macOS: ~/Library/Logs/War Frogs Studio/Hex of Steel/Player.log
  - Linux: ~/.config/unity3d/War Frogs Studio/Hex of Steel/Player.log
- Make sure no other mod replaces TileGO.OnMouseOver without chaining postfix patches.
