﻿// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;

namespace EventsHandler.Services.Validation.Interfaces
{
    /// <summary>
    /// The service to validate a certain types of models.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to be validated.</typeparam>
    public interface IValidationService<TModel> where TModel : IJsonSerializable
    {
        /// <summary>
        /// Validates the given model.
        /// </summary>
        /// <param name="model">The model to be implicitly validated.</param>
        internal HealthCheck Validate(ref TModel model);
    }
}