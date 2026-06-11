namespace TankBattleOnline
{
    public class PlayerSlot
    {
        private static readonly int[] DefaultTankColorArgbValues =
        {
            unchecked((int)0xFFE8B840),
            unchecked((int)0xFF49A8D8),
            unchecked((int)0xFF60B065),
            unchecked((int)0xFFCA6052),
            unchecked((int)0xFFA570C8),
            unchecked((int)0xFFDC8C50),
            unchecked((int)0xFFE6E6E6),
            unchecked((int)0xFF5C7CE2)
        };

        public int PlayerId;
        public PlayerControlType ControlType = PlayerControlType.Computer;
        public int Wins = 0;
        public int Score = 0;
        public int TankColorArgb = GetDefaultTankColorArgb(1);
        public string OwnerToken = "";
        public int LatencyMs = -1;

        public PlayerSlot()
        {
        }

        public PlayerSlot(int playerId, PlayerControlType controlType)
        {
            PlayerId = playerId;
            ControlType = controlType;
            TankColorArgb = GetDefaultTankColorArgb(playerId);
        }

        public string PlayerName
        {
            get
            {
                return "P" + PlayerId.ToString();
            }
        }

        public PlayerSlot Clone()
        {
            return new PlayerSlot
            {
                PlayerId = PlayerId,
                ControlType = ControlType,
                Wins = Wins,
                Score = Score,
                TankColorArgb = TankColorArgb,
                OwnerToken = OwnerToken,
                LatencyMs = LatencyMs
            };
        }

        public void AssignOwner(string ownerToken)
        {
            OwnerToken = ownerToken ?? "";
        }

        public void ClearOwner()
        {
            OwnerToken = "";
            LatencyMs = -1;
        }

        public void SetLatency(int latencyMs)
        {
            LatencyMs = latencyMs;
        }

        public static int GetDefaultTankColorArgb(int playerId)
        {
            if (playerId <= 0)
            {
                playerId = 1;
            }

            return DefaultTankColorArgbValues[(playerId - 1) % DefaultTankColorArgbValues.Length];
        }

        public static int[] GetDefaultTankColorValues()
        {
            return (int[])DefaultTankColorArgbValues.Clone();
        }

        public override string ToString()
        {
            string typeText = "人机";

            if (ControlType == PlayerControlType.HumanLocal)
            {
                typeText = "本机玩家";
            }
            else if (ControlType == PlayerControlType.HumanRemote)
            {
                typeText = "联机玩家";
            }

            return PlayerName + "    " + typeText;
        }
    }
}
