using BlenderGbxTools.Mapping;
using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class ShadedGeomTests
{
    [Test]
    public async Task From_MissingVisual_LeavesGeomEmpty()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithShadedGeom(visual: null, lod: 2)
            .WithMaterialId("Grass")
            .Build();

        // Act
        var shadedGeom = MapFirstShadedGeom(solid2);

        // Assert
        await Assert.That(shadedGeom.Visual).IsNull();
        await Assert.That(shadedGeom.Material).IsNull();
        await Assert.That(shadedGeom.Lod).IsEqualTo(0);
    }

    [Test]
    public async Task From_IndexedVisual_MapsVisualAndLod()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithShadedGeom(new CPlugVisualIndexed(), lod: 2)
            .Build();

        // Act
        var shadedGeom = MapFirstShadedGeom(solid2);

        // Assert
        await Assert.That(shadedGeom.Visual).IsNotNull();
        await Assert.That(shadedGeom.Lod).IsEqualTo(2);
    }

    [Test]
    public async Task From_NonIndexedVisual_LeavesVisualNull()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithShadedGeom(new CPlugVisual3D())
            .WithMaterialId("Grass")
            .Build();

        // Act
        var shadedGeom = MapFirstShadedGeom(solid2);

        // Assert
        await Assert.That(shadedGeom.Visual).IsNull();
        await Assert.That(shadedGeom.Material).IsEqualTo("Grass");
    }

    [Test]
    public async Task From_MaterialIndex_UsesNameOfMaterialAtIndex()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialId("Dirt")
            .WithMaterialId("Grass")
            .WithShadedGeom(new CPlugVisualIndexed(), materialIndex: 1)
            .Build();

        // Act
        var shadedGeom = MapFirstShadedGeom(solid2);

        // Assert
        await Assert.That(shadedGeom.Material).IsEqualTo("Grass");
    }

    private static ShadedGeom MapFirstShadedGeom(CPlugSolid2Model solid2)
    {
        var materials = Solid2Materials.Add(solid2, new MappingContext());
        return ShadedGeom.From(solid2, solid2.ShadedGeoms![0], materials);
    }
}
