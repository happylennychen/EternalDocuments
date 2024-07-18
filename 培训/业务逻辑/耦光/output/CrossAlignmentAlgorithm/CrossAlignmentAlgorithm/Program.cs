using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossAlignmentAlgorithm
{
    public enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            int range = 20, step = 1, threshold = -40;
            if (!ScanAlignment(range, step, threshold))
                Console.WriteLine("Scan Alignment Failed!");
            else
                Console.WriteLine("Scan Alignment Succeeded!");
            Console.ReadLine();
        }

        private static bool ScanAlignment(int range, int step, int threshold)
        {
            Position2D p1 = new Position2D(0, 0);
            Position2D p2 = new Position2D(0, 0);
            Console.WriteLine($"{p1.X},{p1.Y}");
            Direction dir1 = Direction.Left;
            Direction dir2;
            byte changeDirCount = 0;
            for (int i = 1; i < range;)
            {
                Move(step, dir1);
                p1.Update(step, dir1);
                Console.WriteLine($"Move {dir1} to {p1.X},{p1.Y}");
                dir2 = UpdateDir(p1, p2, dir1, i);
                if (dir2 != dir1)
                {
                    changeDirCount++;
                    p2.X = p1.X;
                    p2.Y = p1.Y;
                }
                if (changeDirCount == 2)
                {
                    changeDirCount = 0;
                    i++;
                }
                dir1 = dir2;
                //Console.WriteLine(dir1);
                if(Measure()>threshold)
                    return true;
                //Thread.Sleep(1000);
            }
            return false;
        }

        private static int Measure()
        {
            return -80;
        }

        private static Direction UpdateDir(Position2D pCurr, Position2D pOrig, Direction dir, int boundary)
        {
            int xDis = Math.Abs(pCurr.X - pOrig.X);
            int yDis = Math.Abs(pCurr.Y - pOrig.Y);
            switch (dir)
            {
                case Direction.Left:
                    if (boundary > xDis)
                        return dir;
                    else if (boundary == xDis)
                        return Direction.Up;
                    else
                    {
                        Console.WriteLine("Boundary Error!");
                        return dir;
                    }
                case Direction.Up:
                    if (boundary > yDis)
                        return dir;
                    else if (boundary == yDis)
                        return Direction.Right;
                    else
                    {
                        Console.WriteLine("Boundary Error!");
                        return dir;
                    }
                case Direction.Right:
                    if (boundary > xDis)
                        return dir;
                    else if (boundary == xDis)
                        return Direction.Down;
                    else
                    {
                        Console.WriteLine("Boundary Error!");
                        return dir;
                    }
                case Direction.Down:
                    if (boundary > yDis)
                        return dir;
                    else if (boundary == yDis)
                        return Direction.Left;
                    else
                    {
                        Console.WriteLine("Boundary Error!");
                        return dir;
                    }
                default: return dir;
            }
        }

        private static void Move(int step, Direction dir)
        {
            //throw new NotImplementedException();
        }
    }
}
