// © 2023, Worth Systems.

using System.Collections;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for different types of collections.
    /// </summary>
    internal static class CollectionExtensions
    {
        #region HasAny
        /// <summary>
        /// Determines whether the given collection has any elements.
        /// </summary>
        /// <param name="collection">The collection to be checked.</param>
        /// <returns>
        ///   <see langword="true"/> if not empty; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool HasAny<T>(this T[] collection)
        {
            return collection.Length > 0;  // NOTE: Faster than Any()
        }
        
        /// <inheritdoc cref="HasAny{T}(T[])"/>
        internal static bool HasAny(this ICollection? collection)
        {
            return collection?.Count > 0;  // NOTE: Faster than Any()
        }
        #endregion

        #region IsEmpty
        /// <summary>
        /// Determines whether the given collection doesn't have any elements.
        /// </summary>
        /// <param name="collection">The collection to be checked.</param>
        /// <returns>
        ///   <see langword="true"/> if empty; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsEmpty<T>(this T[] collection)
        {
            return !collection.HasAny();
        }
        
        /// <inheritdoc cref="IsEmpty{T}(T[])"/>
        internal static bool IsEmpty(this ICollection? collection)
        {
            return !collection.HasAny();
        }
        #endregion
    }
}