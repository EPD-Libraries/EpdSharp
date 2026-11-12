using EpdSharp.Benchmarks.Types;

namespace EpdSharp.Benchmarks.Data;

public static class BenchmarkAsset
{
    public static byte[] GetAsset<T>(this T testContainer, string fileName)
        where T : IBenchmarkContainer
    {
        return File.ReadAllBytes(testContainer.GetAssetPath(fileName));
    }

    public static FileStream GetAssetStream<T>(this T testContainer, string fileName)
        where T : IBenchmarkContainer
    {
        return File.OpenRead(testContainer.GetAssetPath(fileName));
    }

    private static string GetAssetPath<T>(this T _, string fileName)
        where T : IBenchmarkContainer
    {
        return Path.Combine("Assets", T.FolderName, fileName);
    }
}