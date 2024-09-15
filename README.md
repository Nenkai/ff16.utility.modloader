# ff16.utility.modloader

[Final Fantasy XVI / 16](https://store.steampowered.com/app/2515020/FINAL_FANTASY_XVI/) Mod loader for [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II).

## Usage

Make sure that you have Reloaded-II and download the latest FFXVI mod loader in [Releases](https://github.com/Nenkai/ff16.utility.modloader/releases) (the .7z file).

Add Final Fantasy XVI as a registered game within Reloaded-II.

Drag and drop the .7z file to the left pane of Reloaded-II.

## Mod File Structure

1. Follow this guide to create a [Reloaded-II](https://reloaded-project.github.io/Reloaded-II/CreatingMods/) mod.
2. Your mod name should start with `ff16.` (for clarity).
3. Add `Final Fantasy XVI / 16 Mod Loader` as dependency.
4. Follow this file structure for game files:

```
FFXVI
└─ data
   ├─ 0001 (folder for each pack name
      ├─ <modded files for 0001 goes here>
```

> [!WARNING]
> You should preserve `.path` files if they were present.

You can use [**this template mod**](https://github.com/Nenkai/ff16.utility.modloader/releases/tag/template-1.0.0) (which changes some of the main menu ui text to `Hello World`) for reference.

## Discord

<a href="https://discord.gg/D7jhUDfYZh">
  <img src="https://discordapp.com/api/guilds/1284918645675397140/widget.png?style=banner2" alt="Discord Banner 1"/>
</a>
