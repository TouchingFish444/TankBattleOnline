namespace TankBattleOnline
{
    partial class GameForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelStart = new System.Windows.Forms.Panel();
            this.btnJoinMain = new System.Windows.Forms.Button();
            this.btnHostMain = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelRoom = new System.Windows.Forms.Panel();
            this.panelPlayerConfig = new TankBattleOnline.PlayerConfigPanel();
            this.panelRoomLeft = new TankBattleOnline.RoomInfoPanel();
            this.panelGame = new System.Windows.Forms.Panel();
            this.canvasGame = new TankBattleOnline.GameCanvasPanel();
            this.panelHud = new TankBattleOnline.GameHudPanel();
            this.timerGame = new System.Windows.Forms.Timer(this.components);
            this.panelStart.SuspendLayout();
            this.panelRoom.SuspendLayout();
            this.panelGame.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelStart
            // 
            this.panelStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.panelStart.Controls.Add(this.btnJoinMain);
            this.panelStart.Controls.Add(this.btnHostMain);
            this.panelStart.Controls.Add(this.lblTitle);
            this.panelStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStart.Location = new System.Drawing.Point(0, 0);
            this.panelStart.Name = "panelStart";
            this.panelStart.Size = new System.Drawing.Size(1152, 648);
            this.panelStart.TabIndex = 0;
            // 
            // btnJoinMain
            // 
            this.btnJoinMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(96)))), ((int)(((byte)(123)))));
            this.btnJoinMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJoinMain.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btnJoinMain.ForeColor = System.Drawing.Color.White;
            this.btnJoinMain.Location = new System.Drawing.Point(486, 355);
            this.btnJoinMain.Name = "btnJoinMain";
            this.btnJoinMain.Size = new System.Drawing.Size(180, 48);
            this.btnJoinMain.TabIndex = 2;
            this.btnJoinMain.Text = "加入游戏";
            this.btnJoinMain.UseVisualStyleBackColor = false;
            this.btnJoinMain.Click += new System.EventHandler(this.btnJoinMain_Click);
            // 
            // btnHostMain
            // 
            this.btnHostMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(92)))), ((int)(((byte)(48)))));
            this.btnHostMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHostMain.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btnHostMain.ForeColor = System.Drawing.Color.White;
            this.btnHostMain.Location = new System.Drawing.Point(486, 290);
            this.btnHostMain.Name = "btnHostMain";
            this.btnHostMain.Size = new System.Drawing.Size(180, 48);
            this.btnHostMain.TabIndex = 1;
            this.btnHostMain.Text = "主持游戏";
            this.btnHostMain.UseVisualStyleBackColor = false;
            this.btnHostMain.Click += new System.EventHandler(this.btnHostMain_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 32F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.Gold;
            this.lblTitle.Location = new System.Drawing.Point(0, 135);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1152, 72);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "TankBattleOnline";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelRoom
            // 
            this.panelRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(24)))), ((int)(((byte)(27)))));
            this.panelRoom.Controls.Add(this.panelPlayerConfig);
            this.panelRoom.Controls.Add(this.panelRoomLeft);
            this.panelRoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRoom.Location = new System.Drawing.Point(0, 0);
            this.panelRoom.Name = "panelRoom";
            this.panelRoom.Size = new System.Drawing.Size(1152, 648);
            this.panelRoom.TabIndex = 1;
            this.panelRoom.Visible = false;
            // 
            // panelPlayerConfig
            // 
            this.panelPlayerConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPlayerConfig.Location = new System.Drawing.Point(288, 0);
            this.panelPlayerConfig.Name = "panelPlayerConfig";
            this.panelPlayerConfig.Size = new System.Drawing.Size(864, 648);
            this.panelPlayerConfig.TabIndex = 1;
            // 
            // panelRoomLeft
            // 
            this.panelRoomLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelRoomLeft.Location = new System.Drawing.Point(0, 0);
            this.panelRoomLeft.Name = "panelRoomLeft";
            this.panelRoomLeft.Size = new System.Drawing.Size(288, 648);
            this.panelRoomLeft.TabIndex = 0;
            // 
            // panelGame
            // 
            this.panelGame.BackColor = System.Drawing.Color.Black;
            this.panelGame.Controls.Add(this.canvasGame);
            this.panelGame.Controls.Add(this.panelHud);
            this.panelGame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGame.Location = new System.Drawing.Point(0, 0);
            this.panelGame.Name = "panelGame";
            this.panelGame.Size = new System.Drawing.Size(1152, 648);
            this.panelGame.TabIndex = 2;
            this.panelGame.Visible = false;
            // 
            // canvasGame
            // 
            this.canvasGame.BackColor = System.Drawing.Color.Black;
            this.canvasGame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvasGame.Location = new System.Drawing.Point(220, 0);
            this.canvasGame.Name = "canvasGame";
            this.canvasGame.Size = new System.Drawing.Size(932, 648);
            this.canvasGame.TabIndex = 1;
            // 
            // panelHud
            // 
            this.panelHud.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelHud.Location = new System.Drawing.Point(0, 0);
            this.panelHud.Name = "panelHud";
            this.panelHud.Size = new System.Drawing.Size(220, 648);
            this.panelHud.TabIndex = 0;
            // 
            // timerGame
            // 
            this.timerGame.Interval = 16;
            this.timerGame.Tick += new System.EventHandler(this.timerGame_Tick);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1152, 648);
            this.Controls.Add(this.panelRoom);
            this.Controls.Add(this.panelGame);
            this.Controls.Add(this.panelStart);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(960, 540);
            this.Name = "GameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TankBattleOnline";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameForm_FormClosing);
            this.Load += new System.EventHandler(this.GameForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameForm_KeyUp);
            this.panelStart.ResumeLayout(false);
            this.panelRoom.ResumeLayout(false);
            this.panelGame.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelStart;
        private System.Windows.Forms.Button btnJoinMain;
        private System.Windows.Forms.Button btnHostMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelRoom;
        private TankBattleOnline.PlayerConfigPanel panelPlayerConfig;
        private TankBattleOnline.RoomInfoPanel panelRoomLeft;
        private System.Windows.Forms.Panel panelGame;
        private TankBattleOnline.GameCanvasPanel canvasGame;
        private TankBattleOnline.GameHudPanel panelHud;
        private System.Windows.Forms.Timer timerGame;
    }
}
