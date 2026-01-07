# BLGM Instructions
Bannerlord.GameMaster (BLGM) is a Bannerlord mod that provides a framework for interacting with, and managing game state such as modifying gameobjects or creating gameobjets and offering several utilities and helpers. BLGM also provides several console commands for easy use of features while ingame. Bannerlord.Commander (BLC) is another mod that actual build a GUI experience around BLGM providing richer, more interactive, and more powerful control. BLGM does not require BLC but BLC requires BLGM.

If you cant find something I tell you to reference or look at, always assume you are looking in the wrong place, because I would never tell you to reference something that doesnt exist entirely. If you cant find something you are looking for, or something I ask you to reference, look in the top level root of the project. If you still cant find it, it might be because its untracked, so in that case use dir commands to list files and folders.

Dont be confused that the root of the project also has a folder inside with the same name: Bannerlord.GameMaster\Bannerlord.GameMaster
The Examples folder is located in the root of the project: C:\Users\Shadow\source\repos\Bannerlord.GameMaster\Examples

## Native Code, Patterns, Data Structures, and Performance
/Examples/DecompiledCode contains several folders of decompiled Bannerlord code if you need to reference native code for anything. The Examples folder is located in the root of the project directory /repos/Bannerlord.GameMaster/Examples/DecompiledCode

Make sure to make use of multithreading when possible. and avoid stallig the main thread or making the UI unresponsive or delayed. Make sure to see how previous systems are implemented before proceeding.

Use Bannerlord native patterns when possible, as they are proven reliable, efficient, and extremely performant.

Use Bannerlord native data structures when possible as they are optimized for 
high performance workflows such as:
MBReadOnlyList, MBArrayList, MBSortedMultiList, MBBindingList, MBList, MBList2D, and any other native bannerlord data strucure when feasible. Many are located in the TaleWorlds.Library namespace.

Sorting and filtering lists must use the game's native patterns such as making use of the IsSorted MBReadOnlyList item property for sorting without rebuilding lists, as these native patterns are lightning fast and work great for lists of 1000s of objects.

## BLGM
BLGM has Culture Lookup helpers that can return a CultureObject for selected culture or list of all cutlures, culutural apropiate random hero, clan, or kingdom, names in Bannerlord.GameMaster.Cultures.CultureLookup. CultureLookup.AllCultures will return all cultures currently in game.

BLGM contains many Hero, Clan, Kingdom, Culture, MobileParty, Item, CharacterObject, and other extensions, reuse this when possible.

BLGM contains an easy to use InformationManager wrapper for displaying messages in game with pre set coloring based on type. Example: InfoMessage.Error("message") shows a red message, InfoMessage.Warning("message") shows a yellow message, and etc

BLGM should contain no UI elements or code other than Console commands and should only contain game logic. Commander is for UI and UI logic, BLGM is for game logic and console commands.

## BLGM Console Commands
BLGM contains several console commands. When writing a new console command, make sure to make use of prexisting code outside of the command. If they code the command requires doesn't exist, Create a seperate class or classes seperate from the command, and make the command use that code. Try to reuse as much exisitng code as possible.

Ensure all commands support named and positional arguments.
All Commands must make use of exisitng validation, parsing, error handeling, and logging systems and standards. Reference exisiting commands for implementation details.

All future commands, should be contained in their own seperate class, but should use exisitng command execution paths such as typing gm.hero.create_hero to execute the command create_hero

Commands must support multi word arguments using single quotes, as bannerlord console splits argument on space regardless of quotes and actually removes double quote mark before our mod can even see them, so single quotes must be used. See existing commands for implementation

Commands with culture arguments need to support using named culture groups ex: all_cultures, main_cultures, bandit_cultures, etc and also support specifying multiple cultures using commas with no space ex: vlandia,battania,empire or just a single culture ex: sturgia See existing commands for implementation details and the CultureLookup class

### Command Common Methods that should be used
See exisitng commands for reference on how they use these methods:
CommandBase.ValidateArgumentCount() - Use Whenever arguments are required which is almost always, as default strucure is to display usage when no arguments are used.
CommandBase.FormatSuccessMessage() - Always use
parsed.FormatArgumentDisplay() - Use Whenever arguments are required (pretty much always)
CommandBase.FormatErrorMessage() - Always Use
CommandBase.ExecuteWithErrorHandling(() =>{} - Always use as a wrapper around command
string usageMessage = CommandValidator.CreateUsageMessage() - Always use
parsed.GetValidationError() - When every arguments are used
parsed = CommandBase.ParseArguments(args) - When every arguments are used
CommandBase.ValidateCampaignMode() - Always use
Cmd.Run(args, () =>() - Always use, provides logging
Check for other common console methods and standards that were not mentioned above

## Code Standards
Use actual types whenever possible instead of var except for objects with complex types such as tuples. 
If you see exisiting instances of using var, please change it to the actual type if feasible.

When constructing a object, use "new(...)" instead of "new Type(...)" when possible, fix any exisiting instances you notice.

Keep all code clean, maintainable as possible

Adhere to:
DRY
Single Responsibility Principle
Interface segregation

Write all code with modularity in mind making it reusable for multiple cases. Adhere to DRY principles. Reuse as much code as possible, dont write repeating code.

Before writing code, make sure exisitng code that does what you need doesn't already exist, or code that can be easily adapted to also support what you need to do.

Ensure code is organized into clear seperate files relating only to their own function
Avoid repeating code using DRY. Seperate logic into methods or even sperate classes when feasible.
Make use of good orangization using folders, namespaces, classes, and methods
Make sure each class is in its own file, when possible avoid huge massive overloaded classes.
Make sure code is high quality, maintainable, human readable, and well organized.

Make use of MARK: for long methods. Use region for sections of code that has many short methods one after another.

### Performance Considerations
Avoid LINQ when performance-critical (use native collections instead, LINQ ok for non critical operations)
String concatenation in loops (use StringBuilder)
Object pooling patterns for frequently created/destroyed objects when applicable

## Documentation
IMPORTANT NOTE FOR UPDATING USER WIKI DOCUMENTATION: The /Wiki folder is for users documentation. the /Docs folder is for developers documentation. The /wiki folder will probably not be visible to you because it is untracked so run command "dir wiki/Bannerlord.GameMaster.wiki to view the existing wiki documents.


NEVER EVER USE EMOJIs