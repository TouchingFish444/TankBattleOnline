using System.Drawing;

namespace TankBattleOnline
{
    public class MapBlock
    {
        public int GridX;
        public int GridY;
        public int X;
        public int Y;
        public int Size = 30;
        public BlockType Type;
        public bool Visible = true;

        public bool CanPass
        {
            get
            {
                return Type == BlockType.Empty || Type == BlockType.Grass;
            }
        }

        public bool CanDestroy
        {
            get
            {
                return Type == BlockType.Brick;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Size, Size);
            }
        }
    }
}
