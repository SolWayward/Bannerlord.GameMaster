using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.Common.EntityFinding
{
    /// <summary>
    /// Result struct for entity finder operations.
    /// Replaces tuple returns with a cleaner, more expressive struct.
    /// </summary>
    /// <typeparam name="T">The entity type (must inherit from MBObjectBase)</typeparam>
    public struct EntityFinderResult<T> where T : MBObjectBase
    {
        /// <summary>
        /// The found entity, or null if not found or error occurred.
        /// </summary>
        public T Entity;

        /// <summary>
        /// Indicates whether the find operation was successful.
        /// </summary>
        public bool IsSuccess;

        /// <summary>
        /// Error or status message. Null when successful.
        /// </summary>
        public string Message;

        /// <summary>
        /// Creates a default failed result.
        /// </summary>
        public EntityFinderResult()
        {
            Entity = null;
            IsSuccess = false;
            Message = "Unhandled failure";
        }

        /// <summary>
        /// Creates a result with specified values.
        /// </summary>
        public EntityFinderResult(T entity, bool isSuccess, string message)
        {
            Entity = entity;
            IsSuccess = isSuccess;
            Message = message;
        }

        /// <summary>
        /// Creates a successful result with the found entity.
        /// </summary>
        public static EntityFinderResult<T> Success(T entity)
        {
            return new(entity, true, null);
        }

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        public static EntityFinderResult<T> Error(string message)
        {
            return new(null, false, message);
        }
    }
}
