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
            .Select(x => x?.Item?.Solid?.Model)
            .OfType<CPlugSolid>() ?? [];

        var tmfObjectLinks = blockInfo.AirMobils?
            .Concat(blockInfo.GroundMobils ?? [])
            .SelectMany(x => x)
            .SelectMany(x => x?.ObjectLink ?? [])
            .Select(x => x.Mobil?.Model?.Item?.Solid?.Model)
            .OfType<CPlugSolid>() ?? [];

        var mpSolids = blockInfo.VariantBaseAir?.Mobils?
            .Concat(blockInfo.VariantBaseGround?.Mobils ?? [])
            .SelectMany(x => x)
            .Select(x => x.SolidFid)
            .OfType<CPlugSolid>() ?? [];

        return tmfSolids.Concat(tmfObjectLinks).Concat(mpSolids);
    }

    public static IEnumerable<CPlugPrefab> GetAllPrefabs(this CGameCtnBlockInfo blockInfo)
    {
        return blockInfo.VariantBaseAir?.Mobils?
            .Concat(blockInfo.VariantBaseGround?.Mobils ?? [])
            .SelectMany(x => x)
            .Select(x => x.PrefabFid)
            .OfType<CPlugPrefab>() ?? [];
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CGameCtnBlockInfo blockInfo)
    {
        var solidSurfaceMaterials = blockInfo.GetAllSolids()
            .SelectMany(x => x.GetDistinctSurfaceMaterials())
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.References.Material.File?.FilePath) })
            .Select(SurfaceMaterial.From);

        var prefabSurfaceMaterials = blockInfo.GetAllPrefabs()
            .SelectMany(prefab => prefab.GetDistinctSurfaceMaterials())
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.References.Material.File?.FilePath) })
            .Select(SurfaceMaterial.From);

        return solidSurfaceMaterials
            .Concat(prefabSurfaceMaterials) // no need to distinct cuz prefabs and solids won't overlap
            .ToImmutableList();
    }
}
