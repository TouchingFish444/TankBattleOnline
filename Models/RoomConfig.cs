namespace TankBattleOnline
{
    public class RoomConfig
    {
        public const string DefaultHostIp = "127.0.0.1";
        public const int DefaultPort = 9000;
        public const string DefaultRoomName = "TankBattleOnline";
        public const int DefaultTickMilliseconds = 16;

        public string HostIp = DefaultHostIp;
        public int Port = DefaultPort;
        public string RoomName = DefaultRoomName;
        public int InitialHp = 5;
        public int TotalRounds = 3;
        public int RoundSeconds = 120;
        public int MapColumns = 40;
        public int MapRows = 24;
        public int MaxPlayers = 8;
        public int BlockSize = 30;
        public int TickMilliseconds = DefaultTickMilliseconds;

        public RoomConfig Clone()
        {
            return new RoomConfig
            {
                HostIp = HostIp,
                Port = Port,
                RoomName = RoomName,
                InitialHp = InitialHp,
                TotalRounds = TotalRounds,
                RoundSeconds = RoundSeconds,
                MapColumns = MapColumns,
                MapRows = MapRows,
                MaxPlayers = MaxPlayers,
                BlockSize = BlockSize,
                TickMilliseconds = TickMilliseconds
            };
        }
    }
}
