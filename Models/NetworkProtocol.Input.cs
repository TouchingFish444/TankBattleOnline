using System.Collections.Generic;

namespace TankBattleOnline
{
    public static partial class NetworkProtocol
    {
        public static string CreateInputMessage(int playerId, PlayerInput input)
        {
            return InputPrefix
                + VersionKey + "=" + ProtocolVersion.ToString()
                + ";PLAYER=" + playerId.ToString()
                + ";UP=" + BoolToInt(input.Up)
                + ";DOWN=" + BoolToInt(input.Down)
                + ";LEFT=" + BoolToInt(input.Left)
                + ";RIGHT=" + BoolToInt(input.Right)
                + ";FIRE=" + BoolToInt(input.Fire);
        }

        public static bool TryParseInputMessage(string message, out int playerId, out PlayerInput input)
        {
            playerId = 0;
            input = new PlayerInput();

            if (message == null || !message.StartsWith(InputPrefix))
            {
                return false;
            }

            string body = message.Substring(InputPrefix.Length);
            Dictionary<string, string> values = ParsePairs(body);

            if (!values.ContainsKey("PLAYER"))
            {
                return false;
            }

            playerId = ToInt(values["PLAYER"], 0);
            input.Up = ToBool(values, "UP");
            input.Down = ToBool(values, "DOWN");
            input.Left = ToBool(values, "LEFT");
            input.Right = ToBool(values, "RIGHT");
            input.Fire = ToBool(values, "FIRE");
            return playerId > 0;
        }
    }
}
