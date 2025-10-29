using BlenderGbxTools.Models;
using System.Collections.Immutable;

namespace BlenderGbxTools;

internal interface IStandalone
{
    ImmutableDictionary<string, Material>? Materials { get; }
    ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; }
    double ExecutionTimeInSeconds { get; }
}
