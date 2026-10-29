using GBX.NET.Components;

namespace BlenderGbxTools.Extensions;

internal static class GbxRefTableFileExtensions
{
    public static string GetFileNameOrUniqueName(this GbxRefTableFile? file)
    {
        return file is null ? Guid.NewGuid().ToString() : Path.GetFileName(file.GetFullPath());
    }
}
