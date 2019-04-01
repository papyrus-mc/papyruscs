using System;
using JetBrains.Annotations;

namespace Maploader.World
{
    public class BlockCoord
    {
        public BlockCoord(BlockData block, int x, int y, int z)
        {
            Block = block;
            X = x;
            Y = y;
            Z = z;
            XZ = x * 256 + z;
        }

        public BlockData Block { get; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public int XZ { get; }

        public override string ToString()
        {
            return $"{X} {Y} {Z} {Block}";
        }
    }

    public class BlockData
    {
        protected bool Equals(BlockData other)
        {
            return string.Equals(Id, other.Id, StringComparison.InvariantCulture) && Data == other.Data;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BlockData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.InvariantCulture.GetHashCode(Id) * 397) ^ Data.GetHashCode();
            }
        }

        public static bool operator ==(BlockData left, BlockData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BlockData left, BlockData right)
        {
            return !Equals(left, right);
        }

        public BlockData(string id, long data)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data;
        }

        [NotNull]
        public string Id { get; }
        public long Data { get; }

        public int Version { get; set; }

        public override string ToString()
        {
            return string.Format($"{Id}:{Data} ({Version})");
        }
    }
}