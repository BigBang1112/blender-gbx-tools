using System.Runtime.InteropServices;

namespace BlenderGbxTools.Tests.Helpers;

internal static class Buffers
{
    public static float[] ToFloats(byte[]? bytes)
    {
        return bytes is null ? [] : MemoryMarshal.Cast<byte, float>(bytes).ToArray();
    }
}
