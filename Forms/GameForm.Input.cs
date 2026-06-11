using System;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public partial class GameForm
    {
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentPage != AppPage.Game)
            {
                return;
            }

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                localInput.Up = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                localInput.Down = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                localInput.Left = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                localInput.Right = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.J)
            {
                localInput.Fire = true;
                e.Handled = true;
            }
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                localInput.Up = false;
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                localInput.Down = false;
            }
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                localInput.Left = false;
            }
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                localInput.Right = false;
            }
        }
    }
}
