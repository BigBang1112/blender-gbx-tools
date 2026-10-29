using BlenderGbxTools.Extensions;
using BlenderGbxTools.Mapping;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed record BlockInfoMobil
{
    public Solid? Solid { get; init; }
    public Prefab? Prefab { get; init; }
    public ImmutableList<ObjectLink>? ObjectLinks { get; init; }
    public Vec3 GeomTranslation { get; init; }
    public Vec3 GeomRotation { get; init; }

    public static BlockInfoMobil From(CSceneMobil? mobil, MappingContext context)
    {
        if (mobil is null)
        {
            return new BlockInfoMobil();
        }

        var solid = default(Solid);

        if (mobil.Item?.Solid?.Model is CPlugSolid plugSolid)
        {
            solid = Solid.From(mobil.Item.Solid.References.Model.File.GetFileNameOrUniqueName(), plugSolid, context);
        }

        return new BlockInfoMobil
        {
            Solid = solid,
            ObjectLinks = mobil.ObjectLink?.Select(x => ObjectLink.From(x, context)).ToImmutableList(),
        };
    }

    public static BlockInfoMobil From(CGameCtnBlockInfoMobil mobil, MappingContext context)
    {
        return new BlockInfoMobil
        {
            Solid = mobil.SolidFid is null
                ? null
                : Solid.From(mobil.References.SolidFid.File.GetFileNameOrUniqueName(), mobil.SolidFid, context),
            Prefab = mobil.PrefabFid is null
                ? null
                : Prefab.From(mobil.References.PrefabFid.File.GetFileNameOrUniqueName(), mobil.PrefabFid, context),
            GeomTranslation = mobil.GeomTranslation,
            GeomRotation = mobil.GeomRotation,
        };
    }
}
