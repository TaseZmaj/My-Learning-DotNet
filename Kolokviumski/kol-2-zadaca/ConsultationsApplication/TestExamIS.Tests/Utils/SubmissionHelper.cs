namespace TestExamIS.Tests.Utils;

using System.IO;
using System.IO.Compression;

public static class SubmissionHelper
{
    public static void ZipProject(string rootPath, string outputZip)
    {
        if (File.Exists(outputZip))
            File.Delete(outputZip);

        Console.WriteLine("Zipping the project...");

        using var zip = ZipFile.Open(outputZip, ZipArchiveMode.Create);
        foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories).Where(x =>
                     !x.EndsWith(".vsidx") && !x.EndsWith(".testlog") && !x.EndsWith(".manifest")))
        {
            if (IsIgnoredPath(file, rootPath)) continue;

            var relativePath = Path.GetRelativePath(rootPath, file);
            zip.CreateEntryFromFile(file, relativePath);
        }

        Console.WriteLine($"Zip created at: {outputZip}");
    }

    private static bool IsIgnoredPath(string filePath, string rootPath)
    {
        var ignoredDirs = new[] { "bin", "obj", ".git", "TestOutput", ".vs", "CopilotIndices" };

        var relativePath = Path.GetRelativePath(rootPath, filePath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar);

        return segments.Any(segment =>
            ignoredDirs.Any(ignore =>
                segment.Equals(ignore, StringComparison.OrdinalIgnoreCase)));
    }
}