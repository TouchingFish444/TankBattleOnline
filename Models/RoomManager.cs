namespace TankBattleOnline
{
    public class RoomManager
    {
        public RoomState Room = new RoomState();

        public void CreateDefaultRoom()
        {
            Room = new RoomState();
            Room.Players.Add(new PlayerSlot(1, PlayerControlType.HumanLocal));
            Room.Players.Add(new PlayerSlot(2, PlayerControlType.Computer));
            Room.SelectedPlayerId = 1;
        }

        public PlayerSlot GetPlayer(int playerId)
        {
            foreach (PlayerSlot slot in Room.Players)
            {
                if (slot.PlayerId == playerId)
                {
                    return slot;
                }
            }

            return null;
        }

        public int GetFirstPlayerIdOrDefault(int defaultPlayerId)
        {
            if (Room.Players.Count == 0)
            {
                return defaultPlayerId;
            }

            return Room.Players[0].PlayerId;
        }

        public int GetTankColorArgbOrDefault(int playerId)
        {
            PlayerSlot slot = GetPlayer(playerId);
            return slot == null ? PlayerSlot.GetDefaultTankColorArgb(playerId) : slot.TankColorArgb;
        }

        public void EnsurePlayerExists(int playerId)
        {
            while (GetPlayer(playerId) == null && Room.Players.Count < Room.Config.MaxPlayers)
            {
                AddPlayer();
            }
        }

        public bool AddPlayer()
        {
            if (Room.Players.Count >= Room.Config.MaxPlayers)
            {
                return false;
            }

            int nextId = 1;

            foreach (PlayerSlot slot in Room.Players)
            {
                if (slot.PlayerId >= nextId)
                {
                    nextId = slot.PlayerId + 1;
                }
            }

            Room.Players.Add(new PlayerSlot(nextId, PlayerControlType.Computer));
            return true;
        }

        public bool RemovePlayer(int playerId)
        {
            if (Room.Players.Count <= 1)
            {
                return false;
            }

            PlayerSlot target = GetPlayer(playerId);

            if (target == null)
            {
                return false;
            }

            Room.Players.Remove(target);

            if (Room.SelectedPlayerId == playerId)
            {
                Room.SelectedPlayerId = Room.Players[0].PlayerId;
            }

            return true;
        }

        public void SelectPlayer(int playerId, ClientRole role)
        {
            PlayerSlot slot = GetPlayer(playerId);

            if (slot == null)
            {
                return;
            }

            Room.SelectedPlayerId = playerId;

            if (role == ClientRole.Host)
            {
                foreach (PlayerSlot item in Room.Players)
                {
                    if (item.ControlType == PlayerControlType.HumanLocal)
                    {
                        item.ControlType = PlayerControlType.Computer;
                    }
                }

                slot.ControlType = PlayerControlType.HumanLocal;
                slot.OwnerToken = "";
                slot.LatencyMs = 0;
            }
        }

        public void SetControlType(int playerId, PlayerControlType controlType)
        {
            PlayerSlot slot = GetPlayer(playerId);

            if (slot != null)
            {
                slot.ControlType = controlType;

                if (controlType != PlayerControlType.HumanRemote)
                {
                    slot.OwnerToken = "";
                    slot.LatencyMs = controlType == PlayerControlType.HumanLocal ? 0 : -1;
                }
            }
        }

        public void SetHostControlType(int playerId, PlayerControlType controlType, int hostPlayerId)
        {
            if (controlType != PlayerControlType.Computer && playerId == hostPlayerId)
            {
                controlType = PlayerControlType.HumanLocal;
            }

            SetControlType(playerId, controlType);
        }
    }
}
