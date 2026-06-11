# TankBattleOnline

TankBattleOnline 是一个基于 C# WinForms 的局域网坦克对战游戏。项目支持主机创建房间、客户端加入、多人/人机混合对战、随机地图、炮弹碰撞、道具升级、回合胜负结算和素材音效播放。

## 运行环境

- Windows
- Visual Studio 2022 或可构建 .NET Framework 项目的 MSBuild 环境
- .NET Framework 4.7.2

## 快速开始

使用 Visual Studio 打开 `TankBattleOnline.slnx`，选择 `Debug` 或 `Release` 配置后运行即可。

也可以在项目根目录执行：

```powershell
dotnet build .\TankBattleOnline.csproj -c Release
```

构建完成后，可执行文件位于：

```text
bin\Release\TankBattleOnline.exe
```

项目会通过 `TankBattleOnline.csproj` 将实际使用的素材嵌入程序集中，联机测试时可以直接分发 `TankBattleOnline.exe`。

## 联机流程

1. 主机点击“主持游戏”。
2. 主机配置房间参数、玩家数量、控制方式和坦克颜色。
3. 主机点击“开放房间”，记下界面显示的 IPv4 地址和端口。
4. 客户端点击“加入游戏”，填写主机 IPv4 和端口，选择要控制的 Player 后加入。
5. 主机点击“开始游戏”。

如果客户端无法连接，请检查双方是否在同一局域网、端口是否一致，以及 Windows 防火墙是否允许程序通信。

## 操作方式

- 移动：`WASD` 或方向键
- 开火：`J`

## 项目结构

```text
Controls/                  自定义 WinForms 控件和绘制面板
Forms/                     GameForm 主窗体及 partial 职责拆分
Models/                    游戏状态、规则、网络协议和资源管理
assets/                    图片与音频素材
Properties/                程序集信息
Program.cs                 应用入口
Forms/GameForm.cs          GameForm 共享状态、构造和事件绑定
Forms/GameForm.Room.cs     房间页面和玩家配置事件
Forms/GameForm.GameLoop.cs 游戏 tick、主机权威状态推进
Forms/GameForm.Network*.cs 主机/客户端网络消息处理与诊断
Forms/GameForm.Input.cs    键盘输入处理
Forms/GameForm.Navigation.cs 页面切换、HUD 和房间 UI 刷新
TankBattleOnline.csproj    项目文件
TankBattleOnline.slnx      解决方案文件
```

## 核心模块

- `GameEngine`：游戏状态推进入口，协调输入、坦克、子弹、道具和回合规则。
- `GameAiController`：人机坦克寻敌、移动和开火决策。
- `GameMapGenerator`：随机地图与障碍物生成。
- `GamePowerUpManager`：星星道具刷新、掉落和拾取升级。
- `GameRoundResolver`：回合结束、胜者判定和比赛总胜结果。
- `NetworkManager`：TCP 连接、消息收发和消息队列。
- `NetworkProtocol.*`：联机消息的创建和解析。

## 打包说明

默认发布为绿色版 zip：

```powershell
.\scripts\package-release.ps1
```

生成结果位于：

```text
dist\TankBattleOnline-Release.zip
```

压缩包内包含 `TankBattleOnline.exe`、运行配置、README 和素材许可证。游戏素材已嵌入 exe，不需要额外拷贝 `assets/` 目录。

## 素材说明

游戏图片和音频位于 `assets/`。素材包说明见 `assets/README.md`，许可证信息见 `assets/LICENSE`。
