# Extensions Template

**Navigation:** [← Back: Item Management Example](../implementation/item-management-example.md) | [Back to Index](../README.md) | [Next: Queries Template →](queries.md)

---

## Purpose

The Extensions layer adds domain-specific behavior to game entities without modifying them. This template provides a ready-to-use pattern for creating extension classes.

## When to Use

- Adding a new entity type (Item, Settlement, Party, etc.)
- Need to categorize entities with type flags
- Need reusable formatting or behavior methods

## Standard Methods Required

Every Extensions class must implement these four methods:

1. **`GetTypes()`** - Returns all applicable type flags for an entity
2. **`HasAllTypes()`** - Checks if entity has ALL specified flags (AND logic)
3. **`HasAnyType()`** - Checks if entity has ANY specified flags (OR logic)
4. **`FormattedDetails()`** - Returns formatted string for display

**Additionally, all Extensions classes must implement the [`IEntityExtensions<TEntity, TTypes>`](../../Bannerlord.GameMaster/Common/Interfaces/IEntityExtensions.cs:5) interface** through a wrapper class (see [Interface Implementation](#interface-implementation) section below).

## Real Examples

- [`HeroExtensions.GetHeroTypes()`](../../Bannerlord.GameMaster/Heroes/HeroExtensions.cs:37)
- [`ClanExtensions.HasAllTypes()`](../../Bannerlord.GameMaster/Clans/ClanExtensions.cs:67)
- [`HeroExtensions.FormattedDetails()`](../../Bannerlord.GameMaster/Heroes/HeroExtensions.cs:86)
- [`HeroExtensionsWrapper`](../../Bannerlord.GameMaster/Heroes/HeroExtensions.cs:122) - Interface implementation example

---

## Template Code

```csharp
using System;
using TaleWorlds.CampaignSystem; // Or appropriate namespace

namespace Bannerlord.GameMaster.{EntityType}s
{
    /// <summary>
    /// Flags enum for {EntityType} categorization
    /// </summary>
    [Flags]
    public enum {EntityType}Types
    {
        None = 0,
        TypeA = 1,      // Replace with actual type
        TypeB = 2,      // Replace with actual type
        TypeC = 4,      // Replace with actual type
        TypeD = 8,      // Add more using powers of 2
        TypeE = 16,
        TypeF = 32,
        TypeG = 64,
        TypeH = 128
    }

    /// <summary>
    /// Extension methods for {EntityType} entities
    /// </summary>
    public static class {EntityType}Extensions
    {
        /// <summary>
        /// Gets all applicable type flags for the {entity}
        /// </summary>
        /// <param name="{entity}">The {entity} to analyze</param>
        /// <returns>Combined type flags</returns>
        public static {EntityType}Types Get{EntityType}Types(this {EntityType} {entity})
        {
            {EntityType}Types types = {EntityType}Types.None;
            
            // TODO: Set flags based on entity properties
            // Example logic:
            // if ({entity}.SomeProperty) types |= {EntityType}Types.TypeA;
            // if ({entity}.OtherProperty) types |= {EntityType}Types.TypeB;
            
            return types;
        }

        /// <summary>
        /// Checks if {entity} has ALL specified types (AND logic)
        /// </summary>
        /// <param name="{entity}">The {entity} to check</param>
        /// <param name="types">Type flags to check</param>
        /// <returns>True if {entity} has all specified types</returns>
        public static bool HasAllTypes(this {EntityType} {entity}, {EntityType}Types types)
        {
            if (types == {EntityType}Types.None) return true;
            var entityTypes = {entity}.Get{EntityType}Types();
            return (entityTypes & types) == types;
        }

        /// <summary>
        /// Checks if {entity} has ANY specified types (OR logic)
        /// </summary>
        /// <param name="{entity}">The {entity} to check</param>
        /// <param name="types">Type flags to check</param>
        /// <returns>True if {entity} has any specified type</returns>
        public static bool HasAnyType(this {EntityType} {entity}, {EntityType}Types types)
        {
            if (types == {EntityType}Types.None) return true;
            var entityTypes = {entity}.Get{EntityType}Types();
            return (entityTypes & types) != {EntityType}Types.None;
        }

        /// <summary>
        /// Returns formatted string representation of {entity} for display
        /// </summary>
        /// <param name="{entity}">The {entity} to format</param>
        /// <returns>Tab-separated formatted string</returns>
        public static string FormattedDetails(this {EntityType} {entity})
        {
            // TODO: Customize the displayed properties
            return $"{{{entity}.StringId}}\t{{{entity}.Name}}\t[Properties]";
        }
    }
}
```

---

## Interface Implementation

**Required:** Every Extensions class must include a wrapper class implementing [`IEntityExtensions<TEntity, TTypes>`](../../Bannerlord.GameMaster/Common/Interfaces/IEntityExtensions.cs:5):

```csharp
/// <summary>
/// Wrapper class implementing IEntityExtensions interface for {EntityType} entities
/// </summary>
public class {EntityType}ExtensionsWrapper : IEntityExtensions<{EntityType}, {EntityType}Types>
{
    public {EntityType}Types GetTypes({EntityType} entity) => entity.Get{EntityType}Types();
    public bool HasAllTypes({EntityType} entity, {EntityType}Types types) => entity.HasAllTypes(types);
    public bool HasAnyType({EntityType} entity, {EntityType}Types types) => entity.HasAnyType(types);
    public string FormattedDetails({EntityType} entity) => entity.FormattedDetails();
}
```

Add this wrapper class at the end of your Extensions file, after all extension methods. The wrapper delegates to your extension methods and provides a consistent interface for generic operations.

**See:** [`HeroExtensionsWrapper`](../../Bannerlord.GameMaster/Heroes/HeroExtensions.cs:122) for a complete implementation example.

---

## Usage Instructions

### Step 1: Replace Placeholders

1. Replace `{EntityType}` with your entity type (e.g., `Item`, `Settlement`)
2. Replace `{entity}` with lowercase variable name (e.g., `item`, `settlement`)
3. Replace `TypeA`, `TypeB`, etc. with meaningful type names
4. Update the namespace to match your domain

### Step 2: Implement GetTypes()

Define the logic for determining which type flags apply to an entity:

```csharp
public static ItemTypes GetItemTypes(this ItemObject item)
{
    ItemTypes types = ItemTypes.None;
    
    if (item.IsFood) types |= ItemTypes.Food;
    if (item.IsTrade) types |= ItemTypes.Trade;
    if (item.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon)
        types |= ItemTypes.Weapon | ItemTypes.OneHanded;
    
    return types;
}
```

### Step 3: Customize FormattedDetails()

Choose which properties to display:

```csharp
public static string FormattedDetails(this ItemObject item)
{
    return $"{item.StringId}\t{item.Name}\tValue: {item.Value}";
}
```

---

## Best Practices

### Flags Enum

✅ **Do:**
- Use powers of 2 for flag values (1, 2, 4, 8, 16, 32, etc.)
- Always include `None = 0`
- Use `[Flags]` attribute
- Use PascalCase for enum values

❌ **Don't:**
- Use sequential numbers (1, 2, 3, 4)
- Skip the `[Flags]` attribute
- Use lowercase enum values

### Type Detection

✅ **Do:**
- Use bitwise OR (`|=`) to combine flags
- Check multiple properties for comprehensive typing
- Return `None` if no types apply

❌ **Don't:**
- Use string concatenation or arrays
- Return null
- Throw exceptions for untyped entities

### FormattedDetails

✅ **Do:**
- Use tab-separated values (`\t`) for consistent alignment
- Include essential properties (ID, Name, key attributes)
- Keep output concise (one line per entity)

❌ **Don't:**
- Use multi-line output
- Include verbose descriptions
- Use inconsistent separators

---

## Testing Your Extensions

Create a simple test to verify your implementation:

```csharp
// In your Tests.cs file
TestRunner.RegisterTest(new TestCase(
    "{entity}_ext_001",
    "GetTypes should return valid type flags",
    "gm.query.{entity}",
    TestExpectation.NoException
)
{
    Category = "{EntityType}Extensions"
});
```

---

## Next Steps

Once your Extensions class is complete:

1. **Create** [Queries layer](queries.md) to search and filter entities
2. **Add** [Query Commands](query-commands.md) for console interface
3. **Implement** [Management Commands](management-commands.md) for state modification
4. **Write** [Tests](testing.md) for all commands
5. **Implement** IEntityExtensions interface wrapper (required)

---

**Navigation:** [← Back: Item Management Example](../implementation/item-management-example.md) | [Back to Index](../README.md) | [Next: Queries Template →](queries.md)