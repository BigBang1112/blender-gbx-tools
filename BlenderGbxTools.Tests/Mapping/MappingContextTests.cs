using BlenderGbxTools.Mapping;
using BlenderGbxTools.Models;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Mapping;

internal sealed class MappingContextTests
{
    [Test]
    public async Task GetName_File_ReturnsFileNameWithoutGbxExtension()
    {
        // Arrange
        var context = new MappingContext();

        // Act
        var name = context.GetName(GbxFiles.Create(@"Media\Material\RoadTech.Material.Gbx"), new CPlugMaterial());

        // Assert
        await Assert.That(name).IsEqualTo("RoadTech");
    }

    [Test]
    public async Task GetName_SameNodeWithoutFile_ReturnsSameUniqueName()
    {
        // Arrange
        var context = new MappingContext();
        var node = new CPlugMaterial();

        // Act
        var first = context.GetName(file: null, node);
        var second = context.GetName(file: null, node);

        // Assert
        await Assert.That(Guid.TryParse(first, out _)).IsTrue();
        await Assert.That(second).IsEqualTo(first);
    }

    [Test]
    public async Task GetName_DifferentNodesWithoutFile_ReturnsDifferentNames()
    {
        // Arrange
        var context = new MappingContext();

        // Act
        var first = context.GetName(file: null, new CPlugMaterial());
        var second = context.GetName(file: null, new CPlugMaterial());

        // Assert
        await Assert.That(second).IsNotEqualTo(first);
    }

    [Test]
    public async Task GetName_NoFileAndNoNode_ReturnsNull()
    {
        // Arrange
        var context = new MappingContext();

        // Act
        var name = context.GetName(file: null, node: null);

        // Assert
        await Assert.That(name).IsNull();
    }

    [Test]
    public async Task AddMaterial_NameAlreadyAdded_KeepsFirstMaterial()
    {
        // Arrange
        var context = new MappingContext();
        var first = new Material { SurfaceId = CPlugSurface.MaterialId.Grass };
        var createCount = 0;

        // Act
        context.AddMaterial("Grass", () => first);
        context.AddMaterial("Grass", () =>
        {
            createCount++;
            return new Material { SurfaceId = CPlugSurface.MaterialId.Ice };
        });

        // Assert
        await Assert.That(context.GetMaterials()["Grass"]).IsSameReferenceAs(first);
        await Assert.That(createCount).IsEqualTo(0);
    }
}
