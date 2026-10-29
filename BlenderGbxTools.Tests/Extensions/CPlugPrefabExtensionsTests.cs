using BlenderGbxTools.Extensions;
using BlenderGbxTools.Tests.Builders;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Extensions;

internal sealed class CPlugPrefabExtensionsTests
{
    [Test]
    public async Task GetAllMaterials_StaticAndDynaObjects_CollectsMeshMaterials()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .WithDynaObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialInst("Wood").Build())
            .Build();

        // Act
        var materials = prefab.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Dirt", "Wood"]);
    }

    [Test]
    public async Task GetAllMaterials_NestedPrefab_IncludesInnerMaterials()
    {
        // Arrange
        var innerPrefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Ice").Build())
            .Build();
        var prefab = new CPlugPrefabBuilder()
            .WithPrefab(innerPrefab)
            .Build();

        // Act
        var materials = prefab.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Ice"]);
    }

    [Test]
    public async Task GetAllMaterials_MaterialSharedByEnts_IsIncludedOnce()
    {
        // Arrange
        var innerPrefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .Build();
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .WithDynaObject(mesh: new CPlugSolid2ModelBuilder().WithMaterialId("Dirt").Build())
            .WithPrefab(innerPrefab)
            .Build();

        // Act
        var materials = prefab.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Dirt"]);
    }

    [Test]
    public async Task GetAllMaterials_EntsWithoutMesh_ReturnsEmpty()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(mesh: null)
            .WithEnt(model: null)
            .Build();

        // Act
        var materials = prefab.GetAllMaterials();

        // Assert
        await Assert.That(materials).IsEmpty();
    }

    [Test]
    public async Task GetDistinctSurfaceMaterials_DynaObject_UsesStaticShape()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithEnt(new CPlugDynaObjectModel
            {
                Shape = new CPlugSurface { Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Ice)] },
                StaticShape = new CPlugSurface { Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Metal)] },
            })
            .Build();

        // Act
        var surfaceMaterials = prefab.GetDistinctSurfaceMaterials().ToList();

        // Assert
        await Assert.That(surfaceMaterials).HasSingleItem();
        await Assert.That(surfaceMaterials[0].SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Metal);
    }

    [Test]
    public async Task GetDistinctSurfaceMaterials_SameSurfaceIdAndMaterial_ReturnsOne()
    {
        // Arrange
        const string grassPath = @"Media\Material\Grass.Material.Gbx";
        var innerPrefab = new CPlugPrefabBuilder()
            .WithStaticObject(shape: new CPlugSurface { Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Grass, grassPath)] })
            .Build();
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(shape: new CPlugSurface { Materials = [SurfMaterials.Create(CPlugSurface.MaterialId.Grass, grassPath)] })
            .WithPrefab(innerPrefab)
            .Build();

        // Act
        var surfaceMaterials = prefab.GetDistinctSurfaceMaterials();

        // Assert
        await Assert.That(surfaceMaterials).HasSingleItem();
    }

    [Test]
    public async Task GetDistinctSurfaceMaterials_SameSurfaceIdDifferentMaterial_ReturnsBoth()
    {
        // Arrange
        var prefab = new CPlugPrefabBuilder()
            .WithStaticObject(shape: new CPlugSurface
            {
                Materials =
                [
                    SurfMaterials.Create(CPlugSurface.MaterialId.Grass, @"Media\Material\Grass.Material.Gbx"),
                    SurfMaterials.Create(CPlugSurface.MaterialId.Grass, @"Media\Material\GrassDark.Material.Gbx"),
                ],
            })
            .Build();

        // Act
        var surfaceMaterials = prefab.GetDistinctSurfaceMaterials();

        // Assert
        await Assert.That(surfaceMaterials).Count().IsEqualTo(2);
    }
}
