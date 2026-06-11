using System;

namespace TankBattleOnline
{
    public static class RoomLayoutCalculator
    {
        public static int CalculateBlockSize(int clientWidth, int clientHeight, int hudWidth, int cols, int rows)
        {
            int safeCols = Math.Max(1, cols);
            int safeRows = Math.Max(1, rows);
            int availableWidth = Math.Max(1, clientWidth - hudWidth - 40);
            int availableHeight = Math.Max(1, clientHeight - 40);
            int maxFitSize = Math.Min(availableWidth / safeCols, availableHeight / safeRows);

            if (maxFitSize >= 30)
            {
                return 30;
            }

            return Math.Max(1, maxFitSize);
        }
    }
}
