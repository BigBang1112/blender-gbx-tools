using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record Solid2
{
    public string? Name { get; init; }
    public ShadedGeom[]? ShadedGeoms { get; init; }

    // Only set on the root model
    public ImmutableDictionary<string, Material?>? Materials { get; init; }
    public double? ExecutionTimeInSeconds { get; init; }

    public static Solid2 From(string fileName, CPlugSolid2Model solid2, MappingContext context)
    {
        var materials = Solid2Materials.Add(solid2, context);

        return new Solid2
        {
            Name = fileName,
            ShadedGeoms = solid2.ShadedGeoms?.Select(x => ShadedGeom.From(solid2, x, materials)).ToArray(),
        };
    }
}
