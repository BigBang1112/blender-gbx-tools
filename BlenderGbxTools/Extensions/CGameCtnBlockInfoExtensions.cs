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

    public static IEnumerable<CPlugPrefab> GetAllPrefabs(this CGameCtnBlockInfo blockInfo)
    {
        return blockInfo.VariantBaseAir?.Mobils?
            .Concat(blockInfo.VariantBaseGround?.Mobils ?? [])
            .SelectMany(x => x)
            .Select(x => x.PrefabFid)
            .OfType<CPlugPrefab>() ?? [];
    }

    public static ImmutableDictionary<string, Material?> GetAllMaterials(this CGameCtnBlockInfo blockInfo)
    {
        // This only resolves materials with recognized Material.Gbx/Shader.Gbx
        var solidMaterials = blockInfo.GetAllSolids()
            .SelectMany(x => x.GetDistinctMaterialTrees())
            .DistinctBy(tree => tree.GetMaterialName())
            .ToImmutableDictionary(tree => tree.GetMaterialName(), tree => tree.Shader is null ? null : new Material((CPlugMaterial)tree.Shader));

        var prefabMaterials = blockInfo.GetAllPrefabs()
            .SelectMany(prefab => prefab.GetAllMaterials())
            .DistinctBy(matPair => matPair.Key)
            .ToImmutableDictionary(matPair => matPair.Key, matPair => matPair.Value);

        return solidMaterials
            .Concat(prefabMaterials)
            .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static ImmutableList<SurfaceMaterial> GetAllSurfaceMaterials(this CGameCtnBlockInfo blockInfo)
    {
        var solidSurfaceMaterials = blockInfo.GetAllSolids()
            .SelectMany(x => x.GetDistinctSurfaceMaterials())
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.MaterialFile?.FilePath) })
            .Select(surfMat => new SurfaceMaterial(surfMat));

        var prefabSurfaceMaterials = blockInfo.GetAllPrefabs()
            .SelectMany(prefab => prefab.GetDistinctSurfaceMaterials())
            .DistinctBy(surfMat => new { surfMat.SurfaceId, MaterialName = GbxPath.GetFileNameWithoutExtension(surfMat.MaterialFile?.FilePath) })
            .Select(surfMat => new SurfaceMaterial(surfMat));

        return solidSurfaceMaterials
            .Concat(prefabSurfaceMaterials) // no need to distinct cuz prefabs and solids won't overlap
            .ToImmutableList();
    }
}
