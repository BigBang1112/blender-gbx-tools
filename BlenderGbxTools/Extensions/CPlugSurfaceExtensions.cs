using BlenderGbxTools.Models;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugSurfaceExtensions
{
    public static ImmutableList<SurfaceMaterial> GetAllMaterials(this CPlugSurface surface)
    {
        return surface.Materials
            .Select(x => new SurfaceMaterial(x))
            .ToImmutableList();
    }
}
