using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlenderGbxTools.Models;

internal sealed class Prefab : IStandalone
{
    public string? Name { get; }
    public ImmutableList<Ent>? Ents { get; }

    public ImmutableDictionary<string, Material?>? Materials { get; }
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; }

    public double ExecutionTimeInSeconds { get; }

    public Prefab()
    {
        
    }

    public Prefab(string fileName, CPlugPrefab prefab, bool standalone = true)
    {
        var startTime = Stopwatch.GetTimestamp();

        Name = fileName;
        Ents = prefab.Ents.Length == 0 ? null : prefab.Ents.Select(x => new Ent(x)).ToImmutableList();

        // Materials not in this dictionary use the default material
        Materials = standalone ? prefab.GetAllMaterials() : null;
        SurfaceMaterials = standalone ? prefab.GetAllSurfaceMaterials() : null;

        ExecutionTimeInSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}
