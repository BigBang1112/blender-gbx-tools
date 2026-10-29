using BlenderGbxTools.Extensions;
using BlenderGbxTools.Mapping;
using BlenderGbxTools.Models;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Engines.Plug;
using System.Diagnostics;
using System.Text.Json;

namespace BlenderGbxTools;

internal static class GbxSerializer
{
    public static void Serialize(Stream stream, string fileName, CMwNod? node)
    {
        var startTime = Stopwatch.GetTimestamp();
        var context = new MappingContext();

        switch (node)
        {
            case CPlugSolid solid:
                JsonSerializer.Serialize(stream, Solid.From(fileName, solid, context) with
                {
                    Materials = context.GetMaterials(),
                    SurfaceMaterials = solid.GetAllSurfaceMaterials(),
                    ExecutionTimeInSeconds = GetElapsedSeconds(startTime),
                }, AppJsonContext.Default.Solid);
                break;
            case CPlugSolid2Model solid2:
                JsonSerializer.Serialize(stream, Solid2.From(fileName, solid2, context) with
                {
                    Materials = context.GetMaterials(),
                    ExecutionTimeInSeconds = GetElapsedSeconds(startTime),
                }, AppJsonContext.Default.Solid2);
                break;
            case CGameCtnBlockInfo blockInfo:
                JsonSerializer.Serialize(stream, BlockInfo.From(blockInfo, context) with
                {
                    Materials = context.GetMaterials(),
                    SurfaceMaterials = blockInfo.GetAllSurfaceMaterials(),
                    ExecutionTimeInSeconds = GetElapsedSeconds(startTime),
                }, AppJsonContext.Default.BlockInfo);
                break;
            case CPlugSurface surface:
                JsonSerializer.Serialize(stream, Surface.From(fileName, surface) with
                {
                    SurfaceMaterials = surface.GetAllMaterials(),
                    ExecutionTimeInSeconds = GetElapsedSeconds(startTime),
                }, AppJsonContext.Default.Surface);
                break;
            case CGameItemModel item:
                JsonSerializer.Serialize(stream, Item.From(fileName, item, context) with
                {
                    Materials = context.GetMaterials(),
                    ExecutionTimeInSeconds = GetElapsedSeconds(startTime),
                }, AppJsonContext.Default.Item);
                break;
            case CPlugPrefab prefab:
                JsonSerializer.Serialize(stream, Prefab.From(fileName, prefab, context) with
                {
                    Materials = context.GetMaterials(),
                    SurfaceMaterials = prefab.GetAllSurfaceMaterials(),
                    ExecutionTimeInSeconds = GetElapsedSeconds(startTime),
                }, AppJsonContext.Default.Prefab);
                break;
            default:
                throw new NotSupportedException("Serialization of this node is not supported.");
        }
    }

    private static double GetElapsedSeconds(long startTime)
    {
        return Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}
