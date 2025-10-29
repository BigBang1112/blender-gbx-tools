using BlenderGbxTools.Models;
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
            .Select(x => x.Node?.Item?.Solid)
            .OfType<CPlugSolid>() ?? [];

        var mpSolids = blockInfo.VariantBaseAir?.Mobils?
            .Concat(blockInfo.VariantBaseGround?.Mobils ?? [])
            .SelectMany(x => x)
            .Select(x => x.SolidFid)
            .OfType<CPlugSolid>() ?? [];

        return tmfSolids.Concat(mpSolids);
    }

    public static ImmutableDictionary<string, Material> GetAllMaterials(this CGameCtnBlockInfo blockInfo)
    {
        // This only resolves materials with recognized Material.Gbx/Shader.Gbx
        var materials = blockInfo.GetAllSolids()
            .SelectMany(x => x.GetDistinctMaterialTrees())
            .DistinctBy(tree => tree.GetMaterialName())
            .ToImmutableDictionary(tree => tree.GetMaterialName(), tree => new Material((CPlugMaterial)tree.Shader!));

        return materials;
    }
}
