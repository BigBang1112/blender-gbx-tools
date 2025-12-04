using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Extensions;

internal static class CPlugMaterialUserInstExtensions
{
    public static string? GetMaterialName(this CPlugMaterialUserInst mat)
    {
        var materialName = mat.MaterialName;

        if (string.IsNullOrEmpty(materialName))
        {
            materialName = mat.Link;
        }

        return materialName;
    }
}
