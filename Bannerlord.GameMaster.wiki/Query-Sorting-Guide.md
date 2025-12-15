# Query Sorting Guide

This guide explains how to use the sorting functionality available in Hero, Clan, Kingdom, and Item query commands.

## Table of Contents

- [Overview](#overview)
- [Sort Syntax](#sort-syntax)
- [Hero Query Sorting](#hero-query-sorting)
- [Clan Query Sorting](#clan-query-sorting)
- [Kingdom Query Sorting](#kingdom-query-sorting)
- [Item Query Sorting](#item-query-sorting)
- [Advanced Sorting Techniques](#advanced-sorting-techniques)
- [Examples](#examples)

## Overview

All query commands support sorting results by various fields or type flags. Sorting can be combined with search terms and type filters to create powerful queries.

### Key Features

- **Multiple sort fields** - Different fields available for each entity type
- **Type flag sorting** - Sort by whether entities have specific type flags
- **Ascending/Descending** - Control sort direction
- **Default sorting** - Results sorted by ID (ascending) when no sort is specified

## Sort Syntax

### Basic Format

```
sort:field          # Sort by field (ascending)
sort:field:asc      # Explicitly sort ascending
sort:field:desc     # Sort descending
```

### Combining with Queries

```
gm.query.hero john lord sort:name
gm.query.clan empire sort:gold:desc
gm.query.kingdom sort:strength:desc atwar
```

## Hero Query Sorting

### Available Sort Fields

| Field | Description | Example |
|-------|-------------|---------|
| `id` | Hero ID (default) | `sort:id` |
| `name` | Hero name | `sort:name` |
| `age` | Hero age | `sort:age:desc` |
| `clan` | Clan name | `sort:clan` |
| `kingdom` | Kingdom name | `sort:kingdom` |

### Type Flag Sorting

You can sort by any [`HeroTypes`](HeroExtensions.cs:10-31) flag:

- `lord`, `wanderer`, `notable`, `merchant`
- `female`, `male`
- `clanleader`, `kingdomruler`, `partyleader`
- `fugitive`, `alive`, `dead`, `prisoner`
- `withoutclan`, `withoutkingdom`, `married`

### Examples

```bash
# Find all heroes, sorted by name
gm.query.hero sort:name

# Find wanderers, sorted by age (youngest first)
gm.query.hero wanderer sort:age

# Find lords sorted by clan name
gm.query.hero lord sort:clan

# Sort heroes by whether they are wanderers (wanderers first)
gm.query.hero sort:wanderer

# Find female clan leaders, sorted by name descending
gm.query.hero female clanleader sort:name:desc
```

## Clan Query Sorting

### Available Sort Fields

| Field | Description | Example |
|-------|-------------|---------|
| `id` | Clan ID (default) | `sort:id` |
| `name` | Clan name | `sort:name` |
| `tier` | Clan tier | `sort:tier:desc` |
| `gold` | Clan gold | `sort:gold:desc` |
| `renown` | Clan renown | `sort:renown:desc` |
| `kingdom` | Kingdom name | `sort:kingdom` |
| `heroes` | Number of heroes | `sort:heroes:desc` |

### Type Flag Sorting

You can sort by any [`ClanTypes`](ClanExtensions.cs:10-30) flag:

- `active`, `eliminated`
- `bandit`, `nonbandit`
- `noble`, `minorfaction`
- `rebel`, `mercenary`
- `mafia`, `outlaw`, `nomad`, `sect`
- `withoutkingdom`, `empty`, `playerclan`

### Examples

```bash
# Find all clans sorted by gold (richest first)
gm.query.clan sort:gold:desc

# Find noble clans sorted by renown
gm.query.clan noble sort:renown:desc

# Find mercenary clans sorted by name
gm.query.clan mercenary sort:name

# Sort clans by whether they are mercenaries
gm.query.clan sort:mercenary

# Find eliminated clans sorted by tier
gm.query.clan eliminated sort:tier:desc
```

## Kingdom Query Sorting

### Available Sort Fields

| Field | Description | Example |
|-------|-------------|---------|
| `id` | Kingdom ID (default) | `sort:id` |
| `name` | Kingdom name | `sort:name` |
| `clans` | Number of clans | `sort:clans:desc` |
| `heroes` | Number of heroes | `sort:heroes:desc` |
| `fiefs` | Number of fiefs | `sort:fiefs:desc` |
| `strength` | Total military strength | `sort:strength:desc` |
| `ruler` | Ruler name | `sort:ruler` |

### Type Flag Sorting

You can sort by any [`KingdomTypes`](KingdomExtensions.cs:10-18) flag:

- `active`, `eliminated`
- `empty`
- `playerkingdom`
- `atwar`

### Examples

```bash
# Find all kingdoms sorted by military strength (strongest first)
gm.query.kingdom sort:strength:desc

# Find kingdoms at war, sorted by number of clans
gm.query.kingdom atwar sort:clans:desc

# Find active kingdoms sorted by fiefs
gm.query.kingdom active sort:fiefs:desc

# Sort kingdoms by whether they are at war
gm.query.kingdom sort:atwar

# Find kingdoms sorted by ruler name
gm.query.kingdom sort:ruler
```

## Item Query Sorting

### Available Sort Fields

| Field | Description | Example |
|-------|-------------|---------|
| `id` | Item ID (default) | `sort:id` |
| `name` | Item name | `sort:name` |
| `tier` | Item tier | `sort:tier:desc` |
| `value` | Item value | `sort:value:desc` |
| `type` | Item type | `sort:type` |

### Type Flag Sorting

You can sort by any [`ItemTypes`](ItemExtensions.cs) flag:

- Weapon types: `weapon`, `1h`, `2h`, `ranged`, `polearm`, `thrown`
- Armor types: `armor`, `head`, `body`, `leg`, `hand`, `cape`
- Other: `mount`, `food`, `trade`, `banner`

### Examples

```bash
# Find swords sorted by value (most expensive first)
gm.query.item sword weapon sort:value:desc

# Find armor sorted by tier
gm.query.item armor tier3 sort:name

# Find all weapons sorted by tier descending
gm.query.item weapon sort:tier:desc

# Sort items by whether they are ranged weapons
gm.query.item sort:ranged
```

## Advanced Sorting Techniques

### Type Flag Sorting Explained

When you sort by a type flag (e.g., `sort:wanderer`), entities are ordered by whether they have that flag:

- **Ascending** (default): Entities WITH the flag come first
- **Descending**: Entities WITHOUT the flag come first

Example:
```bash
# Wanderers first, then non-wanderers
gm.query.hero sort:wanderer

# Non-wanderers first, then wanderers  
gm.query.hero sort:wanderer:desc
```

### Combining Filters and Sorting

You can combine search terms, type filters, and sorting:

```bash
# Find empire nobles sorted by gold
gm.query.clan empire noble sort:gold:desc

# Find female lords named "ira" sorted by age
gm.query.hero ira female lord sort:age

# Find tier 5 bows sorted by value
gm.query.item bow tier5 sort:value:desc

# Find kingdoms at war with "empire" in name, sorted by strength
gm.query.kingdom empire atwar sort:strength:desc
```

### Using ANY vs ALL with Sorting

Both `query` and `query_any` commands support sorting:

```bash
# Heroes who are lords AND wanderers, sorted by name
gm.query.hero lord wanderer sort:name

# Heroes who are lords OR wanderers, sorted by age
gm.query.hero_any lord wanderer sort:age:desc

# Clans that are bandits AND outlaws, sorted by gold
gm.query.clan bandit outlaw sort:gold:desc

# Clans that are bandits OR outlaws, sorted by name
gm.query.clan_any bandit outlaw sort:name
```

## Examples

### Finding Top Performers

```bash
# Richest clans
gm.query.clan sort:gold:desc

# Most powerful kingdoms
gm.query.kingdom sort:strength:desc

# Highest tier items
gm.query.item sort:tier:desc

# Oldest heroes
gm.query.hero sort:age:desc
```

### Organized Listings

```bash
# All heroes alphabetically
gm.query.hero sort:name

# All clans by kingdom
gm.query.clan sort:kingdom

# All kingdoms alphabetically  
gm.query.kingdom sort:name

# All items by type
gm.query.item sort:type
```

### Comparative Analysis

```bash
# Compare kingdoms by military might
gm.query.kingdom atwar sort:strength:desc

# Compare clans by wealth
gm.query.clan noble sort:gold:desc

# Compare wanderers by age (find youngest)
gm.query.hero wanderer sort:age

# Compare weapons by value
gm.query.item weapon 1h sort:value:desc
```

### Filtering and Ranking

```bash
# Top 5 richest noble clans (manually count first 5)
gm.query.clan noble sort:gold:desc

# Youngest female clan leaders
gm.query.hero female clanleader sort:age

# Most valuable tier 6 items
gm.query.item tier6 sort:value:desc

# Strongest kingdoms with the most fiefs
gm.query.kingdom sort:fiefs:desc
```

## Tips and Best Practices

1. **Default Behavior** - If you don't specify a sort, results are sorted by ID ascending
2. **Case Insensitive** - Sort parameters are case-insensitive (`sort:NAME` = `sort:name`)
3. **Order Matters** - Sort parameter can appear anywhere in the command
4. **Type Flag Sorting** - Useful for grouping entities by characteristics
5. **Combine Wisely** - Use search + filters + sorting for precise queries

## Common Use Cases

### Finding Specific Entities

```bash
# Find oldest living heroes
gm.query.hero alive sort:age:desc

# Find richest active clans
gm.query.clan active sort:gold:desc

# Find weakest kingdoms
gm.query.kingdom sort:strength
```

### Analysis and Planning

```bash
# Evaluate military strength
gm.query.kingdom atwar sort:strength:desc

# Check clan economies
gm.query.clan noble sort:gold:desc

# Review hero ages for succession planning
gm.query.hero lord clanleader sort:age:desc
```

### Item Management

```bash
# Find most valuable loot
gm.query.item sort:value:desc

# Check available high-tier equipment
gm.query.item armor tier5 sort:value:desc

# Browse weapons by tier
gm.query.item weapon sort:tier:desc
```

## Related Documentation

- [Hero Commands](Hero-Commands.md)
- [Clan Commands](Clan-Commands.md)
- [Kingdom Commands](Kingdom-Commands.md)
- [Item Query Commands](Item-Query-Commands.md)
- [Query Commands Overview](Query-Commands.md)