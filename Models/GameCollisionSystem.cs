using System.Drawing;

namespace TankBattleOnline
{
    public class GameCollisionSystem
    {
        public bool IsTankBlocked(GameState state, Tank tank)
        {
            if (tank.DrawBounds.Left < 0 || tank.DrawBounds.Top < 0 ||
                tank.DrawBounds.Right > state.MapWidth ||
                tank.DrawBounds.Bottom > state.MapHeight)
            {
                return true;
            }

            foreach (MapBlock block in state.Blocks)
            {
                if (!block.Visible || block.CanPass)
                {
                    continue;
                }

                if (tank.CollisionBounds.IntersectsWith(block.Bounds))
                {
                    return true;
                }
            }

            foreach (Tank other in state.Tanks)
            {
                if (other.PlayerId == tank.PlayerId || !other.Alive || other.Hp <= 0)
                {
                    continue;
                }

                if (tank.CollisionBounds.IntersectsWith(other.CollisionBounds))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanSpawnAt(GameState state, Rectangle rect)
        {
            foreach (MapBlock block in state.Blocks)
            {
                if (block.Visible && block.Bounds.IntersectsWith(rect))
                {
                    return false;
                }
            }

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Alive && tank.DrawBounds.IntersectsWith(rect))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
