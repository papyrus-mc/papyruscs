namespace Maploader.World
{
    public class Coordinate2D
    {
        public Coordinate2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        protected bool Equals(Coordinate2D other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coordinate2D) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(Coordinate2D left, Coordinate2D right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Coordinate2D left, Coordinate2D right)
        {
            return !Equals(left, right);
        }

        public int X { get; }
        public int Y { get; }
    }

    public class Coordinate
    {
        protected bool Equals(Coordinate other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coordinate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !Equals(left, right);
        }

        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            XZ = x * 256 + z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public int XZ { get; }

        public override string ToString()
        {
            return $"Block {X},{Y},{Z}";
        }

    }
}