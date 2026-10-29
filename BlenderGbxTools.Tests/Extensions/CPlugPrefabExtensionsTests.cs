using BlenderGbxTools.Extensions;
using BlenderGbxTools.Tests.Builders;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Extensions;

internal sealed class CPlugPrefabExtensionsTests
{
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
