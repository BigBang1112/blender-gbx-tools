using BlenderGbxTools.Mapping;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record Item
{
    public string? Name { get; init; }
    public Crystal? Crystal { get; init; }
    public Solid2? Solid2 { get; init; }
    public Prefab? Prefab { get; init; }

    // Only set on the root model
    public ImmutableDictionary<string, Material?>? Materials { get; init; }
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; init; }
    public double? ExecutionTimeInSeconds { get; init; }

    public static Item From(string fileName, CGameItemModel item, MappingContext context)
    {
        return new Item
        {
            Name = fileName,
            Crystal = item.EntityModelEdition is CGameCommonItemEntityModelEdition { MeshCrystal: not null } edition
                ? Crystal.From(edition.MeshCrystal, context)
                : null,
            Prefab = item.EntityModel is CPlugPrefab prefab ? Prefab.From(fileName, prefab, context) : null,
        };
    }
}
