using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using Entish;
using EpdSharp.Data;

namespace EpdSharp;

public unsafe struct SarcView
{
    public SarcHeader Header;
    public SfatHeader SfatHeader;

    public ref SfatEntry this[ReadOnlySpan<char> name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Get(SarcUtil.Hash(name, SfatHeader.HashKey));
    }

    public ref SfatEntry this[ReadOnlySpan<byte> name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Get(SarcUtil.Hash(name, SfatHeader.HashKey));
    }

    public ref SfatEntry this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Get(index);
    }

    public Span<byte> GetData(ref SfatEntry entry)
        => Header.DataOffset + entry.DataEndOffset <= Header.FileSize
            ? new Span<byte>(GetDataPointer() + entry.DataStartOffset, entry.DataEndOffset - entry.DataStartOffset)
            : throw new ArgumentOutOfRangeException(nameof(entry));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? GetName(ref SfatEntry entry)
        => Utf8StringMarshaller.ConvertToManaged(GetNameUnmanaged(ref entry));

    public byte* GetNameUnmanaged(ref SfatEntry entry)
    {
        if (!entry.TryGetNameOffset(out int nameOffset)) {
            return null;
        }

        fixed (SarcView* ptr = &this) {
            var offset = sizeof(SarcView)
                         + SfatHeader.EntryCount * sizeof(SfatEntry)
                         + sizeof(SfntHeader) + nameOffset;

            if (offset >= Header.FileSize) {
                throw new ArgumentOutOfRangeException(nameof(entry));
            }

            return (byte*)ptr + offset;
        }
    }

    public static ref SarcView From(Span<byte> data)
    {
        fixed (byte* ptr = data) {
            return ref From((SarcView*)ptr, data.Length);
        }
    }

    public static ref SarcView From(SarcView* ptr, int bufferSize)
    {
        ref var sarc = ref Unsafe.AsRef<SarcView>(ptr);

        if (sizeof(SarcHeader) > bufferSize) {
            throw new InvalidDataException("Source buffer is too small");
        }

        if (sarc.Header.Magic != SarcHeader.ExpectedMagic) {
            throw new InvalidDataException("Invalid magic");
        }

        if (sarc.Header.ByteOrderMark is not (Endianness.Big or Endianness.Little)) {
            throw new InvalidDataException("Invalid byte-order mark");
        }

        bool swap = EndianUtils.ShouldSwap(sarc.Header.ByteOrderMark);

        if (swap) {
            SarcHeader.Swap(&ptr->Header);
            SfatHeader.Swap(&ptr->SfatHeader);
        }

        if (sarc.Header.FileSize > bufferSize) {
            throw new InvalidDataException("Source buffer is too small");
        }

        if (sarc.Header.Version != 0x100) {
            throw new InvalidDataException($"Unexpected SARC version: {sarc.Header.Version}");
        }

        if (sarc.SfatHeader.Magic != SfatHeader.ExpectedMagic) {
            throw new InvalidDataException("Invalid SFAT magic");
        }

        int sfntOffset = sizeof(SarcView) + sarc.SfatHeader.EntryCount * sizeof(SfatEntry);
        if (sfntOffset + sizeof(SfntHeader) > bufferSize) {
            throw new InvalidDataException("Corrupt SARC file (too many nodes)");
        }

        var sfntHeader = (SfntHeader*)((byte*)ptr + sfntOffset);
        if (sfntHeader->Magic != SfntHeader.ExpectedMagic) {
            throw new InvalidDataException("Invalid SFNT magic");
        }

        if (swap) {
            SfntHeader.Swap(sfntHeader);

            for (int i = 0; i < sarc.SfatHeader.EntryCount; i++) {
                SfatEntry.Swap(sarc.GetPointer(i));
            }
        }

        return ref sarc;
    }

    public ref SfatEntry Get(uint hash)
    {
        if (SfatHeader.EntryCount == 0) {
            return ref Unsafe.NullRef<SfatEntry>();
        }

        var nodes = GetPointer(index: 0);

        double l = 0;
        double r = SfatHeader.EntryCount - 1;
        while (l <= r) {
            int m = (int)Math.Floor((l + r) / 2);
            if (nodes[m].FileNameHash < hash) {
                l = m + 1;
            }
            else if (nodes[m].FileNameHash > hash) {
                r = m - 1;
            }
            else {
                return ref nodes[m];
            }
        }

        return ref Unsafe.NullRef<SfatEntry>();
    }

    private SfatEntry* GetPointer(int index) => (SfatEntry*)Unsafe.AsPointer(ref Get(index));

    public ref SfatEntry Get(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, SfatHeader.EntryCount);

        fixed (SarcView* header = &this) {
            return ref ((SfatEntry*)(byte*)(header + 1))[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte* GetDataPointer()
    {
        fixed (SarcView* ptr = &this) {
            return (byte*)ptr + Header.DataOffset;
        }
    }
}