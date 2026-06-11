using System.Collections.Generic;

namespace TankBattleOnline
{
    public class GameRoundResolver
    {
        private const int RoundWinScore = 3;

        public void CheckRoundEnd(GameState state)
        {
            List<Tank> activePlayers = new List<Tank>();

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Hp > 0)
                {
                    activePlayers.Add(tank);
                }
            }

            if (activePlayers.Count <= 1)
            {
                List<int> winners = new List<int>();

                foreach (Tank tank in activePlayers)
                {
                    winners.Add(tank.PlayerId);
                }

                FinishRound(state, winners, "最后存活");
                return;
            }

            if (state.RemainingTicks <= 0)
            {
                FinishByHp(state);
            }
        }

        private void FinishByHp(GameState state)
        {
            int bestHp = -1;

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Hp > bestHp)
                {
                    bestHp = tank.Hp;
                }
            }

            List<int> winners = new List<int>();

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Hp == bestHp)
                {
                    winners.Add(tank.PlayerId);
                }
            }

            FinishRound(state, winners, "时间结束，按生命值判定");
        }

        private void FinishRound(GameState state, List<int> winnerIds, string reason)
        {
            state.RoundOver = true;
            state.RoundWinnerIds.Clear();

            foreach (int winnerId in winnerIds)
            {
                state.RoundWinnerIds.Add(winnerId);

                Tank tank = state.GetTank(winnerId);

                if (tank != null)
                {
                    tank.Wins++;
                    tank.Score += RoundWinScore;
                }
            }

            state.ResultText = "第 " + state.RoundNumber.ToString() + " 局结束：" + FormatWinners(winnerIds) + " 获胜（" + reason + "）";

            if (state.RoundNumber >= state.TotalRounds)
            {
                state.MatchOver = true;
                BuildMatchResult(state);
            }
        }

        private void BuildMatchResult(GameState state)
        {
            int bestScore = -1;

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Score > bestScore)
                {
                    bestScore = tank.Score;
                }
            }

            state.MatchWinnerIds.Clear();

            foreach (Tank tank in state.Tanks)
            {
                if (tank.Score == bestScore)
                {
                    state.MatchWinnerIds.Add(tank.PlayerId);
                }
            }

            state.ResultText = "比赛结束：" + FormatWinners(state.MatchWinnerIds) + " 总胜（最高分）";
        }

        private string FormatWinners(List<int> winnerIds)
        {
            if (winnerIds.Count == 0)
            {
                return "无人";
            }

            List<string> names = new List<string>();

            foreach (int id in winnerIds)
            {
                names.Add("P" + id.ToString());
            }

            return string.Join("、", names.ToArray());
        }
    }
}
