using System;

namespace ScaleSharpLight
{
    /// <summary>
    /// Class for pseudo value for the rust enum aka tagged-union.
    /// </summary>
    public static class TaggedUnion
    {
        /// <summary>
        /// Generate pseudo value for the variant of the rust enum aka tagged-union.
        /// </summary>
        /// <typeparam name="Tag">Enum type as tag</typeparam>
        /// <typeparam name="T">Contained type by the variant</typeparam>
        /// <param name="tag">Enum value as tag</param>
        /// <param name="elem">Contained value by the variant</param>
        /// <returns></returns>
        public static ValueTuple<Tag, T> Element<Tag, T>(
            Tag tag, T elem) where Tag : Enum
        {
            return ValueTuple.Create(tag, elem);
        }
    }
}
