using System;
using System.Collections.Generic;

namespace TankBattleOnline
{
    public class GameState
    {
        public RoomConfig Config = new RoomConfig();
        public List<Tank> Tanks = new List<Tank>();
        public List<Bullet> Bullets = new List<Bullet>();
        public List<MapBlock> Blocks = new List<MapBlock>();
        public List<PowerUp> PowerUps = new List<PowerUp>();
        public int RoundNumber = 0;
        public int TotalRounds = 3;
        public int RemainingTicks = 0;
        public bool RoundOver = false;
        public bool MatchOver = false;
        public List<int> RoundWinnerIds = new List<int>();
        public List<int> MatchWinnerIds = new List<int>();
        public string ResultText = "";
        public int TickCount = 0;
        public int FireSoundCount = 0;

        public void ResetMatch(RoomConfig config)
        {
            Config = config.Clone();
            TotalRounds = Config.TotalRounds;
            RoundNumber = 0;
            TickCount = 0;
            Tanks.Clear();
            ResetRoundTransient();
        }

        public void ResetRoundTransient()
        {
            RoundOver = false;
            MatchOver = false;
            RoundWinnerIds.Clear();
            MatchWinnerIds.Clear();
            ResultText = "";
            FireSoundCount = 0;
            Bullets.Clear();
            PowerUps.Clear();
            TickCount = 0;
        }

        public void ClearFrameEvents()
        {
            FireSoundCount = 0;
        }

        public int MapWidth
        {
            get
            {
                return Config.MapColumns * Config.BlockSize;
            }
        }

        public int MapHeight
        {
            get
            {
                return Config.MapRows * Config.BlockSize;
            }
        }

        public int RemainingSeconds
        {
            get
            {
                return Math.Max(0, (int)Math.Ceiling(RemainingTicks * Config.TickMilliseconds / 1000.0));
            }
        }

        public Tank GetTank(int playerId)
        {
            foreach (Tank tank in Tanks)
            {
                if (tank.PlayerId == playerId)
                {
                    return tank;
                }
            }

            return null;
        }
    }
}
