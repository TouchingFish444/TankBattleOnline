using System;
using System.Collections.Generic;
using System.Drawing;

namespace TankBattleOnline
{
    public class GameEngine
    {
        public GameState State = new GameState();

        private readonly Random random = new Random();
        private readonly GameAiController aiController;
        private readonly GameMapGenerator mapGenerator;
        private readonly GamePowerUpManager powerUpManager;
        private readonly GameRoundResolver roundResolver = new GameRoundResolver();
        private readonly GameBulletSystem bulletSystem = new GameBulletSystem();
        private readonly GameCollisionSystem collisionSystem = new GameCollisionSystem();

        public GameEngine()
        {
            aiController = new GameAiController(random);
            mapGenerator = new GameMapGenerator(random);
            powerUpManager = new GamePowerUpManager(random);
        }

        public void StartMatch(RoomState room)
        {
            State = new GameState();
            State.ResetMatch(room.Config);

            foreach (PlayerSlot slot in room.Players)
            {
                Tank tank = new Tank();
                tank.PlayerId = slot.PlayerId;
                tank.PlayerName = slot.PlayerName;
                tank.ControlType = slot.ControlType;
                tank.Width = GetTankSize();
                tank.Height = GetTankSize();
                tank.CollisionInset = GetTankCollisionInset();
                tank.Speed = GetTankSpeed();
                tank.MaxHp = State.Config.InitialHp;
                tank.Hp = State.Config.InitialHp;
                tank.Wins = slot.Wins;
                tank.Score = slot.Score;
                tank.TankColorArgb = slot.TankColorArgb;
                State.Tanks.Add(tank);
            }

            StartNextRound();
        }

        public void StartNextRound()
        {
            if (State.Tanks.Count == 0)
            {
                return;
            }

            State.RoundNumber++;
            State.ResetRoundTransient();
            State.RemainingTicks = Math.Max(1, State.Config.RoundSeconds * 1000 / State.Config.TickMilliseconds);
            powerUpManager.ResetSchedule();

            mapGenerator.Generate(State);

            foreach (Tank tank in State.Tanks)
            {
                tank.Width = GetTankSize();
                tank.Height = GetTankSize();
                tank.CollisionInset = GetTankCollisionInset();
                tank.Speed = GetTankSpeed();
                tank.Hp = tank.MaxHp;
                tank.Alive = true;
                tank.RespawnTicks = 0;
                tank.FireCooldown = 0;
                tank.UpgradeLevel = 0;
                RespawnTank(tank);
            }
        }

        public void Update(Dictionary<int, PlayerInput> playerInputs)
        {
            State.ClearFrameEvents();

            if (State.RoundOver || State.MatchOver)
            {
                return;
            }

            State.TickCount++;
            State.RemainingTicks = Math.Max(0, State.RemainingTicks - 1);
            powerUpManager.SpawnScheduled(State);

            Dictionary<int, PlayerInput> inputs = BuildInputs(playerInputs);

            foreach (Tank tank in State.Tanks)
            {
                if (tank.Hp <= 0)
                {
                    tank.Alive = false;
                    continue;
                }

                if (!tank.Alive)
                {
                    UpdateRespawn(tank);
                    continue;
                }

                PlayerInput input = inputs.ContainsKey(tank.PlayerId) ? inputs[tank.PlayerId] : new PlayerInput();
                UpdateTank(tank, input);

                if (tank.FireCooldown > 0)
                {
                    tank.FireCooldown--;
                }
            }

            bulletSystem.Update(State, powerUpManager);
            roundResolver.CheckRoundEnd(State);
        }

        private Dictionary<int, PlayerInput> BuildInputs(Dictionary<int, PlayerInput> playerInputs)
        {
            Dictionary<int, PlayerInput> inputs = new Dictionary<int, PlayerInput>();

            if (playerInputs != null)
            {
                foreach (KeyValuePair<int, PlayerInput> item in playerInputs)
                {
                    if (item.Value != null)
                    {
                        inputs[item.Key] = item.Value.Clone();
                    }
                }
            }

            foreach (Tank tank in State.Tanks)
            {
                if (tank.ControlType == PlayerControlType.Computer && tank.Hp > 0 && tank.Alive)
                {
                    inputs[tank.PlayerId] = aiController.BuildInput(State, tank);
                }
            }

            return inputs;
        }

        private void UpdateRespawn(Tank tank)
        {
            if (tank.RespawnTicks > 0)
            {
                tank.RespawnTicks--;
            }

            if (tank.RespawnTicks <= 0 && tank.Hp > 0)
            {
                RespawnTank(tank);
            }
        }

        private void UpdateTank(Tank tank, PlayerInput input)
        {
            int oldX = tank.X;
            int oldY = tank.Y;

            if (input.Up)
            {
                tank.Direction = Direction.Up;
                tank.Y -= tank.Speed;
            }
            else if (input.Down)
            {
                tank.Direction = Direction.Down;
                tank.Y += tank.Speed;
            }
            else if (input.Left)
            {
                tank.Direction = Direction.Left;
                tank.X -= tank.Speed;
            }
            else if (input.Right)
            {
                tank.Direction = Direction.Right;
                tank.X += tank.Speed;
            }

            if (collisionSystem.IsTankBlocked(State, tank))
            {
                tank.X = oldX;
                tank.Y = oldY;

                if (tank.ControlType == PlayerControlType.Computer)
                {
                    aiController.HandleBlocked(tank);
                }
            }

            if (input.Fire)
            {
                bulletSystem.Fire(State, tank);
                input.ClearFire();
            }

            powerUpManager.Collect(State, tank);
        }

        private void RespawnTank(Tank tank)
        {
            Rectangle bounds = GetRandomSpawnBounds();
            tank.X = bounds.X;
            tank.Y = bounds.Y;
            tank.Direction = (Direction)random.Next(0, 4);
            tank.Alive = true;
            tank.RespawnTicks = 0;
            tank.FireCooldown = 0;
        }

        private Rectangle GetRandomSpawnBounds()
        {
            int blockSize = State.Config.BlockSize;
            int tankSize = GetTankSize();
            int maxCol = Math.Max(0, State.Config.MapColumns - 2);
            int maxRow = Math.Max(0, State.Config.MapRows - 2);

            for (int i = 0; i < 400; i++)
            {
                int col = random.Next(0, maxCol + 1);
                int row = random.Next(0, maxRow + 1);
                Rectangle rect = new Rectangle(col * blockSize, row * blockSize, tankSize, tankSize);

                if (collisionSystem.CanSpawnAt(State, rect))
                {
                    return rect;
                }
            }

            for (int row = 0; row <= maxRow; row++)
            {
                for (int col = 0; col <= maxCol; col++)
                {
                    Rectangle rect = new Rectangle(col * blockSize, row * blockSize, tankSize, tankSize);

                    if (collisionSystem.CanSpawnAt(State, rect))
                    {
                        return rect;
                    }
                }
            }

            return new Rectangle(0, 0, tankSize, tankSize);
        }

        private int GetTankSize()
        {
            return State.Config.BlockSize * 2;
        }

        private int GetTankSpeed()
        {
            return Math.Max(4, State.Config.BlockSize / 7);
        }

        private int GetTankCollisionInset()
        {
            return Math.Max(3, State.Config.BlockSize / 10);
        }
    }
}
