// © 2024, Worth Systems.

using Common.Settings.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Common.Settings.Attributes
{
    /// <summary>
    /// The attribute used to mark <see cref="OmcConfiguration"/> configuration properties.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    [ExcludeFromCodeCoverage(Justification = "There is nothing to test here.")]
    public sealed class ConfigAttribute : Attribute;
}