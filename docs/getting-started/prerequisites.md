# Prerequisites

**Navigation:** [← Back to Index](../README.md) | [Next: Architecture Overview →](architecture-overview.md)

---

Before using this guide, you should have the required knowledge, tools, and environment setup.

## Required Knowledge

### C# Language Features

You should be familiar with:
- **Extension methods** and how they work
- **LINQ queries** and lambda expressions
- **Flags enums** and bitwise operations (`|`, `&`, `==`)
- **Nullable reference types** and null handling

### TaleWorlds API Familiarity

You should have basic understanding of:
- `Hero`, `Clan`, `Kingdom` entities
- Campaign mode and game state concepts
- How to access game collections (e.g., `Hero.AllAliveHeroes`)

## Development Environment

### Required Tools

- **Visual Studio 2019 or later** (or JetBrains Rider)
- **.NET Framework 4.7.2 SDK**
- **Mount & Blade II: Bannerlord** installed
- **Basic understanding** of mod development for Bannerlord

### Recommended Tools

- **Git** for version control
- **A text editor** with C# syntax highlighting
- **Console** for running in-game commands

## Helpful Resources

- [TaleWorlds Modding Documentation](https://docs.bannerlordmods.com/)
- [Bannerlord API Documentation](https://apidoc.bannerlord.com/)
- Existing code in this project (Heroes, Clans, Kingdoms systems)

## Verification Checklist

Before proceeding, ensure you have:

- [ ] C# development environment set up
- [ ] Mount & Blade II: Bannerlord installed
- [ ] Basic understanding of TaleWorlds API
- [ ] Familiarity with extension methods and LINQ
- [ ] Git repository cloned (if contributing)

---

## Next Steps

Once you have the prerequisites in place:

1. **Review** [Architecture Overview](architecture-overview.md) to understand the system design
2. **Study** [Implementation Workflow](../implementation/workflow.md) for the development process
3. **Use** [Templates](../templates/extensions.md) as starting points for your code

---

**Navigation:** [← Back to Index](../README.md) | [Next: Architecture Overview →](architecture-overview.md)