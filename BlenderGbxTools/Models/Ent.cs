using GBX.NET;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Models;

internal sealed class Ent
{
    public Solid2? Mesh { get; }
    public Surface? Surface { get; }
    public Prefab? Prefab { get; }

    public Vec3 Position { get; }
    public Quat Rotation { get; }

    public Ent(CPlugPrefab.EntRef ent)
    {
        switch (ent.Model)
        {
            case CPlugStaticObjectModel staticModel:
                if (staticModel.Mesh is not null)
                {
                    Mesh = new Solid2("", staticModel.Mesh, standalone: false);
                }

                if (staticModel.Shape is not null)
                {
                    Surface = new Surface(null, staticModel.Shape, standalone: false);
                }
                break;
            case CPlugDynaObjectModel dynaModel:
                if (dynaModel.Mesh is not null)
                {
                    Mesh = new Solid2("", dynaModel.Mesh, standalone: false);
                }

                if (dynaModel.StaticShape is not null)
                {
                    Surface = new Surface(null, dynaModel.StaticShape, standalone: false);
                }
                break;
            case CPlugPrefab prefab:
                Prefab = new Prefab("", prefab, standalone: false);
                break;
        }

        Position = ent.Position;
        Rotation = ent.Rotation;
    }
}
