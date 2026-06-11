using System;
using System.Drawing;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public class RoomInfoPanel : Panel
    {
        private readonly Label lblTitle = new Label();
        private readonly Label lblMessage = new Label();
        private readonly Label lblSelfPlayerCaption = new Label();
        private readonly Label lblHostIpCaption = new Label();
        private readonly ComboBox cmbSelfPlayer = new ComboBox();
        private readonly TextBox txtHostIp = new TextBox();
        private readonly TextBox txtPort = new TextBox();
        private readonly TextBox txtRoomName = new TextBox();
        private readonly NumericUpDown numHp = new NumericUpDown();
        private readonly NumericUpDown numRounds = new NumericUpDown();
        private readonly NumericUpDown numRoundSeconds = new NumericUpDown();
        private readonly NumericUpDown numMapCols = new NumericUpDown();
        private readonly NumericUpDown numMapRows = new NumericUpDown();
        private readonly Button btnOpenRoom = new Button();
        private readonly Button btnJoinRoom = new Button();
        private readonly Button btnStartMatch = new Button();
        private readonly Button btnBackToMenu = new Button();

        private bool binding = false;

        public int SelectedPlayerId { get; private set; }

        public event EventHandler OpenRoomClicked;
        public event EventHandler JoinRoomClicked;
        public event EventHandler StartMatchClicked;
        public event EventHandler BackToMenuClicked;

        public RoomInfoPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            Width = 288;
            BackColor = Color.FromArgb(16, 18, 21);

            lblTitle.Text = "房间信息";
            lblTitle.Font = new Font("微软雅黑", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            Controls.Add(lblTitle);

            cmbSelfPlayer.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSelfPlayer.SelectedIndexChanged += cmbSelfPlayer_SelectedIndexChanged;
            lblSelfPlayerCaption.Text = "选择控制 Player";
            lblSelfPlayerCaption.ForeColor = Color.Silver;
            Controls.Add(lblSelfPlayerCaption);
            Controls.Add(cmbSelfPlayer);

            lblHostIpCaption.Text = "IPv4 地址";
            lblHostIpCaption.ForeColor = Color.Silver;
            Controls.Add(lblHostIpCaption);
            Controls.Add(txtHostIp);
            Controls.Add(CreateLabel("端口", 166));
            Controls.Add(txtPort);
            Controls.Add(CreateLabel("房间名称", 220));
            Controls.Add(txtRoomName);
            Controls.Add(CreateLabel("生命值", 274));
            Controls.Add(numHp);
            Controls.Add(CreateLabel("游戏总局数", 328));
            Controls.Add(numRounds);
            Controls.Add(CreateLabel("每局时间（秒）", 382));
            Controls.Add(numRoundSeconds);
            Controls.Add(CreateLabel("地图大小（列 x 行）", 436));
            Controls.Add(numMapCols);
            Controls.Add(numMapRows);

            ConfigureNumber(numHp, 1, 20, 5);
            ConfigureNumber(numRounds, 1, 9, 3);
            ConfigureNumber(numRoundSeconds, 30, 600, 120);
            ConfigureNumber(numMapCols, 16, 40, 40);
            ConfigureNumber(numMapRows, 9, 24, 24);

            ConfigureButton(btnOpenRoom, "开放房间", Color.FromArgb(157, 92, 48));
            btnOpenRoom.Click += RaiseOpenRoomClicked;
            Controls.Add(btnOpenRoom);

            ConfigureButton(btnJoinRoom, "加入房间", Color.FromArgb(50, 96, 123));
            btnJoinRoom.Click += RaiseJoinRoomClicked;
            Controls.Add(btnJoinRoom);

            ConfigureButton(btnStartMatch, "开始游戏", Color.FromArgb(80, 120, 70));
            btnStartMatch.Click += RaiseStartMatchClicked;
            Controls.Add(btnStartMatch);

            ConfigureButton(btnBackToMenu, "返回主菜单", Color.FromArgb(66, 68, 73));
            btnBackToMenu.Click += RaiseBackToMenuClicked;
            Controls.Add(btnBackToMenu);

            lblMessage.ForeColor = Color.Silver;
            Controls.Add(lblMessage);
        }

        public void LoadRoom(RoomState room)
        {
            binding = true;
            RoomConfig config = room.Config;
            txtHostIp.Text = config.HostIp;
            txtPort.Text = config.Port.ToString();
            txtRoomName.Text = config.RoomName;
            numHp.Value = Clamp(config.InitialHp, numHp.Minimum, numHp.Maximum);
            numRounds.Value = Clamp(config.TotalRounds, numRounds.Minimum, numRounds.Maximum);
            numRoundSeconds.Value = Clamp(config.RoundSeconds, numRoundSeconds.Minimum, numRoundSeconds.Maximum);
            numMapCols.Value = Clamp(config.MapColumns, numMapCols.Minimum, numMapCols.Maximum);
            numMapRows.Value = Clamp(config.MapRows, numMapRows.Minimum, numMapRows.Maximum);

            cmbSelfPlayer.Items.Clear();

            foreach (PlayerSlot slot in room.Players)
            {
                cmbSelfPlayer.Items.Add("P" + slot.PlayerId.ToString());
            }

            SelectedPlayerId = room.SelectedPlayerId;
            SelectPlayer(room.SelectedPlayerId);
            binding = false;
        }

        public bool ApplyTo(RoomState room)
        {
            int port;

            if (!int.TryParse(txtPort.Text, out port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("端口必须是 1-65535 的数字。");
                return false;
            }

            RoomConfig config = room.Config;
            config.HostIp = txtHostIp.Text.Trim();
            config.Port = port;
            config.RoomName = txtRoomName.Text.Trim();
            config.InitialHp = (int)numHp.Value;
            config.TotalRounds = (int)numRounds.Value;
            config.RoundSeconds = (int)numRoundSeconds.Value;
            config.MapColumns = (int)numMapCols.Value;
            config.MapRows = (int)numMapRows.Value;
            room.SelectedPlayerId = SelectedPlayerId;
            return true;
        }

        public void SetRole(ClientRole role, string message)
        {
            bool isHost = role == ClientRole.Host;
            lblTitle.Text = isHost ? "房间信息：主持" : "房间信息：加入";
            lblHostIpCaption.Text = isHost ? "对外 IPv4 地址" : "主机 IPv4 地址";
            txtHostIp.Enabled = true;
            txtRoomName.Enabled = isHost;
            numHp.Enabled = isHost;
            numRounds.Enabled = isHost;
            numRoundSeconds.Enabled = isHost;
            numMapCols.Enabled = isHost;
            numMapRows.Enabled = isHost;
            btnOpenRoom.Visible = isHost;
            btnStartMatch.Visible = isHost;
            btnJoinRoom.Visible = !isHost;
            lblMessage.Text = message;
        }

        public void SetMessage(string message)
        {
            lblMessage.Text = message;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeControls();
        }

        private Label CreateLabel(string text, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Color.Silver;
            label.Location = new Point(24, top);
            label.Size = new Size(240, 18);
            return label;
        }

        private void ConfigureNumber(NumericUpDown number, int min, int max, int value)
        {
            number.Minimum = min;
            number.Maximum = max;
            number.Value = value;
        }

        private void ConfigureButton(Button button, string text, Color backColor)
        {
            button.Text = text;
            button.BackColor = backColor;
            button.FlatStyle = FlatStyle.Flat;
            button.ForeColor = Color.White;
        }

        private void ArrangeControls()
        {
            lblTitle.Location = new Point(24, 28);
            lblTitle.Size = new Size(240, 40);

            lblSelfPlayerCaption.Location = new Point(24, 68);
            lblSelfPlayerCaption.Size = new Size(240, 18);
            PlaceWide(cmbSelfPlayer, 88);
            lblHostIpCaption.Location = new Point(24, 116);
            lblHostIpCaption.Size = new Size(240, 18);
            PlaceWide(txtHostIp, 132);
            PlaceWide(txtPort, 186);
            PlaceWide(txtRoomName, 240);
            PlaceWide(numHp, 294);
            PlaceWide(numRounds, 348);
            PlaceWide(numRoundSeconds, 402);

            numMapCols.Location = new Point(24, 456);
            numMapCols.Size = new Size(106, 24);
            numMapRows.Location = new Point(146, 456);
            numMapRows.Size = new Size(106, 24);

            btnOpenRoom.Location = new Point(24, 502);
            btnOpenRoom.Size = new Size(106, 36);
            btnJoinRoom.Location = new Point(24, 502);
            btnJoinRoom.Size = new Size(228, 36);
            btnStartMatch.Location = new Point(146, 502);
            btnStartMatch.Size = new Size(106, 36);
            btnBackToMenu.Location = new Point(24, 550);
            btnBackToMenu.Size = new Size(228, 36);

            lblMessage.Location = new Point(24, 596);
            lblMessage.Size = new Size(240, 44);
        }

        private void PlaceWide(Control control, int top)
        {
            control.Location = new Point(24, top);
            control.Size = new Size(228, 24);
        }

        private decimal Clamp(int value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private void SelectPlayer(int playerId)
        {
            string text = "P" + playerId.ToString();

            for (int i = 0; i < cmbSelfPlayer.Items.Count; i++)
            {
                if (cmbSelfPlayer.Items[i].ToString() == text)
                {
                    cmbSelfPlayer.SelectedIndex = i;
                    return;
                }
            }

            if (cmbSelfPlayer.Items.Count > 0)
            {
                cmbSelfPlayer.SelectedIndex = 0;
                ParseSelectedPlayer();
            }
        }

        private void ParseSelectedPlayer()
        {
            if (cmbSelfPlayer.SelectedItem == null)
            {
                return;
            }

            string text = cmbSelfPlayer.SelectedItem.ToString().Replace("P", "");
            int playerId;

            if (int.TryParse(text, out playerId))
            {
                SelectedPlayerId = playerId;
            }
        }

        private void cmbSelfPlayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!binding)
            {
                ParseSelectedPlayer();
            }
        }

        private void RaiseOpenRoomClicked(object sender, EventArgs e)
        {
            if (OpenRoomClicked != null)
            {
                OpenRoomClicked(this, EventArgs.Empty);
            }
        }

        private void RaiseJoinRoomClicked(object sender, EventArgs e)
        {
            if (JoinRoomClicked != null)
            {
                JoinRoomClicked(this, EventArgs.Empty);
            }
        }

        private void RaiseStartMatchClicked(object sender, EventArgs e)
        {
            if (StartMatchClicked != null)
            {
                StartMatchClicked(this, EventArgs.Empty);
            }
        }

        private void RaiseBackToMenuClicked(object sender, EventArgs e)
        {
            if (BackToMenuClicked != null)
            {
                BackToMenuClicked(this, EventArgs.Empty);
            }
        }
    }
}
