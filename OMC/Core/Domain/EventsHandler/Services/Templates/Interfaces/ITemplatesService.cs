// © 2023, Worth Systems.

namespace EventsHandler.Services.Templates.Interfaces
{
    /// <summary>
    /// The service performing operations on a given templates of data.
    /// </summary>
    /// <typeparam name="TTemplate">The type of the template.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public interface ITemplatesService<in TTemplate, in TModel>
        where TTemplate : class
        where TModel : IJsonSerializable
    {
        /// <summary>
        /// Extracts placeholders from a given template.
        /// </summary>
        /// <param name="template">The template to be analyzed.</param>
        /// <returns>
        ///   The array of found template placeholders if any are existing; otherwise, empty array.
        /// </returns>
        internal string[] GetPlaceholders(TTemplate template);

        /// <summary>
        /// Tries to map the received placeholders into properties from the POCO model.
        /// </summary>
        /// <param name="placeholders">The placeholders to be used as keys.</param>
        /// <param name="model">The model to be traversed looking for matching values.</param>
        /// <returns>
        ///   The dictionary of mapped placeholders (keys) and POCO model properties (values); otherwise, empty dictionary.
        /// </returns>
        internal Dictionary<string, dynamic> MapPersonalization(string[] placeholders, TModel model);
    }
}