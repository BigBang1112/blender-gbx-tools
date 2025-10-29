using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlenderGbxTools.Models;

internal sealed class Solid : IStandalone
{
    public string? Name { get; }
    public Tree? Tree { get; }

    public ImmutableDictionary<string, Material>? Materials { get; }
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; }

    public double ExecutionTimeInSeconds { get; }

    public Solid()
    {
        
    }

    public Solid(string fileName, CPlugSolid solid, bool standalone = true)
    {
        var startTime = Stopwatch.GetTimestamp();

        Name = fileName;
        Tree = solid.Tree is CPlugTree tree ? new Tree(tree) : null;

        // Materials not in this dictionary use the default material
        Materials = standalone ? solid.GetAllMaterials() : null;
        SurfaceMaterials = standalone ? solid.GetAllSurfaceMaterials() : null;

        ExecutionTimeInSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}
