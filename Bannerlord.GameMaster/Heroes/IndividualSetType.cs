namespace Bannerlord.GameMaster.Heroes
{
    /// <summary>
    /// Identifies the type of an individual set file for import operations.
    /// Used by CharacterSetFileManager.ImportFromIndividualSet() to determine
    /// which file manager to use for loading and which section to apply.
    /// </summary>
    public enum IndividualSetType
    {
        Appearance,
        Development,
        Traits,
        BattleEquipment,
        CivilianEquipment
    }
}
