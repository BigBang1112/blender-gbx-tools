using BlenderGbxTools;
using GBX.NET;
using GBX.NET.LZO;

if (args.Length == 0)
{ 
    Console.WriteLine($"Usage: BlenderGbxTools{(OperatingSystem.IsWindows() ? ".exe" : "")} <path to Gbx file>");
    
    Environment.ExitCode = 1;
    return;
}

var filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine("File does not exist");
    Environment.ExitCode = 2;
    return;
}

Gbx.LZO = new MiniLZO();

var node = Gbx.ParseNode(filePath, new() { SafeSkippableChunks = true });

using var stream = Console.OpenStandardOutput();

GbxSerializer.Serialize(stream, Path.GetFileName(filePath), node);