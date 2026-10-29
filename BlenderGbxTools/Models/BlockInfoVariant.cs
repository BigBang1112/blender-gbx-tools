using BlenderGbxTools.Extensions;
using BlenderGbxTools.Mapping;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed record BlockInfoVariant
{
    public ImmutableArray<ImmutableArray<BlockInfoMobil>> Mobils { get; init; } = [];
    public ImmutableArray<BlockInfoUnit> Units { get; init; } = [];
    public Solid? Waypoint { get; init; }
    public Solid? Helper { get; init; }

    public byte[]? SpawnLoc { get; init; }
    public Vec3? SpawnTrans { get; init; }
    public float? SpawnPitch { get; init; }
    public float? SpawnYaw { get; init; }

    public static BlockInfoVariant From(CGameCtnBlockInfoVariant variant, MappingContext context)
    {
        return new BlockInfoVariant
        {
            Mobils = variant.Mobils?
                .Select(mobils =>
                    mobils.Select(mobil => BlockInfoMobil.From(mobil, context))
                    .ToImmutableArray())
                .ToImmutableArray() ?? [],
            Waypoint = variant.WaypointTriggerSolid is CPlugSolid waypointSolid
                ? Solid.From(variant.References.WaypointTriggerSolid.File.GetFileNameOrUniqueName(), waypointSolid, context)
                : null,
            Units = variant.BlockUnitModels?
                .Select(BlockInfoUnit.From)
                .ToImmutableArray() ?? [],
            Helper = variant.HelperSolidFid is CPlugSolid helperSolid
                ? Solid.From(variant.References.HelperSolidFid.File.GetFileNameOrUniqueName(), helperSolid, context)
                : null,
            SpawnTrans = variant.SpawnTrans,
            SpawnPitch = variant.SpawnPitch,
            SpawnYaw = variant.SpawnYaw,
        };
    }

    public static BlockInfoVariant From(NodeRefList<CSceneMobil>[] mobils, NodeRefList<CGameCtnBlockUnitInfo?> units, Iso4? spawnLoc, CSceneMobil? helperMobil, MappingContext context)
    {
        var helper = default(Solid);

        if (helperMobil?.Item?.Solid?.Model is CPlugSolid solid)
        {
            helper = Solid.From(helperMobil.Item.Solid.References.Model.File.GetFileNameOrUniqueName(), solid, context);
        }

        return new BlockInfoVariant
        {
            Mobils = mobils.Length > 0 && mobils.Any(x => x.Count > 0)
                ? mobils
                    .Select(mobilsRow =>
                        mobilsRow.Select(mobil => BlockInfoMobil.From(mobil, context))
                        .ToImmutableArray())
                    .ToImmutableArray()
                : [],
            Units = units
                .OfType<CGameCtnBlockUnitInfo>()
                .Select(BlockInfoUnit.From)
                .ToImmutableArray(),
            Helper = helper,
            SpawnLoc = spawnLoc.HasValue ? MemoryMarshal.AsBytes([spawnLoc.Value]).ToArray() : null,
        };
    }
}
