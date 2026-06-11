using System;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        private void btnHostMain_Click(object sender, EventArgs e)
        {
            currentRole = ClientRole.Host;
            selfPlayerId = 1;
            connectedHostIp = "";
            connectionText = "主持者";
            network.Stop();
            ResetNetworkDiagnostics();
            remotePlayerSelections.Clear();
            remoteConnectionTokens.Clear();
            roomManager.CreateDefaultRoom();
            roomManager.Room.SelectedPlayerId = selfPlayerId;
            roomManager.Room.Config.HostIp = LocalNetworkInfo.GetLocalIPv4();
            ShowRoomPage("配置房间后可以开放端口，也可以直接开始本机测试。");
        }

        private void btnJoinMain_Click(object sender, EventArgs e)
        {
            currentRole = ClientRole.Client;
            selfPlayerId = 2;
            connectedHostIp = "";
            connectionText = "加入者";
            network.Stop();
            ResetNetworkDiagnostics();
            remotePlayerSelections.Clear();
            remoteConnectionTokens.Clear();
            roomManager.CreateDefaultRoom();
            roomManager.Room.SelectedPlayerId = selfPlayerId;
            ShowRoomPage("填写主机 IPv4 和端口，选择 Player 后加入房间。");
        }

        private void btnOpenRoom_Click(object sender, EventArgs e)
        {
            if (!ApplyRoomControlsToState())
            {
                return;
            }

            try
            {
                network.Stop();
                ResetNetworkDiagnostics();
                remotePlayerSelections.Clear();
                remoteConnectionTokens.Clear();
                ClearRoomOwnerTokens();
                network.StartHost(roomManager.Room.Config.Port);
                connectionText = "主机已开放，客户端数：" + network.ClientCount.ToString();
                SetRoomMessage("房间已开放：" + roomManager.Room.Config.HostIp + ":" + roomManager.Room.Config.Port.ToString());
                timerGame.Interval = roomManager.Room.Config.TickMilliseconds;
                timerGame.Start();
                RefreshRoomControls();
                BroadcastRoomState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("开放房间失败：" + ex.Message);
            }
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (!ApplyRoomControlsToState())
            {
                return;
            }

            selfPlayerId = GetSelectedPlayerFromRoomInfo();
            int requestedPlayerId = selfPlayerId;
            int requestedTankColorArgb = roomManager.GetTankColorArgbOrDefault(requestedPlayerId);
            string requestedHostIp = roomManager.Room.Config.HostIp;
            network.Stop();
            ResetNetworkDiagnostics();

            if (network.ConnectToHost(roomManager.Room.Config.HostIp, roomManager.Room.Config.Port))
            {
                connectedHostIp = requestedHostIp;
                selfPlayerId = 0;
                roomManager.Room.SelectedPlayerId = requestedPlayerId;
                network.SendJoin(requestedPlayerId, requestedTankColorArgb, clientToken);
                UpdateClientConnectionText();
                SetRoomMessage("已请求控制 P" + requestedPlayerId.ToString() + "，等待主机确认。");
                timerGame.Interval = roomManager.Room.Config.TickMilliseconds;
                timerGame.Start();
                RefreshRoomControls();
            }
            else
            {
                MessageBox.Show("连接失败，请检查 IP、端口和防火墙设置。");
            }
        }

        private void btnStartMatch_Click(object sender, EventArgs e)
        {
            if (!ApplyRoomControlsToState())
            {
                return;
            }

            if (roomManager.Room.Players.Count < 2)
            {
                roomManager.AddPlayer();
            }

            bool changed = ProcessHostMessages();

            if (changed)
            {
                RefreshRoomControls();
                BroadcastRoomState();
            }

            roomManager.SelectPlayer(selfPlayerId, ClientRole.Host);
            remoteInputs.Clear();
            engine.StartMatch(roomManager.Room.Clone());
            roundEndDelayTicks = 0;
            ShowGamePage();
            network.SendStart();
            network.SendFullState(engine.State);
            resources.PlaySound("start.wav");
        }

        private void btnBackToMenu_Click(object sender, EventArgs e)
        {
            ShowMainMenu();
        }

        private void btnLeaveGame_Click(object sender, EventArgs e)
        {
            if (currentRole == ClientRole.Host)
            {
                network.SendRoomReturn();
            }

            ReturnToRoom("已返回房间。");
        }

        private void btnAddPlayer_Click(object sender, EventArgs e)
        {
            if (!roomManager.AddPlayer())
            {
                SetRoomMessage("Player 数量已达到上限：" + roomManager.Room.Config.MaxPlayers.ToString());
            }

            RefreshRoomControls();
            BroadcastRoomState();
        }

        private void btnRemovePlayer_Click(object sender, EventArgs e)
        {
            PlayerSlot slot = GetSelectedSlot();

            if (slot == null)
            {
                return;
            }

            if (!roomManager.RemovePlayer(slot.PlayerId))
            {
                SetRoomMessage("至少需要保留 1 个 Player。");
            }

            if (selfPlayerId == slot.PlayerId && roomManager.Room.Players.Count > 0)
            {
                selfPlayerId = roomManager.GetFirstPlayerIdOrDefault(selfPlayerId);
            }

            RemoveRemoteSelectionsForPlayer(slot.PlayerId);
            RefreshRoomControls();
            BroadcastRoomState();
        }

        private void btnSelectPlayer_Click(object sender, EventArgs e)
        {
            PlayerSlot slot = GetSelectedSlot();

            if (slot == null)
            {
                return;
            }

            if (currentRole == ClientRole.Client)
            {
                if (!network.IsRunning)
                {
                    selfPlayerId = slot.PlayerId;
                    roomManager.Room.SelectedPlayerId = selfPlayerId;
                    SetRoomMessage("准备控制 Player：P" + selfPlayerId.ToString());
                    RefreshRoomControls();
                    return;
                }

                roomManager.Room.SelectedPlayerId = slot.PlayerId;
                network.SendJoin(slot.PlayerId, slot.TankColorArgb, clientToken);
                SetRoomMessage("已请求控制 P" + slot.PlayerId.ToString() + "，等待主机确认。");
                RefreshRoomControls();
                return;
            }

            selfPlayerId = slot.PlayerId;

            if (currentRole == ClientRole.Host)
            {
                RemoveRemoteSelectionsForPlayer(slot.PlayerId);
            }

            roomManager.SelectPlayer(selfPlayerId, currentRole);
            SetRoomMessage("当前控制 Player：P" + selfPlayerId.ToString());
            RefreshRoomControls();
            SendClientPlayerSelection();
            BroadcastRoomState();
        }

        private void playerConfig_ControlTypeChanged(object sender, EventArgs e)
        {
            if (refreshingRoomControls || currentRole != ClientRole.Host)
            {
                return;
            }

            if (playerConfigView.SelectedControlType != PlayerControlType.HumanRemote)
            {
                RemoveRemoteSelectionsForPlayer(playerConfigView.SelectedPlayerId);
            }

            roomManager.SetHostControlType(playerConfigView.SelectedPlayerId, playerConfigView.SelectedControlType, selfPlayerId);
            RefreshRoomControls();
            BroadcastRoomState();
        }

        private void playerConfig_TankColorChanged(object sender, EventArgs e)
        {
            if (refreshingRoomControls)
            {
                return;
            }

            PlayerSlot slot = roomManager.GetPlayer(playerConfigView.SelectedPlayerId);

            if (slot == null)
            {
                return;
            }

            slot.TankColorArgb = playerConfigView.SelectedTankColorArgb;
            RefreshRoomControls();
            SendClientPlayerSelection();
            BroadcastRoomState();
        }

        private bool ApplyRoomControlsToState()
        {
            if (!roomInfoView.ApplyTo(roomManager.Room))
            {
                return false;
            }

            RoomConfig config = roomManager.Room.Config;
            config.BlockSize = RoomLayoutCalculator.CalculateBlockSize(
                ClientSize.Width,
                ClientSize.Height,
                panelHud.Width,
                config.MapColumns,
                config.MapRows);
            roomManager.Room.SelectedPlayerId = roomInfoView.SelectedPlayerId;
            selfPlayerId = roomManager.Room.SelectedPlayerId;
            return true;
        }

        private int GetSelectedPlayerFromRoomInfo()
        {
            if (roomInfoView == null || roomInfoView.SelectedPlayerId <= 0)
            {
                return selfPlayerId;
            }

            return roomInfoView.SelectedPlayerId;
        }

        private PlayerSlot GetSelectedSlot()
        {
            int playerId = playerConfigView == null || playerConfigView.SelectedPlayerId <= 0
                ? selfPlayerId
                : playerConfigView.SelectedPlayerId;
            return roomManager.GetPlayer(playerId);
        }

    }
}
