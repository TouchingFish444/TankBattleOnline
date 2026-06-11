using System.Collections.Generic;

namespace TankBattleOnline
{
    public static partial class NetworkProtocol
    {
        public const int ProtocolVersion = 1;
        public const string VersionKey = "VER";
        public const string StartMessage = "START";
        public const string RoomMessage = "ROOM";
        public const string DisconnectMessage = "DISCONNECT";
        public const string JoinMessageType = "JOIN";
        public const string JoinAckMessageType = "JOINACK";
        public const string RoomNoticeMessageType = "ROOMNOTICE";
        public const string SoundMessageType = "SOUND";
        public const string PingMessageType = "PING";
        public const string PongMessageType = "PONG";
        public const string LatencyMessageType = "LATENCY";
        public const string RoomStateMessageType = "ROOMSTATE";
        public const string InputMessageType = "INPUT";
        public const string StateMessageType = "STATE";
        public const string FrameMessageType = "FRAME";
        public const string JoinPrefix = JoinMessageType + "|";
        public const string JoinAckPrefix = JoinAckMessageType + "|";
        public const string RoomNoticePrefix = RoomNoticeMessageType + "|";
        public const string SoundPrefix = SoundMessageType + "|";
        public const string PingPrefix = PingMessageType + "|";
        public const string PongPrefix = PongMessageType + "|";
        public const string LatencyPrefix = LatencyMessageType + "|";
        public const string RoomStatePrefix = RoomStateMessageType + "|";
        public const string InputPrefix = InputMessageType + "|";
        public const string StatePrefix = StateMessageType + "|";
        public const string FramePrefix = FrameMessageType + "|";

        public static event System.Action<string> ParseWarning;

        private static void ReportParseFailure(string messageType, string reason)
        {
            System.Diagnostics.Debug.WriteLine("NetworkProtocol parse warning: " + messageType + ": " + reason);

            if (ParseWarning != null)
            {
                ParseWarning(messageType + ": " + reason);
            }
        }

        private static void AppendVersion(System.Text.StringBuilder builder)
        {
            builder.Append(VersionKey).Append("=").Append(ProtocolVersion).Append(";");
        }

        private static Dictionary<string, string> ParsePairs(string body)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            string[] pairs = body.Split(';');

            foreach (string pair in pairs)
            {
                int index = pair.IndexOf('=');

                if (index <= 0)
                {
                    continue;
                }

                string key = pair.Substring(0, index);
                string value = pair.Substring(index + 1);
                values[key] = value;
            }

            return values;
        }

        private static string GetValue(Dictionary<string, string> values, string key, string defaultValue)
        {
            return values.ContainsKey(key) ? values[key] : defaultValue;
        }

        private static int ToInt(string text, int defaultValue)
        {
            int value;

            if (int.TryParse(text, out value))
            {
                return value;
            }

            return defaultValue;
        }

        private static bool ToBool(Dictionary<string, string> values, string key)
        {
            return values.ContainsKey(key) && values[key] == "1";
        }

        private static int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }

        private static string JoinIds(List<int> ids)
        {
            List<string> parts = new List<string>();

            foreach (int id in ids)
            {
                parts.Add(id.ToString());
            }

            return string.Join(",", parts.ToArray());
        }

        private static List<int> ParseIds(string text)
        {
            List<int> ids = new List<int>();

            if (string.IsNullOrEmpty(text))
            {
                return ids;
            }

            string[] parts = text.Split(',');

            foreach (string part in parts)
            {
                int value = ToInt(part, 0);

                if (value > 0)
                {
                    ids.Add(value);
                }
            }

            return ids;
        }
    }
}
