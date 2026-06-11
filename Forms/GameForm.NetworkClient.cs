namespace TankBattleOnline
{
    public partial class GameForm
    {
        private void ProcessClientMessages()
        {
            string message;

            while (network.TryGetMessage(out message))
            {
                if (message == NetworkProtocol.RoomMessage)
                {
                    ReturnToRoom("主机已返回房间。");
                    return;
                }

                if (message == NetworkProtocol.DisconnectMessage)
                {
                    connectionText = "连接已断开";
                    SetRoomMessage(connectionText);
                    continue;
                }

                if (message == NetworkProtocol.StartMessage)
                {
                    UpdateClientConnectionText();
                    continue;
                }

                string ackToken;
                bool accepted;
                int acceptedPlayerId;
                string reason;
                int pingId;
                int sentTick;

                if (NetworkProtocol.TryParseJoinAckMessage(message, out ackToken, out accepted, out acceptedPlayerId, out reason))
                {
                    ApplyJoinAck(ackToken, accepted, acceptedPlayerId, reason);
                    continue;
                }

                string notice;

                if (NetworkProtocol.TryParseRoomNoticeMessage(message, out notice))
                {
                    SetRoomMessage(notice);
                    continue;
                }

                if (NetworkProtocol.TryParsePongMessage(message, out ackToken, out pingId, out sentTick))
                {
                    ApplyPong(ackToken, pingId);
                    continue;
                }

                string soundFileName;

                if (NetworkProtocol.TryParseSoundMessage(message, out soundFileName))
                {
                    resources.PlaySound(soundFileName);
                    continue;
                }

                RoomState room;

                if (NetworkProtocol.TryParseRoomStateMessage(message, out room))
                {
                    ApplyRoomStateFromHost(room);
                    continue;
                }

                if (NetworkProtocol.TryApplyFrameStateMessage(message, engine.State))
                {
                    if (currentPage != AppPage.Game)
                    {
                        ShowGamePage();
                    }

                    continue;
                }

                GameState state = NetworkProtocol.ParseStateMessage(message);

                if (state != null)
                {
                    engine.State = state;

                    if (currentPage != AppPage.Game)
                    {
                        ShowGamePage();
                    }
                }
            }
        }

        private void ApplyJoinAck(string ackToken, bool accepted, int acceptedPlayerId, string reason)
        {
            if (ackToken != clientToken)
            {
                return;
            }

            if (accepted && acceptedPlayerId > 0)
            {
                selfPlayerId = acceptedPlayerId;
                roomManager.Room.SelectedPlayerId = selfPlayerId;
                UpdateClientConnectionText();
            }
            else if (!accepted)
            {
                int ownedPlayerId = FindPlayerOwnedByClient(clientToken);
                selfPlayerId = ownedPlayerId;
                roomManager.Room.SelectedPlayerId = selfPlayerId;
                UpdateClientConnectionText();
            }

            if (!string.IsNullOrEmpty(reason))
            {
                SetRoomMessage(reason);
            }

            RefreshRoomControls();
        }

        private void ApplyRoomStateFromHost(RoomState room)
        {
            roomManager.Room = room;

            if (currentRole == ClientRole.Client)
            {
                if (!string.IsNullOrEmpty(connectedHostIp))
                {
                    roomManager.Room.Config.HostIp = connectedHostIp;
                }

                int ownedPlayerId = FindPlayerOwnedByClient(clientToken);
                selfPlayerId = ownedPlayerId;
                UpdateClientConnectionText();
            }
            else if (roomManager.GetPlayer(selfPlayerId) == null)
            {
                selfPlayerId = roomManager.GetFirstPlayerIdOrDefault(selfPlayerId);
            }

            roomManager.Room.SelectedPlayerId = selfPlayerId;
            timerGame.Interval = roomManager.Room.Config.TickMilliseconds;
            RefreshRoomControls();
        }

        private void UpdateClientConnectionText()
        {
            if (currentRole != ClientRole.Client)
            {
                return;
            }

            string hostIp = !string.IsNullOrEmpty(connectedHostIp)
                ? connectedHostIp
                : roomManager.Room.Config.HostIp;

            if (string.IsNullOrEmpty(hostIp))
            {
                hostIp = "未知主机";
            }

            string playerText = selfPlayerId > 0
                ? "控制 P" + selfPlayerId.ToString()
                : "等待选择 Player";
            string latencyText = localLatencyMs >= 0
                ? localLatencyMs.ToString() + " ms"
                : "--";

            connectionText = "主机 " + hostIp + "，" + playerText + "，延迟 " + latencyText;
        }
    }
}
