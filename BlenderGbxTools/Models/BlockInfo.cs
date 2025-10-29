using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Game;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfo : IStandalone
{
    public string? Name { get; }
    public BlockInfoVariant? VariantAir { get; }
    public BlockInfoVariant? VariantGround { get; }

    public ImmutableDictionary<string, Material>? Materials { get; }

    public double ExecutionTimeInSeconds { get; }

    public BlockInfo()
    {
        
    }

    public BlockInfo(CGameCtnBlockInfo blockInfo, bool standalone = true)
    {
        var startTime = Stopwatch.GetTimestamp();

        Name = blockInfo.Ident.Id;

        VariantAir = blockInfo.VariantBaseAir is null
            ? new BlockInfoVariant(blockInfo.AirMobils ?? [], blockInfo.AirBlockUnitInfos ?? [])
            : new BlockInfoVariant(blockInfo.VariantBaseAir);

        VariantGround = blockInfo.VariantBaseGround is null
            ? new BlockInfoVariant(blockInfo.GroundMobils ?? [], blockInfo.GroundBlockUnitInfos ?? [])
            : new BlockInfoVariant(blockInfo.VariantBaseGround);

        // Materials not in this dictionary use the default material
        Materials = standalone ? blockInfo.GetAllMaterials() : null;

        ExecutionTimeInSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}
