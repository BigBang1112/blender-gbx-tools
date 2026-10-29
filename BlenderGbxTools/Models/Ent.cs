using BlenderGbxTools.Mapping;
using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed record Ent
{
    public Solid2? Mesh { get; init; }
    public Surface? Surface { get; init; }
    public Prefab? Prefab { get; init; }

    public Vec3 Position { get; init; }
    public Quat Rotation { get; init; }

    public static Ent From(CPlugPrefab.EntRef ent, MappingContext context)
    {
        var mesh = default(CPlugSolid2Model);
        var shape = default(CPlugSurface);

        switch (ent.Model)
        {
            case CPlugStaticObjectModel staticModel:
                mesh = staticModel.Mesh;
                shape = staticModel.Shape;
                break;
            case CPlugDynaObjectModel dynaModel:
                mesh = dynaModel.Mesh;
                shape = dynaModel.StaticShape;
                break;
        }

        return new Ent
        {
            Mesh = mesh is null ? null : Solid2.From("", mesh, context),
            Surface = shape is null ? null : Surface.From(null, shape, context),
            Prefab = ent.Model is CPlugPrefab prefab ? Prefab.From("", prefab, context) : null,
            Position = ent.Position,
            Rotation = ent.Rotation,
        };
    }
}
