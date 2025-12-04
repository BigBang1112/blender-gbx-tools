using BlenderGbxTools.Models;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;

namespace BlenderGbxTools.Extensions;

internal static class CPlugCrystalExtensions
{
    public static ImmutableDictionary<string, Material?> GetAllMaterials(this CPlugCrystal crystal)
    {
        var materials = ImmutableDictionary.CreateBuilder<string, Material?>();

        foreach (var mat in crystal.Materials ?? [])
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

        return materials.ToImmutable();
    }
}
