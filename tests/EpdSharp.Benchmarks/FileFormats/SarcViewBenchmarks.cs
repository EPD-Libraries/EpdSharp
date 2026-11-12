using BenchmarkDotNet.Attributes;
using EpdSharp.Benchmarks.Data;
using EpdSharp.Benchmarks.Types;
using Revrs;
using SarcLibrary;

namespace EpdSharp.Benchmarks.FileFormats;

[MemoryDiagnoser]
public class SarcViewBenchmarks : IBenchmarkContainer
{
    public static string FolderName => "sarc";
    
    // To avoid uploading game files these are used by default
    // Replace them with larger files for a more accurate benchmark
    public const string Asset_WorkingBigEndian = "Working-BigEndian.sarc";
    public const string Asset_WorkingLittleEndian = "Working-LittleEndian.sarc";

    private byte[] _sarcSwap = [];
    private byte[] _sarcNative = [];
    
    [IterationSetup]
    public void Setup()
    {
        if (BitConverter.IsLittleEndian) {
            _sarcSwap = this.GetAsset(Asset_WorkingBigEndian);
            _sarcNative = this.GetAsset(Asset_WorkingLittleEndian);
        }
        else {
            _sarcSwap = this.GetAsset(Asset_WorkingLittleEndian);
            _sarcNative = this.GetAsset(Asset_WorkingBigEndian);
        }
    }
    
    [Benchmark]
    public void SarcViewNative()
    {
        SarcView.From(_sarcNative);
    }
    
    [Benchmark]
    public void SarcViewSwap()
    {
        SarcView.From(_sarcSwap);
    }
    
    [Benchmark]
    public void LegacyImmutableSarcNative()
    {
        var reader = new RevrsReader(_sarcNative);
        _ = new ImmutableSarc(ref reader);
    }
    
    [Benchmark]
    public void LegacyImmutableSarcSwap()
    {
        var reader = new RevrsReader(_sarcSwap);
        _ = new ImmutableSarc(ref reader);
    }
}