using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public partial class GameForm : Form
    {
        private readonly GameResourceManager resources = new GameResourceManager();
        private readonly NetworkManager network = new NetworkManager();
        private readonly RoomManager roomManager = new RoomManager();
        private readonly PlayerInput localInput = new PlayerInput();
        private readonly Dictionary<int, PlayerInput> remoteInputs = new Dictionary<int, PlayerInput>();
        private readonly Dictionary<string, int> remotePlayerSelections = new Dictionary<string, int>();
        private readonly Dictionary<int, string> remoteConnectionTokens = new Dictionary<int, string>();
        private readonly Dictionary<int, int> pendingPingTicks = new Dictionary<int, int>();
        private readonly string clientToken = Guid.NewGuid().ToString("N");

        private GameEngine engine = new GameEngine();
        private ClientRole currentRole = ClientRole.None;
        private AppPage currentPage = AppPage.MainMenu;
        private int selfPlayerId = 1;
        private int roundEndDelayTicks = 0;
        private int nextPingId = 1;
        private int lastPingTick = 0;
        private int localLatencyMs = -1;
        private int fpsWindowTick = 0;
        private int fpsFrameCount = 0;
        private int currentFps = 0;
        private string connectedHostIp = "";
        private string connectionText = "未连接";
        private string roomMessage = "";
        private bool refreshingRoomControls = false;
        private RoomInfoPanel roomInfoView;
        private PlayerConfigPanel playerConfigView;
        private GameCanvasPanel gameCanvasView;
        private GameHudPanel gameHudView;

        private readonly string[] playerColorNames =
        {
            "金色",
            "青色",
            "绿色",
            "红色",
            "紫色",
            "橙色",
            "白色",
            "蓝色"
        };

        private readonly int[] playerColorValues = PlayerSlot.GetDefaultTankColorValues();

        public GameForm()
        {
            InitializeComponent();
            BindCustomPanelEvents();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            resources.LoadAll();
            roomManager.CreateDefaultRoom();
            roomManager.Room.Config.HostIp = LocalNetworkInfo.GetLocalIPv4();
            roomInfoView.LoadRoom(roomManager.Room);
            ShowMainMenu();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            network.Stop();
        }

        private void BindCustomPanelEvents()
        {
            roomInfoView = panelRoomLeft;
            roomInfoView.OpenRoomClicked += btnOpenRoom_Click;
            roomInfoView.JoinRoomClicked += btnJoinRoom_Click;
            roomInfoView.StartMatchClicked += btnStartMatch_Click;
            roomInfoView.BackToMenuClicked += btnBackToMenu_Click;

            playerConfigView = panelPlayerConfig;
            playerConfigView.AddPlayerClicked += btnAddPlayer_Click;
            playerConfigView.RemovePlayerClicked += btnRemovePlayer_Click;
            playerConfigView.SelectPlayerClicked += btnSelectPlayer_Click;
            playerConfigView.ControlTypeChanged += playerConfig_ControlTypeChanged;
            playerConfigView.TankColorChanged += playerConfig_TankColorChanged;

            gameHudView = panelHud;
            gameHudView.LeaveGameClicked += btnLeaveGame_Click;

            gameCanvasView = canvasGame;
        }
    }
}
