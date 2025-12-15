# Testing Template

**Navigation:** [← Back: Management Commands Template](management-commands.md) | [Back to Index](../README.md) | [Next: Best Practices →](../guides/best-practices.md)

---

## Purpose

The Testing layer provides automated validation of all commands. Tests ensure commands work correctly and catch regressions.

## Test Categories Required

- Argument validation tests (missing args, invalid args)
- Error handling tests (invalid IDs, edge cases)
- Success path tests (when possible)

## Real Examples

- [`StandardTests.RegisterAll()`](../../Bannerlord.GameMaster/Console/Testing/StandardTests.cs)
- [`NamePriorityTests`](../../Bannerlord.GameMaster/Console/Testing/NamePriorityTests.cs)

---

## Template Code

See [IMPLEMENTATION_GUIDE.md lines 747-883](../../IMPLEMENTATION_GUIDE.md) for the complete template.

Key patterns:
- Register tests with unique IDs
- Organize by category
- Test both error and success cases
- Use custom validators for complex scenarios

---

## Test Execution

```bash
# Run all tests
gm.test.run_all

# Run by category
gm.test.run_category {EntityType}Query

# Run single test
gm.test.run_single test_id_001
```

---

## Next Steps

1. **Follow** [Testing Guide](../guides/testing.md) for detailed procedures
2. **Review** [Best Practices](../guides/best-practices.md) for quality standards

---

**Navigation:** [← Back: Management Commands Template](management-commands.md) | [Back to Index](../README.md) | [Next: Best Practices →](../guides/best-practices.md)