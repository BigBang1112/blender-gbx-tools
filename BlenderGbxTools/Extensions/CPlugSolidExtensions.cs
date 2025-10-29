using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugSolidExtensions
{
    public static IEnumerable<CPlugTree> GetDistinctMaterialTrees(this CPlugSolid solid)
    {
        return solid.GetAllChildren(includeVisualMipLevels: true)
            .DistinctBy(tree => tree.GetMaterialName())
            .Where(tree => tree.Shader is CPlugMaterial);
    }

    public static ImmutableDictionary<string, Material> GetAllMaterials(this CPlugSolid solid)
    {
        // This only resolves materials with recognized Material.Gbx/Shader.Gbx
        return solid.GetDistinctMaterialTrees()
            .ToImmutableDictionary(
                tree => tree.GetMaterialName(),
                tree => new Material((CPlugMaterial)tree.Shader!));
    }

    public static IEnumerable<CPlugSurface.SurfMaterial> GetDistinctSurfaceMaterials(this CPlugSolid solid)
    {
        return solid.GetAllChildren(includeVisualMipLevels: true)
            .Select(x => x.Surface)
            .OfType<CPlugSurface>()
            .SelectMany(x => x.Materials)
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.MaterialFile?.FilePath) });
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CPlugSolid solid)
    {
        return solid.GetDistinctSurfaceMaterials()
            .Select(x => new SurfaceMaterial(x))
            .ToImmutableList();
    }
}
