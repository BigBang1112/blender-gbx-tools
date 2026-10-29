using GBX.NET;
using GBX.NET.Components;
using GBX.NET.Engines.MwFoundations;

namespace BlenderGbxTools.Tests.Helpers;

internal static class GbxFiles
{
    public static GbxRefTableFile Create(string filePath)
    {
        // A new table per file keeps parallel tests from sharing state
        return new GbxRefTable().CreateFile(filePath, flags: 0, useFile: true);
    }

    public static NodeRef<T> CreateRef<T>(string filePath, T? node = null) where T : CMwNod
    {
        var refTable = new GbxRefTable();

        // Setting Node directly would clear the file descriptor, so the node is resolved through the table instead
        if (node is not null)
        {
            refTable.ExternalNodes[filePath] = () => new Gbx<T>(node);
        }

        var reference = new NodeRef<T> { Descriptor = refTable.CreateFile(filePath, flags: 0, useFile: true) };

        if (node is not null)
        {
            reference.Resolve();
        }

        return reference;
    }
}
