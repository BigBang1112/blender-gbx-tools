using BlenderGbxTools.Mapping;
using BlenderGbxTools.Models;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Models;

internal sealed class CrystalTests
{
    [Test]
    public async Task From_NamedMaterials_AddsMaterialsByName()
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
        var context = new MappingContext();

        // Act
        Crystal.From(crystal, context);

        // Assert
        var materials = context.GetMaterials();
        await Assert.That(materials.Keys).IsEquivalentTo(["Wood", "Stadium/Grass"]);
        await Assert.That(materials["Wood"]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Wood);
    }

    [Test]
    public async Task From_UnnamedMaterial_IsNotAdded()
    {
        // Arrange
        var crystal = new CPlugCrystal { Materials = [CreateMaterial(new CPlugMaterialUserInst())] };
        var context = new MappingContext();

        // Act
        Crystal.From(crystal, context);

        // Assert
        await Assert.That(context.GetMaterials()).IsEmpty();
    }

    [Test]
    public async Task From_MaterialWithoutUserInst_Throws()
    {
        // Arrange
        var crystal = new CPlugCrystal { Materials = [CreateMaterial(userInst: null)] };

        // Act
        var act = () => Crystal.From(crystal, new MappingContext());

        // Assert
        await Assert.That(act).Throws<Exception>().WithMessageContaining("Custom material is null");
    }

    private static CPlugCrystal.Material CreateMaterial(CPlugMaterialUserInst? userInst)
    {
        return new CPlugCrystal.Material { MaterialUserInst = userInst };
    }
}
