using System.Collections.Generic;

namespace TankBattleOnline
{
    public class RoomState
    {
        public RoomConfig Config = new RoomConfig();
        public List<PlayerSlot> Players = new List<PlayerSlot>();
        public int SelectedPlayerId = 1;

        public RoomState Clone()
        {
            RoomState state = new RoomState();
            state.Config = Config.Clone();
            state.SelectedPlayerId = SelectedPlayerId;

            foreach (PlayerSlot slot in Players)
            {
                state.Players.Add(slot.Clone());
            }

            return state;
        }
    }
}
