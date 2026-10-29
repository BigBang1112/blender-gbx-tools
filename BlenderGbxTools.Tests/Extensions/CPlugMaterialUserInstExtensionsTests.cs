using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Extensions;

internal sealed class CPlugMaterialUserInstExtensionsTests
{
    [Test]
    [Arguments("Wood", "Stadium/Grass", "Wood")]
    [Arguments("", "Stadium/Grass", "Stadium/Grass")]
    [Arguments(null, "Stadium/Grass", "Stadium/Grass")]
    [Arguments(null, null, null)]
    public async Task GetMaterialName_MaterialNameAndLink_PrefersMaterialName(string? materialName, string? link, string? expectedName)
    {
        // Arrange
        var materialInst = new CPlugMaterialUserInst { MaterialName = materialName, Link = link };

        // Act
        var name = materialInst.GetMaterialName();

        // Assert
        await Assert.That(name).IsEqualTo(expectedName);
    }
}
