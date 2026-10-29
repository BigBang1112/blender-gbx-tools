using BlenderGbxTools.Mapping;
using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class PrefabTests
{
    [Test]
    public async Task From_NoEnts_LeavesEntsNull()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder().Build();

        // Act
        var result = Prefab.From("Empty.Prefab.Gbx", prefab, new MappingContext());

        // Assert
        await Assert.That(result.Ents).IsNull();
    }

    [Test]
    public async Task From_StaticAndDynaObjects_AddsMeshMaterials()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .WithDynaObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialInst("Wood").Build())
            .Build();
        var context = new MappingContext();

        // Act
        Prefab.From("Tree.Prefab.Gbx", prefab, context);

        // Assert
        await Assert.That(context.GetMaterials().Keys).IsEquivalentTo(["Dirt", "Wood"]);
    }

    [Test]
    public async Task From_NestedPrefab_AddsInnerMaterials()
    {
        // Arrange
        var innerPrefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Ice").Build())
            .Build();
        var prefab = new CPlugPrefabBuilder()
            .WithPrefab(innerPrefab)
            .Build();
        var context = new MappingContext();

        // Act
        var result = Prefab.From("Outer.Prefab.Gbx", prefab, context);

        // Assert
        await Assert.That(result.Ents![0].Prefab).IsNotNull();
        await Assert.That(context.GetMaterials().Keys).IsEquivalentTo(["Ice"]);
    }

    [Test]
    public async Task From_MaterialSharedByEnts_IsAddedOnce()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .WithDynaObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .Build();
        var context = new MappingContext();

        // Act
        Prefab.From("Tree.Prefab.Gbx", prefab, context);

        // Assert
        await Assert.That(context.GetMaterials().Keys).IsEquivalentTo(["Dirt"]);
    }

    [Test]
    public async Task From_DynaObject_UsesStaticShape()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithEnt(new CPlugDynaObjectModel
            {
                Shape = new CPlugSurface { Surf = new CPlugSurface.Sphere { Size = 1 } },
                StaticShape = new CPlugSurface { Surf = new CPlugSurface.Sphere { Size = 2 } },
            })
            .Build();

        // Act
        var result = Prefab.From("Door.Prefab.Gbx", prefab, new MappingContext());

        // Assert
        await Assert.That(result.Ents![0].Surface!.Sphere).IsEqualTo(2);
    }
}
