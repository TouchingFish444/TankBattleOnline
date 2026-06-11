using System.Collections.Generic;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        private void ProcessRoomNetworkMessages()
        {
            if (currentRole == ClientRole.Host)
            {
                bool changed = ProcessHostMessages();
                connectionText = "主机已开放，客户端数：" + network.ClientCount.ToString();

                if (changed)
                {
                    RefreshRoomControls();
                    BroadcastRoomState();
                }
            }
            else if (currentRole == ClientRole.Client)
            {
                ProcessClientMessages();
            }
        }

        private void SetPlayerControlType(int playerId, PlayerControlType controlType)
        {
            roomManager.SetControlType(playerId, controlType);

            Tank tank = engine.State.GetTank(playerId);

            if (tank != null)
            {
                tank.ControlType = controlType;
            }
        }

        private void ClearRoomOwnerTokens()
        {
            foreach (PlayerSlot slot in roomManager.Room.Players)
            {
                slot.ClearOwner();
            }
        }

        private int FindPlayerOwnedByClient(string ownerToken)
        {
            if (string.IsNullOrEmpty(ownerToken))
            {
                return 0;
            }

            foreach (PlayerSlot slot in roomManager.Room.Players)
            {
                if (slot.ControlType == PlayerControlType.HumanRemote && slot.OwnerToken == ownerToken)
                {
                    return slot.PlayerId;
                }
            }

            return 0;
        }

        private void BroadcastRoomState()
        {
            if (currentRole == ClientRole.Host && network.IsRunning)
            {
                network.SendRoomState(roomManager.Room);
            }
        }

        private void SendClientPlayerSelection()
        {
            if (currentRole == ClientRole.Client && network.IsRunning && selfPlayerId > 0)
            {
                network.SendJoin(selfPlayerId, roomManager.GetTankColorArgbOrDefault(selfPlayerId), clientToken);
            }
        }

        private void RemoveRemoteSelectionsForPlayer(int playerId)
        {
            List<string> tokens = new List<string>();

            foreach (KeyValuePair<string, int> item in remotePlayerSelections)
            {
                if (item.Value == playerId)
                {
                    tokens.Add(item.Key);
                }
            }

            foreach (string token in tokens)
            {
                ClearRemoteSelectionByToken(token);
            }

            List<int> connectionIds = new List<int>();

            foreach (KeyValuePair<int, string> item in remoteConnectionTokens)
            {
                if (tokens.Contains(item.Value))
                {
                    connectionIds.Add(item.Key);
                }
            }

            foreach (int connectionId in connectionIds)
            {
                remoteConnectionTokens.Remove(connectionId);
            }
        }

        private int GetElapsedMilliseconds(int startTick, int endTick)
        {
            return System.Math.Max(0, unchecked(endTick - startTick));
        }
    }
}
