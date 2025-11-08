Hex of Steel – Tile Ping Mod
====================================================================

Hos Ping Mod adds quick tile pings to highlight tiles using Alt+Left Click.

====================================================================

Features
--------
- Alt+Left Click any tile on the map to create a ping highlight.
- Built-in 0.4 s cooldown prevents ping spam.
- Ignores clicks while the pointer is over UI, so chats and menus stay usable.
- Works in multiplayer sessions; everyone in the room sees the same highlight.

Controls
--------
- Hold Left Alt or Right Alt, then left-click the tile you want to ping.
- Release the key and click again to ping a new position.
- Pings reuse the game's native highlight and expire based on game rules.

Build
-----
1. Install the .NET 8 SDK and your preferred C# IDE (Visual Studio, Rider, or VS Code).
2. Open hos_ping_mod.sln.
3. Restore references if prompted (Harmony and the game assemblies must be available).
4. Build the project; HosPingMod.dll is emitted to output/net48/.

Installation
------------
1. Create or open a mod folder inside Hex of Steel.
2. Copy output/net48/HosPingMod.dll into the mod's Libraries directory.
3. Launch Hex of Steel, enable the ping mod in the Mods menu (and under Harmony), and start a multiplayer game.

Troubleshooting
---------------
- Check Player.log if pings stop appearing or the mod fails to load.
  - Windows: %USERPROFILE%/AppData/LocalLow/War Frogs Studio/Hex of Steel/Player.log
  - macOS: ~/Library/Logs/War Frogs Studio/Hex of Steel/Player.log
  - Linux: ~/.config/unity3d/War Frogs Studio/Hex of Steel/Player.log
- Make sure no other mod replaces TileGO.OnMouseOver without chaining postfix patches.

Development Notes
-----------------
- Entry point: Scripts/HosPingMod.cs wires Harmony and registers patches.
- Input helper: Scripts/InputHelper.cs proxies Unity input (needed outside Unity's player loop).
- Ping logic: Scripts/TilePingService.cs applies cooldowns and triggers highlights.
- Patch hook: Scripts/TileGOPatch.cs listens for Alt+clicks on tiles.
