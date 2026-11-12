using Entish.Attributes;

namespace EpdSharp.Data;

[Swappable]
public partial struct SfatEntry
{
    public uint FileNameHash;
    public int FileAttributes;
    public int DataStartOffset;
    public int DataEndOffset;

    public bool TryGetNameOffset(out int nameOffset)
    {
        nameOffset = (FileAttributes & 0xFFFF) * 4;
        return (byte)(FileAttributes >> 24) == 1;
    }
}
