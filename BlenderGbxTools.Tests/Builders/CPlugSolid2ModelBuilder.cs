using BlenderGbxTools.Tests.Helpers;
using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Builders;

internal sealed class CPlugSolid2ModelBuilder
{
    private readonly NodeRefList<CPlugVisual> visuals = new();
    private readonly List<CPlugSolid2Model.ShadedGeom> shadedGeoms = [];
    private readonly NodeRefList<CPlugMaterial> materials = new();
    private readonly NodeRefList<CPlugMaterialUserInst> materialInsts = new();
    private readonly List<CPlugSolid2Model.Material> customMaterials = [];
    private readonly List<string> materialIds = [];

    public CPlugSolid2ModelBuilder WithMaterialFile(string filePath, CPlugMaterial? material = null)
    {
        materials.Add(GbxFiles.CreateRef(filePath, material));
        return this;
    }

    public CPlugSolid2ModelBuilder WithMaterialInst(string? materialName, string? link = null)
    {
        materialInsts.Add(new CPlugMaterialUserInst { MaterialName = materialName, Link = link });
        return this;
    }

    public CPlugSolid2ModelBuilder WithCustomMaterial(string? materialName, CPlugMaterialUserInst? userInst)
    {
        customMaterials.Add(new CPlugSolid2Model.Material { MaterialName = materialName, MaterialUserInst = userInst });
        return this;
    }

    public CPlugSolid2ModelBuilder WithMaterialId(string materialId)
    {
        materialIds.Add(materialId);
        return this;
    }

    public CPlugSolid2ModelBuilder WithShadedGeom(CPlugVisual? visual, int materialIndex = 0, int lod = 0)
    {
        visuals.Add(new NodeRef<CPlugVisual> { Node = visual });
#pragma warning disable CS0618 // ShadedGeom model still reads Lod
        shadedGeoms.Add(new CPlugSolid2Model.ShadedGeom { VisualIndex = visuals.Count - 1, MaterialIndex = materialIndex, Lod = lod });
#pragma warning restore CS0618
        return this;
    }

    public CPlugSolid2Model Build()
    {
        // Unused lists stay null, as they are in a parsed model
        return new CPlugSolid2Model
        {
            Visuals = visuals.Count == 0 ? null : visuals,
            ShadedGeoms = shadedGeoms.Count == 0 ? null : [.. shadedGeoms],
            Materials = materials.Count == 0 ? null : materials,
            MaterialInsts = materialInsts.Count == 0 ? null : materialInsts,
            CustomMaterials = customMaterials.Count == 0 ? null : [.. customMaterials],
            MaterialIds = materialIds.Count == 0 ? null : [.. materialIds],
        };
    }
}
