using System;
using System.Collections.Generic;
using System.Text;

namespace TankBattleOnline
{
    public static partial class NetworkProtocol
    {
        public static string CreateJoinMessage(int playerId, int tankColorArgb)
        {
            return CreateJoinMessage(playerId, tankColorArgb, "");
        }

        public static string CreateJoinMessage(int playerId, int tankColorArgb, string clientToken)
        {
            return JoinPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";PLAYER=" + playerId.ToString()
                + ";COLOR=" + tankColorArgb.ToString()
                + ";CLIENT=" + Uri.EscapeDataString(clientToken ?? "");
        }

        public static bool TryParseJoinMessage(string message, out int playerId, out int tankColorArgb)
        {
            string clientToken;
            return TryParseJoinMessage(message, out playerId, out tankColorArgb, out clientToken);
        }

        public static bool TryParseJoinMessage(string message, out int playerId, out int tankColorArgb, out string clientToken)
        {
            playerId = 0;
            tankColorArgb = PlayerSlot.GetDefaultTankColorArgb(1);
            clientToken = "";

            if (message == null || !message.StartsWith(JoinPrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(JoinPrefix.Length));

            if (values.ContainsKey("PLAYER"))
            {
                playerId = ToInt(values["PLAYER"], 0);
            }

            tankColorArgb = ToInt(GetValue(values, "COLOR", PlayerSlot.GetDefaultTankColorArgb(playerId).ToString()), PlayerSlot.GetDefaultTankColorArgb(playerId));
            clientToken = Uri.UnescapeDataString(GetValue(values, "CLIENT", ""));
            return playerId > 0;
        }

        public static string CreateJoinAckMessage(string clientToken, bool accepted, int playerId, string reason)
        {
            return JoinAckPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";CLIENT=" + Uri.EscapeDataString(clientToken ?? "")
                + ";OK=" + (accepted ? "1" : "0")
                + ";PLAYER=" + playerId.ToString()
                + ";REASON=" + Uri.EscapeDataString(reason ?? "");
        }

        public static bool TryParseJoinAckMessage(string message, out string clientToken, out bool accepted, out int playerId, out string reason)
        {
            clientToken = "";
            accepted = false;
            playerId = 0;
            reason = "";

            if (message == null || !message.StartsWith(JoinAckPrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(JoinAckPrefix.Length));
            clientToken = Uri.UnescapeDataString(GetValue(values, "CLIENT", ""));
            accepted = GetValue(values, "OK", "0") == "1";
            playerId = ToInt(GetValue(values, "PLAYER", "0"), 0);
            reason = Uri.UnescapeDataString(GetValue(values, "REASON", ""));
            return !string.IsNullOrEmpty(clientToken);
        }

        public static string CreateRoomNoticeMessage(string notice)
        {
            return RoomNoticePrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";TEXT=" + Uri.EscapeDataString(notice ?? "");
        }

        public static bool TryParseRoomNoticeMessage(string message, out string notice)
        {
            notice = "";

            if (message == null || !message.StartsWith(RoomNoticePrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(RoomNoticePrefix.Length));
            notice = Uri.UnescapeDataString(GetValue(values, "TEXT", ""));
            return !string.IsNullOrEmpty(notice);
        }

        public static string CreateSoundMessage(string fileName)
        {
            if (!IsAllowedSoundFileName(fileName))
            {
                return "";
            }

            return SoundPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";FILE=" + Uri.EscapeDataString(fileName ?? "");
        }

        public static bool TryParseSoundMessage(string message, out string fileName)
        {
            fileName = "";

            if (message == null || !message.StartsWith(SoundPrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(SoundPrefix.Length));
            fileName = Uri.UnescapeDataString(GetValue(values, "FILE", ""));
            return IsAllowedSoundFileName(fileName);
        }

        private static bool IsAllowedSoundFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            string normalized = fileName.Replace('\\', '/').ToLowerInvariant();
            return normalized == "fire.wav" || normalized == "start.wav";
        }

        public static string CreatePingMessage(string clientToken, int pingId, int sentTick)
        {
            return PingPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";CLIENT=" + Uri.EscapeDataString(clientToken ?? "")
                + ";ID=" + pingId.ToString()
                + ";TICK=" + sentTick.ToString();
        }

        public static bool TryParsePingMessage(string message, out string clientToken, out int pingId, out int sentTick)
        {
            clientToken = "";
            pingId = 0;
            sentTick = 0;

            if (message == null || !message.StartsWith(PingPrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(PingPrefix.Length));
            clientToken = Uri.UnescapeDataString(GetValue(values, "CLIENT", ""));
            pingId = ToInt(GetValue(values, "ID", "0"), 0);
            sentTick = ToInt(GetValue(values, "TICK", "0"), 0);
            return !string.IsNullOrEmpty(clientToken) && pingId > 0;
        }

        public static string CreatePongMessage(string clientToken, int pingId, int sentTick)
        {
            return PongPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";CLIENT=" + Uri.EscapeDataString(clientToken ?? "")
                + ";ID=" + pingId.ToString()
                + ";TICK=" + sentTick.ToString();
        }

        public static bool TryParsePongMessage(string message, out string clientToken, out int pingId, out int sentTick)
        {
            clientToken = "";
            pingId = 0;
            sentTick = 0;

            if (message == null || !message.StartsWith(PongPrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(PongPrefix.Length));
            clientToken = Uri.UnescapeDataString(GetValue(values, "CLIENT", ""));
            pingId = ToInt(GetValue(values, "ID", "0"), 0);
            sentTick = ToInt(GetValue(values, "TICK", "0"), 0);
            return !string.IsNullOrEmpty(clientToken) && pingId > 0;
        }

        public static string CreateLatencyMessage(string clientToken, int latencyMs)
        {
            return LatencyPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";CLIENT=" + Uri.EscapeDataString(clientToken ?? "")
                + ";MS=" + latencyMs.ToString();
        }

        public static bool TryParseLatencyMessage(string message, out string clientToken, out int latencyMs)
        {
            clientToken = "";
            latencyMs = -1;

            if (message == null || !message.StartsWith(LatencyPrefix))
            {
                return false;
            }

            Dictionary<string, string> values = ParsePairs(message.Substring(LatencyPrefix.Length));
            clientToken = Uri.UnescapeDataString(GetValue(values, "CLIENT", ""));
            latencyMs = ToInt(GetValue(values, "MS", "-1"), -1);
            return !string.IsNullOrEmpty(clientToken) && latencyMs >= 0;
        }

        public static string CreateRoomStateMessage(RoomState room)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RoomStatePrefix);
            AppendVersion(builder);
            builder.Append("HOST=").Append(Uri.EscapeDataString(room.Config.HostIp)).Append(";");
            builder.Append("PORT=").Append(room.Config.Port).Append(";");
            builder.Append("ROOM=").Append(Uri.EscapeDataString(room.Config.RoomName)).Append(";");
            builder.Append("HP=").Append(room.Config.InitialHp).Append(";");
            builder.Append("ROUNDS=").Append(room.Config.TotalRounds).Append(";");
            builder.Append("SECONDS=").Append(room.Config.RoundSeconds).Append(";");
            builder.Append("COLS=").Append(room.Config.MapColumns).Append(";");
            builder.Append("ROWS=").Append(room.Config.MapRows).Append(";");
            builder.Append("MAX=").Append(room.Config.MaxPlayers).Append(";");
            builder.Append("BS=").Append(room.Config.BlockSize).Append(";");
            builder.Append("TICKMS=").Append(room.Config.TickMilliseconds).Append(";");
            builder.Append("SELECT=").Append(room.SelectedPlayerId).Append(";");
            builder.Append("PLAYERS=").Append(SerializePlayers(room.Players));
            return builder.ToString();
        }

        public static bool TryParseRoomStateMessage(string message, out RoomState room)
        {
            room = null;

            if (message == null || !message.StartsWith(RoomStatePrefix))
            {
                return false;
            }

            try
            {
                Dictionary<string, string> values = ParsePairs(message.Substring(RoomStatePrefix.Length));
                RoomState parsed = new RoomState();
                parsed.Config.HostIp = Uri.UnescapeDataString(GetValue(values, "HOST", "127.0.0.1"));
                parsed.Config.Port = ToInt(GetValue(values, "PORT", "9000"), 9000);
                parsed.Config.RoomName = Uri.UnescapeDataString(GetValue(values, "ROOM", "TankBattleOnline"));
                parsed.Config.InitialHp = ToInt(GetValue(values, "HP", "5"), 5);
                parsed.Config.TotalRounds = ToInt(GetValue(values, "ROUNDS", "3"), 3);
                parsed.Config.RoundSeconds = ToInt(GetValue(values, "SECONDS", "120"), 120);
                parsed.Config.MapColumns = ToInt(GetValue(values, "COLS", "40"), 40);
                parsed.Config.MapRows = ToInt(GetValue(values, "ROWS", "24"), 24);
                parsed.Config.MaxPlayers = ToInt(GetValue(values, "MAX", "8"), 8);
                parsed.Config.BlockSize = ToInt(GetValue(values, "BS", "30"), 30);
                parsed.Config.TickMilliseconds = ToInt(GetValue(values, "TICKMS", "16"), 16);
                parsed.SelectedPlayerId = ToInt(GetValue(values, "SELECT", "1"), 1);
                parsed.Players = ParsePlayers(GetValue(values, "PLAYERS", ""));

                if (parsed.Players.Count == 0)
                {
                    parsed.Players.Add(new PlayerSlot(1, PlayerControlType.HumanLocal));
                    parsed.Players.Add(new PlayerSlot(2, PlayerControlType.Computer));
                }

                room = parsed;
                return true;
            }
            catch
            {
                ReportParseFailure(RoomStateMessageType, "ROOMSTATE 解析失败");
                return false;
            }
        }

        private static string SerializePlayers(List<PlayerSlot> players)
        {
            List<string> parts = new List<string>();

            foreach (PlayerSlot slot in players)
            {
                parts.Add(slot.PlayerId.ToString()
                    + "," + slot.ControlType.ToString()
                    + "," + slot.Wins.ToString()
                    + "," + slot.TankColorArgb.ToString()
                    + "," + Uri.EscapeDataString(slot.OwnerToken ?? "")
                    + "," + slot.Score.ToString()
                    + "," + slot.LatencyMs.ToString());
            }

            return string.Join("/", parts.ToArray());
        }

        private static List<PlayerSlot> ParsePlayers(string text)
        {
            List<PlayerSlot> players = new List<PlayerSlot>();

            if (string.IsNullOrEmpty(text))
            {
                return players;
            }

            string[] playerParts = text.Split('/');

            foreach (string item in playerParts)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                string[] parts = item.Split(',');

                if (parts.Length < 4)
                {
                    continue;
                }

                int playerId = ToInt(parts[0], 0);

                if (playerId <= 0)
                {
                    continue;
                }

                PlayerControlType controlType = PlayerControlType.Computer;

                try
                {
                    controlType = (PlayerControlType)Enum.Parse(typeof(PlayerControlType), parts[1]);
                }
                catch
                {
                }

                PlayerSlot slot = new PlayerSlot(playerId, controlType);
                slot.Wins = ToInt(parts[2], 0);
                slot.TankColorArgb = ToInt(parts[3], PlayerSlot.GetDefaultTankColorArgb(playerId));
                slot.OwnerToken = parts.Length >= 5 ? Uri.UnescapeDataString(parts[4]) : "";
                slot.Score = parts.Length >= 6 ? ToInt(parts[5], 0) : 0;
                slot.LatencyMs = parts.Length >= 7 ? ToInt(parts[6], -1) : -1;
                players.Add(slot);
            }

            return players;
        }
    }
}
