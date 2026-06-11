using System;
using System.Collections.Generic;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        private void timerGame_Tick(object sender, EventArgs e)
        {
            if (currentPage == AppPage.Room)
            {
                UpdateNetworkDiagnostics();
                ProcessRoomNetworkMessages();
                return;
            }

            if (currentPage != AppPage.Game)
            {
                return;
            }

            if (currentRole == ClientRole.Host)
            {
                ProcessHostMessages();
                engine.Update(BuildAuthorityInputs());
                PlayFireSoundIfNeeded(engine.State.FireSoundCount);
                localInput.ClearFire();
                network.SendState(engine.State);
                SendFireSoundIfNeeded(engine.State.FireSoundCount);
                HandleAuthorityRoundFlow();
            }
            else if (currentRole == ClientRole.Client)
            {
                UpdateNetworkDiagnostics();
                network.SendInput(selfPlayerId, localInput);
                localInput.ClearFire();
                ProcessClientMessages();
            }

            UpdateFpsCounter();
            UpdateHud();
            gameCanvasView.Bind(engine.State, resources);
        }

        private void PlayFireSoundIfNeeded(int fireSoundCount)
        {
            if (fireSoundCount > 0)
            {
                resources.PlaySound("fire.wav");
            }
        }

        private void SendFireSoundIfNeeded(int fireSoundCount)
        {
            if (fireSoundCount > 0)
            {
                network.SendSound("fire.wav");
            }
        }

        private void UpdateFpsCounter()
        {
            int now = Environment.TickCount;

            if (fpsWindowTick == 0)
            {
                fpsWindowTick = now;
            }

            fpsFrameCount++;

            int elapsed = Math.Max(1, unchecked(now - fpsWindowTick));

            if (elapsed >= 1000)
            {
                currentFps = (int)Math.Round(fpsFrameCount * 1000.0 / elapsed);
                fpsFrameCount = 0;
                fpsWindowTick = now;
            }
        }

        private void ResetFpsCounter()
        {
            fpsWindowTick = 0;
            fpsFrameCount = 0;
            currentFps = 0;
        }

        private Dictionary<int, PlayerInput> BuildAuthorityInputs()
        {
            Dictionary<int, PlayerInput> inputs = new Dictionary<int, PlayerInput>();

            foreach (KeyValuePair<int, PlayerInput> item in remoteInputs)
            {
                inputs[item.Key] = item.Value.Clone();
            }

            inputs[selfPlayerId] = localInput.Clone();
            return inputs;
        }

        private void HandleAuthorityRoundFlow()
        {
            if (!engine.State.RoundOver)
            {
                roundEndDelayTicks = 0;
                return;
            }

            if (roundEndDelayTicks == 0)
            {
                roundEndDelayTicks = 90;
                return;
            }

            roundEndDelayTicks--;

            if (roundEndDelayTicks > 0)
            {
                return;
            }

            if (engine.State.MatchOver)
            {
                network.SendState(engine.State);
                network.SendRoomReturn();
                ReturnToRoom(engine.State.ResultText);
            }
            else
            {
                engine.StartNextRound();
                network.SendFullState(engine.State);
                roundEndDelayTicks = 0;
            }
        }
    }
}
