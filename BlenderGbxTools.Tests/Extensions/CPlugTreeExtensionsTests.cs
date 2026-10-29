using BlenderGbxTools.Extensions;
using BlenderGbxTools.Tests.Helpers;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Extensions;

internal sealed class CPlugTreeExtensionsTests
{
    [Test]
    public async Task GetMaterialName_ShaderFile_ReturnsFileNameWithoutGbxExtension()
    {
        // Arrange
        var tree = new CPlugTree();
        tree.References.Shader.Descriptor = GbxFiles.Create(@"Media\Material\RoadTech.Material.Gbx");

        // Act
        var name = tree.GetMaterialName();

        // Assert
        await Assert.That(name).IsEqualTo("RoadTech");
    }

    [Test]
    public async Task GetMaterialName_NoShaderFile_ReturnsUniqueName()
    {
        // Arrange
        var tree = new CPlugTree();

        // Act
        var first = tree.GetMaterialName();
        var second = tree.GetMaterialName();

        // Assert
        await Assert.That(Guid.TryParse(first, out _)).IsTrue();
        await Assert.That(first).IsNotEqualTo(second);
    }
}
