using BlenderGbxTools.Models;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Engines.Plug;
using System.Text.Json;

namespace BlenderGbxTools;

internal static class GbxSerializer
{
    public static void Serialize(Stream stream, string fileName, CMwNod? node)
    {
        switch (node)
        {
            case CPlugSolid solid:
                JsonSerializer.Serialize(stream, new Solid(fileName, solid), AppJsonContext.Default.Solid);
                break;
            case CPlugSolid2Model solid2:
                JsonSerializer.Serialize(stream, new Solid2(fileName, solid2), AppJsonContext.Default.Solid2);
                break;
            case CGameCtnBlockInfo blockInfo:
                JsonSerializer.Serialize(stream, new BlockInfo(blockInfo), AppJsonContext.Default.BlockInfo);
                break;
            case CPlugSurface surface:
                JsonSerializer.Serialize(stream, new Surface(fileName, surface), AppJsonContext.Default.Surface);
                break;
            case CGameItemModel item:
                JsonSerializer.Serialize(stream, new Item(fileName, item), AppJsonContext.Default.Item);
                break;
            case CPlugPrefab prefab:
                JsonSerializer.Serialize(stream, new Prefab(fileName, prefab), AppJsonContext.Default.Prefab);
                break;
            default:
                throw new NotSupportedException("Serialization of this node is not supported.");
        }
    }
}
