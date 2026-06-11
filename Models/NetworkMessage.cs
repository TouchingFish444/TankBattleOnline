namespace TankBattleOnline
{
    public class NetworkMessage
    {
        public int ConnectionId;
        public string Text;

        public NetworkMessage(int connectionId, string text)
        {
            ConnectionId = connectionId;
            Text = text;
        }
    }
}
