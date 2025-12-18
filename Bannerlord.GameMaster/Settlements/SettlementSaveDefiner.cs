using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace Bannerlord.GameMaster.Settlements
{
    /// <summary>
    /// Defines saveable types for settlement customization data.
    /// Using ID range 900_000_000 for GameMaster mod to avoid conflicts.
    /// </summary>
    public class SettlementSaveDefiner : SaveableTypeDefiner
    {
        public SettlementSaveDefiner() : base(900_000_000) { }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(SettlementNameData), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<string, string>));
            ConstructContainerDefinition(typeof(List<string>));
        }
    }

    /// <summary>
    /// Saveable data class for custom settlement names.
    /// Stores both custom names and original names for reset functionality.
    /// Properties are automatically serialized by the TaleWorlds SaveSystem.
    /// </summary>
    public class SettlementNameData
    {
        /// <summary>
        /// Maps settlement StringId to custom name
        /// </summary>
        public Dictionary<string, string> CustomNames { get; set; }

        /// <summary>
        /// Maps settlement StringId to original name (for reset functionality)
        /// </summary>
        public Dictionary<string, string> OriginalNames { get; set; }

        public SettlementNameData()
        {
            CustomNames = new Dictionary<string, string>();
            OriginalNames = new Dictionary<string, string>();
        }
    }
}
