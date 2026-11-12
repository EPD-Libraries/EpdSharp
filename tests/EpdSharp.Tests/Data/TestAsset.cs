using EpdSharp.Tests.Types;

namespace EpdSharp.Tests.Data;

public static class TestAsset
{
    public static byte[] GetAsset<T>(this T testContainer, string fileName)
        where T : ITestContainer
    {
        return File.ReadAllBytes(testContainer.GetAssetPath(fileName));
    }

    public static FileStream GetAssetStream<T>(this T testContainer, string fileName)
        where T : ITestContainer
    {
        return File.OpenRead(testContainer.GetAssetPath(fileName));
    }

    private static string GetAssetPath<T>(this T _, string fileName)
        where T : ITestContainer
    {
        return Path.Combine("Assets", T.FolderName, fileName);
    }
}