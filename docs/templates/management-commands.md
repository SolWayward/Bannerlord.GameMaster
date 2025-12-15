# Management Commands Template

**Navigation:** [← Back: Query Commands Template](query-commands.md) | [Back to Index](../README.md) | [Next: Testing Template →](testing.md)

---

## Purpose

Management Commands provide console commands that modify game state. These commands handle property updates, entity transfers, state changes, and relationships.

## Command Organization

Organize commands using regions:
- Property Modification
- Entity Transfer/Assignment
- State Changes
- Relationships
- Creation/Deletion

## Real Examples

- [`HeroManagementCommands.SetClan()`](../../Bannerlord.GameMaster/Console/HeroManagementCommands.cs:25)
- [`HeroManagementCommands.SetAge()`](../../Bannerlord.GameMaster/Console/HeroManagementCommands.cs:228)

---

## Template Code

See [IMPLEMENTATION_GUIDE.md lines 606-745](../../IMPLEMENTATION_GUIDE.md) for the complete template.

Key patterns:
- Use `Cmd.Run()` wrapper
- Validate campaign mode, arguments, entities, values
- Use `ExecuteWithErrorHandling()` for state changes
- Provide clear success messages with before/after state

---

## Next Steps

1. **Write** [Tests](testing.md) for all commands
2. **Follow** [Best Practices](../guides/best-practices.md) for consistency

---

**Navigation:** [← Back: Query Commands Template](query-commands.md) | [Back to Index](../README.md) | [Next: Testing Template →](testing.md)