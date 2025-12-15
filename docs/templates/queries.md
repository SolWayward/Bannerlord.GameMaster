# Queries Template

**Navigation:** [← Back: Extensions Template](extensions.md) | [Back to Index](../README.md) | [Next: Query Commands Template →](query-commands.md)

---

## Purpose

The Queries layer provides a unified search and filtering interface for entities. This template helps you create consistent query classes.

## When to Use

- After Extensions layer is complete
- Need to search entities by name/ID
- Need to filter entities by type flags

## Standard Methods Required

Every Queries class must implement these five methods:

1. **`GetById()`** - Find entity by exact ID
2. **`Query{EntityType}s()`** - Main search/filter method
3. **`Parse{EntityType}Type()`** - Convert string to single enum value
4. **`Parse{EntityType}Types()`** - Convert strings to combined flags
5. **`GetFormattedDetails()`** - Format list of entities for display

**Additionally, all Queries classes must implement the [`IEntityQueries<TEntity, TTypes>`](../../Bannerlord.GameMaster/Common/Interfaces/IEntityQueries.cs:6) interface** through a wrapper class (see [Interface Implementation](#interface-implementation) section below).

## Real Examples

- [`HeroQueries.QueryHeroes()`](../../Bannerlord.GameMaster/Heroes/HeroQueries.cs:29)
- [`ClanQueries.ParseClanType()`](../../Bannerlord.GameMaster/Clans/ClanQueries.cs:54)
- [`HeroQueriesWrapper`](../../Bannerlord.GameMaster/Heroes/HeroQueries.cs:109) - Interface implementation example

---

## Template Code

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem; // Or appropriate namespace

namespace Bannerlord.GameMaster.{EntityType}s
{
    /// <summary>
    /// Provides utility methods for querying {EntityType} entities
    /// </summary>
    public static class {EntityType}Queries
    {
        /// <summary>
        /// Finds a {entity} with the specified ID, using case-insensitive comparison
        /// </summary>
        /// <param name="{entity}Id">The string ID of the {entity} to find</param>
        /// <returns>The matching {EntityType}, or null if not found</returns>
        public static {EntityType} Get{EntityType}ById(string {entity}Id)
        {
            // TODO: Replace with appropriate collection access
            return {EntityType}.FindFirst(e =>
                e.StringId.Equals({entity}Id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Main unified method to find {entity}s by search string and type flags
        /// </summary>
        /// <param name="query">Optional case-insensitive substring to filter by name or ID</param>
        /// <param name="requiredTypes">{EntityType} type flags (AND logic by default)</param>
        /// <param name="matchAll">If true, {entity} must have ALL flags. If false, ANY flag</param>
        /// <returns>List of {entity}s matching all criteria</returns>
        public static List<{EntityType}> Query{EntityType}s(
            string query = "",
            {EntityType}Types requiredTypes = {EntityType}Types.None,
            bool matchAll = true)
        {
            // TODO: Replace with appropriate collection access
            IEnumerable<{EntityType}> entities = {EntityType}.All;

            // Filter by name/ID if provided
            if (!string.IsNullOrEmpty(query))
            {
                string lowerFilter = query.ToLower();
                entities = entities.Where(e =>
                    e.Name.ToString().ToLower().Contains(lowerFilter) ||
                    e.StringId.ToLower().Contains(lowerFilter));
            }

            // Filter by types
            if (requiredTypes != {EntityType}Types.None)
            {
                entities = entities.Where(e =>
                    matchAll ? e.HasAllTypes(requiredTypes) : e.HasAnyType(requiredTypes));
            }

            return entities.ToList();
        }

        /// <summary>
        /// Parse a string into {EntityType}Types enum value
        /// </summary>
        /// <param name="typeString">String representation of type</param>
        /// <returns>Parsed enum value or None if invalid</returns>
        public static {EntityType}Types Parse{EntityType}Type(string typeString)
        {
            // TODO: Add any custom aliases or normalization
            // Example: "1h" -> "OneHanded"
            
            if (Enum.TryParse<{EntityType}Types>(typeString, true, out var result))
                return result;
            return {EntityType}Types.None;
        }

        /// <summary>
        /// Parse multiple strings and combine into {EntityType}Types flags
        /// </summary>
        /// <param name="typeStrings">Collection of type strings</param>
        /// <returns>Combined type flags</returns>
        public static {EntityType}Types Parse{EntityType}Types(IEnumerable<string> typeStrings)
        {
            {EntityType}Types combined = {EntityType}Types.None;
            foreach (var typeString in typeStrings)
            {
                var parsed = Parse{EntityType}Type(typeString);
                if (parsed != {EntityType}Types.None)
                    combined |= parsed;
            }
            return combined;
        }

        /// <summary>
        /// Returns a formatted string listing {entity} details
        /// </summary>
        /// <param name="entities">List of {entity}s to format</param>
        /// <returns>Formatted multi-line string</returns>
        public static string GetFormattedDetails(List<{EntityType}> entities)
        {
            if (entities.Count == 0)
                return "";
            return string.Join("\n", entities.Select(e => e.FormattedDetails())) + "\n";
        }
    }
}
```

---

## Interface Implementation

**Required:** Every Queries class must include a wrapper class implementing [`IEntityQueries<TEntity, TTypes>`](../../Bannerlord.GameMaster/Common/Interfaces/IEntityQueries.cs:6):

```csharp
/// <summary>
/// Wrapper class implementing IEntityQueries interface for {EntityType} entities
/// </summary>
public class {EntityType}QueriesWrapper : IEntityQueries<{EntityType}, {EntityType}Types>
{
    public {EntityType} GetById(string id) => {EntityType}Queries.Get{EntityType}ById(id);
    public List<{EntityType}> Query(string query, {EntityType}Types types, bool matchAll)
        => {EntityType}Queries.Query{EntityType}s(query, types, matchAll);
    public {EntityType}Types ParseType(string typeString) => {EntityType}Queries.Parse{EntityType}Type(typeString);
    public {EntityType}Types ParseTypes(IEnumerable<string> typeStrings)
        => {EntityType}Queries.Parse{EntityType}Types(typeStrings);
    public string GetFormattedDetails(List<{EntityType}> entities)
        => {EntityType}Queries.GetFormattedDetails(entities);
}
```

Add this wrapper class at the end of your Queries file, after all static methods. The wrapper delegates to your static query methods and provides a consistent interface for generic operations.

**See:** [`HeroQueriesWrapper`](../../Bannerlord.GameMaster/Heroes/HeroQueries.cs:109) for a complete implementation example.

---

## Usage Instructions

### Step 1: Replace Placeholders

1. Replace `{EntityType}` with your entity type (e.g., `Item`, `Settlement`)
2. Replace `{entity}` with lowercase variable name (e.g., `item`, `settlement`)
3. Update the namespace to match your domain
4. Replace collection access (`{EntityType}.All`) with appropriate API

### Step 2: Implement GetById()

Update the entity collection access:

```csharp
// For Items
return Items.All.FirstOrDefault(i => 
    i.StringId.Equals(itemId, StringComparison.OrdinalIgnoreCase));

// For Heroes
return Hero.FindFirst(h => 
    h.StringId.Equals(heroId, StringComparison.OrdinalIgnoreCase));
```

### Step 3: Add Aliases to ParseType()

Support common abbreviations:

```csharp
public static ItemTypes ParseItemType(string typeString)
{
    var normalized = typeString.ToLower() switch
    {
        "1h" => "OneHanded",
        "2h" => "TwoHanded",
        _ => typeString
    };
    
    return Enum.TryParse<ItemTypes>(normalized, true, out var result) 
        ? result : ItemTypes.None;
}
```

---

## Best Practices

### Query Method

✅ **Do:**
- Provide default values for optional parameters
- Support both name and ID filtering
- Use case-insensitive comparison
- Return empty list (not null) when no matches

❌ **Don't:**
- Return null for no matches
- Throw exceptions for empty results
- Use case-sensitive comparison

### Type Parsing

✅ **Do:**
- Support common abbreviations
- Use case-insensitive parsing
- Return `None` for invalid input
- Combine multiple types with bitwise OR

❌ **Don't:**
- Throw exceptions for invalid types
- Return null
- Use magic strings

### Collection Access

✅ **Do:**
- Use efficient LINQ queries
- Filter progressively (name → types)
- Convert to List at the end

❌ **Don't:**
- Call ToList() multiple times
- Create multiple enumerations
- Load all entities into memory unnecessarily

---

## Testing Your Queries

```csharp
// Test basic query
TestRunner.RegisterTest(new TestCase(
    "{entity}_query_001",
    "Query without parameters should return all {entity}s",
    "gm.query.{entity}",
    TestExpectation.Contains
)
{
    Category = "{EntityType}Query",
    ExpectedText = "{entity}(s)"
});

// Test type filtering
TestRunner.RegisterTest(new TestCase(
    "{entity}_query_002",
    "Query with type filter should work",
    "gm.query.{entity} typeA",
    TestExpectation.Contains
)
{
    Category = "{EntityType}Query",
    ExpectedText = "{entity}(s)"
});
```

---

## Using QueryArgumentParser

When implementing query commands that use your Queries class, use the generic [`QueryArgumentParser<TEnum>`](../../Bannerlord.GameMaster/Console/Common/QueryArgumentParser.cs:11) to parse command arguments:

```csharp
// In your Query Commands file
using Bannerlord.GameMaster.Console.Common;

// Define type keywords for your entity
private static readonly HashSet<string> {EntityType}TypeKeywords = new()
{
    "typeA", "typeB", "typeC"  // Replace with actual type names
};

public static string Query{EntityType}(List<string> args)
{
    // Parse arguments using QueryArgumentParser
    var (query, types) = QueryArgumentParser<{EntityType}Types>.Parse(
        args,
        {EntityType}TypeKeywords,
        {EntityType}Queries.Parse{EntityType}Types,
        {EntityType}Types.None  // default types
    );
    
    // Use parsed values with your query method
    var results = {EntityType}Queries.Query{EntityType}s(query, types);
    return FormatResults(results);
}
```

**Benefits:**
- Eliminates code duplication across entity types
- Separates search terms from type keywords automatically
- Consistent argument handling
- See [`HeroQueryCommands`](../../Bannerlord.GameMaster/Console/Query/HeroQueryCommands.cs:15) for real implementation

---

## Next Steps

Once your Queries class is complete:

1. **Create** [Query Commands](query-commands.md) for console interface using QueryArgumentParser
2. **Add** [Management Commands](management-commands.md) for state modification
3. **Write** [Tests](testing.md) for all commands
4. **Implement** IEntityQueries interface wrapper (required)

---

**Navigation:** [← Back: Extensions Template](extensions.md) | [Back to Index](../README.md) | [Next: Query Commands Template →](query-commands.md)