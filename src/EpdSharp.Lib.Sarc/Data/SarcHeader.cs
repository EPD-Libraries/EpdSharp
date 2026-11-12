using Entish;
using Entish.Attributes;

namespace EpdSharp.Data;

[Swappable]
public partial struct SarcHeader
{
    public const uint ExpectedMagic = 0x43524153;

    [NeverSwap]
    public uint Magic;

    public ushort HeaderSize;

    [NeverSwap]
    public Endianness ByteOrderMark;

    public int FileSize;
    public int DataOffset;
    public ushort Version;
    
    private ushort _reserved1;
}