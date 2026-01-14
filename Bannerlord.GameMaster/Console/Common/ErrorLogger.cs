using System;

namespace Bannerlord.GameMaster.Console.Common
{
    public static class ErrorLogger
    {
        /// <summary>
        /// Logs the consoleMessae and loggedException in the game's RGL log file then returns consoleMessage to display to user in console
        /// </summary>
        /// <param name="consoleMessage">Used to include message in log and also to chain message to display to user</param>
        /// <param name="loggedException">The exception to log in the rgl log file</param>
        /// <returns>consoleMessage for easy displaying back to user</returns>
        public static string LogError(string consoleMessage, Exception loggedException)
        {
            string error = $"BLGM Command Error: {consoleMessage}\nStack Trace:\n{loggedException.StackTrace}";
            TaleWorlds.Library.Debug.Print(error);

            return consoleMessage;
        }
    }
}