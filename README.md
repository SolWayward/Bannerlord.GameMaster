# Bannerlord.GameMaster (BLGM)
BLGM provides the ability to Create and manage kingdoms, heroes, wanderers, clans, and tools to control, upgrade, and change ownership of settlements. Easily add members to your clan, or clans to your kingdom and way more. A powerful query system is also included, allowing you to easily add items, modifiers, and troops as well.
BLGM provides the ability to Create and manage kingdoms, heroes, wanderers, clans, and tools to control, upgrade, and change ownership of settlements. Easily add members to your clan, or clans to your kingdom and way more. A powerful query system is also included, allowing you to easily add items, modifiers, and troops as well.

## Quick Links
- **[User Command Documentation](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)**  
- **[Developer API Reference](https://solwayward.github.io/Bannerlord.GameMaster/api/index)** 

Console commands and framework, providing Settlement, Hero, Clan, Item, troop, and Kingdom management. All commands provide the ability to use object names, or object ids as arguments as well as a query system to search and find objects.
  
```Press Alt + ~ (tilde key) in-game to open console```

## Latest Update v1.3.13.16
```
Fixed Hero Skills
Improved Generated Hero Equipment
	Heroes now get equipment relating to their skills
	Heroes no longer get any weapon sets that don't make sense
	Heroes can also be equipped with items added from other mods
Fixed Load Equipment command not loading modifiers
Added commands to reequip heroes gm.hero.equip_hero, gm.clan.equip_heroes, gm.kingdom.equip_heroes
```  

## Key Features

- **Hero Management** - Modify attributes, gold, health, relationships, life state
- **Clan Management** - Control membership, finances, renown, leadership
- **Kingdom Management** - Create NPC Kingdoms, Handle diplomacy, settlements, clan membership
- **Item Management** - Full inventory/equipment control with quality modifiers
- **Equipment Save/Load** - Save and load hero equipment sets to files
- **Advanced Queries** - Powerful search with AND/OR logic, sorting, filtering
- **Command Logging** - Track all command usage for debugging
- **Convenience Features** - Use Ids or names for objects, and even partial matches. Positional and named arguments using ArgName:ArgValue

**Tested:** Bannerlord 1.3.9, 1.3.10, 1.3.12, 1.3.13, and works with or without Warsails.

**BLGM** extends Bannerlord's console with powerful commands for managing heroes, clans, kingdoms, items, and game state. This mod is useful for taking control of your game, testing things out, fixing saves, or whatever other reason you may need to take control of your game. 
  
The purpose of this mod is mainly for my own use as a foundation for another mod I am working on it but it can also be useful to modders for testing, debugging, or to quickly implement functionality in their mods as well, an API is also provided. The mod will regular be updated with additional features and more powerful functionality.

- All commands use the `gm.` prefix for easy organization.  
- Commands can be used by targeting entity names or entity Ids.
- Query system is also included for quickly searching for entities.  

## Installation

1. Download from [Releases](https://github.com/SolWayward/Bannerlord.GameMaster/releases)
2. Extract to: `...\Mount & Blade II Bannerlord\Modules\`
3. Unblock dlls
4. Enable in Bannerlord launcher
5. Press Alt + `~` (tilde key) in-game to open console
6. type gm. to discover commands

**No external dependencies required** - This mod uses only native Bannerlord APIs

## Quick Start

### Command Structure
```
gm.<category>.<command> [parameters]
```

### Example Commands

```bash
# Manage heroes
gm.hero.set_gold player 50000
gm.hero.set_health lord_1_1 100

# Search and query
gm.query.hero empire lord female
gm.query.item sword tier5 sort:value:desc

# Manage items and equipment
gm.item.add imperial_sword 5 player
gm.item.equip chainmail player

# Save/load equipment sets
gm.item.save_equipment_both player my_loadout
gm.item.load_equipment_both companion my_loadout

# Clan operations
gm.clan.add_hero clan_empire_1 lord_2_5
gm.clan.set_gold clan_battania_1 100000

# Kingdom diplomacy
gm.kingdom.declare_war empire battania
gm.kingdom.add_clan empire clan_neutral_1

# Enable logging
gm.log.enable
```

## Query System

Use powerful queries to search and filter:

**Basic Queries:**
- `gm.query.hero <terms>` - Find heroes by name, culture, type
- `gm.query.clan <terms>` - Find clans with filters
- `gm.query.kingdom <terms>` - Find kingdoms
- `gm.query.item <terms>` - Search items with filters

**Advanced Features:**
- **AND logic** (default): `gm.query.hero empire lord` (empire AND lord)
- **OR logic**: `gm.query.hero OR empire battania` (empire OR battania)
- **Sorting**: `gm.query.item bow sort:value:desc` (sort by value descending)
- **Tier filtering**: `gm.query.item armor tier5` (tier 5 armor only)
- **Type filtering**: `gm.query.item weapon OneHandedWeapon` (specific type)

**Sort Options:** `name`, `value`, `tier`, `type` (add `:asc` or `:desc`)  

**Multi-word Parameters:** Use single quotes to use arguments with spaces ex: `'Multi word argument'`

**Multiple Commands:** Use semicolons to separate multiple commands to run commands one after the other. (useful for copying and pasting commands you use often)

## Available Commands

### Hero Commands
`create_lord`, `generate_lords`, `set_gold`, `set_health`, `set_age`, `kill`, `imprison`, `release`, `teleport`, `set_clan`, `set_relation`, `create_companions`, `add_hero_to_party`, `rename`, `create_party`

[Full Hero Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Hero-Overview)

### Clan Commands
`create_clan`, `add_hero`, `remove_hero`, `set_gold`, `add_gold`, `distribute_gold`, `set_renown`, `set_tier`, `set_leader`, `destroy`, `rename`, `set_culture`, `generate_clans`, `Create_minor_clan`

[Full Clan Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Clan-Overview)

### Kingdom Commands
`create_kingdom`, `generate_Kingdoms`, `add_clan`, `remove_clan`, `declare_war`, `make_peace`, `set_ruler`, `destroy`, `declare_alliance`, `trade_agreement`, `pay_tribute`, `get_tribute_info`

[Full Kingdom Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Kingdom-Overview)

### Item Management Commands
`add`, `remove`, `remove_all`, `transfer`, `equip`, `unequip`, `equip_slot`, `unequip_slot`, `list_equipped`, `list_inventory`, `set_equipped_modifier`, `set_inventory_modifier`, `save_equipment`, `save_equipment_civilian`, `save_equipment_both`, `load_equipment`, `load_equipment_civilian`, `load_equipment_both`

[Full Item Management Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Item-Overview)

### Settlement Management Commands
`set_culture`, `set_owner`, `set_owner_clan`, `set_owner_kingdom`, `upgrade_buildings`, `add_militia`, `fill_garrison`, `give_food`, `give_gold`, `rename`, `reset_name`, `reset_name_all`, `set_hearths`, `set_loyalty`, `set_prosperity`, `set_security`, `spawn_wanderer`, `create_notable_caravan`, `create_player_caravan`

[Full Settlement Management Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Settlement-Overview)

### Troop Commands
`give_hero_troops`, `add_basic`, `add_elite`, `add_mercenary`, `add_mixed`, `upgrade_troops`, `add_xp`

[Full Troop Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Troop-Overview)

### Query Commands
`hero`, `clan`, `kingdom`, `item`, `modifiers`, `culture`, `character` - All support AND/OR logic, sorting, and filtering

[Full Query Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/API-Query-Overview)

## Important Notes

- **Backup your saves** - Many commands make permanent changes
- **Some actions are irreversible** - Killing heroes, destroying clans
- **Multi-word Parameters** - Use single quotes to use arguments with spaces ex: `'Multi word argument'`
- **Renaming Settlements** - Name may not update right away. Open trade menu in settlement or load save to force update.

## Support

- **Documentation:** [GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)
- **Issues:** [GitHub Issues](https://github.com/SolWayward/Bannerlord.GameMaster/issues)
- **Discussions:** [GitHub Discussions](https://github.com/SolWayward/Bannerlord.GameMaster/discussions)

## Contributing

Contributions welcome! Report bugs, request features, or submit pull requests.

---

**For complete command documentation with all parameters and examples, visit the [GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki).**
