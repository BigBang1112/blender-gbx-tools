using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoVariant
{
    public ImmutableArray<ImmutableArray<BlockInfoMobil>> Mobils { get; } = [];
    public ImmutableArray<BlockInfoUnit> Units { get; } = [];
    public Solid? Waypoint { get; }

    public BlockInfoVariant()
    {
        
    }

    public BlockInfoVariant(CGameCtnBlockInfoVariant variant)
    {
        Mobils = variant.Mobils?
            .Select(mobils =>
                mobils.Select(mobil => new BlockInfoMobil(mobil))
                .ToImmutableArray())
            .ToImmutableArray() ?? [];

        if (variant.WaypointTriggerSolid is CPlugSolid waypointSolid)
        {
            var fileName = variant.WaypointTriggerSolidFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(variant.WaypointTriggerSolidFile.GetFullPath());
            Waypoint = new Solid(fileName, waypointSolid, standalone: false);
        }

        Units = variant.BlockUnitModels?
            .Select(unit => new BlockInfoUnit(unit))
            .ToImmutableArray() ?? [];
    }

    public BlockInfoVariant(External<CSceneMobil>[][] mobils, CGameCtnBlockUnitInfo[] units)
    {
        if (mobils.Length > 0 && mobils.Any(x => x.Length > 0))
        {
            Mobils = mobils
                .Select(mobilsRow =>
                    mobilsRow.Select(mobil => new BlockInfoMobil(mobil))
                    .ToImmutableArray())
                .ToImmutableArray();
        }

        Units = units
            .Select(unit => new BlockInfoUnit(unit))
            .ToImmutableArray();
    }
}
