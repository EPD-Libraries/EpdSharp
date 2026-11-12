using Entish.Attributes;

namespace EpdSharp.Data;

[Swappable]
public partial struct SfatHeader
{
    public const uint ExpectedMagic = 0x54414653;
    
    [NeverSwap]
    public uint Magic;
    public ushort HeaderSize;
    public ushort EntryCount;
    public uint HashKey;
}
