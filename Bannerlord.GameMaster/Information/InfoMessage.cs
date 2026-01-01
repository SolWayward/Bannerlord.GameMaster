using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Information
{
    public static class InfoMessage
    {
        /// <summary>
        /// Default White information message used as general log messages for BLGM
        /// </summary>
        public static void Log(string message)
        {
            Write(message, Color.White);
        }

        /// <summary>
        /// Green information message used as success messages for BLGM
        /// </summary>
        public static void Success(string message)
        {
            Write(message, Colors.Green);
        }

        /// <summary>
        /// Yellow information message used as warning messages for BLGM
        /// </summary>
        public static void Warning(string message)
        {
            Write(message, Colors.Yellow);
        }

        /// <summary>
        /// Red information message used as Error messages for BLGM
        /// </summary>
        public static void Error(string message)
        {
            Write(message, Colors.Red);
        }

        /// <summary>
        /// Magenta information message used for attention grabbing important messages for BLGM
        /// </summary>
        public static void Important(string message)
        {
            Write(message, Colors.Magenta);
        }

        /// <summary>
        /// Cyan information message used as status messages for BLGM
        /// </summary>
        public static void Status(string message)
        {
            Write(message, Colors.Cyan);
        }

        /// <summary>
        /// Blue information message used as alternate status messages for BLGM
        /// </summary>
        public static void Status2(string message)
        {
            Write(message, Colors.Cyan);
        }

        /// <summary>
        /// Information message allowing for custom color
        /// </summary>
        public static void Write(string message, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(message, color));
        }
    }
}