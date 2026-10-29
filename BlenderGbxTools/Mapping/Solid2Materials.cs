using BlenderGbxTools.Extensions;
using BlenderGbxTools.Models;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Mapping;

internal sealed class Solid2Materials
{
    private readonly string?[] fileMaterials;
    private readonly string?[] materialInsts;
    private readonly string?[] customMaterials;
    private readonly string[] materialIds;

    private Solid2Materials(string?[] fileMaterials, string?[] materialInsts, string?[] customMaterials, string[] materialIds)
    {
        this.fileMaterials = fileMaterials;
        this.materialInsts = materialInsts;
        this.customMaterials = customMaterials;
        this.materialIds = materialIds;
    }

    public static Solid2Materials Add(CPlugSolid2Model solid2, MappingContext context)
    {
        var fileMaterials = (solid2.References.Materials ?? []).Select(reference =>
        {
            var name = context.GetName(reference.File, reference.Node);

            if (name is not null)
            {
                context.AddMaterial(name, () => reference.Node is null ? null : Material.From(reference.Node));
            }

            return name;
        }).ToArray();

        var materialInsts = (solid2.MaterialInsts ?? []).Select(materialInst =>
        {
            var name = materialInst?.GetMaterialName();

            if (!string.IsNullOrEmpty(name))
            {
                context.AddMaterial(name, () => Material.From(materialInst!));
            }

            return name;
        }).ToArray();

        var customMaterials = (solid2.CustomMaterials ?? []).Select(customMaterial =>
        {
            var userInst = customMaterial.MaterialUserInst;
            var name = userInst?.GetMaterialName() ?? customMaterial.MaterialName;

            if (!string.IsNullOrEmpty(name))
            {
                context.AddMaterial(name, () => userInst is null ? null : Material.From(userInst));
            }

            return name;
        }).ToArray();

        var materialIds = solid2.MaterialIds ?? [];

        foreach (var materialId in materialIds)
        {
            context.AddMaterial(materialId, () => null);
        }

        return new Solid2Materials(fileMaterials, materialInsts, customMaterials, materialIds);
    }

    public string? GetName(int materialIndex)
    {
        // The first list long enough to contain the index is the one in use
        if (materialIndex < fileMaterials.Length)
        {
            return fileMaterials[materialIndex];
        }

        if (materialIndex < materialInsts.Length)
        {
            return materialInsts[materialIndex];
        }

        if (materialIndex < customMaterials.Length)
        {
            return customMaterials[materialIndex];
        }

        if (materialIndex < materialIds.Length)
        {
            return materialIds[materialIndex];
        }

        return null;
    }
}
