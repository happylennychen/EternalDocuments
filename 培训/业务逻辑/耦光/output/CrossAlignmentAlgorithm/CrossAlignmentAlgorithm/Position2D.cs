using System;

namespace CrossAlignmentAlgorithm
{
    internal class Position2D
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position2D(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        internal void Update(int dis, Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: Y += dis; break;
                case Direction.Down: Y -= dis; break;
                case Direction.Left: X -= dis; break;
                case Direction.Right: X += dis; break;
            }
        }
    }
}