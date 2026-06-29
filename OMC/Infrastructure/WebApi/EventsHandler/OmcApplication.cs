// © 2024, Worth Systems.

using System.Diagnostics.CodeAnalysis;

// Marker type used by WebApplicationFactory<TEntryPoint> in integration tests to locate this assembly.
// WebApplicationFactory uses TEntryPoint only to find the assembly; Program.Main is discovered automatically.
[ExcludeFromCodeCoverage]
public sealed class OmcApplication { }
