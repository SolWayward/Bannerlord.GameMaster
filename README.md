# Bannerlord.GameMaster (BLGM)

BLGM provides the ability to create and manage kingdoms, heroes, wanderers, clans, and tools to control, upgrade, and change ownership of settlements. Easily add members to your clan, or clans to your kingdom and way more. A powerful query system is also included, allowing you to easily add items, modifiers, and troops as well. Edit any hero's appearance, modify any clan's banner, or change any kingdom's colors.  

[**User Command Documentation**](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)  
*Documentation for players using BLGM to enhance their gameplay*

[**Developer API Reference**](https://solwayward.github.io/Bannerlord.GameMaster/api/index)  
*API Reference for mod developers using BLGM as a framework or helper for developing mods*

---

## Latest Update v1.3.14.4

```
Added new commands:
	gm.hero.marry
	gm.hero.divorce
	gm.hero.impregnate
	gm.kingdom.rename

Added ability to override names blgm uses when generating objects
	This will allow you to use a custom name set, or names in a different language
	See article / discussion for instructions

Previous Updates:
Added all 229 possible colors the custom banner editor
Added command gm.kingdom.sync_vassal_banners to manually update any clan banners that were changed back to the colors of the kingdom
Added command gm.kingdom.edit_banner as shortcut for updating the ruling clans banner of a kingdom
Added command gm.clan.sync_kingdom_colors to sync individual clan back to kingdom colors

Added command gm.clan.edit.banner
Added command gm.hero.edit_apperance
Added command gm.hero.open_inventory

Improved settlement renaming to immediately update name on map
```

---

## Overview

**BLGM** extends Bannerlord's console with powerful commands for managing heroes, clans, kingdoms, items, and game state. This mod is useful for taking control of your game, testing things out, fixing saves, or whatever other reason you may need to take control of your game. Add family members or companions to your clan, add clans to your kingdom, edit the appearance of your wife, modify the banner of a clan in your kingdom, change Vlandia's kingdom colors from red to white, create clan parties exceeding your party limit, quickly add troops or items, instantly upgrade all buildings in a settlement or raise the settlement's prosperity, change the culture of a settlement, hero, clan, or even kingdom, rename a settlement or even take ownership of a settlement, and much more. The user command documentation wiki lists and explains every command and their usage.

---

## Key Features

- Create new NPC Kingdoms, Clans, Heroes
- Create new Heroes for your clan or add existing heroes to your clan
- Create new Clans for your kingdom or add existing clans to your kingdom
- Edit any Clan banner or change any kingdom's colors
- Modify the appearance of any hero with the visual hero editor
- Change the culture of any settlement, kingdom, clan, or hero
- Upgrade settlement buildings, or modify any values such as prosperity, loyalty and more
- Rename settlements or change ownership of any settlement
- Save and load equipment sets to easily equip different heroes
- Add items or item modifiers to heroes
- Add troops or heroes to any party
- Upgrade troops in parties
- Specify objects by names, partial names, or stringIds
- Look up objects with powerful query commands
- "player" is always short for player hero/clan/kingdom
- And much, much more

---

# Quick Start

BLGM is easy to use.

To open the console, press **Alt + ~** (Alt + tilde)

Typing **gm.** into the console will show every command category. Typing **gm.hero** will show all hero commands.
Commands are as easy as **gm.hero.generate_lords 10**

Optional arguments can be used with commands as well such as **gm.hero.generate_lords 10 culture:vlandia gender:female clan:player**

Typing any command without any args will show help for the command with info on how to use the command, required/optional arguments, and examples.
Alternatively, you can use the BLGM Command Documentation to view detailed info about all commands.

Arguments can either be specified using positional arguments or named arguments.
Any argument with spaces such as names must use single quotes: **'Arg with Spaces'**

### Command Structure

```
gm.<category>.<command> [parameters]
```

### Example Commands

```
gm.hero.create_hero Maximus
gm.hero.edit_appearance derthert
gm.clan.edit_banner 'dey meroc'
gm.clan.generate_clans 5
gm.kingdom.create_kingdom Poros
gm.kingdom.generate_kingdoms 5
gm.troops.upgrade_troops player
gm.item.add imperial_sword 5 player
gm.item.save_equipment_both player my_loadout
gm.settlement.set_culture 'Ocs Hall' sturgia
gm.settlement.rename Galend 'Kings Landing'
gm.settlement.upgrade_buildings Charas 3
gm.settlement.set_owner Sargot player
gm.query.hero empire lord female
gm.query.item sword tier5 sort:value:desc
gm.item.add Justicier count:1 player
gm.item.set_equipped_modifier player legendary
gm.item.save_equipment_both derthert my_loadout
gm.item.load_equipment_both derthert my_loadout
gm.kingdom.declare_war empire battania
gm.kingdom.declare_alliance empire battania
```

*And many more commands - over 114 commands as of v1.3.14.1*

---

### Unblocking DLLs

Remember to unblock DLLs as BLGM uses multiple DLL and not just one singular DLL file.
Open PowerShell inside the root modules folder and run command `dir -r | unblock-file`

The below commands will unblock every DLL for every mod in the modules folder.
Your modules folder location may vary, but I included examples for standard Steam and Windows install locations.

**PowerShell for standard Steam install directory:**

```powershell
cd "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules"
dir -r | unblock-file
```

**PowerShell for standard Windows install directory:**

```powershell
cd "C:\Program Files (x86)\Mount & Blade II Bannerlord\Modules"
dir -r | unblock-file
```
