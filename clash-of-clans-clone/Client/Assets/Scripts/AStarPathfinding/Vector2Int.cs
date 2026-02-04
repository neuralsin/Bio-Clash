namespace AStarPathfinding 
{
    public struct Vector2Int 
    {
        public int X;
        public int Y;

        public Vector2Int(int x, int y) 
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"[{X},{Y}]";
        public override bool Equals(object obj) => obj is Vector2Int other && this == other;
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
        public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;
    }
}