using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TTTGamemode
{
    [ClassLibrary("sbox-ttt", Title = "Trouble in Terrorist Town")]
    partial class Game : Sandbox.Game
    {
        public enum Round { Waiting, PreRound, InProgress, PostRound }

        public static Game Instance { get => Current as Game; }

        [ServerVar("ttt_min_players", Help = "The minimum players required to start.")]
        public static int TTTMinPlayers { get; set; } = 2;

        [ServerVar("ttt_preround_timer", Help = "The amount of time allowed for preparation.")]
        public static int TTTPreRoundTime { get; set; } = 20;

        [ServerVar("ttt_round_timer", Help = "The amount of time allowed for the main round.")]
        public static int TTTRoundTime { get; set; } = 300;

        [ServerVar("ttt_postround_timer", Help = "The amount of time before the next round starts.")]
        public static int TTTPostRoundTime { get; set; } = 10;

        [ServerVar("ttt_kill_time_reward", Help = "The amount of extra time given to traitors for killing an innocent.")]
        public static int TTTKillTimeReward { get; set; } = 30;

        [Net] public Round CurrentRound => Game.Round.Waiting;
        [Net] public int TimeRemaining => 0;

        #region TTT Methods
        private void ChangeRound(Round round)
        {
            switch (round)
            {
                case Game.Round.Waiting:
                    TimeRemaining = 0;
                    break;

                case Game.Round.PreRound:
                    TimeRemaining = TTTPreRoundTime;
                    // TODO: Make players invincible or something
                    break;

                case Game.Round.InProgress:
                    TimeRemaining = TTTRoundTime;
                    // TODO: Give players a role, start tracking karma
                    break;

                case Game.Round.PostRound:
                    TimeRemaining = TTTPostRoundTime;
                    // TODO: Disable karma tracking for le epic RDM
                    break;
            }

            CurrentRound = round;
        }

        private void CheckMinimumPlayers()
        {
            if (Sandbox.Player.All.Count >= TTTMinPlayers)
            {
                if (CurrentRound == Round.Waiting)
                {
                    ChangeRound(Round.PreRound);
                }
            }
            else if (CurrentRound != Round.Waiting)
            {
                ChangeRound(Round.Waiting);
            }
        }

        private void CheckRoundState()
        {
            if (CurrentRound != Round.InProgress) return;

            // TODO: Check if all traitors are dead

            // TODO: Check if all innocents are dead
        }

        private void UpdateRoundTimer()
        {
            if (CurrentRound == Round.Waiting) return;

            if (TimeRemaining <= 0)
            {
                switch (CurrentRound)
                {
                    case Round.PreRound:
                        ChangeRound(Round.InProgress);
                        break;

                    case Round.InProgress:
                        ChangeRound(Round.PostRound);
                        break;

                    case Round.PostRound:
                        ChangeRound(Round.PreRound);
                        break;
                }
            }
            else
            {
                TimeRemaining--;
            }
        }
        #endregion

        #region Game Timer
        private async Task StartGameTimer()
        {
            while (true)
            {
                UpdateGameTimer();
                await Task.DelaySeconds(1);
            }
        }

        private void UpdateGameTimer()
        {
            CheckMinimumPlayers();
            CheckRoundState();
            UpdateRoundTimer();
        }
        #endregion

        #region Gamemode Overrides
        public override void DoPlayerNoclip(Sandbox.Player player)
        {
            // Do nothing. The player can't noclip in this mode.
        }

        public override void DoPlayerSuicide(Sandbox.Player player)
        {
            // Do nothing. The player can't suicide in this mode.
            base.DoPlayerSuicide(player);
        }

        public override void PostLevelLoaded()
        {
            _ = StartGameTimer();

            base.PostLevelLoaded();
        }

        public override void PlayerKilled(Sandbox.Player player)
        {
            CheckRoundState();

            base.PlayerKilled(player);
        }

        public override void PlayerDisconnected(Sandbox.Player player, NetworkDisconnectionReason reason)
        {
            Log.Info(player.Name + " left, checking minimum player count...");

            CheckRoundState();

            base.PlayerDisconnected(player, reason);
        }

        public override Player CreatePlayer() => new();
        #endregion
    }
}
