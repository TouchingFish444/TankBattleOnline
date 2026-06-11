using System;
using System.Drawing;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public class PlayerConfigPanel : Panel
    {
        private readonly Label lblTitle = new Label();
        private readonly Button btnAddPlayer = new Button();
        private readonly Button btnRemovePlayer = new Button();
        private readonly FlowLayoutPanel cardsPanel = new BufferedFlowLayoutPanel();

        public int SelectedPlayerId { get; private set; }
        public PlayerControlType SelectedControlType { get; private set; }
        public int SelectedTankColorArgb { get; private set; }

        public event EventHandler AddPlayerClicked;
        public event EventHandler RemovePlayerClicked;
        public event EventHandler SelectPlayerClicked;
        public event EventHandler ControlTypeChanged;
        public event EventHandler TankColorChanged;

        public PlayerConfigPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            BackColor = Color.FromArgb(27, 30, 34);

            lblTitle.Text = "Player 配置";
            lblTitle.Font = new Font("微软雅黑", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            Controls.Add(lblTitle);

            ConfigureButton(btnAddPlayer, "添加 Player", Color.FromArgb(157, 92, 48));
            btnAddPlayer.Click += RaiseAddPlayerClicked;
            Controls.Add(btnAddPlayer);

            ConfigureButton(btnRemovePlayer, "删除 Player", Color.FromArgb(91, 48, 48));
            btnRemovePlayer.Click += RaiseRemovePlayerClicked;
            Controls.Add(btnRemovePlayer);

            cardsPanel.AutoScroll = true;
            cardsPanel.WrapContents = true;
            cardsPanel.FlowDirection = FlowDirection.LeftToRight;
            cardsPanel.BackColor = BackColor;
            cardsPanel.Padding = new Padding(0, 0, 0, 16);
            Controls.Add(cardsPanel);
        }

        public void Bind(
            RoomState room,
            int selfPlayerId,
            ClientRole role,
            GameResourceManager resources,
            string[] colorNames,
            int[] colorValues,
            string localClientToken)
        {
            bool isHost = role == ClientRole.Host;
            btnAddPlayer.Enabled = isHost;
            btnRemovePlayer.Enabled = isHost;

            SuspendLayout();
            cardsPanel.SuspendLayout();

            for (int i = cardsPanel.Controls.Count - 1; i >= 0; i--)
            {
                cardsPanel.Controls[i].Dispose();
            }

            cardsPanel.Controls.Clear();

            foreach (PlayerSlot slot in room.Players)
            {
                PlayerCardPanel card = new PlayerCardPanel();
                card.Width = GetCardWidth();
                card.Bind(slot, selfPlayerId, role, resources, colorNames, colorValues, localClientToken);
                card.SelectPlayerClicked += card_SelectPlayerClicked;
                card.ControlTypeChanged += card_ControlTypeChanged;
                card.TankColorChanged += card_TankColorChanged;
                cardsPanel.Controls.Add(card);
            }

            cardsPanel.ResumeLayout();
            ResumeLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeControls();
            UpdateCardWidths();
        }

        private void ArrangeControls()
        {
            int left = 36;
            lblTitle.Location = new Point(left, 32);
            lblTitle.Size = new Size(Math.Max(240, Width - left * 2), 42);

            btnAddPlayer.Location = new Point(left + 2, 88);
            btnAddPlayer.Size = new Size(140, 36);

            btnRemovePlayer.Location = new Point(left + 158, 88);
            btnRemovePlayer.Size = new Size(140, 36);

            int top = 140;
            cardsPanel.Bounds = new Rectangle(
                left,
                top,
                Math.Max(320, ClientSize.Width - left * 2),
                Math.Max(240, ClientSize.Height - top - 28));
        }

        private void ConfigureButton(Button button, string text, Color backColor)
        {
            button.Text = text;
            button.BackColor = backColor;
            button.FlatStyle = FlatStyle.Flat;
            button.ForeColor = Color.White;
        }

        private int GetCardWidth()
        {
            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth + 16;
            int availableWidth = Math.Max(360, cardsPanel.ClientSize.Width - scrollbarWidth - 36);
            int gap = 12;
            return Math.Max(180, (availableWidth - gap * 3) / 4);
        }

        private void UpdateCardWidths()
        {
            int cardWidth = GetCardWidth();

            foreach (Control control in cardsPanel.Controls)
            {
                control.Width = cardWidth;
            }
        }

        private void RaiseAddPlayerClicked(object sender, EventArgs e)
        {
            if (AddPlayerClicked != null)
            {
                AddPlayerClicked(this, EventArgs.Empty);
            }
        }

        private void RaiseRemovePlayerClicked(object sender, EventArgs e)
        {
            if (RemovePlayerClicked != null)
            {
                RemovePlayerClicked(this, EventArgs.Empty);
            }
        }

        private void card_SelectPlayerClicked(object sender, EventArgs e)
        {
            PlayerCardPanel card = sender as PlayerCardPanel;

            if (card == null)
            {
                return;
            }

            SelectedPlayerId = card.PlayerId;

            if (SelectPlayerClicked != null)
            {
                SelectPlayerClicked(this, EventArgs.Empty);
            }
        }

        private void card_ControlTypeChanged(object sender, EventArgs e)
        {
            PlayerCardPanel card = sender as PlayerCardPanel;

            if (card == null)
            {
                return;
            }

            SelectedPlayerId = card.PlayerId;
            SelectedControlType = card.SelectedControlType;

            if (ControlTypeChanged != null)
            {
                ControlTypeChanged(this, EventArgs.Empty);
            }
        }

        private void card_TankColorChanged(object sender, EventArgs e)
        {
            PlayerCardPanel card = sender as PlayerCardPanel;

            if (card == null)
            {
                return;
            }

            SelectedPlayerId = card.PlayerId;
            SelectedTankColorArgb = card.SelectedTankColorArgb;

            if (TankColorChanged != null)
            {
                TankColorChanged(this, EventArgs.Empty);
            }
        }

        private class BufferedFlowLayoutPanel : FlowLayoutPanel
        {
            public BufferedFlowLayoutPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                UpdateStyles();
            }
        }
    }
}
