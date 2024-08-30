// © 2024, Worth Systems.

using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Services.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    [ExcludeFromCodeCoverage(Justification = "There is nothing to test here.")]
    internal sealed class ConfigAttribute : Attribute
    {
    }
}