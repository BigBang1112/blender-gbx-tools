using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoVariant
{
    public ImmutableArray<ImmutableArray<BlockInfoMobil>> Mobils { get; } = [];
    public ImmutableArray<BlockInfoUnit> Units { get; } = [];
    public Solid? Waypoint { get; }
    public Solid? Helper { get; }

    public byte[]? SpawnLoc { get; }
    public Vec3? SpawnTrans { get; }
    public float? SpawnPitch { get; }
    public float? SpawnYaw { get; }

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
            var waypointFile = variant.References.WaypointTriggerSolid.File;
            var fileName = waypointFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(waypointFile.GetFullPath());
            Waypoint = new Solid(fileName, waypointSolid, standalone: false);
        }

        Units = variant.BlockUnitModels?
            .Select(unit => new BlockInfoUnit(unit))
            .ToImmutableArray() ?? [];

        if (variant.HelperSolidFid is CPlugSolid helperSolid)
        {
            var helperFile = variant.References.HelperSolidFid.File;
            var fileName = helperFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(helperFile.GetFullPath());
            Helper = new Solid(fileName, helperSolid, standalone: false);
        }

        SpawnTrans = variant.SpawnTrans;
        SpawnPitch = variant.SpawnPitch;
        SpawnYaw = variant.SpawnYaw;
    }

    public BlockInfoVariant(NodeRefList<CSceneMobil>[] mobils, NodeRefList<CGameCtnBlockUnitInfo?> units, Iso4? spawnLoc, CSceneMobil? helperMobil)
    {
        if (mobils.Length > 0 && mobils.Any(x => x.Count > 0))
        {
            Mobils = mobils
                .Select(mobilsRow =>
                    mobilsRow.Select(mobil => new BlockInfoMobil(mobil))
                    .ToImmutableArray())
                .ToImmutableArray();
        }

        Units = units
            .OfType<CGameCtnBlockUnitInfo>()
            .Select(unit => new BlockInfoUnit(unit))
            .ToImmutableArray();

        if (helperMobil?.Item?.Solid?.Model is CPlugSolid solid)
        {
            var modelFile = helperMobil.Item.Solid.References.Model.File;
            var fileName = modelFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(modelFile.GetFullPath());
            Helper = new Solid(fileName, solid, standalone: false);
        }

        SpawnLoc = spawnLoc.HasValue ? MemoryMarshal.AsBytes([spawnLoc.Value]).ToArray() : null;
    }
}
