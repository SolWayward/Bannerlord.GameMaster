using Bannerlord.GameMaster.Information;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Common
{
    public struct BLGMResult
    {
        public bool wasSuccessful;
        public string message;

        public BLGMResult()
        {
            wasSuccessful = false;
            message = "unhandled failure";
        }

        public BLGMResult(bool wasSuccessful, string message)
        {
            this.wasSuccessful = wasSuccessful;
            this.message = message;
        }

        /// <summary>
        /// Displays a game log message using InformationManager.DisplayMessage() <br />
        /// Message is red if wasSuccessful = false, green if wasSuccessful = true
        /// </summary>
        public BLGMResult DisplayMessage()
        {
            if (wasSuccessful)
                InfoMessage.Success(message);
            else
                InfoMessage.Error(message);

            return this; //Allow chaining
        }

        /// <summary>
        /// Writes to the game's main rgl log file the message and if successful
        /// </summary>
        public BLGMResult Log()
        {
            if (wasSuccessful)
                Debug.Print($"[BLGM] SUCCESS: {message}");       
            else
                Debug.Print($"[BLGM] ERROR: {message}");

            return this; //Allow chaining
        }

        /// <summary>
        /// Displays message in game and writes to game's main log file
        /// </summary>
        public BLGMResult DisplayAndLog()
        {
            DisplayMessage();
            Log();
            return this; //Allow chaining
        }
    }
}