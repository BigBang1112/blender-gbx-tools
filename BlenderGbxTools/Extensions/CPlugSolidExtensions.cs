using BlenderGbxTools.Models;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugSolidExtensions
{
    public static IEnumerable<CPlugTree> GetDistinctMaterialTrees(this CPlugSolid solid)
    {
        return solid.GetAllChildren(includeVisualMipLevels: true)
            .DistinctBy(tree => tree.GetMaterialName())
            .Where(tree => tree.Shader is CPlugMaterial);
    }

    public static ImmutableDictionary<string, Material> GetAllMaterials(this CPlugSolid solid)
    {
        // This only resolves materials with recognized Material.Gbx/Shader.Gbx
        var materials = solid.GetDistinctMaterialTrees()
            .ToImmutableDictionary(
                tree => tree.GetMaterialName(),
                tree => new Material((CPlugMaterial)tree.Shader!));

        return materials;
    }
}
