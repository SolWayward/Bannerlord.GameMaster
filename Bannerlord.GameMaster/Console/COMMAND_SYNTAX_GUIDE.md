# Console Command Syntax Guide

This guide explains the enhanced command syntax features available in Bannerlord.GameMaster console commands.

## Table of Contents
1. [Basic Syntax](#basic-syntax)
2. [Multi-Word Arguments with Quotes](#multi-word-arguments-with-quotes)
3. [Named Arguments](#named-arguments)
4. [Culture Specifications](#culture-specifications)
5. [Examples](#examples)

---

## Basic Syntax

All commands follow the basic structure:
```
gm.category.command_name <required_arg> [optional_arg]
```

- **Required arguments** are shown in angle brackets `<>`
- **Optional arguments** are shown in square brackets `[]`

---

## Multi-Word Arguments with Quotes

When you need to pass multi-word text (like names with spaces), use **SINGLE QUOTES**:

```bash
gm.clan.create_clan 'House Torivon'
gm.hero.create_lord 'Sir Galahad' vlandia male
gm.settlement.rename pen 'Castle of Stone'
```

**Important**: Use SINGLE quotes (`'`) not double quotes (`"`). The TaleWorlds console system removes double quotes before passing arguments.

### How It Works
- Text inside single quotes is treated as a single argument
- Example: `'House Torivon'` becomes one argument: `House Torivon`
- Without quotes: `House Torivon` would be two separate arguments: `House` and `Torivon`

---

## Named Arguments

Named arguments allow you to specify arguments in any order using the syntax `argName:argContent` (no spaces around the colon).

### Syntax
```
argName:value
```

### Benefits
1. **Specify arguments in any order**
2. **Skip optional arguments** without using placeholders
3. **Clearer command intent**

### Examples

#### Traditional Positional Arguments
```bash
gm.hero.generate_lords 10 vlandia,battania male player_faction 0.8
```

#### Using Named Arguments
```bash
gm.hero.generate_lords count:10 cultures:vlandia,battania gender:male clan:player_faction randomFactor:0.8
```

#### Mixed (Positional + Named)
```bash
gm.hero.generate_lords 10 cultures:vlandia,battania gender:male
```

#### Skip Optional Arguments
```bash
# Want to specify clan but not cultures or gender? Use named args!
gm.hero.generate_lords count:10 clan:player_faction
```

### Named Arguments with Multi-Word Values
Combine quotes with named arguments:
```bash
gm.clan.create_clan name:'House Stark' kingdom:sturgia createParty:true
```

---

## Culture Specifications

When specifying cultures, use **commas** (not semicolons) to separate multiple cultures:

### Single Culture
```bash
gm.hero.generate_lords 10 vlandia
```

### Multiple Cultures
```bash
gm.hero.generate_lords 10 vlandia,battania,sturgia
gm.clan.generate_clans 5 aserai,khuzait
```

### Culture Groups
Use predefined culture groups:
- `main_cultures` - All main factions (Vlandia, Sturgia, Empire, Aserai, Khuzait, Battania)
- `bandit_cultures` - All bandit cultures
- `all_cultures` - Every culture in the game

```bash
gm.hero.generate_lords 20 main_cultures
gm.clan.generate_clans 10 all_cultures
```

### Available Cultures
- **Main**: vlandia, sturgia, empire, aserai, khuzait, battania, nord
- **Bandits**: looters, desert_bandits, forest_bandits, mountain_bandits, steppe_bandits, sea_raiders
- **Special**: darshi, vakken

---

## Examples

### Hero Generation

#### Create Multiple Lords
```bash
# Positional arguments
gm.hero.generate_lords 15 vlandia,battania male player_faction

# Named arguments
gm.hero.generate_lords count:15 cultures:vlandia,battania gender:male clan:player_faction

# Mixed approach
gm.hero.generate_lords 15 cultures:vlandia,battania gender:male
```

#### Create Single Lord with Custom Name
```bash
# Positional
gm.hero.create_lord 'Sir Percival' vlandia male player_faction true

# Named arguments
gm.hero.create_lord name:'Sir Percival' cultures:vlandia gender:male clan:player_faction withParty:true

# Named with defaults
gm.hero.create_lord name:'Sir Percival' clan:player_faction
```

#### Create Companions
```bash
# Positional
gm.hero.create_companions 5 player vlandia,battania female

# Named arguments
gm.hero.create_companions count:5 hero:player cultures:vlandia,battania gender:female

# Skip optional arguments
gm.hero.create_companions count:5 hero:player gender:female
```

### Clan Generation

#### Create Clan
```bash
# Positional
gm.clan.create_clan 'House Stark' null sturgia true 5

# Named arguments (clearer!)
gm.clan.create_clan name:'House Stark' kingdom:sturgia createParty:true companionCount:5

# Skip optional arguments
gm.clan.create_clan name:'House Stark' kingdom:sturgia
```

#### Generate Multiple Clans
```bash
# Positional
gm.clan.generate_clans 10 vlandia,battania empire true 3

# Named arguments
gm.clan.generate_clans count:10 cultures:vlandia,battania kingdom:empire createParties:true companionCount:3

# Using defaults
gm.clan.generate_clans count:10 cultures:vlandia,battania
```

### Settlement Commands

#### Spawn Wanderer
```bash
# Positional
gm.settlement.spawn_wanderer pen 'Wandering Bard' vlandia,empire female 0.8

# Named arguments
gm.settlement.spawn_wanderer settlement:pen name:'Wandering Bard' cultures:vlandia,empire gender:female randomFactor:0.8

# Auto-generate name
gm.settlement.spawn_wanderer settlement:pen cultures:vlandia gender:female
```

#### Rename Settlement
```bash
# Multi-word names require quotes
gm.settlement.rename pen 'Castle of Stone'

# Named argument version
gm.settlement.rename settlement:pen name:'Castle of Stone'
```

### Item Commands

#### Add Items
```bash
# Positional
gm.item.add imperial_sword 5 player masterwork

# Named arguments
gm.item.add item:imperial_sword count:5 hero:player modifier:masterwork

# Without modifier
gm.item.add item:imperial_sword count:5 hero:player
```

---

## Best Practices

1. **Use quotes for multi-word text**: Always use single quotes for names with spaces
2. **Use named arguments for clarity**: When commands have many optional parameters, named arguments make intent clearer
3. **Mix approaches as needed**: You can use positional for required args and named for optionals
4. **Use commas for cultures**: Separate multiple cultures with commas (no spaces): `vlandia,battania,empire`
5. **No spaces around colons**: Named arguments must be `name:value` not `name: value` or `name :value`

---

## Quick Reference

### Quote Syntax
```bash
'text with spaces'          # Correct - single quotes
"text with spaces"          # Wrong - double quotes don't work
text_without_spaces         # OK - no quotes needed
```

### Named Argument Syntax
```bash
name:value                  # Correct
name:value                  # Correct - no spaces
name: value                 # Wrong - space after colon
name :value                 # Wrong - space before colon
name : value                # Wrong - spaces around colon
```

### Culture Syntax
```bash
vlandia,battania,empire     # Correct - commas, no spaces
vlandia, battania           # Wrong - space after comma
vlandia;battania            # Old syntax - no longer works
main_cultures               # Correct - predefined group
```

---

## Migration from Old Syntax

If you have old commands using semicolons, update them to use commas:

### Old (No Longer Works)
```bash
gm.hero.generate_lords 10 vlandia;battania;sturgia
gm.clan.generate_clans 5 aserai;khuzait empire
```

### New (Correct)
```bash
gm.hero.generate_lords 10 vlandia,battania,sturgia
gm.clan.generate_clans 5 aserai,khuzait empire
```

---

## Troubleshooting

### Common Issues

**Issue**: Multi-word names not working
```bash
gm.clan.create_clan House Stark    # Wrong - treated as two arguments
```
**Solution**: Use single quotes
```bash
gm.clan.create_clan 'House Stark'  # Correct
```

**Issue**: Named argument not recognized
```bash
gm.hero.generate_lords count: 10   # Wrong - space after colon
```
**Solution**: Remove spaces
```bash
gm.hero.generate_lords count:10    # Correct
```

**Issue**: Cultures not parsing
```bash
gm.hero.generate_lords 10 vlandia;battania   # Wrong - semicolons no longer work
```
**Solution**: Use commas
```bash
gm.hero.generate_lords 10 vlandia,battania   # Correct
```

---

## Technical Notes

### Argument Processing Order
1. **Quote parsing**: Single-quoted strings are combined into single arguments
2. **Named argument detection**: Arguments containing `:` are parsed as name:value pairs
3. **Positional argument extraction**: Remaining arguments are treated as positional

### Compatibility
- All commands support both positional and named arguments
- Old commands without named arguments still work as before
- Mixing positional and named arguments is fully supported
- Quote parsing is handled automatically by the `Cmd.Run` wrapper

---

For more examples and command-specific documentation, use the help commands:
```bash
gm.help
gm.hero.help
gm.clan.help
```
