# Code Quality Checklist

**Navigation:** [← Back: Troubleshooting](../guides/troubleshooting.md) | [Back to Index](../README.md) | [Next: Identified Improvements →](identified-improvements.md)

---

## Before Committing

### Extensions Layer

- [ ] Flags enum uses powers of 2
- [ ] `GetTypes()` method implemented
- [ ] `HasAllTypes()` method implemented
- [ ] `HasAnyType()` method implemented
- [ ] `FormattedDetails()` method implemented
- [ ] XML comments on all public methods

### Queries Layer

- [ ] `GetById()` method implemented
- [ ] `Query{Type}s()` method implemented
- [ ] `Parse{Type}Type()` method implemented
- [ ] `Parse{Type}Types()` method implemented
- [ ] `GetFormattedDetails()` method implemented
- [ ] Supports AND and OR logic

### Query Commands

- [ ] Uses `Cmd.Run()` wrapper
- [ ] Validates campaign mode
- [ ] `ParseArguments()` helper method
- [ ] Main query command implemented
- [ ] Info command implemented
- [ ] Usage messages clear and helpful

### Management Commands

- [ ] Uses `Cmd.Run()` wrapper
- [ ] Validates campaign mode
- [ ] Validates argument count
- [ ] Uses `CommandBase.Find{Entity}()` for resolution
- [ ] Uses `CommandValidator` for value validation
- [ ] Uses `ExecuteWithErrorHandling()` wrapper
- [ ] Provides detailed success messages
- [ ] Commands organized by region

### Testing

- [ ] Tests for all query commands
- [ ] Tests for all management commands
- [ ] Argument validation tests
- [ ] Invalid input tests
- [ ] Tests organized by category
- [ ] Custom validators where appropriate

### General

- [ ] Follows naming conventions
- [ ] No code duplication
- [ ] Consistent error message format
- [ ] All public methods documented
- [ ] No compiler warnings
- [ ] Code formatted consistently

---

## Next Steps

1. **Review** [Identified Improvements](identified-improvements.md) for enhancement opportunities
2. **Study** [Architecture Analysis](architecture-analysis.md) for deeper understanding

---

**Navigation:** [← Back: Troubleshooting](../guides/troubleshooting.md) | [Back to Index](../README.md) | [Next: Identified Improvements →](identified-improvements.md)