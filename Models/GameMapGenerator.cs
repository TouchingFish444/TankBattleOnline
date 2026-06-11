using System;

namespace TankBattleOnline
{
    public class GameMapGenerator
    {
        private readonly Random random;

        public GameMapGenerator(Random random)
        {
            this.random = random;
        }

        public void Generate(GameState state)
        {
            state.Blocks.Clear();
            int cols = state.Config.MapColumns;
            int rows = state.Config.MapRows;
            int maxGroupCol = cols - 1;
            int maxGroupRow = rows - 1;

            for (int row = 0; row < maxGroupRow; row += 2)
            {
                for (int col = 0; col < maxGroupCol; col += 2)
                {
                    BlockType type = PickBlockType();

                    if (type == BlockType.Brick || type == BlockType.Steel)
                    {
                        AddBlockGroup(state, col, row, type);
                    }
                    else if (type == BlockType.Grass || type == BlockType.Water)
                    {
                        AddLargeBlock(state, col, row, type);
                    }
                }
            }
        }

        private BlockType PickBlockType()
        {
            int value = random.Next(100);

            if (value < 18)
            {
                return BlockType.Brick;
            }

            if (value < 25)
            {
                return BlockType.Steel;
            }

            if (value < 32)
            {
                return BlockType.Water;
            }

            if (value < 46)
            {
                return BlockType.Grass;
            }

            return BlockType.Empty;
        }

        private void AddBlockGroup(GameState state, int gridX, int gridY, BlockType type)
        {
            AddSmallBlock(state, gridX, gridY, type);
            AddSmallBlock(state, gridX + 1, gridY, type);
            AddSmallBlock(state, gridX, gridY + 1, type);
            AddSmallBlock(state, gridX + 1, gridY + 1, type);
        }

        private void AddSmallBlock(GameState state, int gridX, int gridY, BlockType type)
        {
            AddBlock(state, gridX, gridY, state.Config.BlockSize, type);
        }

        private void AddLargeBlock(GameState state, int gridX, int gridY, BlockType type)
        {
            AddBlock(state, gridX, gridY, state.Config.BlockSize * 2, type);
        }

        private void AddBlock(GameState state, int gridX, int gridY, int size, BlockType type)
        {
            int blockSize = state.Config.BlockSize;
            MapBlock block = new MapBlock();
            block.GridX = gridX;
            block.GridY = gridY;
            block.X = gridX * blockSize;
            block.Y = gridY * blockSize;
            block.Size = size;
            block.Type = type;
            block.Visible = true;
            state.Blocks.Add(block);
        }
    }
}
