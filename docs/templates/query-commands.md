# Query Commands Template

**Navigation:** [← Back: Queries Template](queries.md) | [Back to Index](../README.md) | [Next: Management Commands Template →](management-commands.md)

---

## Purpose

Query Commands expose query functionality as read-only console commands. These commands allow users to search and display entity information without modifying game state.

## Standard Commands Required

1. **`gm.query.{entity}`** - List/search entities with filters
2. **`gm.query.{entity}_any`** - Search with OR logic (optional)
3. **`gm.query.{entity}_info`** - Display detailed entity information

## Real Examples

- [`HeroQueryCommands.QueryHeroes()`](../../Bannerlord.GameMaster/Console/Query/HeroQueryCommands.cs:72)
- [`ClanQueryCommands.QueryClanInfo()`](../../Bannerlord.GameMaster/Console/Query/ClanQueryCommands.cs:140)

---

## Template Code

See [IMPLEMENTATION_GUIDE.md lines 476-604](../../IMPLEMENTATION_GUIDE.md) for the complete template.

Key patterns:
- Use `Cmd.Run()` wrapper
- Validate campaign mode first
- Parse arguments into query and types
- Call Queries layer for data
- Format and return results

---

## Next Steps

1. **Create** [Management Commands](management-commands.md) for state modification
2. **Write** [Tests](testing.md) for all commands

---

**Navigation:** [← Back: Queries Template](queries.md) | [Back to Index](../README.md) | [Next: Management Commands Template →](management-commands.md)