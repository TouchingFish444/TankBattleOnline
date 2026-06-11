using System.Drawing;

namespace TankBattleOnline
{
    public class Bullet
    {
        public int OwnerId;
        public int X;
        public int Y;
        public int Width = 12;
        public int Height = 12;
        public int Speed = 10;
        public int LastX;
        public int LastY;
        public int PowerLevel = 0;
        public Direction Direction;
        public bool Alive = true;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }

        public Rectangle PathBounds
        {
            get
            {
                int left = System.Math.Min(X, LastX);
                int top = System.Math.Min(Y, LastY);
                int right = System.Math.Max(X + Width, LastX + Width);
                int bottom = System.Math.Max(Y + Height, LastY + Height);
                return new Rectangle(left, top, right - left, bottom - top);
            }
        }
    }
}
