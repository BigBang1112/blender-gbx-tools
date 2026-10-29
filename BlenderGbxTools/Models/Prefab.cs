using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record Prefab
{
    public string? Name { get; init; }
    public ImmutableList<Ent>? Ents { get; init; }

    // Only set on the root model
    public ImmutableDictionary<string, Material?>? Materials { get; init; }
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; init; }
    public double? ExecutionTimeInSeconds { get; init; }

    public static Prefab From(string fileName, CPlugPrefab prefab, MappingContext context)
    {
        return new Prefab
        {
            Name = fileName,
            Ents = prefab.Ents.Length == 0 ? null : prefab.Ents.Select(x => Ent.From(x, context)).ToImmutableList(),
        };
    }
}
