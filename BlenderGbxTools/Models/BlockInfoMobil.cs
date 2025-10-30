using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using System.Collections.Immutable;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoMobil
{
    public Solid? Solid { get; }
    public ImmutableList<ObjectLink>? ObjectLinks { get; }

    public BlockInfoMobil(External<CSceneMobil> mobil)
    {
        if (mobil.Node is null)
        {
            return;
        }

        if (mobil.Node.Item?.Solid?.Tree is CPlugSolid solid)
        {
            var fileName = mobil.Node.Item.Solid.TreeFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(mobil.Node.Item.Solid.TreeFile.GetFullPath());
            Solid = new Solid(fileName, solid, standalone: false);
        }

        ObjectLinks = mobil.Node
            .ObjectLink?
            .Select(x => new ObjectLink(x))
            .ToImmutableList();
    }

    public BlockInfoMobil(CGameCtnBlockInfoMobil mobil)
    {
        if (mobil.SolidFid is not null)
        {
            var fileName = mobil.SolidFidFile is null ? Guid.NewGuid().ToString() : Path.GetFileName(mobil.SolidFidFile.GetFullPath());
            Solid = new Solid(fileName, mobil.SolidFid, standalone: false);
        }
    }
}
