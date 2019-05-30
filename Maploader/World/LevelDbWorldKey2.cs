using System;
using System.ComponentModel;
using System.Diagnostics;
using Maploader.World;

namespace PapyrusCs.Database
{
    public struct LevelDbWorldKey2
    {
        public byte[] Key { get; }

        public bool Equals(LevelDbWorldKey2 other)
        {
            return (SubChunkId == 0xFF || other.SubChunkId == 0xFF)
                ? KeyType == other.KeyType && X == other.X && Z == other.Z
                : KeyType == other.KeyType && X == other.X && Z == other.Z && SubChunkId == other.SubChunkId;
        }

        public override bool Equals(object obj)
        {
            return obj is LevelDbWorldKey2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = KeyType.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) X;
                hashCode = (hashCode * 397) ^ (int) Z;
                //hashCode = (hashCode * 397) ^ SubChunkId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(LevelDbWorldKey2 left, LevelDbWorldKey2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LevelDbWorldKey2 left, LevelDbWorldKey2 right)
        {
            return !left.Equals(right);
        }

        public LevelDbWorldKey2(byte[] key)
        {
            Key = key;
            X = (key[0] | (key[1] << 8) | (key[2] << 16) | (key[3] << 24));
            Z = (key[4] | (key[5] << 8) | (key[6] << 16) | (key[7] << 24));
            KeyType = key[8];
            SubChunkId = key[9];
        }

        public LevelDbWorldKey2(int x, int z)
        {
            Key = new byte[] {0};
            X = x;
            Z = z;
            KeyType = 47;
            SubChunkId = 0xFF;
        }

        public byte KeyType { get; }
        public Int32 X { get; }
        public Int32 Z { get; }
        public UInt64 XZ => (((UInt64) X) << 32) | (UInt32) Z;
        public byte SubChunkId { get; }


        public UInt64 GetXZGroup(int chunkPerDimension)
        {
            UInt64 result = ((UInt64) CoordHelpers.GetGroupedCoordinate(this.X, chunkPerDimension)) << 32;
            result |= (UInt32) CoordHelpers.GetGroupedCoordinate(this.Z, chunkPerDimension);

            return result;
        }
    }
}