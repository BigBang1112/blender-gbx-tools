using GBX.NET.Engines.Game;

namespace BlenderGbxTools.Models;

internal sealed record BlockInfoUnit
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }

    public static BlockInfoUnit From(CGameCtnBlockUnitInfo unit)
    {
        return new BlockInfoUnit
        {
            X = unit.RelativeOffset.X,
            Y = unit.RelativeOffset.Y,
            Z = unit.RelativeOffset.Z,
        };
    }
}
