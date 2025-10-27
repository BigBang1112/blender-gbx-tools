using BlenderGbxTools.Models;
using System.Text.Json.Serialization;

namespace BlenderGbxTools;

[JsonSerializable(typeof(Solid))]
[JsonSerializable(typeof(BlockInfo))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
internal sealed partial class AppJsonContext : JsonSerializerContext;