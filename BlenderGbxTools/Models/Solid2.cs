using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlenderGbxTools.Models;

internal sealed class Solid2 : IStandalone
{
    public string? Name { get; set; }
    public ImmutableDictionary<string, Material?>? Materials { get; }
    public ShadedGeom[]? ShadedGeoms { get; set; }

    public double ExecutionTimeInSeconds { get; }

    ImmutableList<SurfaceMaterial>? IStandalone.SurfaceMaterials { get; } = null;

    public Solid2()
    {

    }

    public Solid2(string fileName, CPlugSolid2Model solid2, bool standalone = true)
    {
        var startTime = Stopwatch.GetTimestamp();

        Name = fileName;
        ShadedGeoms = solid2.ShadedGeoms?.Select(x => new ShadedGeom(solid2, x)).ToArray();

        // Materials not in this dictionary use the default material
        Materials = standalone ? solid2.GetAllMaterials() : null;

        ExecutionTimeInSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}