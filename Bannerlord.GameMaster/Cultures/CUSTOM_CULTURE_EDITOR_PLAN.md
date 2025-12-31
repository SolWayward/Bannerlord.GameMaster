# Custom Culture Editor - Implementation Plan

## Overview
Create an in-game editor allowing users to customize a template culture by mixing/matching existing game assets and adding custom values. All customizations persist via CampaignBehavior serialization.

## Phase 1: Foundation

### XML Template Culture
- Create `gm_custom_culture` in ModuleData/spcultures.xml
- Provide reasonable defaults for all required properties
- Use neutral/generic values as starting point

### Persistence System
- Create `CultureCustomizationBehavior : CampaignBehaviorBase`
- Follow `SettlementNameBehavior` pattern (parallel lists for serialization)
- Store: culture_id → customization_data mapping
- `SyncData()` for save/load
- `OnSessionLaunched()` to reapply customizations

## Phase 2: Customization Features

### Visual (Easy - Direct Property Assignment)
- **Colors**: Color, Color2, BackgroundColor1, BackgroundColor2
  - Hex color picker interface
  - Store as uint values
- **Banner Icons**: Reference existing banner symbols

### 3D Assets (Easy - Reference Existing)
- **Scene Prefabs**: Mix/match from all cultures
  - Settlement walls, keeps, castles
  - Village layouts, ranch scenes
  - Tournament arenas, ambush scenes
- Implementation: Assign references from existing CultureObjects

### Names (Easy - List Manipulation)
- **Hero Names**: Male/Female pools
  - Add custom names to culture.MaleNameList/FemaleNameList
  - Option to import names from other cultures
  - Store as pipe-separated strings
- **Clan Names**: culture.ClanNameList
- Uses existing `CultureLookup.GetUniqueRandomHeroName()` system

### Troops (Current: Mix/Match, Future: XML Generation)
- **Phase 2A (NOW)**: Assign existing CharacterObjects
  - culture.BasicTroops / EliteBasicTroops
  - Select from all vanilla troops across cultures
  - Store as StringId lists
- **Phase 2B (FUTURE)**: Dynamic XML generation
  - Generate custom_troops.xml at runtime
  - Create hybrid troops (Vlandia armor + Battania weapons)
  - Load xml into game durring runtime, or edit template xml that is already loaded so game doesnt have to reload

### Items (Easy - Reference Existing)
- **Equipment Pools**: culture's default items
  - Civilian equipment sets
  - Battle equipment preferences
  - Mix armor/weapons from any culture
- Store as ItemObject StringId lists
- Explore what is needed to define what items show up in settlement trade window

### Gameplay (Moderate - Property Values)
- **Culture Traits**: Existing properties
  - CanHaveSettlement, IsBandit, IsMainCulture
  - Prosperity/militia modifiers
  - Recruitment speed, party size bonuses
- Store as key-value pairs (property_name → value)
- Explore what it takes to change players culture and ensure everything applies and works properly

## Phase 3: Editor Interface

### Console Commands (MVP)
```
gm.culture.customize <culture_id>
  --colors <hex1> <hex2>
  --add-names <male/female/clan> <name1,name2,...>
  --import-names <source_culture_id> <male/female/clan>
  --set-troops <troop_id1,troop_id2,...>
  --set-scenes <wall=vlandia> <arena=battania>
  --set-trait <trait_name> <value>
  --reset (restore to XML defaults)
```

### UI (Future Enhancement)
- In-game GUI for visual editing
- Real-time preview of changes
- Drag-drop asset selection
- Color wheel interface

## Implementation Order

1. **CultureCustomizationBehavior** - Persistence foundation
2. **Color Customization** - Simplest, immediate visual feedback
3. **Name Pool System** - Extends existing CultureLookup
4. **Troop/Item Assignment** - References only, no creation
5. **Scene Prefab Mixing** - Reference assignment
6. **Trait Customization** - Property value setting
7. **Console Commands** - User interface
8. **Reset Functionality** - Restore original values

## Data Structures

```csharp
// Stored per culture_id
CustomCultureData {
    Colors: uint[] (Color, Color2, BG1, BG2)
    MaleNames: string (pipe-separated)
    FemaleNames: string (pipe-separated)  
    ClanNames: string (pipe-separated)
    BasicTroops: string (StringIds, comma-separated)
    EliteBasicTroops: string (StringIds)
    SceneOverrides: Dictionary<string, string> (scene_type → source_culture)
    Traits: Dictionary<string, string> (property_name → value)
    OriginalValues: string (for reset - JSON serialized)
}
```

## Dependencies

- Existing: `CultureLookup`, `ObjectManager`, `MBObjectManager`
- Pattern: `SettlementNameBehavior`, `SettlementSaveDefiner`
- New: `CultureCustomizationBehavior`, `CultureCustomizationCommands`

## Future Enhancements (Post-MVP)

- **Dynamic Troop XML Generation** - Custom troop creation
- **Item Modifier System** - Custom weapon/armor stats
- **Culture Relationships** - Set diplomacy modifiers
- **Export/Import** - Share culture configs between games
- **Preset Library** - Pre-made culture templates

## Success Criteria

- User can customize template culture via commands
- All changes persist through save/load
- Game recognizes customizations (heroes use new names, colors show in UI)
- Can reset to defaults
- No save corruption or references breaking
- Compatible with multiplayer (host determines culture state)
