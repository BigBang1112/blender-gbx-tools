using GBX.NET.Engines.Game;

namespace BlenderGbxTools.Models;

internal sealed class BlockInfoUnit
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public BlockInfoUnit()
    {
        
    }

    public BlockInfoUnit(CGameCtnBlockUnitInfo unit)
    {
        X = unit.RelativeOffset.X;
        Y = unit.RelativeOffset.Y;
        Z = unit.RelativeOffset.Z;
    }
}
