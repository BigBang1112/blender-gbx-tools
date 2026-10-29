using BlenderGbxTools.Extensions;
using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed record Crystal
{
    public static Crystal From(CPlugCrystal crystal, MappingContext context)
    {
        foreach (var mat in crystal.Materials)
        {
            if (mat.MaterialUserInst is null)
            {
                throw new Exception("Custom material is null");
            }

            var materialName = mat.MaterialUserInst.GetMaterialName();

            if (string.IsNullOrEmpty(materialName))
            {
                continue;
            }

            context.AddMaterial(materialName, () => Material.From(mat.MaterialUserInst));
        }

        return new Crystal();
    }
}
