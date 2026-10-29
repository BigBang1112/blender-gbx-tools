using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record Solid
{
    public string? Name { get; init; }
    public Tree? Tree { get; init; }

    // Only set on the root model
    public ImmutableDictionary<string, Material?>? Materials { get; init; }
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; init; }
    public double? ExecutionTimeInSeconds { get; init; }

    public static Solid From(string fileName, CPlugSolid solid, MappingContext context)
    {
        return new Solid
        {
            Name = fileName,
            Tree = solid.Tree is CPlugTree tree ? Tree.From(tree, context) : null,
        };
    }
}
