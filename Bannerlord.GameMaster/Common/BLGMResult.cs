using System;
using Bannerlord.GameMaster.Information;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Common
{
    public struct BLGMResult
    {
        ///<summary>Indicated if the operation failed or succeeded</summary>
        public bool wasSuccessful;
        
        ///<summary>Message with details of the operation</summary>
        public string message;
        
        ///<summary>Exception that occured, null if no exception</summary>
        public Exception exception = null;

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

        public BLGMResult(bool wasSuccessful, string message, Exception exception)
        {
            this.wasSuccessful = wasSuccessful;
            this.message = message;
            this.exception = exception;
        }

        /// <summary>
        /// Displays a game log message using InformationManager.DisplayMessage() <br />
        /// Message is red if wasSuccessful = false, green if wasSuccessful = true
        /// </summary>
        public readonly BLGMResult DisplayMessage()
        {
            if (wasSuccessful)
                InfoMessage.Success($"[GameMaster] {message}");
            else
                InfoMessage.Error($"[GameMaster] Error: {message}");

            return this; //Allow chaining
        }

        /// <summary>
        /// Writes to the game's main rgl log file the message and if successful
        /// </summary>
        public readonly BLGMResult Log()
        {
            string logEntry;

            if (wasSuccessful)
            {
                logEntry = $"[BLGM] SUCCESS: {message}";     
            }  
            else
            {
                logEntry = $"[BLGM] ERROR: {message}";
                
                if (exception != null && !string.IsNullOrWhiteSpace(exception.StackTrace))
                    logEntry += $"\nStack Trace:\n{exception.StackTrace}";           
            }

            Debug.Print(logEntry);

            return this; // Allow chaining
        }

        /// <summary>
        /// Displays message in game and writes to game's main log file
        /// </summary>
        public readonly BLGMResult DisplayAndLog()
        {
            DisplayMessage();
            Log();
            return this; // Allow chaining
        }
    }
}