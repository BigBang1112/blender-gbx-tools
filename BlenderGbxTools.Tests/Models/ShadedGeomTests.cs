using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class ShadedGeomTests
{
    [Test]
    public async Task Constructor_MissingVisual_LeavesGeomEmpty()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithShadedGeom(visual: null, lod: 2)
            .WithMaterialId("Grass")
            .Build();

        // Act
        var shadedGeom = new ShadedGeom(solid2, solid2.ShadedGeoms![0]);

        // Assert
        await Assert.That(shadedGeom.Visual).IsNull();
        await Assert.That(shadedGeom.Material).IsNull();
        await Assert.That(shadedGeom.Lod).IsEqualTo(0);
    }

    [Test]
    public async Task Constructor_IndexedVisual_MapsVisualAndLod()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithShadedGeom(new CPlugVisualIndexed(), lod: 2)
            .Build();

        // Act
        var shadedGeom = new ShadedGeom(solid2, solid2.ShadedGeoms![0]);

        // Assert
        await Assert.That(shadedGeom.Visual).IsNotNull();
        await Assert.That(shadedGeom.Lod).IsEqualTo(2);
    }

    [Test]
    public async Task Constructor_NonIndexedVisual_LeavesVisualNull()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithShadedGeom(new CPlugVisual3D())
            .WithMaterialId("Grass")
            .Build();

        // Act
        var shadedGeom = new ShadedGeom(solid2, solid2.ShadedGeoms![0]);

        // Assert
        await Assert.That(shadedGeom.Visual).IsNull();
        await Assert.That(shadedGeom.Material).IsEqualTo("Grass");
    }

    [Test]
    [MethodDataSource(nameof(MaterialSources))]
    public async Task Constructor_MaterialSource_ResolvesMaterialName(CPlugSolid2ModelBuilder builder, string? expectedMaterial)
    {
        // Arrange
        var solid2 = builder.WithShadedGeom(new CPlugVisualIndexed()).Build();

        // Act
        var shadedGeom = new ShadedGeom(solid2, solid2.ShadedGeoms![0]);

        // Assert
        await Assert.That(shadedGeom.Material).IsEqualTo(expectedMaterial);
    }

    public static IEnumerable<Func<(CPlugSolid2ModelBuilder, string?)>> MaterialSources()
    {
        // Material file
        yield return () => (new CPlugSolid2ModelBuilder().WithMaterialFile(@"Media\Material\Asphalt.Material.Gbx"), "Asphalt");
        // Material instance, with and without a name
        yield return () => (new CPlugSolid2ModelBuilder().WithMaterialInst("Wood"), "Wood");
        yield return () => (new CPlugSolid2ModelBuilder().WithMaterialInst("", link: "Stadium/Grass"), "Stadium/Grass");
        // Custom material, with and without a user instance
        yield return () => (new CPlugSolid2ModelBuilder().WithCustomMaterial("Ignored", new CPlugMaterialUserInst { MaterialName = "Plastic" }), "Plastic");
        yield return () => (new CPlugSolid2ModelBuilder().WithCustomMaterial("Fallback", userInst: null), "Fallback");
        // Material ID
        yield return () => (new CPlugSolid2ModelBuilder().WithMaterialId("Dirt"), "Dirt");
        // No material at the index
        yield return () => (new CPlugSolid2ModelBuilder(), null);
    }

    [Test]
    public async Task Constructor_SeveralMaterialSources_PrefersMaterialFiles()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialFile(@"Media\Material\Asphalt.Material.Gbx")
            .WithMaterialInst("Wood")
            .WithMaterialId("Dirt")
            .WithShadedGeom(new CPlugVisualIndexed())
            .Build();

        // Act
        var shadedGeom = new ShadedGeom(solid2, solid2.ShadedGeoms![0]);

        // Assert
        await Assert.That(shadedGeom.Material).IsEqualTo("Asphalt");
    }
}
