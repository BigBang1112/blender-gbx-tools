using BlenderGbxTools.Extensions;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlenderGbxTools.Models;

internal sealed class Item : IStandalone
{
    public string? Name { get; set; }
    public ImmutableDictionary<string, Material?>? Materials { get; }
    public Crystal? Crystal { get; set; }
    public Solid2? Solid2 { get; set; }
    public Prefab? Prefab { get; set; }

    public double ExecutionTimeInSeconds { get; }

    ImmutableList<SurfaceMaterial>? IStandalone.SurfaceMaterials { get; } = null;

    public Item()
    {

    }

    public Item(string fileName, CGameItemModel item, bool standalone = true)
    {
        var startTime = Stopwatch.GetTimestamp();

        Name = fileName;

        if (item.EntityModelEdition is CGameCommonItemEntityModelEdition { MeshCrystal: not null } edition)
        {
            Crystal = new Crystal(edition.MeshCrystal);

            // Materials not in this dictionary use the default material
            Materials = standalone ? edition.MeshCrystal.GetAllMaterials() : null;
        }

        if (item.EntityModel is CPlugPrefab prefab)
        {
            Prefab = new Prefab(fileName, prefab, standalone: false);
            // Materials not in this dictionary use the default material
            Materials = standalone ? prefab.GetAllMaterials() : null;
        }

        ExecutionTimeInSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}