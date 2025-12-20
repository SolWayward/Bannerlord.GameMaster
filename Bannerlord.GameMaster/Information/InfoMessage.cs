using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Information
{
    public static class InfoMessage
    {
        public static void Display(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message));
        }
    }
}