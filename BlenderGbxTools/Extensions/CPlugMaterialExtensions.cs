using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Extensions;

internal static class CPlugMaterialExtensions
{
    public static string GetShaderName(this CPlugMaterial material)
    {
        return GbxPath.GetFileNameWithoutExtension(material.ShaderFile?.FilePath) ?? Guid.NewGuid().ToString();
    }
}
