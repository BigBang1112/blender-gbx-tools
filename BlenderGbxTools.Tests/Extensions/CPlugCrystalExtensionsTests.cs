using BlenderGbxTools.Extensions;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Extensions;

internal sealed class CPlugCrystalExtensionsTests
{
    [Test]
    public async Task GetAllMaterials_NamedMaterials_KeysByMaterialName()
    {
        // Arrange
        var crystal = new CPlugCrystal
        {
            Materials =
            [
                CreateMaterial(new CPlugMaterialUserInst { MaterialName = "Wood", SurfacePhysicId = CPlugSurface.MaterialId.Wood }),
                CreateMaterial(new CPlugMaterialUserInst { Link = "Stadium/Grass" }),
            ],
        };

        // Act
        var materials = crystal.GetAllMaterials();

        // Assert
        await Assert.That(materials.Keys).IsEquivalentTo(["Wood", "Stadium/Grass"]);
        await Assert.That(materials["Wood"]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Wood);
    }

    [Test]
    public async Task GetAllMaterials_UnnamedMaterial_IsSkipped()
    {
        // Arrange
        var crystal = new CPlugCrystal { Materials = [CreateMaterial(new CPlugMaterialUserInst())] };

        // Act
        var materials = crystal.GetAllMaterials();

        // Assert
        await Assert.That(materials).IsEmpty();
    }

    [Test]
    public async Task GetAllMaterials_MaterialWithoutUserInst_Throws()
    {
        // Arrange
        var crystal = new CPlugCrystal { Materials = [CreateMaterial(userInst: null)] };

        // Act
        var act = () => crystal.GetAllMaterials();

        // Assert
        await Assert.That(act).Throws<Exception>().WithMessageContaining("Custom material is null");
    }

    private static CPlugCrystal.Material CreateMaterial(CPlugMaterialUserInst? userInst)
    {
        return new CPlugCrystal.Material { MaterialUserInst = userInst };
    }
}
