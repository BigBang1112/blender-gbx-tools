using BlenderGbxTools.Extensions;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Extensions;

internal sealed class CPlugSolid2ModelExtensionsTests
{
    [Test]
    public async Task GetAllMaterials_MaterialFiles_KeysByFileName()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialFile(@"Media\Material\Asphalt.Material.Gbx", new CPlugMaterial { SurfaceId = CPlugSurface.MaterialId.Pavement })
            .WithMaterialFile(@"Media\Material\Missing.Material.Gbx")
            .Build();

        // Act
        var materials = solid2.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Asphalt", "Missing"]);
        await Assert.That(materials["Asphalt"]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Pavement);
        await Assert.That(materials["Missing"]).IsNull();
    }

    [Test]
    public async Task GetAllMaterials_MaterialInsts_KeysByNameOrLink()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialInst("Wood")
            .WithMaterialInst("", link: "Stadium/Grass")
            .Build();

        // Act
        var materials = solid2.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Wood", "Stadium/Grass"]);
    }

    [Test]
    public async Task GetAllMaterials_UnnamedMaterialInst_IsSkipped()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialInst(materialName: null, link: null)
            .Build();

        // Act
        var materials = solid2.GetAllMaterials();

        // Assert
        await Assert.That(materials).IsEmpty();
    }

    [Test]
    public async Task GetAllMaterials_CustomMaterial_KeysByUserInstName()
    {
        // Arrange
        var userInst = new CPlugMaterialUserInst { MaterialName = "Plastic", SurfacePhysicId = CPlugSurface.MaterialId.Rubber };
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithCustomMaterial("Ignored", userInst)
            .Build();

        // Act
        var materials = solid2.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Plastic"]);
        await Assert.That(materials["Plastic"]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Rubber);
    }

    [Test]
    public async Task GetAllMaterials_CustomMaterialWithoutUserInst_Throws()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithCustomMaterial("Plastic", userInst: null)
            .Build();

        // Act
        var act = () => solid2.GetAllMaterials();

        // Assert
        await Assert.That(act).Throws<Exception>().WithMessageContaining("Custom material is null");
    }

    [Test]
    public async Task GetAllMaterials_MaterialIds_UseDefaultMaterial()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialId("Dirt")
            .Build();

        // Act
        var materials = solid2.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Dirt"]);
        await Assert.That(materials["Dirt"]).IsNull();
    }
}
