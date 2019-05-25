using System;

namespace PapyrusCs.Database
{

    public class LevelDbWorldKey3
    {
        protected bool Equals(LevelDbWorldKey3 other)
        {
            return KeyType == other.KeyType && X == other.X && Y == other.Y && SubChunkId == other.SubChunkId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LevelDbWorldKey3) obj);
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

        public static bool operator ==(LevelDbWorldKey3 left, LevelDbWorldKey3 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LevelDbWorldKey3 left, LevelDbWorldKey3 right)
        {
            return !Equals(left, right);
        }

        public LevelDbWorldKey3(byte[] key)
        {
            X = (uint)(key[0] | (key[1] << 8) | (key[2] << 16) | (key[3] << 24));
            Y = (uint)(key[4] | (key[5] << 8) | (key[6] << 16) | (key[7] << 24));
            KeyType = key[8];
            SubChunkId = key[9];
        }

        public byte KeyType { get; }
        public UInt32 X { get; }
        public UInt32 Y { get; }
        public byte SubChunkId { get; }
    }
}