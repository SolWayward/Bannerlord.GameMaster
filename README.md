# Bannerlord.GameMaster (BLGM)

A console commands mod for Mount & Blade II: Bannerlord that provides game management capabilities through an command-line interface. 
Tested using Bannerlord *1.3.9*, *1.3.10*, *1.3.12 beta*, and *War Sails* <br /><br />
BLGM is intended to be the foundation of other mods I am working on but is also useful for taking control of your playthrough, testing / debugging your mod, or even to quickly add functionality to your mod.  
This mod is a work in progress will be updated regular with additonal functionality, but is available now as is. <br /><br />

**Note**: this mod also provides a C# api for use with other mods (undocumented) 

## Overview

BLGM extends Bannerlord's console with a set of commands for managing heroes, clans, kingdoms, and game state. Whether you're testing scenarios, testing or debugging your mod, or simply want more control over your game, GameMaster provides the tools you need.

**All commands use the `gm.` prefix and are organized into logical categories for easy discovery and use.**

## Key Features

- **Hero Management** - Complete control over individual heroes including attributes, gold, health, relationships, and life state
- **Clan Management** - Comprehensive clan operations including membership, gold distribution, renown, and leadership
- **Kingdom Management** - Full kingdom control including diplomacy, settlements, clan membership, and rulers
- **Advanced Queries** - Powerful search and filter capabilities with AND/OR logic for finding heroes, clans, and kingdoms
- **Command Logging** - Built-in logging system for tracking command usage, debugging, and analysis
- **Testing Framework** - Automated test suite for validating commands and ensuring reliability

## Documentation

**Complete documentation is available in our [GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)**

### Quick Links

- [Home](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Home) - Welcome and getting started
- [Hero Commands](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Hero-Commands) - Managing individual heroes
- [Clan Commands](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Clan-Commands) - Clan operations and management
- [Kingdom Commands](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Kingdom-Commands) - Kingdom diplomacy and control
- [Query Commands](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Query-Commands) - Advanced search and filtering
- [Logger Commands](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Logger-Commands) - Command logging and tracking
- [Testing Commands](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Testing-Commands) - Automated testing framework

## Installation

1. **Download** the latest release from the [Releases page](https://github.com/SolWayward/Bannerlord.GameMaster/releases)
2. **Extract** the mod files to your Bannerlord Modules folder:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\
   ```
3. **Enable** the mod in the Bannerlord launcher
4. **Launch** the game and press `~` or `` ` `` to open the console

### Dependencies

GameMaster requires the following mods (automatically managed by the game):
- Harmony
- ButterLib
- UIExtenderEx
- MCM (Mod Configuration Menu)

## Quick Start

### Accessing the Console

Press `~` or `` ` `` (tilde/backtick key) while in-game to open the console.

### Basic Command Structure

```
gm.<category>.<command> [parameters]
```

Replace `<category>` with one of: `hero`, `clan`, `kingdom`, `query`, `log`, or `test`

### Example Commands

**Set a hero's gold:**
```
gm.hero.set_gold lord_1_1 10000
```

**Search for heroes:**
```
gm.query.hero empire lord female
```
Finds all female lords in the Empire faction.

**Declare war between kingdoms:**
```
gm.kingdom.declare_war empire battania
```

**Enable command logging:**
```
gm.log.enable
```

**Add a hero to a clan:**
```
gm.clan.add_hero clan_empire_1 lord_2_5
```

## Command Categories

### Hero Commands

Manage individual heroes with commands for:
- Life state (kill, imprison, release)
- Attributes (age, health, gold)
- Clan transfers
- Relationships between heroes

[View All Hero Commands →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Hero-Commands)

### Clan Commands

Control clans including:
- Adding/removing heroes
- Gold management and distribution
- Renown and tier adjustments
- Leadership changes
- Clan destruction

[View All Clan Commands →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Clan-Commands)

### Kingdom Commands

Manage kingdoms with:
- Clan membership
- War and peace declarations
- Settlement transfers
- Ruler changes
- Kingdom destruction

[View All Kingdom Commands →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Kingdom-Commands)

### Query Commands

Advanced search capabilities:
- Hero queries by name, type, and status
- Clan filtering with type restrictions
- Kingdom searches
- Flexible AND/OR logic

[View All Query Commands →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Query-Commands)

### Logger Commands

Command logging features:
- Enable/disable logging
- View logging status
- Clear log files
- Custom log paths

[View All Logger Commands →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Logger-Commands)

### Testing Commands

Automated testing system:
- Run test suites
- Execute individual tests
- View test results
- Standard and integration tests

[View All Testing Commands →](https://github.com/SolWayward/Bannerlord.GameMaster/wiki/Testing-Commands)

## Command Conventions

Understanding parameter notation:

- `<parameter>` - **Required** - Must be provided for the command to execute
- `[parameter]` - **Optional** - Can be omitted
- `hero_id` - Hero identifiers (partial names or StringIds)
- `clan_id` - Clan identifiers (partial names or StringIds)
- `kingdom_id` - Kingdom identifiers (partial names or StringIds)

The system intelligently searches for matches when you provide partial names or IDs.

## Important Notes

### Before You Begin

- **Backup your saves** - Many commands make permanent changes
- **Test in a separate save** - Experiment safely
- **Enable logging** - Use `gm.log.enable` to track your command usage
- **Some actions are irreversible** - Killing heroes and destroying clans cannot be undone

### Finding Entity IDs

Use Query commands to find the exact IDs of heroes, clans, and kingdoms:

```
gm.query.hero <search_term>
gm.query.clan <search_term>
gm.query.kingdom <search_term>
```

This ensures you're targeting the correct entities before using management commands.

## Contributing

Contributions are welcome! Here's how you can help:

1. **Report Bugs** - Open an issue with detailed reproduction steps
2. **Request Features** - Suggest new commands or improvements
3. **Submit Pull Requests** - Contribute code improvements or new features
4. **Improve Documentation** - Help make the wiki more comprehensive

### Development Setup

1. Clone the repository
2. Open the solution in Visual Studio 2022 or later
3. Ensure you have Bannerlord and required dependencies installed
4. Build the project

### Coding Standards

- Follow C# naming conventions
- Include XML documentation for public APIs
- Add unit tests for new commands
- Update wiki documentation for new features

## Support

- **Documentation:** [GitHub Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki)
- **Issues:** [GitHub Issues](https://github.com/SolWayward/Bannerlord.GameMaster/issues)
- **Discussions:** [GitHub Discussions](https://github.com/SolWayward/Bannerlord.GameMaster/discussions)

## Project Structure

```
Bannerlord.GameMaster/
├── Console/              # Console command implementations
│   ├── Common/           # Shared command infrastructure
│   ├── Query/            # Query command implementations
│   └── Testing/          # Testing framework
├── Heroes/               # Hero-related utilities
├── Clans/                # Clan-related utilities
├── Kingdoms/             # Kingdom-related utilities
├── _Module/              # Bannerlord module files
└── wiki/                 # Documentation source
```

## Roadmap

Future enhancements planned:

- [ ] Additional hero attribute commands
- [ ] Party management commands
- [ ] Settlement management enhancements
- [ ] Economy manipulation commands
- [ ] Save game utilities
- [ ] GUI command interface (optional)

## License

This project is released as open-source software. Please check the repository for specific license details.

## Acknowledgments

- Built for the Mount & Blade II: Bannerlord modding community
- Thanks to all contributors and testers
- Special thanks to TaleWorlds Entertainment for creating Bannerlord

---

**For detailed command documentation, parameters, examples, and usage notes, please visit the [GameMaster Wiki](https://github.com/SolWayward/Bannerlord.GameMaster/wiki).**

*Remember: Press `~` or `` ` `` in-game to access the console.*
