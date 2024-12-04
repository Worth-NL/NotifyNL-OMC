// © 2023, Worth Systems.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ZhvModels.Mapping.Helpers
{
    /// <summary>
    /// A special collection, containing metadata of business objects properties (<see cref="PropertyInfo"/>s)
    /// including their names in two languages (English and Dutch) using bidirectional mapping (EN => NL => EN).
    /// </summary>
    /// <seealso cref="IReadOnlyList{T}"/>
    public sealed class PropertiesMetadata : IReadOnlyList<PropertyInfo>
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Gets the collection of properties.
        /// </summary>
        private PropertyInfo[] Properties { get; }

        /// <summary>
        /// Gets the pair of names English => (index, Dutch).
        /// </summary>
        private Dictionary<string, (int Index, string DutchName)> English_To_Dutch { get; }

        /// <summary>
        /// Gets the pair of names Dutch => (index, English).
        /// </summary>
        private Dictionary<string, (int Index, string EnglishName)> Dutch_To_English { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesMetadata"/> class.
        /// </summary>
        /// <param name="instance">The instance from which properties will be mapped.</param>
        /// <param name="exclusions">The names of the properties to be excluded.</param>
        public PropertiesMetadata(object instance, params string[] exclusions)
        {
            // Preparation
            PropertyInfo[] properties = GetFiltered(GetPublicInstanceProperties(instance), exclusions);

            // Initialization
            this.Count = properties.Length;

            this.Properties = new PropertyInfo[this.Count];
            this.English_To_Dutch = new Dictionary<string, (int, string)>(this.Count);
            this.Dutch_To_English = new Dictionary<string, (int, string)>(this.Count);

            // Mapping
            MapDictionaries(properties);
        }

        #region Preparation
        /// <summary>
        /// Retrieves the instance public properties from a specified <see cref="Type"/>.
        /// </summary>
        private static PropertyInfo[] GetPublicInstanceProperties(object instance)
        {
            return instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Filters the given properties (<see cref="PropertyInfo"/>s) based on the provided properties names.
        /// </summary>
        private static PropertyInfo[] GetFiltered(IEnumerable<PropertyInfo> properties, params string[] exclusions)
        {
            return properties.Where(property => !exclusions.Contains(property.Name))
                             .ToArray();
        }
        #endregion

        #region Mapping
        /// <summary>
        /// Maps the public dictionaries.
        /// </summary>
        private void MapDictionaries(IReadOnlyList<PropertyInfo> properties)
        {
            for (int index = 0; index < properties.Count; index++)
            {
                PropertyInfo currentProperty = properties[index];
                string englishPropertyName = RetrievePropertyEnglishName(currentProperty);
                string dutchPropertyName = RetrievePropertyDutchName(currentProperty);

                this.Properties[index] = currentProperty;
                this.English_To_Dutch.Add(englishPropertyName, (index, dutchPropertyName));
                this.Dutch_To_English.Add(dutchPropertyName, (index, englishPropertyName));
            }
        }

        /// <summary>
        /// Gets the plane C# (English) name of the property (<see cref="PropertyInfo"/>).
        /// </summary>
        private static string RetrievePropertyEnglishName(MemberInfo property)
        {
            return property.Name;
        }

        /// <summary>
        /// Gets the custom JSON-associated (Dutch) name of the property (<see cref="PropertyInfo"/>)
        /// retrieved from its <see cref="JsonPropertyNameAttribute"/>.
        /// </summary>
        private static string RetrievePropertyDutchName(MemberInfo property)
        {
            return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name  // Dutch name (if existing)
                   ?? string.Empty;
        }
        #endregion

        #region Collection methods
        /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc cref="IEnumerable{TProperty}.GetEnumerator"/>
        public IEnumerator<PropertyInfo> GetEnumerator()
        {
            return ((IEnumerable<PropertyInfo>)this.Properties).GetEnumerator();  // NOTE: Foreach and yield return PropertyInfo elements
        }

        /// <inheritdoc cref="IReadOnlyCollection{TProperty}.Count"/>
        public int Count { get; }

        /// <summary>
        /// Gets the <see cref="Nullable{PropertyInfo}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Nullable{PropertyInfo}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        public PropertyInfo this[int index]
        {
            get => this.Properties[index > this.Count ? this.Count     // No exceptions if the index > Length
                                                      : index < 0 ? 0  // No exceptions if the index < Length
                                                                  : index];
        }
        #endregion

        #region Bidirectional methods
        /// <summary>
        /// Gets the JSON-associated (Dutch) name of the property from the public bilingual dictionary of names.
        /// </summary>
        public string GetPropertyDutchName(PropertyInfo property)
            => this.English_To_Dutch[RetrievePropertyEnglishName(property)].DutchName;

        /// <summary>
        /// Gets the plain C# (English) name of the property from the public bilingual dictionary of names.
        /// </summary>
        public string GetPropertyEnglishName(string dutchName)
            => this.Dutch_To_English[dutchName].EnglishName;

        /// <summary>
        ///   Safe version.
        ///   <inheritdoc cref="GetPropertyDutchName(PropertyInfo)"/>
        /// </summary>
        public bool TryGetPropertyDutchName(string englishName, out (int Index, string DutchName) record)
            => this.English_To_Dutch.TryGetValue(englishName, out record);

        /// <summary>
        ///   Safe version.
        ///   <inheritdoc cref="GetPropertyEnglishName(string)"/>
        /// </summary>
        public bool TryGetPropertyEnglishName(string dutchName, out (int Index, string EnglishName) record)
            => this.Dutch_To_English.TryGetValue(dutchName, out record);

        /// <summary>
        /// Gets the property (<see cref="PropertyInfo"/>) value by its C# (English) name.
        /// </summary>
        public bool TryGetPropertyValueByEnglishName<TInstance>(string englishName, TInstance instance, [MaybeNullWhen(false)] out object value)
        {
            return TryGetPropertyValueByKeyToIndex(TryGetPropertyDutchName, englishName, instance, out value);
        }

        /// <summary>
        /// Gets the property (<see cref="PropertyInfo"/>) value by its JSON-associated (Dutch) name.
        /// </summary>
        public bool TryGetPropertyValueByDutchName<TInstance>(string dutchName, TInstance instance, [MaybeNullWhen(false)] out object value)
        {
            return TryGetPropertyValueByKeyToIndex(TryGetPropertyEnglishName, dutchName, instance, out value);
        }

        /// <summary>
        /// Invokes custom <see cref="TryGet"/> delegate to obtain value from a respective property (<see cref="PropertyInfo"/>).
        /// </summary>
        private bool TryGetPropertyValueByKeyToIndex<TInstance>(TryGet function, string key, TInstance instance, out object? value)
        {
            if (function.Invoke(key, out (int Index, string Name) record))  // NOTE: Given "key" (if existing) should return (Index, Name) pair
            {
                PropertyInfo property = this.Properties[record.Index];  // NOTE: And then, this index can be used to obtain the property metadata
                value = instance.GetPropertyValue(property);  // NOTE: From which a specific value can be extracted and returned as out parameter

                return true;
            }

            value = null;

            return false;
        }

        /// <summary>
        /// Custom delegate to be used by public logic.
        /// </summary>
        private delegate bool TryGet(string key, out (int, string) value);
        #endregion
    }
}