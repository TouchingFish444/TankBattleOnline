using System;
using System.Collections.Generic;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        private bool ApplyRemoteLatency(string ownerToken, int latencyMs)
        {
            int playerId = FindPlayerOwnedByClient(ownerToken);
            PlayerSlot slot = roomManager.GetPlayer(playerId);

            if (slot == null || slot.LatencyMs == latencyMs)
            {
                return false;
            }

            slot.SetLatency(latencyMs);
            return true;
        }

        private void UpdateNetworkDiagnostics()
        {
            if (currentRole != ClientRole.Client || !network.IsRunning)
            {
                return;
            }

            int now = Environment.TickCount;

            if (lastPingTick == 0 || GetElapsedMilliseconds(lastPingTick, now) >= 1000)
            {
                int pingId = nextPingId++;

                if (nextPingId <= 0)
                {
                    nextPingId = 1;
                }

                pendingPingTicks[pingId] = now;
                lastPingTick = now;
                network.SendPing(clientToken, pingId, now);
            }

            RemoveExpiredPings(now);
        }

        private void ResetNetworkDiagnostics()
        {
            pendingPingTicks.Clear();
            lastPingTick = 0;
            localLatencyMs = -1;
        }

        private void ApplyPong(string pongClientToken, int pingId)
        {
            if (pongClientToken != clientToken || !pendingPingTicks.ContainsKey(pingId))
            {
                return;
            }

            int sentTick = pendingPingTicks[pingId];
            pendingPingTicks.Remove(pingId);
            localLatencyMs = GetElapsedMilliseconds(sentTick, Environment.TickCount);
            ApplyLocalLatency();
            network.SendLatency(clientToken, localLatencyMs);
            UpdateClientConnectionText();
        }

        private void ApplyLocalLatency()
        {
            int playerId = FindPlayerOwnedByClient(clientToken);
            PlayerSlot slot = roomManager.GetPlayer(playerId);

            if (slot != null)
            {
                slot.SetLatency(localLatencyMs);
            }
        }

        private void RemoveExpiredPings(int now)
        {
            List<int> expiredIds = new List<int>();

            foreach (KeyValuePair<int, int> item in pendingPingTicks)
            {
                if (GetElapsedMilliseconds(item.Value, now) > 5000)
                {
                    expiredIds.Add(item.Key);
                }
            }

            foreach (int pingId in expiredIds)
            {
                pendingPingTicks.Remove(pingId);
            }
        }
    }
}
