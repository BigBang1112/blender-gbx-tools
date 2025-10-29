using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoMobil
{
    public Solid? Solid { get; }

    public BlockInfoMobil(External<CSceneMobil> mobil)
    {
        if (mobil.Node?.Item?.Solid?.Tree is CPlugSolid solid)
        {
            var fileName = mobil.File is null ? Guid.NewGuid().ToString() : Path.GetFileName(mobil.File.GetFullPath());
            Solid = new Solid(fileName, solid, standalone: false);
        }
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
