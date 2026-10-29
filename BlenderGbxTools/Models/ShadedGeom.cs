using BlenderGbxTools.Mapping;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed record ShadedGeom
{
    public string? Material { get; init; }
    public Visual? Visual { get; init; }
    public int Lod { get; init; }

    public static ShadedGeom From(CPlugSolid2Model solid2, CPlugSolid2Model.ShadedGeom geom, Solid2Materials materials)
    {
        var visual = solid2.Visuals?[geom.VisualIndex];

        if (visual is null)
        {
            return new ShadedGeom();
        }

        return new ShadedGeom
        {
            Material = materials.GetName(geom.MaterialIndex),
            Visual = visual is CPlugVisualIndexed indexed ? Visual.From(indexed) : null,
            Lod = geom.Lod,
        };
    }
}
