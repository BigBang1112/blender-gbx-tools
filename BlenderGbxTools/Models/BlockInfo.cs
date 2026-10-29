using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Game;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record BlockInfo
{
    public string? Name { get; init; }
    public BlockInfoVariant? VariantAir { get; init; }
    public BlockInfoVariant? VariantGround { get; init; }

    // Only set on the root model
    public ImmutableDictionary<string, Material?>? Materials { get; init; }
    public ImmutableList<SurfaceMaterial>? SurfaceMaterials { get; init; }
    public double? ExecutionTimeInSeconds { get; init; }

    public static BlockInfo From(CGameCtnBlockInfo blockInfo, MappingContext context)
    {
        return new BlockInfo
        {
            Name = blockInfo.Ident.Id,
            VariantAir = blockInfo.VariantBaseAir is null
                ? BlockInfoVariant.From(blockInfo.AirMobils ?? [], blockInfo.AirBlockUnitInfos ?? [], blockInfo.SpawnLocAir, blockInfo.AirHelperMobil, context)
                : BlockInfoVariant.From(blockInfo.VariantBaseAir, context),
            VariantGround = blockInfo.VariantBaseGround is null
                ? BlockInfoVariant.From(blockInfo.GroundMobils ?? [], blockInfo.GroundBlockUnitInfos ?? [], blockInfo.SpawnLocGround, blockInfo.GroundHelperMobil, context)
                : BlockInfoVariant.From(blockInfo.VariantBaseGround, context),
        };
    }
}
