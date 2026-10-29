using BlenderGbxTools.Mapping;
using GBX.NET;
using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace BlenderGbxTools.Models;

internal sealed record Tree
{
    public string? Name { get; init; }
    public Visual? Visual { get; init; }
    public byte[]? Location { get; init; }
    public ImmutableList<Tree>? Children { get; init; }
    public ulong? Flags { get; init; }
    public ImmutableDictionary<float, Tree>? VisualMip { get; init; }
    public Light? Light { get; init; }
    public Surface? Surface { get; init; }
    public string? Material { get; init; }
    public bool Visible { get; init; }

    public static Tree From(CPlugTree tree, MappingContext context)
    {
        var visualMip = default(ImmutableDictionary<float, Tree>);

        if (tree is CPlugTreeVisualMip mip)
        {
            visualMip = mip.Levels
                .Where(x => x.Tree is not null)
                .DistinctBy(x => x.FarZ) // FarZ can be duplicated sometimes, epic Nadeo. should be additionally logged
                .ToImmutableDictionary(x => x.FarZ, x => From(x.Tree!, context));
        }

        var material = context.GetName(tree.References.Shader.File, tree.Shader);

        if (material is not null && tree.Shader is CPlugMaterial plugMaterial)
        {
            context.AddMaterial(material, () => Models.Material.From(plugMaterial));
        }

        return new Tree
        {
            Name = tree.PlugId ?? "[unnamed]",
            Visual = tree.Visual is CPlugVisualIndexed visualIndexed ? Visual.From(visualIndexed) : null,
            Location = LocationToByteArray(tree),
            Children = tree.Children is not { Count: > 0 } ? null : tree.Children.Select(x => From(x, context)).ToImmutableList(),
            VisualMip = visualMip,
            Flags = tree.Flags,
            Light = tree is CPlugTreeLight treeLight ? Light.From(treeLight) : null,
            Material = material,
            Surface = tree.Surface is CPlugSurface plugSurface ? Surface.From(null, plugSurface) : null,
            Visible = tree.IsVisible,
        };
    }

    private static byte[]? LocationToByteArray(CPlugTree tree)
    {
        if (tree.Location is not Iso4 t || tree.Location == Iso4.Identity)
        {
            return null;
        }

        return MemoryMarshal.AsBytes([t]).ToArray();
    }
}
