using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugSolidExtensions
{
    public static IEnumerable<CPlugSurface.SurfMaterial> GetDistinctSurfaceMaterials(this CPlugSolid solid)
    {
        return solid.GetAllChildren(includeVisualMipLevels: true)
            .Select(x => x.Surface)
            .OfType<CPlugSurface>()
            .SelectMany(x => x.Materials)
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.References.Material.File?.FilePath) });
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CPlugSolid solid)
    {
        return solid.GetDistinctSurfaceMaterials()
            .Select(SurfaceMaterial.From)
            .ToImmutableList();
    }
}
