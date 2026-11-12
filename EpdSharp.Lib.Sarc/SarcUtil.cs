namespace EpdSharp;

public static class SarcUtil
{
    public static uint Hash(ReadOnlySpan<byte> key, uint hashKey)
    {
        uint hash = 0;
        foreach (byte t in key) {
            hash = hash * hashKey + t;
        }

        return hash;
    }
    
    public static uint Hash(ReadOnlySpan<char> key, uint hashKey)
    {
        uint hash = 0;
        foreach (byte t in key) {
            hash = hash * hashKey + t;
        }

        return hash;
    }
}