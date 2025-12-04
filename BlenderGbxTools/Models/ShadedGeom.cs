using BlenderGbxTools.Extensions;
using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class ShadedGeom
{
    public string? Material { get; set; }
    public Visual? Visual { get; set; }
    public int Lod { get; set; }

    public ShadedGeom()
    {
        
    }

    public ShadedGeom(CPlugSolid2Model solid2, CPlugSolid2Model.ShadedGeom geom)
    {
        var visual = solid2.Visuals?[geom.VisualIndex];

        if (visual is null)
        {
            return;
        }
        
        Material = GetMaterialName(solid2, geom);
        Visual = visual is CPlugVisualIndexed indexed ? new Visual(indexed) : null;
        Lod = geom.Lod;
    }

    private string? GetMaterialName(CPlugSolid2Model solid2, CPlugSolid2Model.ShadedGeom geom)
    {
        if (solid2.Materials?.Length > geom.MaterialIndex)
        {
            return GbxPath.GetFileNameWithoutExtension(solid2.Materials[geom.MaterialIndex].File?.FilePath);
        }

        if (solid2.MaterialInsts?.Length > geom.MaterialIndex)
        {
            return solid2.MaterialInsts[geom.MaterialIndex].GetMaterialName();
        }

        if (solid2.CustomMaterials?.Length > geom.MaterialIndex)
        {
            var customMat = solid2.CustomMaterials[geom.MaterialIndex];
            return customMat.MaterialUserInst?.GetMaterialName() ?? customMat.MaterialName;
        }

        if (solid2.MaterialIds?.Length > geom.MaterialIndex)
        {
            return solid2.MaterialIds[geom.MaterialIndex];
        }

        return null;
    }
}
