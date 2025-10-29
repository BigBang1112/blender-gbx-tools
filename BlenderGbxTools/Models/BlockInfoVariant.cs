using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Scene;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoVariant
{
    public BlockInfoMobil[][] Mobils { get; } = [];
    public BlockInfoUnit[] Units { get; } = [];

    public BlockInfoVariant()
    {
        
    }

    public BlockInfoVariant(CGameCtnBlockInfoVariant variant)
    {
        Mobils = variant.Mobils?
            .Select(mobils =>
                mobils.Select(mobil => new BlockInfoMobil(mobil))
                .ToArray())
            .ToArray() ?? [];

        Units = variant.BlockUnitModels?
            .Select(unit => new BlockInfoUnit(unit))
            .ToArray() ?? [];
    }

    public BlockInfoVariant(External<CSceneMobil>[][] mobils, CGameCtnBlockUnitInfo[] units)
    {
        Mobils = mobils
            .Select(mobilsRow =>
                mobilsRow.Select(mobil => new BlockInfoMobil(mobil))
                .ToArray())
            .ToArray();

        Units = units
            .Select(unit => new BlockInfoUnit(unit))
            .ToArray();
    }
}
