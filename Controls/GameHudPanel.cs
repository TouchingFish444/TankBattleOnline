using System;
using System.Drawing;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public class GameHudPanel : Panel
    {
        private readonly Label lblTitle = new Label();
        private readonly Label lblPlayer = new Label();
        private readonly Label lblHp = new Label();
        private readonly Label lblTime = new Label();
        private readonly Label lblRound = new Label();
        private readonly Label lblWins = new Label();
        private readonly Label lblFps = new Label();
        private readonly Label lblMode = new Label();
        private readonly Button btnLeaveGame = new Button();

        public event EventHandler LeaveGameClicked;

        public GameHudPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            Width = 220;
            BackColor = Color.FromArgb(16, 18, 21);

            lblTitle.Text = "玩家信息";
            lblTitle.Font = new Font("微软雅黑", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            Controls.Add(lblTitle);

            lblPlayer.Font = new Font("微软雅黑", 13F, FontStyle.Bold);
            lblPlayer.ForeColor = Color.Gold;
            lblPlayer.AutoEllipsis = true;
            Controls.Add(lblPlayer);

            ConfigureStatusLabel(lblHp);
            ConfigureStatusLabel(lblTime);
            ConfigureStatusLabel(lblRound);
            ConfigureStatusLabel(lblWins);
            ConfigureStatusLabel(lblFps);

            lblMode.Font = new Font("微软雅黑", 9F);
            lblMode.ForeColor = Color.FromArgb(170, 176, 184);
            lblMode.AutoEllipsis = true;
            Controls.Add(lblMode);

            btnLeaveGame.Text = "返回房间";
            btnLeaveGame.BackColor = Color.FromArgb(66, 68, 73);
            btnLeaveGame.FlatStyle = FlatStyle.Flat;
            btnLeaveGame.ForeColor = Color.White;
            btnLeaveGame.Click += btnLeaveGame_Click;
            Controls.Add(btnLeaveGame);
        }

        public void Bind(GameState state, int selfPlayerId, ClientRole role, string connectionText, int currentFps)
        {
            Tank self = state.GetTank(selfPlayerId);

            if (self == null && state.Tanks.Count > 0)
            {
                self = state.Tanks[0];
            }

            if (self != null)
            {
                lblPlayer.Text = "Player：P" + self.PlayerId.ToString();
                lblHp.Text = "生命：" + Math.Max(0, self.Hp).ToString() + " / 强化：" + self.UpgradeLevel.ToString();
                lblWins.Text = "得分：" + self.Score.ToString() + " / 胜局：" + self.Wins.ToString();
            }
            else
            {
                lblPlayer.Text = "Player：P" + selfPlayerId.ToString();
                lblHp.Text = "生命：- / 强化：-";
                lblWins.Text = "得分：0 / 胜局：0";
            }

            lblTime.Text = "倒计时：" + state.RemainingSeconds.ToString();
            lblRound.Text = "局数：" + state.RoundNumber.ToString() + "/" + state.TotalRounds.ToString();
            lblFps.Text = "FPS：" + currentFps.ToString();

            string roleText = role == ClientRole.Host ? "主持游戏" : "加入游戏";
            lblMode.Text = "模式：" + roleText + "\r\n连接：" + connectionText;

            if (!string.IsNullOrEmpty(state.ResultText))
            {
                lblMode.Text += "\r\n" + state.ResultText;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeControls();
        }

        private void ConfigureStatusLabel(Label label)
        {
            label.Font = new Font("微软雅黑", 12F);
            label.ForeColor = Color.White;
            label.AutoEllipsis = true;
            Controls.Add(label);
        }

        private void ArrangeControls()
        {
            int contentWidth = Math.Max(120, ClientSize.Width - 48);
            int buttonTop = Math.Max(380, ClientSize.Height - 88);

            lblTitle.Location = new Point(24, 28);
            lblTitle.Size = new Size(contentWidth, 40);
            lblPlayer.Location = new Point(24, 78);
            lblPlayer.Size = new Size(contentWidth, 30);
            lblHp.Location = new Point(24, 118);
            lblHp.Size = new Size(contentWidth, 30);
            lblTime.Location = new Point(24, 156);
            lblTime.Size = new Size(contentWidth, 30);
            lblRound.Location = new Point(24, 194);
            lblRound.Size = new Size(contentWidth, 30);
            lblWins.Location = new Point(24, 232);
            lblWins.Size = new Size(contentWidth, 30);
            lblFps.Location = new Point(24, 270);
            lblFps.Size = new Size(contentWidth, 30);
            lblMode.Location = new Point(24, 308);
            lblMode.Size = new Size(contentWidth, Math.Max(70, buttonTop - 324));

            btnLeaveGame.Location = new Point(24, buttonTop);
            btnLeaveGame.Size = new Size(contentWidth, 38);
        }

        private void btnLeaveGame_Click(object sender, EventArgs e)
        {
            if (LeaveGameClicked != null)
            {
                LeaveGameClicked(this, EventArgs.Empty);
            }
        }
    }
}
