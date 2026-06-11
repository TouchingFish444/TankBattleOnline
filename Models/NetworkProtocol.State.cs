using System;
using System.Collections.Generic;
using System.Text;

namespace TankBattleOnline
{
    public static partial class NetworkProtocol
    {
        public static string CreateStateMessage(GameState state)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(StatePrefix);
            AppendVersion(builder);
            builder.Append("ROOM=").Append(Uri.EscapeDataString(state.Config.RoomName)).Append(";");
            builder.Append("ROUND=").Append(state.RoundNumber).Append(";");
            builder.Append("TOTAL=").Append(state.TotalRounds).Append(";");
            builder.Append("TIME=").Append(state.RemainingTicks).Append(";");
            builder.Append("TICKMS=").Append(state.Config.TickMilliseconds).Append(";");
            builder.Append("HP=").Append(state.Config.InitialHp).Append(";");
            builder.Append("COLS=").Append(state.Config.MapColumns).Append(";");
            builder.Append("ROWS=").Append(state.Config.MapRows).Append(";");
            builder.Append("BS=").Append(state.Config.BlockSize).Append(";");
            builder.Append("RO=").Append(state.RoundOver ? "1" : "0").Append(";");
            builder.Append("MO=").Append(state.MatchOver ? "1" : "0").Append(";");
            builder.Append("RESULT=").Append(Uri.EscapeDataString(state.ResultText)).Append(";");
            builder.Append("RW=").Append(JoinIds(state.RoundWinnerIds)).Append(";");
            builder.Append("MW=").Append(JoinIds(state.MatchWinnerIds)).Append(";");
            builder.Append("T=").Append(SerializeTanks(state.Tanks)).Append(";");
            builder.Append("B=").Append(SerializeBullets(state.Bullets)).Append(";");
            builder.Append("PU=").Append(SerializePowerUps(state.PowerUps)).Append(";");
            builder.Append("M=").Append(SerializeBlocks(state.Blocks));
            return builder.ToString();
        }

        public static string CreateFrameStateMessage(GameState state)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(FramePrefix);
            AppendVersion(builder);
            builder.Append("ROUND=").Append(state.RoundNumber).Append(";");
            builder.Append("TOTAL=").Append(state.TotalRounds).Append(";");
            builder.Append("TIME=").Append(state.RemainingTicks).Append(";");
            builder.Append("RO=").Append(state.RoundOver ? "1" : "0").Append(";");
            builder.Append("MO=").Append(state.MatchOver ? "1" : "0").Append(";");
            builder.Append("RESULT=").Append(Uri.EscapeDataString(state.ResultText)).Append(";");
            builder.Append("RW=").Append(JoinIds(state.RoundWinnerIds)).Append(";");
            builder.Append("MW=").Append(JoinIds(state.MatchWinnerIds)).Append(";");
            builder.Append("T=").Append(SerializeTanks(state.Tanks)).Append(";");
            builder.Append("B=").Append(SerializeBullets(state.Bullets)).Append(";");
            builder.Append("PU=").Append(SerializePowerUps(state.PowerUps)).Append(";");
            builder.Append("BV=").Append(SerializeBlockVisibility(state.Blocks));
            return builder.ToString();
        }

        public static GameState ParseStateMessage(string message)
        {
            if (message == null || !message.StartsWith(StatePrefix))
            {
                return null;
            }

            try
            {
                Dictionary<string, string> values = ParsePairs(message.Substring(StatePrefix.Length));
                GameState state = new GameState();
                state.Config.RoomName = Uri.UnescapeDataString(GetValue(values, "ROOM", "TankBattleOnline"));
                state.RoundNumber = ToInt(GetValue(values, "ROUND", "1"), 1);
                state.TotalRounds = ToInt(GetValue(values, "TOTAL", "3"), 3);
                state.RemainingTicks = ToInt(GetValue(values, "TIME", "0"), 0);
                state.Config.TickMilliseconds = ToInt(GetValue(values, "TICKMS", "16"), 16);
                state.Config.InitialHp = ToInt(GetValue(values, "HP", "5"), 5);
                state.Config.MapColumns = ToInt(GetValue(values, "COLS", "40"), 40);
                state.Config.MapRows = ToInt(GetValue(values, "ROWS", "24"), 24);
                state.Config.BlockSize = ToInt(GetValue(values, "BS", "30"), 30);
                state.RoundOver = GetValue(values, "RO", "0") == "1";
                state.MatchOver = GetValue(values, "MO", "0") == "1";
                state.ResultText = Uri.UnescapeDataString(GetValue(values, "RESULT", ""));
                state.RoundWinnerIds = ParseIds(GetValue(values, "RW", ""));
                state.MatchWinnerIds = ParseIds(GetValue(values, "MW", ""));
                state.Tanks = ParseTanks(GetValue(values, "T", ""), state.Config);
                state.Bullets = ParseBullets(GetValue(values, "B", ""));
                state.PowerUps = ParsePowerUps(GetValue(values, "PU", ""));
                state.Blocks = ParseBlocks(GetValue(values, "M", ""));
                return state;
            }
            catch
            {
                ReportParseFailure(StateMessageType, "STATE 解析失败");
                return null;
            }
        }

        public static bool TryApplyFrameStateMessage(string message, GameState state)
        {
            if (message == null || state == null || !message.StartsWith(FramePrefix))
            {
                return false;
            }

            try
            {
                Dictionary<string, string> values = ParsePairs(message.Substring(FramePrefix.Length));
                state.RoundNumber = ToInt(GetValue(values, "ROUND", state.RoundNumber.ToString()), state.RoundNumber);
                state.TotalRounds = ToInt(GetValue(values, "TOTAL", state.TotalRounds.ToString()), state.TotalRounds);
                state.RemainingTicks = ToInt(GetValue(values, "TIME", state.RemainingTicks.ToString()), state.RemainingTicks);
                state.RoundOver = GetValue(values, "RO", state.RoundOver ? "1" : "0") == "1";
                state.MatchOver = GetValue(values, "MO", state.MatchOver ? "1" : "0") == "1";
                state.ResultText = Uri.UnescapeDataString(GetValue(values, "RESULT", state.ResultText));
                state.RoundWinnerIds = ParseIds(GetValue(values, "RW", JoinIds(state.RoundWinnerIds)));
                state.MatchWinnerIds = ParseIds(GetValue(values, "MW", JoinIds(state.MatchWinnerIds)));
                state.Tanks = ParseTanks(GetValue(values, "T", ""), state.Config);
                state.Bullets = ParseBullets(GetValue(values, "B", ""));
                state.PowerUps = ParsePowerUps(GetValue(values, "PU", ""));
                ApplyBlockVisibility(GetValue(values, "BV", ""), state.Blocks);
                return true;
            }
            catch
            {
                ReportParseFailure(FrameMessageType, "FRAME 解析失败");
                return false;
            }
        }

        private static string SerializeTanks(List<Tank> tanks)
        {
            List<string> parts = new List<string>();

            foreach (Tank tank in tanks)
            {
                parts.Add(tank.PlayerId.ToString()
                    + "," + tank.X.ToString()
                    + "," + tank.Y.ToString()
                    + "," + tank.Direction.ToString()
                    + "," + tank.Hp.ToString()
                    + "," + tank.MaxHp.ToString()
                    + "," + (tank.Alive ? "1" : "0")
                    + "," + tank.RespawnTicks.ToString()
                    + "," + tank.FireCooldown.ToString()
                    + "," + tank.Wins.ToString()
                    + "," + tank.ControlType.ToString()
                    + "," + tank.UpgradeLevel.ToString()
                    + "," + tank.TankColorArgb.ToString()
                    + "," + tank.Score.ToString());
            }

            return string.Join("/", parts.ToArray());
        }

        private static List<Tank> ParseTanks(string text, RoomConfig config)
        {
            List<Tank> tanks = new List<Tank>();

            if (string.IsNullOrEmpty(text))
            {
                return tanks;
            }

            string[] tankParts = text.Split('/');

            foreach (string item in tankParts)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                string[] parts = item.Split(',');

                if (parts.Length < 11)
                {
                    continue;
                }

                Tank tank = new Tank();
                tank.PlayerId = ToInt(parts[0], 0);
                tank.PlayerName = "P" + tank.PlayerId.ToString();
                tank.X = ToInt(parts[1], 0);
                tank.Y = ToInt(parts[2], 0);
                tank.Direction = (Direction)Enum.Parse(typeof(Direction), parts[3]);
                tank.Hp = ToInt(parts[4], config.InitialHp);
                tank.MaxHp = ToInt(parts[5], config.InitialHp);
                tank.Alive = parts[6] == "1";
                tank.RespawnTicks = ToInt(parts[7], 0);
                tank.FireCooldown = ToInt(parts[8], 0);
                tank.Wins = ToInt(parts[9], 0);
                tank.ControlType = (PlayerControlType)Enum.Parse(typeof(PlayerControlType), parts[10]);
                tank.UpgradeLevel = parts.Length >= 12 ? ToInt(parts[11], 0) : 0;
                tank.TankColorArgb = parts.Length >= 13 ? ToInt(parts[12], PlayerSlot.GetDefaultTankColorArgb(tank.PlayerId)) : PlayerSlot.GetDefaultTankColorArgb(tank.PlayerId);
                tank.Score = parts.Length >= 14 ? ToInt(parts[13], 0) : 0;
                tank.Width = config.BlockSize * 2;
                tank.Height = config.BlockSize * 2;
                tank.CollisionInset = Math.Max(3, config.BlockSize / 10);
                tank.Speed = Math.Max(4, config.BlockSize / 7);
                tanks.Add(tank);
            }

            return tanks;
        }

        private static string SerializeBullets(List<Bullet> bullets)
        {
            List<string> parts = new List<string>();

            foreach (Bullet bullet in bullets)
            {
                if (!bullet.Alive)
                {
                    continue;
                }

                parts.Add(bullet.OwnerId.ToString()
                    + "," + bullet.X.ToString()
                    + "," + bullet.Y.ToString()
                    + "," + bullet.Direction.ToString()
                    + "," + bullet.Width.ToString()
                    + "," + bullet.Height.ToString()
                    + "," + bullet.Speed.ToString()
                    + "," + bullet.PowerLevel.ToString());
            }

            return string.Join("/", parts.ToArray());
        }

        private static List<Bullet> ParseBullets(string text)
        {
            List<Bullet> bullets = new List<Bullet>();

            if (string.IsNullOrEmpty(text))
            {
                return bullets;
            }

            string[] bulletParts = text.Split('/');

            foreach (string item in bulletParts)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                string[] parts = item.Split(',');

                if (parts.Length < 7)
                {
                    continue;
                }

                Bullet bullet = new Bullet();
                bullet.OwnerId = ToInt(parts[0], 0);
                bullet.X = ToInt(parts[1], 0);
                bullet.Y = ToInt(parts[2], 0);
                bullet.Direction = (Direction)Enum.Parse(typeof(Direction), parts[3]);
                bullet.Width = ToInt(parts[4], 12);
                bullet.Height = ToInt(parts[5], 12);
                bullet.Speed = ToInt(parts[6], 10);
                bullet.PowerLevel = parts.Length >= 8 ? ToInt(parts[7], 0) : 0;
                bullet.LastX = bullet.X;
                bullet.LastY = bullet.Y;
                bullet.Alive = true;
                bullets.Add(bullet);
            }

            return bullets;
        }

        private static string SerializePowerUps(List<PowerUp> powerUps)
        {
            List<string> parts = new List<string>();

            foreach (PowerUp powerUp in powerUps)
            {
                if (!powerUp.Visible)
                {
                    continue;
                }

                parts.Add(powerUp.X.ToString()
                    + "," + powerUp.Y.ToString()
                    + "," + powerUp.Size.ToString());
            }

            return string.Join("/", parts.ToArray());
        }

        private static List<PowerUp> ParsePowerUps(string text)
        {
            List<PowerUp> powerUps = new List<PowerUp>();

            if (string.IsNullOrEmpty(text))
            {
                return powerUps;
            }

            string[] powerUpParts = text.Split('/');

            foreach (string item in powerUpParts)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                string[] parts = item.Split(',');

                if (parts.Length < 3)
                {
                    continue;
                }

                PowerUp powerUp = new PowerUp();
                powerUp.X = ToInt(parts[0], 0);
                powerUp.Y = ToInt(parts[1], 0);
                powerUp.Size = ToInt(parts[2], 40);
                powerUp.Visible = true;
                powerUps.Add(powerUp);
            }

            return powerUps;
        }

        private static string SerializeBlocks(List<MapBlock> blocks)
        {
            List<string> parts = new List<string>();

            foreach (MapBlock block in blocks)
            {
                parts.Add(block.GridX.ToString()
                    + "," + block.GridY.ToString()
                    + "," + block.X.ToString()
                    + "," + block.Y.ToString()
                    + "," + block.Size.ToString()
                    + "," + block.Type.ToString()
                    + "," + (block.Visible ? "1" : "0"));
            }

            return string.Join("/", parts.ToArray());
        }

        private static string SerializeBlockVisibility(List<MapBlock> blocks)
        {
            StringBuilder builder = new StringBuilder();

            foreach (MapBlock block in blocks)
            {
                builder.Append(block.Visible ? "1" : "0");
            }

            return builder.ToString();
        }

        private static void ApplyBlockVisibility(string text, List<MapBlock> blocks)
        {
            if (string.IsNullOrEmpty(text) || blocks == null || text.Length != blocks.Count)
            {
                return;
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].Visible = text[i] == '1';
            }
        }

        private static List<MapBlock> ParseBlocks(string text)
        {
            List<MapBlock> blocks = new List<MapBlock>();

            if (string.IsNullOrEmpty(text))
            {
                return blocks;
            }

            string[] blockParts = text.Split('/');

            foreach (string item in blockParts)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                string[] parts = item.Split(',');

                if (parts.Length < 7)
                {
                    continue;
                }

                MapBlock block = new MapBlock();
                block.GridX = ToInt(parts[0], 0);
                block.GridY = ToInt(parts[1], 0);
                block.X = ToInt(parts[2], 0);
                block.Y = ToInt(parts[3], 0);
                block.Size = ToInt(parts[4], 30);
                block.Type = (BlockType)Enum.Parse(typeof(BlockType), parts[5]);
                block.Visible = parts[6] == "1";
                blocks.Add(block);
            }

            return blocks;
        }
    }
}
