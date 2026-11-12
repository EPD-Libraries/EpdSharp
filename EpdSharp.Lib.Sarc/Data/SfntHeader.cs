using Entish.Attributes;

namespace EpdSharp.Data;

[Swappable]
public partial struct SfntHeader
{
    public const uint ExpectedMagic = 0x544E4653;
    
    [NeverSwap]
    public uint Magic;
    public ushort HeaderSize;
}
