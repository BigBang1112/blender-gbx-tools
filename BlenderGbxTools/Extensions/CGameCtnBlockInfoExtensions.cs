using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CGameCtnBlockInfoExtensions
{
    public static IEnumerable<CPlugSolid> GetAllSolids(this CGameCtnBlockInfo blockInfo)
    {
        var tmfSolids = blockInfo.AirMobils?
            .Concat(blockInfo.GroundMobils ?? [])
            .SelectMany(x => x)
            .Select(x => x.Node?.Item?.Solid?.Tree)
            .OfType<CPlugSolid>() ?? [];

        var tmfObjectLinks = blockInfo.AirMobils?
            .Concat(blockInfo.GroundMobils ?? [])
            .SelectMany(x => x)
            .SelectMany(x => x.Node?.ObjectLink ?? [])
            .Select(x => x.Mobil?.Item?.Solid?.Tree)
            .OfType<CPlugSolid>() ?? [];

        var mpSolids = blockInfo.VariantBaseAir?.Mobils?
            .Concat(blockInfo.VariantBaseGround?.Mobils ?? [])
            .SelectMany(x => x)
            .Select(x => x.SolidFid)
            .OfType<CPlugSolid>() ?? [];

        return tmfSolids.Concat(tmfObjectLinks).Concat(mpSolids);
    }

    public static ImmutableDictionary<string, Material> GetAllMaterials(this CGameCtnBlockInfo blockInfo)
    {
        // This only resolves materials with recognized Material.Gbx/Shader.Gbx
        return blockInfo.GetAllSolids()
            .SelectMany(x => x.GetDistinctMaterialTrees())
            .DistinctBy(tree => tree.GetMaterialName())
            .ToImmutableDictionary(tree => tree.GetMaterialName(), tree => new Material((CPlugMaterial)tree.Shader!));
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CGameCtnBlockInfo blockInfo)
    {
        return blockInfo.GetAllSolids()
            .SelectMany(x => x.GetDistinctSurfaceMaterials())
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.MaterialFile?.FilePath) })
            .Select(surfMat => new SurfaceMaterial(surfMat))
            .ToImmutableList();
    }
}
