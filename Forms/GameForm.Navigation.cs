using System;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            ArrangeStartPage();
        }

        private void ShowMainMenu()
        {
            timerGame.Stop();
            network.Stop();
            ResetNetworkDiagnostics();
            localInput.Reset();
            remoteInputs.Clear();
            currentRole = ClientRole.None;
            currentPage = AppPage.MainMenu;
            connectedHostIp = "";
            connectionText = "未连接";
            panelGame.Visible = false;
            panelRoom.Visible = false;
            panelStart.Visible = true;
            ArrangeStartPage();
            panelStart.BringToFront();
        }

        private void ShowRoomPage(string message)
        {
            currentPage = AppPage.Room;
            panelStart.Visible = false;
            panelGame.Visible = false;
            panelRoom.Visible = true;
            panelRoom.BringToFront();
            SetRoomMessage(message);
            RefreshRoomControls();
        }

        private void ShowGamePage()
        {
            currentPage = AppPage.Game;
            panelStart.Visible = false;
            panelRoom.Visible = false;
            panelGame.Visible = true;
            panelGame.BringToFront();
            timerGame.Interval = engine.State.Config.TickMilliseconds;
            ResetFpsCounter();
            timerGame.Start();
            UpdateHud();
            gameCanvasView.Bind(engine.State, resources);
        }

        private void ReturnToRoom(string message)
        {
            localInput.Reset();
            remoteInputs.Clear();
            currentPage = AppPage.Room;
            panelGame.Visible = false;
            panelStart.Visible = false;
            panelRoom.Visible = true;
            panelRoom.BringToFront();
            SetRoomMessage(message);
            RefreshRoomControls();

            if (network.IsRunning)
            {
                timerGame.Start();
            }
            else
            {
                timerGame.Stop();
            }
        }

        private void SetRoomMessage(string message)
        {
            roomMessage = message;

            if (roomInfoView != null)
            {
                roomInfoView.SetMessage(message);
            }
        }

        private string GetRoomMessage()
        {
            return roomMessage;
        }

        private void UpdateHud()
        {
            gameHudView.Bind(engine.State, selfPlayerId, currentRole, connectionText, currentFps);
        }

        private void RefreshRoomControls()
        {
            refreshingRoomControls = true;
            roomManager.Room.SelectedPlayerId = selfPlayerId;
            roomInfoView.LoadRoom(roomManager.Room);
            roomInfoView.SetRole(currentRole, GetRoomMessage());
            playerConfigView.Bind(roomManager.Room, selfPlayerId, currentRole, resources, playerColorNames, playerColorValues, clientToken);
            refreshingRoomControls = false;
        }

        private void ArrangeStartPage()
        {
            if (panelStart == null || btnHostMain == null || btnJoinMain == null || lblTitle == null)
            {
                return;
            }

            int width = panelStart.ClientSize.Width;
            int height = panelStart.ClientSize.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            int buttonWidth = btnHostMain.Width;
            int buttonHeight = btnHostMain.Height;
            int buttonLeft = Math.Max(0, (width - buttonWidth) / 2);
            int hostTop = Math.Max(190, height / 2 - 34);

            lblTitle.Location = new System.Drawing.Point(0, Math.Max(80, hostTop - 155));
            lblTitle.Size = new System.Drawing.Size(width, 72);
            btnHostMain.Location = new System.Drawing.Point(buttonLeft, hostTop);
            btnHostMain.Size = new System.Drawing.Size(buttonWidth, buttonHeight);
            btnJoinMain.Location = new System.Drawing.Point(buttonLeft, hostTop + buttonHeight + 17);
            btnJoinMain.Size = new System.Drawing.Size(buttonWidth, buttonHeight);
        }
    }
}
