using System;
using System.Collections.Generic;

namespace TankBattleOnline
{
    public class GameAiController
    {
        private readonly Random random;
        private readonly Dictionary<int, Direction> directions = new Dictionary<int, Direction>();
        private readonly Dictionary<int, int> thinkTicks = new Dictionary<int, int>();

        public GameAiController(Random random)
        {
            this.random = random;
        }

        public PlayerInput BuildInput(GameState state, Tank tank)
        {
            PlayerInput input = new PlayerInput();
            Tank target = FindNearestTarget(state, tank);

            if (target == null)
            {
                return input;
            }

            Direction? fireDirection = GetFireDirection(state, tank, target);

            if (fireDirection.HasValue)
            {
                tank.Direction = fireDirection.Value;
                input.Fire = true;
                return input;
            }

            EnsureTracked(tank.PlayerId);
            UpdateMoveDirection(tank, target);
            ApplyMoveInput(input, directions[tank.PlayerId]);

            if (random.Next(100) < 4)
            {
                input.Fire = true;
            }

            return input;
        }

        public void HandleBlocked(Tank tank)
        {
            thinkTicks[tank.PlayerId] = 0;
            directions[tank.PlayerId] = (Direction)random.Next(0, 4);
        }

        private void EnsureTracked(int playerId)
        {
            if (directions.ContainsKey(playerId))
            {
                return;
            }

            directions[playerId] = Direction.Up;
            thinkTicks[playerId] = 0;
        }

        private void UpdateMoveDirection(Tank tank, Tank target)
        {
            if (thinkTicks[tank.PlayerId] > 0)
            {
                thinkTicks[tank.PlayerId]--;
                return;
            }

            int dx = target.CenterX - tank.CenterX;
            int dy = target.CenterY - tank.CenterY;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                directions[tank.PlayerId] = dx < 0 ? Direction.Left : Direction.Right;
            }
            else
            {
                directions[tank.PlayerId] = dy < 0 ? Direction.Up : Direction.Down;
            }

            if (random.Next(100) < 22)
            {
                directions[tank.PlayerId] = (Direction)random.Next(0, 4);
            }

            thinkTicks[tank.PlayerId] = random.Next(10, 30);
        }

        private void ApplyMoveInput(PlayerInput input, Direction direction)
        {
            if (direction == Direction.Up)
            {
                input.Up = true;
            }
            else if (direction == Direction.Down)
            {
                input.Down = true;
            }
            else if (direction == Direction.Left)
            {
                input.Left = true;
            }
            else
            {
                input.Right = true;
            }
        }

        private Tank FindNearestTarget(GameState state, Tank tank)
        {
            Tank target = null;
            int bestDistance = int.MaxValue;

            foreach (Tank item in state.Tanks)
            {
                if (item.PlayerId == tank.PlayerId || item.Hp <= 0 || !item.Alive)
                {
                    continue;
                }

                int dx = item.CenterX - tank.CenterX;
                int dy = item.CenterY - tank.CenterY;
                int distance = dx * dx + dy * dy;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    target = item;
                }
            }

            return target;
        }

        private Direction? GetFireDirection(GameState state, Tank shooter, Tank target)
        {
            int tolerance = Math.Max(8, state.Config.BlockSize / 3);

            if (Math.Abs(shooter.CenterX - target.CenterX) <= tolerance)
            {
                return target.CenterY < shooter.CenterY ? Direction.Up : Direction.Down;
            }

            if (Math.Abs(shooter.CenterY - target.CenterY) <= tolerance)
            {
                return target.CenterX < shooter.CenterX ? Direction.Left : Direction.Right;
            }

            return null;
        }
    }
}
