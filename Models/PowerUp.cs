using System.Drawing;

namespace TankBattleOnline
{
    public class PowerUp
    {
        public int X;
        public int Y;
        public int Size = 40;
        public bool Visible = true;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Size, Size);
            }
        }
    }
}
