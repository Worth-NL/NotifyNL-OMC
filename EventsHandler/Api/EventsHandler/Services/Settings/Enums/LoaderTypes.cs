// © 2024, Worth Systems.

namespace EventsHandler.Services.Settings.Enums
{
    /// <summary>
    /// The types of DAO (Data Access Object) data providers.
    /// </summary>
    public enum LoaderTypes
    {
        /// <summary>
        /// The configuration JSON loader.
        /// </summary>
        AppSettings,

        /// <summary>
        /// The environment variables loader.
        /// </summary>
        Environment,

        /// <summary>
        /// Both configuration loaders.
        /// </summary>
        Both
    }
}
