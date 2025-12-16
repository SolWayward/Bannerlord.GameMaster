# Bannerlord.GameMaster (BLGM)
**[Complete Documentation for users on GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)**
[Source code and implementation Documention for developers](https://solwayward.github.io/Bannerlord.GameMaster/)

Console commands and power tools mod for Mount & Blade II: Bannerlord providing game management through the game console.

**Tested:** Bannerlord 1.3.9, 1.3.10, 1.3.12 beta, and works with or without Warsails.

**BLGM** extends Bannerlord's console with powerful commands for managing heroes, clans, kingdoms, items, and game state. This mod is useful for taking control of your game, testing things out, fixing saves, or whatever other reason you may need to take control of your game. 
  
The purpose of this mod is mainly for my own use as a foundation for another mod I am working on it but it can also be useful to modders for testing, debugging, or to quickly implement functionality in their mods as well, an API is also provided. The mod will regular be updated with additional features and more powerful functionality.

- All commands use the `gm.` prefix for easy organization.  
- Commands can be used by targeting entity names or entity Ids.
- Query system is also included for quickly searching for entities.  

## Key Features

- **Hero Management** - Modify attributes, gold, health, relationships, life state
- **Clan Management** - Control membership, finances, renown, leadership
- **Kingdom Management** - Handle diplomacy, settlements, clan membership
- **Item Management** - Full inventory/equipment control with quality modifiers
- **Equipment Save/Load** - Save and load hero equipment sets to files
- **Advanced Queries** - Powerful search with AND/OR logic, sorting, filtering
- **Command Logging** - Track all command usage for debugging
- **Testing Framework** - Automated validation system

## Installation

1. Download from [Releases](https://github.com/SolWayward/Bannerlord.GameMaster/releases)
2. Extract to: `...\Mount & Blade II Bannerlord\Modules\`
3. Enable in Bannerlord launcher
4. Press `~` or `` ` `` in-game to open console

**Dependencies:** Harmony, ButterLib, UIExtenderEx, MCM (auto-managed by game)

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

## Available Commands

### Hero Commands
`set_gold`, `set_health`, `set_age`, `kill`, `imprison`, `release`, `teleport`, `set_clan`, `set_relation`

[Full Hero Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Hero-Commands)

### Clan Commands
`add_hero`, `remove_hero`, `set_gold`, `add_gold`, `distribute_gold`, `set_renown`, `set_tier`, `set_leader`, `destroy`

[Full Clan Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Clan-Commands)

### Kingdom Commands
`add_clan`, `remove_clan`, `declare_war`, `make_peace`, `add_settlement`, `remove_settlement`, `set_ruler`, `destroy`

[Full Kingdom Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Kingdom-Commands)

### Item Management Commands
`add`, `remove`, `remove_all`, `transfer`, `equip`, `unequip`, `equip_slot`, `unequip_slot`, `list_equipped`, `list_inventory`, `set_equipped_modifier`, `set_inventory_modifier`, `save_equipment`, `save_equipment_civilian`, `save_equipment_both`, `load_equipment`, `load_equipment_civilian`, `load_equipment_both`

[Full Item Management Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Item-Management-Commands)

### Query Commands
`hero`, `clan`, `kingdom`, `item`, `modifiers` - All support AND/OR logic, sorting, and filtering

[Full Query Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Query-Commands)

### Logger Commands
`enable`, `disable`, `status`, `clear`

[Full Logger Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Logger-Commands)

### Testing Commands
`run`, `run_category`, `run_integration` (⚠️ For developers only - modifies game state)

[Full Testing Commands Documentation →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Testing-Commands)

## Important Notes

- ⚠️ **Backup your saves** - Many commands make permanent changes
- 🧪 **Test in separate save** - Experiment safely
- 📝 **Enable logging** - Use `gm.log.enable` for tracking
- ⛔ **Some actions are irreversible** - Killing heroes, destroying clans

## Support

- **Documentation:** [GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)
- **Issues:** [GitHub Issues](https://github.com/SolWayward/Bannerlord.GameMaster/issues)
- **Discussions:** [GitHub Discussions](https://github.com/SolWayward/Bannerlord.GameMaster/discussions)

## Contributing

Contributions welcome! Report bugs, request features, or submit pull requests.

---

**For complete command documentation with all parameters and examples, visit the [GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki).**
