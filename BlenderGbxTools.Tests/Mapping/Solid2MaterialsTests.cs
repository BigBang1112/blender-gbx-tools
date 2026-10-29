using BlenderGbxTools.Mapping;
using BlenderGbxTools.Tests.Builders;
using GBX.NET.Engines.Plug;

namespace BlenderGbxTools.Tests.Mapping;

internal sealed class Solid2MaterialsTests
{
    [Test]
    [MethodDataSource(nameof(MaterialSources))]
    public async Task GetName_MaterialSource_ResolvesMaterialName(CPlugSolid2ModelBuilder builder, string? expectedName)
    {
        // Arrange
        var solid2 = builder.Build();
        var materials = Solid2Materials.Add(solid2, new MappingContext());

        // Act
        var name = materials.GetName(0);

        // Assert
        await Assert.That(name).IsEqualTo(expectedName);
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
    public async Task GetName_SeveralMaterialSources_PrefersMaterialFiles()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialFile(@"Media\Material\Asphalt.Material.Gbx")
            .WithMaterialInst("Wood")
            .WithMaterialId("Dirt")
            .Build();
        var materials = Solid2Materials.Add(solid2, new MappingContext());

        // Act
        var name = materials.GetName(0);

        // Assert
        await Assert.That(name).IsEqualTo("Asphalt");
    }

    [Test]
    public async Task Add_MaterialFiles_AddsResolvedAndMissingMaterials()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialFile(@"Media\Material\Asphalt.Material.Gbx", new CPlugMaterial { SurfaceId = CPlugSurface.MaterialId.Pavement })
            .WithMaterialFile(@"Media\Material\Missing.Material.Gbx")
            .Build();
        var context = new MappingContext();

        // Act
        Solid2Materials.Add(solid2, context);

        // Assert
        var materials = context.GetMaterials();
        await Assert.That(materials.Keys).IsEquivalentTo(["Asphalt", "Missing"]);
        await Assert.That(materials["Asphalt"]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Pavement);
        await Assert.That(materials["Missing"]).IsNull();
    }

    [Test]
    public async Task Add_EmbeddedMaterialWithoutFile_UsesSameNameAsShadedGeoms()
    {
        // Arrange
        var solid2 = new CPlugSolid2Model
        {
            Materials = new([new CPlugMaterial { SurfaceId = CPlugSurface.MaterialId.Metal }]),
        };
        var context = new MappingContext();

        // Act
        var name = Solid2Materials.Add(solid2, context).GetName(0);

        // Assert
        await Assert.That(name).IsNotNull();
        await Assert.That(context.GetMaterials()[name!]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Metal);
    }

    [Test]
    public async Task Add_UnnamedMaterialInst_IsNotAdded()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialInst(materialName: null, link: null)
            .Build();
        var context = new MappingContext();

        // Act
        Solid2Materials.Add(solid2, context);

        // Assert
        await Assert.That(context.GetMaterials()).IsEmpty();
    }

    [Test]
    public async Task Add_CustomMaterial_UsesUserInstSurfaceId()
    {
        // Arrange
        var userInst = new CPlugMaterialUserInst { MaterialName = "Plastic", SurfacePhysicId = CPlugSurface.MaterialId.Rubber };
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithCustomMaterial("Ignored", userInst)
            .Build();
        var context = new MappingContext();

        // Act
        Solid2Materials.Add(solid2, context);

        // Assert
        var materials = context.GetMaterials();
        await Assert.That(materials.Keys).IsEquivalentTo(["Plastic"]);
        await Assert.That(materials["Plastic"]!.SurfaceId).IsEqualTo(CPlugSurface.MaterialId.Rubber);
    }

    [Test]
    public async Task Add_CustomMaterialWithoutUserInst_UsesDefaultMaterial()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithCustomMaterial("Plastic", userInst: null)
            .Build();
        var context = new MappingContext();

        // Act
        Solid2Materials.Add(solid2, context);

        // Assert
        var materials = context.GetMaterials();
        await Assert.That(materials.Keys).IsEquivalentTo(["Plastic"]);
        await Assert.That(materials["Plastic"]).IsNull();
    }

    [Test]
    public async Task Add_SameNameInSeveralSources_KeepsFirst()
    {
        // Arrange
        var solid2 = new CPlugSolid2ModelBuilder()
            .WithMaterialInst("Dirt")
            .WithMaterialId("Dirt")
            .Build();
        var context = new MappingContext();

        // Act
        Solid2Materials.Add(solid2, context);

        // Assert
        var materials = context.GetMaterials();
        await Assert.That(materials.Keys).IsEquivalentTo(["Dirt"]);
        await Assert.That(materials["Dirt"]).IsNotNull();
    }
}
