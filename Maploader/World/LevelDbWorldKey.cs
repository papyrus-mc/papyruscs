using System;

namespace PapyrusCs.Database
{
    public struct LevelDbWorldKey
    {
        public bool Equals(LevelDbWorldKey other)
        {
            return KeyType == other.KeyType && X == other.X && Y == other.Y && SubChunkId == other.SubChunkId;
        }

        public override bool Equals(object obj)
        {
            return obj is LevelDbWorldKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = KeyType.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) X;
                hashCode = (hashCode * 397) ^ (int) Y;
                hashCode = (hashCode * 397) ^ SubChunkId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(LevelDbWorldKey left, LevelDbWorldKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LevelDbWorldKey left, LevelDbWorldKey right)
        {
            return !left.Equals(right);
        }

        public LevelDbWorldKey(byte[] key)
        {
            X = BitConverter.ToUInt32(key, 0);
            Y = BitConverter.ToUInt32(key, 4);
            KeyType = key[8];
            SubChunkId = key[9];
        }

        public byte KeyType { get; }
        public UInt32 X { get; }
        public UInt32 Y { get; }
        public byte SubChunkId { get; }
    }
}