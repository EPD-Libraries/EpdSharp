using System.Runtime.CompilerServices;
using AwesomeAssertions;
using EpdSharp.Data;
using EpdSharp.Tests.Data;
using EpdSharp.Tests.Types;

namespace EpdSharp.Tests;

public class SarcViewTests : ITestContainer
{
    public static string FolderName => "sarc";

    public const string Asset_WorkingBigEndian = "Working-BigEndian.sarc";
    public const string Asset_WorkingLittleEndian = "Working-LittleEndian.sarc";

    [Fact]
    public void Read_ShouldBe_Valid()
    {
        var data = this.GetAsset(Asset_WorkingLittleEndian);
        ref var sarc = ref SarcView.From(data);

        sarc.SfatHeader.EntryCount.Should().Be(2);
    }

    [Fact]
    public void Swap_ShouldBe_Valid()
    {
        var data = this.GetAsset(Asset_WorkingBigEndian);
        ref var sarc = ref SarcView.From(data);

        sarc.SfatHeader.EntryCount.Should().Be(2);
    }

    [Fact]
    public void ReadEmpty_ShouldThrow_BufferTooSmall()
    {
        new Action(() => SarcView.From([]))
            .Should()
            .Throw<InvalidDataException>()
            .WithMessage("Source buffer is too small");
    }

    [Fact]
    public void ReadInvalidMagic_ShouldThrow_InvalidMagic()
    {
        var data = this.GetAsset(Asset_WorkingLittleEndian);
        data[0] = 0x00;

        new Action(() => SarcView.From(data))
            .Should()
            .Throw<InvalidDataException>()
            .WithMessage("Invalid magic");
    }

    [Fact]
    public void ReadInvalidByteOrderMark_ShouldThrow_InvalidByteOrderMark()
    {
        var data = this.GetAsset(Asset_WorkingLittleEndian);
        data[6] = 0x00;

        new Action(() => SarcView.From(data))
            .Should()
            .Throw<InvalidDataException>()
            .WithMessage("Invalid byte-order mark");
    }

    [Fact]
    public void GetEntry_Should_ReturnCorrectEntry()
    {
        ref var sarc = ref SarcView.From(this.GetAsset(Asset_WorkingLittleEndian));

        {
            const string fileName = "Test.txt";
            ref var entry = ref sarc[fileName];
            
            var data = sarc.GetData(ref entry);
            data.ToArray().Should().Equal([.. "Test Content"u8]);

            sarc.GetName(ref entry).Should().Be(fileName);
        }

        {
            const string fileName = "Test/Test Inner.txt";
            ref var entry = ref sarc[fileName];
            
            var data = sarc.GetData(ref entry);
            data.ToArray().Should().Equal([.. "Test Content (Inner)"u8]);
            
            sarc.GetName(ref entry).Should().Be(fileName);
        }
    }

    [Fact]
    public void GetMissingEntry_Should_ReturnNull()
    {
        ref var sarc = ref SarcView.From(this.GetAsset(Asset_WorkingLittleEndian));
        ref var entry = ref sarc["?"];
        Unsafe.IsNullRef(ref entry).Should().BeTrue();
    }

    [Fact]
    public void GetEntryOutOfRange_ShouldThrow_ArgumentOutOfRangeException()
    {
        new Action(() => _ = ref SarcView.From(this.GetAsset(Asset_WorkingLittleEndian))[int.MaxValue])
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetOverflowData_ShouldThrow_ArgumentOutOfRangeException()
    {
        var sarc = new SarcView {
            Header = new SarcHeader {
                FileSize = Unsafe.SizeOf<SarcView>(),
                DataOffset = Unsafe.SizeOf<SarcView>()
            }
        };
        
        new Action(() => {
                var fakeEntry = new SfatEntry { DataStartOffset = 0x0, DataEndOffset = 0x8 };
                sarc.GetData(ref fakeEntry);
            })
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetOverflowName_ShouldThrow_ArgumentOutOfRangeException()
    {
        var sarc = new SarcView {
            Header = new SarcHeader {
                FileSize = Unsafe.SizeOf<SarcView>() + Unsafe.SizeOf<SfntHeader>()
            }
        };
        
        new Action(() => {
                var fakeEntry = new SfatEntry { FileAttributes = 0x1000000 };
                sarc.GetName(ref fakeEntry);
            })
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
}