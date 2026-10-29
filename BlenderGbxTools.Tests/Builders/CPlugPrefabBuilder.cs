using GBX.NET;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Builders;

internal sealed class CPlugPrefabBuilder
{
    private readonly List<CPlugPrefab.EntRef> ents = [];

    public CPlugPrefabBuilder WithStaticObject(CPlugSolid2Model? mesh = null, CPlugSurface? shape = null, Vec3 position = default)
    {
        return WithEnt(new CPlugStaticObjectModel { Mesh = mesh, Shape = shape }, position);
    }

    public CPlugPrefabBuilder WithDynaObject(CPlugSolid2Model? mesh = null, CPlugSurface? staticShape = null)
    {
        return WithEnt(new CPlugDynaObjectModel { Mesh = mesh, StaticShape = staticShape });
    }

    public CPlugPrefabBuilder WithPrefab(CPlugPrefab prefab)
    {
        return WithEnt(prefab);
    }

    public CPlugPrefabBuilder WithEnt(CMwNod? model, Vec3 position = default)
    {
        ents.Add(new CPlugPrefab.EntRef { Model = model, Position = position });
        return this;
    }

    public CPlugPrefab Build()
    {
        return new CPlugPrefab { Ents = [.. ents] };
    }
}
