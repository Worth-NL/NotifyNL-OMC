// © 2024, Worth Systems.

using System.Diagnostics.CodeAnalysis;

namespace Common.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    [ExcludeFromCodeCoverage(Justification = "There is nothing to test here.")]
    public sealed class ConfigAttribute : Attribute;
}