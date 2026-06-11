using System;

namespace TankBattleOnline
{
    public class GameBulletSystem
    {
        private const int RespawnDelayTicks = 30;
        private const int FireCooldownTicks = 0;

        public void Fire(GameState state, Tank tank)
        {
            if (tank.FireCooldown > 0 || !tank.Alive || tank.Hp <= 0)
            {
                return;
            }

            if (CountActiveBullets(state, tank.PlayerId) >= GetMaxActiveBullets(tank))
            {
                return;
            }

            Bullet bullet = new Bullet();
            bullet.OwnerId = tank.PlayerId;
            bullet.PowerLevel = tank.UpgradeLevel;
            bullet.Direction = tank.Direction;
            bullet.Width = Math.Max(8, state.Config.BlockSize / 3);
            bullet.Height = bullet.Width;
            bullet.Speed = GetBulletSpeed(state, tank);
            bullet.X = tank.CenterX - bullet.Width / 2;
            bullet.Y = tank.CenterY - bullet.Height / 2;
            bullet.LastX = bullet.X;
            bullet.LastY = bullet.Y;

            state.Bullets.Add(bullet);
            state.FireSoundCount++;
            tank.FireCooldown = FireCooldownTicks;
        }

        public void Update(GameState state, GamePowerUpManager powerUpManager)
        {
            foreach (Bullet bullet in state.Bullets)
            {
                if (!bullet.Alive)
                {
                    continue;
                }

                bullet.LastX = bullet.X;
                bullet.LastY = bullet.Y;

                if (bullet.Direction == Direction.Up)
                {
                    bullet.Y -= bullet.Speed;
                }
                else if (bullet.Direction == Direction.Down)
                {
                    bullet.Y += bullet.Speed;
                }
                else if (bullet.Direction == Direction.Left)
                {
                    bullet.X -= bullet.Speed;
                }
                else
                {
                    bullet.X += bullet.Speed;
                }

                CheckBulletCollision(state, powerUpManager, bullet);
            }

            CheckBulletBulletCollision(state);
            state.Bullets.RemoveAll(bullet => !bullet.Alive);
        }

        private int CountActiveBullets(GameState state, int playerId)
        {
            int count = 0;

            foreach (Bullet bullet in state.Bullets)
            {
                if (bullet.Alive && bullet.OwnerId == playerId)
                {
                    count++;
                }
            }

            return count;
        }

        private int GetMaxActiveBullets(Tank tank)
        {
            return tank.UpgradeLevel >= 2 ? 2 : 1;
        }

        private int GetBulletSpeed(GameState state, Tank tank)
        {
            int baseSpeed = Math.Max(8, state.Config.BlockSize / 4);

            if (tank.UpgradeLevel >= 1)
            {
                return baseSpeed + Math.Max(3, state.Config.BlockSize / 8);
            }

            return baseSpeed;
        }

        private void CheckBulletBulletCollision(GameState state)
        {
            for (int i = 0; i < state.Bullets.Count; i++)
            {
                Bullet first = state.Bullets[i];

                if (!first.Alive)
                {
                    continue;
                }

                for (int j = i + 1; j < state.Bullets.Count; j++)
                {
                    Bullet second = state.Bullets[j];

                    if (!second.Alive)
                    {
                        continue;
                    }

                    if (first.OwnerId == second.OwnerId)
                    {
                        continue;
                    }

                    if (first.Bounds.IntersectsWith(second.Bounds) ||
                        first.PathBounds.IntersectsWith(second.PathBounds))
                    {
                        first.Alive = false;
                        second.Alive = false;
                        break;
                    }
                }
            }
        }

        private void CheckBulletCollision(GameState state, GamePowerUpManager powerUpManager, Bullet bullet)
        {
            if (bullet.X < 0 || bullet.Y < 0 ||
                bullet.X > state.MapWidth || bullet.Y > state.MapHeight)
            {
                bullet.Alive = false;
                return;
            }

            foreach (MapBlock block in state.Blocks)
            {
                if (!block.Visible || block.Type == BlockType.Grass || block.Type == BlockType.Water)
                {
                    continue;
                }

                if (bullet.Bounds.IntersectsWith(block.Bounds))
                {
                    bullet.Alive = false;

                    if (block.CanDestroy)
                    {
                        DestroyBrickPair(state, bullet, block);
                    }
                    else if (block.Type == BlockType.Steel && bullet.PowerLevel >= 3)
                    {
                        block.Visible = false;
                    }

                    return;
                }
            }

            foreach (Tank tank in state.Tanks)
            {
                if (tank.PlayerId == bullet.OwnerId || !tank.Alive || tank.Hp <= 0)
                {
                    continue;
                }

                if (bullet.Bounds.IntersectsWith(tank.CollisionBounds))
                {
                    bullet.Alive = false;

                    if (tank.UpgradeLevel > 0)
                    {
                        tank.UpgradeLevel--;
                        powerUpManager.SpawnStar(state);
                    }
                    else
                    {
                        tank.Hp--;
                        tank.Alive = false;

                        Tank shooter = state.GetTank(bullet.OwnerId);

                        if (shooter != null)
                        {
                            shooter.Score++;
                        }

                        if (tank.Hp > 0)
                        {
                            tank.RespawnTicks = RespawnDelayTicks;
                        }
                        else
                        {
                            tank.RespawnTicks = 0;
                        }
                    }

                    return;
                }
            }
        }

        private void DestroyBrickPair(GameState state, Bullet bullet, MapBlock hitBlock)
        {
            hitBlock.Visible = false;

            int neighborX = hitBlock.GridX;
            int neighborY = hitBlock.GridY;

            if (bullet.Direction == Direction.Up || bullet.Direction == Direction.Down)
            {
                neighborX += hitBlock.GridX % 2 == 0 ? 1 : -1;
            }
            else
            {
                neighborY += hitBlock.GridY % 2 == 0 ? 1 : -1;
            }

            MapBlock neighbor = FindBlock(state, neighborX, neighborY);

            if (neighbor != null && neighbor.Visible && neighbor.Type == BlockType.Brick)
            {
                neighbor.Visible = false;
            }
        }

        private MapBlock FindBlock(GameState state, int gridX, int gridY)
        {
            foreach (MapBlock block in state.Blocks)
            {
                if (block.GridX == gridX && block.GridY == gridY)
                {
                    return block;
                }
            }

            return null;
        }
    }
}
