using System.Drawing;

namespace TankBattleOnline
{
    public class Tank
    {
        public int PlayerId;
        public string PlayerName = "";
        public PlayerControlType ControlType = PlayerControlType.Computer;
        public int X;
        public int Y;
        public int Width = 60;
        public int Height = 60;
        public int CollisionInset = 3;
        public int Speed = 4;
        public int Hp = 5;
        public int MaxHp = 5;
        public int Wins = 0;
        public int Score = 0;
        public int UpgradeLevel = 0;
        public int TankColorArgb = PlayerSlot.GetDefaultTankColorArgb(1);
        public Direction Direction = Direction.Up;
        public bool Alive = true;
        public int FireCooldown = 0;
        public int RespawnTicks = 0;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }

        public Rectangle DrawBounds
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }

        public Rectangle CollisionBounds
        {
            get
            {
                return new Rectangle(
                    X + CollisionInset,
                    Y + CollisionInset,
                    Width - CollisionInset * 2,
                    Height - CollisionInset * 2);
            }
        }

        public int CenterX
        {
            get
            {
                return X + Width / 2;
            }
        }

        public int CenterY
        {
            get
            {
                return Y + Height / 2;
            }
        }
    }
}
