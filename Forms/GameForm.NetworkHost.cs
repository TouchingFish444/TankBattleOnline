using System.Collections.Generic;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        private bool ProcessHostMessages()
        {
            NetworkMessage envelope;
            bool changed = false;

            while (network.TryGetMessage(out envelope))
            {
                string message = envelope.Text;
                int playerId;
                int tankColorArgb;
                string clientToken;
                PlayerInput input;
                string pingClientToken;
                int pingId;
                int sentTick;
                int latencyMs;

                if (NetworkProtocol.TryParseJoinMessage(message, out playerId, out tankColorArgb, out clientToken))
                {
                    int acceptedPlayerId;
                    string response;
                    string notice;
                    bool accepted = TryAcceptRemotePlayerSelection(
                        playerId,
                        tankColorArgb,
                        clientToken,
                        envelope.ConnectionId,
                        out acceptedPlayerId,
                        out response,
                        out notice);

                    network.SendJoinAck(clientToken, accepted, acceptedPlayerId, response);

                    if (!string.IsNullOrEmpty(notice))
                    {
                        network.SendRoomNotice(notice);
                        SetRoomMessage(notice);
                    }
                    else
                    {
                        SetRoomMessage(response);
                    }

                    changed = true;
                }
                else if (NetworkProtocol.TryParsePingMessage(message, out pingClientToken, out pingId, out sentTick))
                {
                    network.SendPong(pingClientToken, pingId, sentTick);
                }
                else if (NetworkProtocol.TryParseLatencyMessage(message, out pingClientToken, out latencyMs))
                {
                    changed = ApplyRemoteLatency(pingClientToken, latencyMs) || changed;
                }
                else if (NetworkProtocol.TryParseInputMessage(message, out playerId, out input))
                {
                    if (IsRemoteInputAllowed(envelope.ConnectionId, playerId))
                    {
                        remoteInputs[playerId] = input;
                    }
                }
                else if (message == NetworkProtocol.DisconnectMessage)
                {
                    string notice;
                    bool disconnected = HandleRemoteDisconnect(envelope.ConnectionId, out notice);

                    if (disconnected && !string.IsNullOrEmpty(notice))
                    {
                        network.SendRoomNotice(notice);
                    }

                    changed = disconnected || changed;
                }
            }

            return changed;
        }

        private bool TryAcceptRemotePlayerSelection(
            int requestedPlayerId,
            int tankColorArgb,
            string remoteClientToken,
            int connectionId,
            out int acceptedPlayerId,
            out string response,
            out string notice)
        {
            acceptedPlayerId = 0;
            notice = "";

            if (string.IsNullOrEmpty(remoteClientToken))
            {
                remoteClientToken = "CONN:" + connectionId.ToString();
            }

            int previousPlayerId = 0;

            if (remotePlayerSelections.ContainsKey(remoteClientToken))
            {
                previousPlayerId = remotePlayerSelections[remoteClientToken];
            }

            if (requestedPlayerId > 0 && requestedPlayerId <= roomManager.Room.Config.MaxPlayers)
            {
                roomManager.EnsurePlayerExists(requestedPlayerId);
            }

            acceptedPlayerId = ResolveRemotePlayerId(requestedPlayerId, remoteClientToken);

            if (acceptedPlayerId <= 0)
            {
                response = "房间没有可用的联机 Player。";
                return false;
            }

            ApplyRemotePlayerSelection(acceptedPlayerId, tankColorArgb, remoteClientToken, connectionId);

            if (acceptedPlayerId == requestedPlayerId)
            {
                response = previousPlayerId == acceptedPlayerId
                    ? "P" + acceptedPlayerId.ToString() + " 设置已更新。"
                    : "P" + acceptedPlayerId.ToString() + " 已加入房间。";
            }
            else
            {
                response = "P" + requestedPlayerId.ToString() + " 不可用，已分配 P" + acceptedPlayerId.ToString() + "。";
            }

            if (previousPlayerId <= 0)
            {
                notice = "P" + acceptedPlayerId.ToString() + " 已加入房间。";
            }
            else if (previousPlayerId != acceptedPlayerId)
            {
                notice = "P" + previousPlayerId.ToString() + " 已切换到 P" + acceptedPlayerId.ToString() + "。";
            }

            return true;
        }

        private int ResolveRemotePlayerId(int requestedPlayerId, string remoteClientToken)
        {
            if (CanRemoteControlPlayer(requestedPlayerId, remoteClientToken))
            {
                return requestedPlayerId;
            }

            foreach (PlayerSlot slot in roomManager.Room.Players)
            {
                if (CanRemoteControlPlayer(slot.PlayerId, remoteClientToken))
                {
                    return slot.PlayerId;
                }
            }

            return 0;
        }

        private bool CanRemoteControlPlayer(int playerId, string remoteClientToken)
        {
            if (playerId <= 0 || playerId == selfPlayerId)
            {
                return false;
            }

            PlayerSlot slot = roomManager.GetPlayer(playerId);

            if (slot == null || slot.ControlType == PlayerControlType.HumanLocal)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(slot.OwnerToken) && slot.OwnerToken != remoteClientToken)
            {
                return false;
            }

            return !IsPlayerSelectedByAnotherClient(playerId, remoteClientToken);
        }

        private void ApplyRemotePlayerSelection(int playerId, int tankColorArgb, string remoteClientToken, int connectionId)
        {
            if (string.IsNullOrEmpty(remoteClientToken))
            {
                remoteClientToken = "CONN:" + connectionId.ToString();
            }

            ClearConnectionSelection(connectionId, remoteClientToken);
            ClearPreviousRemoteSelection(remoteClientToken, playerId);
            ClearOtherConnectionsForToken(remoteClientToken, connectionId);

            PlayerSlot slot = roomManager.GetPlayer(playerId);

            if (slot != null)
            {
                slot.TankColorArgb = tankColorArgb;
                slot.AssignOwner(remoteClientToken);
            }

            if (playerId != selfPlayerId)
            {
                SetPlayerControlType(playerId, PlayerControlType.HumanRemote);
            }

            if (!string.IsNullOrEmpty(remoteClientToken))
            {
                remotePlayerSelections[remoteClientToken] = playerId;
                remoteConnectionTokens[connectionId] = remoteClientToken;
            }
        }

        private void ClearConnectionSelection(int connectionId, string nextClientToken)
        {
            if (!remoteConnectionTokens.ContainsKey(connectionId))
            {
                return;
            }

            string previousToken = remoteConnectionTokens[connectionId];

            if (previousToken == nextClientToken)
            {
                return;
            }

            ClearRemoteSelectionByToken(previousToken);
            remoteConnectionTokens.Remove(connectionId);
        }

        private void ClearPreviousRemoteSelection(string remoteClientToken, int nextPlayerId)
        {
            if (string.IsNullOrEmpty(remoteClientToken) || !remotePlayerSelections.ContainsKey(remoteClientToken))
            {
                return;
            }

            int previousPlayerId = remotePlayerSelections[remoteClientToken];

            if (previousPlayerId == nextPlayerId || previousPlayerId == selfPlayerId)
            {
                return;
            }

            if (!IsPlayerSelectedByAnotherClient(previousPlayerId, remoteClientToken))
            {
                SetPlayerControlType(previousPlayerId, PlayerControlType.Computer);
            }

            remoteInputs.Remove(previousPlayerId);
        }

        private bool IsPlayerSelectedByAnotherClient(int playerId, string ignoredClientToken)
        {
            foreach (KeyValuePair<string, int> item in remotePlayerSelections)
            {
                if (item.Key != ignoredClientToken && item.Value == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRemoteInputAllowed(int connectionId, int playerId)
        {
            string remoteClientToken;
            int selectedPlayerId;

            if (!remoteConnectionTokens.TryGetValue(connectionId, out remoteClientToken))
            {
                return false;
            }

            if (!remotePlayerSelections.TryGetValue(remoteClientToken, out selectedPlayerId))
            {
                return false;
            }

            return selectedPlayerId == playerId;
        }

        private bool HandleRemoteDisconnect(int connectionId, out string notice)
        {
            notice = "";

            if (!remoteConnectionTokens.ContainsKey(connectionId))
            {
                return false;
            }

            string remoteClientToken = remoteConnectionTokens[connectionId];
            remoteConnectionTokens.Remove(connectionId);

            if (HasConnectionForToken(remoteClientToken))
            {
                return false;
            }

            int playerId = ClearRemoteSelectionByToken(remoteClientToken);

            if (playerId > 0)
            {
                notice = "P" + playerId.ToString() + " 已断开连接。";
            }
            else
            {
                notice = "有客户端断开连接。";
            }

            SetRoomMessage(notice);
            return true;
        }

        private int ClearRemoteSelectionByToken(string remoteClientToken)
        {
            if (string.IsNullOrEmpty(remoteClientToken) || !remotePlayerSelections.ContainsKey(remoteClientToken))
            {
                return 0;
            }

            int playerId = remotePlayerSelections[remoteClientToken];
            remotePlayerSelections.Remove(remoteClientToken);
            remoteInputs.Remove(playerId);

            if (playerId != selfPlayerId && !IsPlayerSelectedByAnotherClient(playerId, remoteClientToken))
            {
                SetPlayerControlType(playerId, PlayerControlType.Computer);
            }

            List<int> staleConnectionIds = new List<int>();

            foreach (KeyValuePair<int, string> item in remoteConnectionTokens)
            {
                if (item.Value == remoteClientToken)
                {
                    staleConnectionIds.Add(item.Key);
                }
            }

            foreach (int connectionId in staleConnectionIds)
            {
                remoteConnectionTokens.Remove(connectionId);
            }

            return playerId;
        }

        private bool HasConnectionForToken(string remoteClientToken)
        {
            foreach (KeyValuePair<int, string> item in remoteConnectionTokens)
            {
                if (item.Value == remoteClientToken)
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearOtherConnectionsForToken(string remoteClientToken, int keptConnectionId)
        {
            List<int> staleConnectionIds = new List<int>();

            foreach (KeyValuePair<int, string> item in remoteConnectionTokens)
            {
                if (item.Key != keptConnectionId && item.Value == remoteClientToken)
                {
                    staleConnectionIds.Add(item.Key);
                }
            }

            foreach (int connectionId in staleConnectionIds)
            {
                remoteConnectionTokens.Remove(connectionId);
            }
        }
    }
}
