using System;
using Bannerlord.GameMaster.Common.Interfaces;
using Bannerlord.GameMaster.Information;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Common
{
    /// <summary>
    /// An object containing a bool indicating if an operation succeeded, a string message with details of the result of the operation,
    /// and an exception if an exception occured. Also includes convenience methods for logging to game rgl log and system console and or displaying in game.
    /// </summary>
    public class BLGMResult : ResultBase<BLGMResult>
    {
        protected override string Prefix => "[BLGM]";

        /// <inheritdoc/>
        public BLGMResult() : base() { }

        /// <inheritdoc/>
        public BLGMResult(bool isSuccess, string message) : base(isSuccess, message) { }

        /// <inheritdoc/>
        public BLGMResult(bool isSuccess, string message, Exception ex) : base(isSuccess, message, ex) { }
    }
}