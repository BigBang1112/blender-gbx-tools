using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools;

internal static class CPlugTreeExtensions
{
    public static string GetMaterialName(this CPlugTree tree)
    {
        return GbxPath.GetFileNameWithoutExtension(tree.ShaderFile?.FilePath) ?? Guid.NewGuid().ToString();
    }
}
