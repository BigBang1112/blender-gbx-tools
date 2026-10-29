using System.Runtime.InteropServices;

namespace BlenderGbxTools.Tests.Helpers;

internal static class Buffers
{
    /// <summary>
    /// Reads the raw float buffers sent to the Blender plugin back as floats.
    /// </summary>
    public static float[] ToFloats(byte[]? bytes)
    {
        return bytes is null ? [] : MemoryMarshal.Cast<byte, float>(bytes).ToArray();
    }
}
