using BlenderGbxTools.Models;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugSolid2ModelExtensions
{
    public static ImmutableDictionary<string, Material?> GetAllMaterials(this CPlugSolid2Model solid2)
    {
        var materials = ImmutableDictionary.CreateBuilder<string, Material?>();

        foreach (var mat in solid2.Materials ?? [])
        {
            var materialName = GbxPath.GetFileNameWithoutExtension(mat.File?.FilePath) ?? Guid.NewGuid().ToString();
            var material = mat.Node is null ? null : new Material(mat.Node);
            materials.Add(materialName, material);
        }

        foreach (var mat in solid2.MaterialInsts ?? [])
        {
            var material = new Material(mat);
            var materialName = mat.GetMaterialName();

            if (string.IsNullOrEmpty(materialName))
            {
                continue;
            }

            materials.Add(materialName, material);
        }

        foreach (var mat in solid2.CustomMaterials ?? [])
        {
            if (mat.MaterialUserInst is null)
            {
                throw new Exception("Custom material is null");
            }

            var material = new Material(mat.MaterialUserInst);
            var materialName = mat.MaterialUserInst.GetMaterialName();

            if (string.IsNullOrEmpty(materialName))
            {
                continue;
            }

            materials.Add(materialName, material);
        }

        foreach (var mat in solid2.MaterialIds ?? [])
        {
            materials.Add(mat, null);
        }

        return materials.ToImmutable();
    }
}
