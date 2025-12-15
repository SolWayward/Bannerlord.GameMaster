# Best Practices & Conventions

**Navigation:** [← Back: Testing Template](../templates/testing.md) | [Back to Index](../README.md) | [Next: Testing Guide →](testing.md)

---

## Naming Conventions

| Component | Convention | Example |
|-----------|-----------|---------|
| **Files** | PascalCase | `HeroExtensions.cs` |
| **Classes** | PascalCase | `CommandBase`, `HeroQueries` |
| **Methods** | PascalCase | `QueryHeroes()`, `GetItemById()` |
| **Parameters** | camelCase | `requiredTypes`, `matchAll` |
| **Console Commands** | lowercase with dots | `gm.hero.set_clan` |
| **Enum Values** | PascalCase | `HeroTypes.Lord` |

## File Organization

```csharp
// 1. Using statements (grouped and ordered)
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

// 2. Namespace
namespace Bannerlord.GameMaster.{Domain}
{
    // 3. Enums (if applicable)
    [Flags]
    public enum {Type}Types { ... }

    // 4. Main class
    public static class {Type}Extensions
    {
        // 5. Public methods (most important first)
        public static {Type}Types Get{Type}Types(this {Type} entity) { ... }
        
        // 6. Private helper methods (at bottom)
        private static void HelperMethod() { ... }
    }
}
```

## Code Organization Regions

Use regions to group related functionality:

```csharp
public static class HeroManagementCommands
{
    #region Clan Management
    // set_clan, remove_clan
    #endregion

    #region Hero State Management
    // kill, imprison, release
    #endregion

    #region Hero Attributes
    // set_age, set_gold, heal
    #endregion
}
```

## Error Message Format

**Always use consistent formatting:**

```csharp
// ✅ Correct
return CommandBase.FormatErrorMessage("Hero not found.");
// Returns: "Error: Hero not found.\n"

return CommandBase.FormatSuccessMessage("Hero updated.");
// Returns: "Success: Hero updated.\n"

// ❌ Wrong
return "Error\n";
return "Hero not found";
```

## Validation Chain Order

**Always validate in this order:**

1. Campaign mode
2. Argument count
3. Entity resolution
4. Value validation
5. Business logic validation
6. Execute with error handling

```csharp
// 1. Campaign mode
if (!CommandBase.ValidateCampaignMode(out string error))
    return error;

// 2. Argument count
if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
    return error;

// 3. Entity resolution
var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
if (heroError != null) return heroError;

// 4. Value validation
if (!CommandValidator.ValidateIntegerRange(args[1], 0, 100, out int value, out error))
    return CommandBase.FormatErrorMessage(error);

// 5. Business logic (if applicable)
if (hero.IsAlive == false)
    return CommandBase.FormatErrorMessage("Cannot modify dead hero.");

// 6. Execute
return CommandBase.ExecuteWithErrorHandling(() => { ... });
```

## Documentation Standards

**XML Comments Required For:**
- All public methods
- All public classes
- Complex private methods

```csharp
/// <summary>
/// Finds a hero with the specified ID, using case-insensitive comparison
/// </summary>
/// <param name="heroId">The string ID of the hero to find</param>
/// <returns>The matching Hero, or null if not found</returns>
public static Hero GetHeroById(string heroId)
{
    // Implementation
}
```

## Logging Guidelines

**Always use `Cmd.Run()` wrapper:**

```csharp
// ✅ Correct - Automatic logging
[CommandLineFunctionality.CommandLineArgumentFunction("set_age", "gm.hero")]
public static string SetAge(List<string> args)
{
    return Cmd.Run(args, () =>
    {
        // Your logic here
        return CommandBase.FormatSuccessMessage("Age set.");
    });
}

// ❌ Wrong - No logging
public static string SetAge(List<string> args)
{
    // Direct implementation without Cmd.Run()
}
```

## Query Command Patterns

### Using QueryArgumentParser

When creating query commands, use the generic [`QueryArgumentParser<TEnum>`](../../Bannerlord.GameMaster/Console/Common/QueryArgumentParser.cs:11) to separate search terms from type keywords. This eliminates code duplication across entity types.

```csharp
// Define type keywords for your entity
private static readonly HashSet<string> HeroTypeKeywords = new()
{
    "lord", "wanderer", "merchant", "alive", "dead", "player"
};

// Parse arguments in your query command
public static string QueryHero(List<string> args)
{
    var (query, types) = QueryArgumentParser<HeroTypes>.Parse(
        args,
        HeroTypeKeywords,
        HeroQueries.ParseHeroTypes,
        HeroTypes.Alive  // default types
    );
    
    var heroes = HeroQueries.QueryHeroes(query, types);
    return FormatResults(heroes);
}
```

**Benefits:**
- Eliminates ~37 lines of duplicated parsing code per entity type
- Consistent argument handling across all query commands
- Cleaner, more maintainable command implementations

## Interface Implementation

### IEntityExtensions Interface

All extension classes **must** implement [`IEntityExtensions<TEntity, TTypes>`](../../Bannerlord.GameMaster/Common/Interfaces/IEntityExtensions.cs:5):

```csharp
// In your Extensions class file
public class HeroExtensionsWrapper : IEntityExtensions<Hero, HeroTypes>
{
    public HeroTypes GetTypes(Hero entity) => entity.GetHeroTypes();
    public bool HasAllTypes(Hero entity, HeroTypes types) => entity.HasAllTypes(types);
    public bool HasAnyType(Hero entity, HeroTypes types) => entity.HasAnyType(types);
    public string FormattedDetails(Hero entity) => entity.FormattedDetails();
}
```

**Requirements:**
- Create wrapper class at end of Extensions file
- Implement all four interface methods
- Use extension methods for implementation
- See [`HeroExtensions.cs`](../../Bannerlord.GameMaster/Heroes/HeroExtensions.cs:122) for complete example

### IEntityQueries Interface

All query classes **must** implement [`IEntityQueries<TEntity, TTypes>`](../../Bannerlord.GameMaster/Common/Interfaces/IEntityQueries.cs:6):

```csharp
// In your Queries class file
public class HeroQueriesWrapper : IEntityQueries<Hero, HeroTypes>
{
    public Hero GetById(string id) => HeroQueries.GetHeroById(id);
    public List<Hero> Query(string query, HeroTypes types, bool matchAll)
        => HeroQueries.QueryHeroes(query, types, matchAll);
    public HeroTypes ParseType(string typeString) => HeroQueries.ParseHeroType(typeString);
    public HeroTypes ParseTypes(IEnumerable<string> typeStrings)
        => HeroQueries.ParseHeroTypes(typeStrings);
    public string GetFormattedDetails(List<Hero> entities)
        => HeroQueries.GetFormattedDetails(entities);
}
```

**Requirements:**
- Create wrapper class at end of Queries file
- Implement all five interface methods
- Delegate to static query methods
- See [`HeroQueries.cs`](../../Bannerlord.GameMaster/Heroes/HeroQueries.cs:109) for complete example

## Error Handling Verification

**All state-modifying commands use `ExecuteWithErrorHandling()` wrapper** - this is already verified across all 30 management commands:

```csharp
// ✅ All management commands follow this pattern
return CommandBase.ExecuteWithErrorHandling(() =>
{
    // Your state-modifying logic here
    hero.ChangeState(CampaignSystem.CharacterStates.Dead);
    return CommandBase.FormatSuccessMessage($"Hero {hero.Name} killed.");
});
```

**No changes needed** - this pattern is already consistently applied throughout the codebase. When adding new management commands, ensure they use this wrapper.

---

## Next Steps

1. **Review** [Testing Guide](testing.md) for test procedures
2. **Check** [Troubleshooting](troubleshooting.md) for common issues
3. **Use** [Code Quality Checklist](../reference/code-quality-checklist.md) before committing
4. **Reference** [Implementation Improvements](../reference/implementation-improvements.md) for architecture patterns

---

**Navigation:** [← Back: Testing Template](../templates/testing.md) | [Back to Index](../README.md) | [Next: Testing Guide →](testing.md)