using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoMobil
{
    public Solid? Solid { get; }
    public Prefab? Prefab { get; }
    public ImmutableList<ObjectLink>? ObjectLinks { get; }
    public Vec3 GeomTranslation { get; }
    public Vec3 GeomRotation { get; }

    public BlockInfoMobil(CSceneMobil? mobil)
    {
        if (mobil is null)
        {
            return;
        }

        if (mobil.Item?.Solid?.Model is CPlugSolid solid)
        {
            var modelFile = mobil.Item.Solid.References.Model.File;
            var fileName = modelFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(modelFile.GetFullPath());
            Solid = new Solid(fileName, solid, standalone: false);
        }

        ObjectLinks = mobil
            .ObjectLink?
            .Select(x => new ObjectLink(x))
            .ToImmutableList();
    }

    public BlockInfoMobil(CGameCtnBlockInfoMobil mobil)
    {
        if (mobil.SolidFid is not null)
        {
            var solidFile = mobil.References.SolidFid.File;
            var fileName = solidFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(solidFile.GetFullPath());
            Solid = new Solid(fileName, mobil.SolidFid, standalone: false);
        }

        if (mobil.PrefabFid is not null)
        {
            var prefabFile = mobil.References.PrefabFid.File;
            var fileName = prefabFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(prefabFile.GetFullPath());
            Prefab ??= new Prefab(fileName, mobil.PrefabFid, standalone: false);
        }

        GeomTranslation = mobil.GeomTranslation;
        GeomRotation = mobil.GeomRotation;
    }
}
