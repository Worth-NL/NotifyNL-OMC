// © 2023, Worth Systems.

using ZhvModels.Enums;
using ZhvModels.Mapping.Models.Interfaces;

namespace EventsHandler.Services.Validation.Interfaces
{
    /// <summary>
    /// The service to validate a certain types of models.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public interface IValidationService<TModel>
        where TModel : IJsonSerializable
    {
        /// <summary>
        /// Validates the given model.
        /// </summary>
        /// <param name="model">The model to be implicitly validated.</param>
        internal HealthCheck Validate(ref TModel model);
    }
}