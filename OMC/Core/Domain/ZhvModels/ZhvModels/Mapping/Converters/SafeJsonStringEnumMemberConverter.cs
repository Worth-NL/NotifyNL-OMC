// © 2023, Worth Systems.

using System.Text.Json.Serialization;

namespace ZhvModels.Mapping.Converters
{
    /// <summary>
    /// An extension of <see cref="JsonStringEnumMemberConverter"/> JSON converter which will ensure to generate default
    /// <typeparamref name="TEnum"/> value in case that provided string cannot be recognized as one of the supported values.
    /// </summary>
    /// <typeparam name="TEnum">The type of enum.</typeparam>
    /// <seealso cref="JsonStringEnumMemberConverter"/>
    public sealed class SafeJsonStringEnumMemberConverter<TEnum> : JsonStringEnumMemberConverter
        where TEnum : Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeJsonStringEnumMemberConverter{TEnum}"/> class.
        /// </summary>
        public SafeJsonStringEnumMemberConverter()  // NOTE: Must be public!
            : base(new JsonStringEnumMemberConverterOptions { DeserializationFailureFallbackValue = default(TEnum) })
        {
        }
    }
}