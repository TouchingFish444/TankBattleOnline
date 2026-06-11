namespace TankBattleOnline
{
    public class PlayerInput
    {
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
        public bool Fire;

        public PlayerInput Clone()
        {
            return new PlayerInput
            {
                Up = Up,
                Down = Down,
                Left = Left,
                Right = Right,
                Fire = Fire
            };
        }

        public void ClearFire()
        {
            Fire = false;
        }

        public void Reset()
        {
            Up = false;
            Down = false;
            Left = false;
            Right = false;
            Fire = false;
        }

        public bool HasMove()
        {
            return Up || Down || Left || Right;
        }
    }
}
