using System;
using System.Drawing;

namespace TankBattleOnline
{
    public class GamePowerUpManager
    {
        private const int MaxUpgradeLevel = 3;

        private readonly Random random;
        private int nextSpawnIndex = 1;

        public GamePowerUpManager(Random random)
        {
            this.random = random;
        }

        public void ResetSchedule()
        {
            nextSpawnIndex = 1;
        }

        public void SpawnScheduled(GameState state)
        {
            int totalTicks = Math.Max(1, state.Config.RoundSeconds * 1000 / state.Config.TickMilliseconds);

            while (nextSpawnIndex < 5)
            {
                int spawnTick = totalTicks * nextSpawnIndex / 5;

                if (spawnTick <= 0)
                {
                    spawnTick = 1;
                }

                if (state.TickCount < spawnTick)
                {
                    return;
                }

                SpawnStar(state);
                nextSpawnIndex++;
            }
        }

        public void Collect(GameState state, Tank tank)
        {
            foreach (PowerUp powerUp in state.PowerUps)
            {
                if (!powerUp.Visible)
                {
                    continue;
                }

                if (tank.CollisionBounds.IntersectsWith(powerUp.Bounds))
                {
                    powerUp.Visible = false;
                    tank.UpgradeLevel = Math.Min(MaxUpgradeLevel, tank.UpgradeLevel + 1);
                }
            }

            state.PowerUps.RemoveAll(powerUp => !powerUp.Visible);
        }

        public void SpawnStar(GameState state)
        {
            Rectangle bounds = GetRandomPowerUpBounds(state);

            if (bounds == Rectangle.Empty)
            {
                return;
            }

            PowerUp powerUp = new PowerUp();
            powerUp.X = bounds.X;
            powerUp.Y = bounds.Y;
            powerUp.Size = bounds.Width;
            powerUp.Visible = true;
            state.PowerUps.Add(powerUp);
        }

        private Rectangle GetRandomPowerUpBounds(GameState state)
        {
            int size = Math.Max(30, state.Config.BlockSize + 10);
            int maxX = Math.Max(0, state.MapWidth - size);
            int maxY = Math.Max(0, state.MapHeight - size);

            for (int i = 0; i < 300; i++)
            {
                int x = random.Next(0, maxX + 1);
                int y = random.Next(0, maxY + 1);
                Rectangle rect = new Rectangle(x, y, size, size);

                if (CanPlaceAt(state, rect))
                {
                    return rect;
                }
            }

            for (int y = 0; y <= maxY; y += Math.Max(1, state.Config.BlockSize))
            {
                for (int x = 0; x <= maxX; x += Math.Max(1, state.Config.BlockSize))
                {
                    Rectangle rect = new Rectangle(x, y, size, size);

                    if (CanPlaceAt(state, rect))
                    {
                        return rect;
                    }
                }
            }

            return Rectangle.Empty;
        }

        private bool CanPlaceAt(GameState state, Rectangle rect)
        {
            foreach (MapBlock block in state.Blocks)
            {
                if (!block.Visible)
                {
                    continue;
                }

                if (!block.CanPass && block.Bounds.IntersectsWith(rect))
                {
                    return false;
                }
            }

            foreach (PowerUp powerUp in state.PowerUps)
            {
                if (powerUp.Visible && powerUp.Bounds.IntersectsWith(rect))
                {
                    return false;
                }
            }

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Alive && tank.Hp > 0 && tank.DrawBounds.IntersectsWith(rect))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
