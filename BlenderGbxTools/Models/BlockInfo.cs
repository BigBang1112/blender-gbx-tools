using GBX.NET.Engines.Game;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfo : IStandalone
{
    public string? Name { get; private set; }
    public BlockInfoVariant? VariantAir { get; private set; }
    public BlockInfoVariant? VariantGround { get; private set; }

    public ImmutableDictionary<string, Material>? Materials { get; private set; }
    public double ExecutionTimeInSeconds { get; private set; }

    public BlockInfo()
    {
        
    }

    public BlockInfo(CGameCtnBlockInfo blockInfo, bool standalone = true)
    {
        Name = blockInfo.Ident.Id;
        /*AirVariants = GetVariantsFromMobils("Air", blockInfo.AirMobils).ToArray(),
        GroundVariants = GetVariantsFromMobils("Ground", blockInfo.GroundMobils).ToArray(),
        AirUnits = GetUnitsFromBlockInfo(blockInfo.AirBlockUnitInfos),
        GroundUnits = GetUnitsFromBlockInfo(blockInfo.GroundBlockUnitInfos),
        Materials = standalone ? GetAllSolids(blockInfo)
            .SelectMany(x => x.Tree is null ? Enumerable.Empty<Material>() : Solid.ScanMaterials((CPlugTree)x.Tree))
            .DistinctBy(x => x.Name)
            .ToDictionary(x => x.Name!) : null*/
    }
}
