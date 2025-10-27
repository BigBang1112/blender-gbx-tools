using GBX.NET.Engines.Plug;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlenderGbxTools.Models;

internal sealed class Solid : IStandalone
{
    public string? Name { get; }
    public Tree? Tree { get; }
    public ImmutableDictionary<string, Material>? Materials { get; }
    public double ExecutionTimeInSeconds { get; }

    public Solid()
    {
        
    }

    public Solid(string fileName, CPlugSolid solid, bool standalone = true)
    {
        var startTime = Stopwatch.GetTimestamp();

        // This only resolves materials with recognized Material.Gbx/Shader.Gbx
        // Materials not in this dictionary use the default material
        var materials = standalone
            ? solid.GetAllChildren(includeVisualMipLevels: true)
                .DistinctBy(tree => tree.GetMaterialName())
                .Where(tree => tree.Shader is CPlugMaterial)
                .ToImmutableDictionary(
                    tree => tree.GetMaterialName(),
                    tree => new Material((CPlugMaterial)tree.Shader!))
            : null;

        Name = fileName;
        Tree = solid.Tree is CPlugTree tree ? new Tree(tree) : null;
        Materials = materials;
        ExecutionTimeInSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
    }
}
