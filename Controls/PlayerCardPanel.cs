using System;
using System.Drawing;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public class PlayerCardPanel : Panel
    {
        private readonly Label lblName = new Label();
        private readonly PictureBox picTank = new PictureBox();
        private readonly ComboBox cmbControl = new ComboBox();
        private readonly ComboBox cmbColor = new ComboBox();
        private readonly Button btnSelect = new Button();

        private GameResourceManager resources;
        private int[] colorValues = new int[0];
        private bool binding = false;

        public int PlayerId { get; private set; }

        public PlayerControlType SelectedControlType
        {
            get
            {
                return cmbControl.SelectedIndex == 0 ? PlayerControlType.Computer : PlayerControlType.HumanRemote;
            }
        }

        public int SelectedTankColorArgb
        {
            get
            {
                if (cmbColor.SelectedIndex >= 0 && cmbColor.SelectedIndex < colorValues.Length)
                {
                    return colorValues[cmbColor.SelectedIndex];
                }

                return PlayerSlot.GetDefaultTankColorArgb(PlayerId);
            }
        }

        public event EventHandler SelectPlayerClicked;
        public event EventHandler ControlTypeChanged;
        public event EventHandler TankColorChanged;

        public PlayerCardPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            Height = 224;
            Margin = new Padding(0, 0, 12, 12);
            BorderStyle = BorderStyle.FixedSingle;

            lblName.Font = new Font("微软雅黑", 15F, FontStyle.Bold);
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(lblName);

            picTank.BackColor = Color.FromArgb(18, 20, 23);
            picTank.BorderStyle = BorderStyle.FixedSingle;
            picTank.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(picTank);

            AddCaption("控制方式", 66);
            ConfigureComboBox(cmbControl);
            cmbControl.Items.Add("人机");
            cmbControl.Items.Add("玩家");
            cmbControl.SelectedIndexChanged += cmbControl_SelectedIndexChanged;
            Controls.Add(cmbControl);

            AddCaption("坦克颜色", 122);
            ConfigureComboBox(cmbColor);
            cmbColor.SelectedIndexChanged += cmbColor_SelectedIndexChanged;
            Controls.Add(cmbColor);

            btnSelect.FlatStyle = FlatStyle.Flat;
            btnSelect.ForeColor = Color.White;
            btnSelect.Click += btnSelect_Click;
            Controls.Add(btnSelect);
        }

        public void Bind(
            PlayerSlot slot,
            int selfPlayerId,
            ClientRole role,
            GameResourceManager gameResources,
            string[] colorNames,
            int[] tankColorValues,
            string localClientToken)
        {
            binding = true;
            PlayerId = slot.PlayerId;
            resources = gameResources;
            colorValues = tankColorValues ?? new int[0];

            bool selected = slot.PlayerId == selfPlayerId;
            bool isHost = role == ClientRole.Host;
            bool occupiedByRemote = slot.ControlType == PlayerControlType.HumanRemote
                && !string.IsNullOrEmpty(slot.OwnerToken);
            bool occupiedByOther = role == ClientRole.Client
                && (slot.ControlType == PlayerControlType.HumanLocal
                    || (slot.ControlType == PlayerControlType.HumanRemote
                        && !string.IsNullOrEmpty(slot.OwnerToken)
                        && slot.OwnerToken != localClientToken));
            BackColor = selected
                ? Color.FromArgb(42, 58, 62)
                : occupiedByRemote
                    ? Color.FromArgb(38, 44, 52)
                    : Color.FromArgb(34, 38, 43);

            lblName.Text = slot.PlayerName;
            lblName.ForeColor = selected ? Color.Gold : occupiedByRemote ? Color.FromArgb(125, 206, 255) : Color.White;

            cmbControl.SelectedIndex = slot.ControlType == PlayerControlType.Computer ? 0 : 1;
            cmbControl.Enabled = isHost;

            cmbColor.Items.Clear();
            cmbColor.Items.AddRange(colorNames);
            cmbColor.SelectedIndex = GetColorIndex(slot.TankColorArgb);
            cmbColor.Enabled = isHost || selected;

            btnSelect.Text = selected ? "当前控制" : occupiedByOther ? "已占用" : occupiedByRemote ? "远端占用" : "选择控制";
            btnSelect.Enabled = isHost || selected || !occupiedByOther;
            btnSelect.BackColor = selected
                ? Color.FromArgb(157, 92, 48)
                : occupiedByRemote
                    ? Color.FromArgb(45, 91, 116)
                    : Color.FromArgb(50, 96, 123);

            RefreshPreview(slot.TankColorArgb);
            ArrangeControls();
            binding = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeControls();
        }

        private void AddCaption(string text, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Color.Silver;
            label.Location = new Point(12, top);
            label.Size = new Size(160, 18);
            label.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(label);
        }

        private void ConfigureComboBox(ComboBox comboBox)
        {
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.Font = new Font("微软雅黑", 9F);
            comboBox.BackColor = Color.FromArgb(28, 31, 36);
            comboBox.ForeColor = Color.White;
        }

        private void ArrangeControls()
        {
            int innerWidth = Math.Max(120, Width - 24);
            lblName.Location = new Point(12, 10);
            lblName.Size = new Size(Math.Max(60, Width - 84), 32);

            picTank.Location = new Point(Math.Max(12, Width - 60), 10);
            picTank.Size = new Size(48, 48);

            cmbControl.Location = new Point(12, 85);
            cmbControl.Size = new Size(innerWidth, 30);

            cmbColor.Location = new Point(12, 141);
            cmbColor.Size = new Size(innerWidth, 30);

            btnSelect.Location = new Point(12, 184);
            btnSelect.Size = new Size(innerWidth, 30);
        }

        private int GetColorIndex(int tankColorArgb)
        {
            for (int i = 0; i < colorValues.Length; i++)
            {
                if (colorValues[i] == tankColorArgb)
                {
                    return i;
                }
            }

            return 0;
        }

        private void RefreshPreview(int tankColorArgb)
        {
            if (resources == null)
            {
                picTank.Image = null;
                return;
            }

            picTank.Image = resources.GetTankImage(PlayerId, Direction.Up, 0, tankColorArgb);
        }

        private void cmbControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!binding && ControlTypeChanged != null)
            {
                ControlTypeChanged(this, EventArgs.Empty);
            }
        }

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (binding)
            {
                return;
            }

            RefreshPreview(SelectedTankColorArgb);

            if (TankColorChanged != null)
            {
                TankColorChanged(this, EventArgs.Empty);
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (SelectPlayerClicked != null)
            {
                SelectPlayerClicked(this, EventArgs.Empty);
            }
        }
    }
}
