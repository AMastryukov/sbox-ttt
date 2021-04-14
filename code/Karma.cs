using System;
using Sandbox;
using System.Collections.Generic;

namespace TTTGamemode
{
    public class KarmaSystem
    {
        [ServerVar("ttt_karma_default", Help = "The default amount of karma given to a player.")]
        public static int TTTKarmaDefault { get; set; } = 1000;

        [ServerVar("ttt_karma_max", Help = "The maximum amount of karma a player can achieve.")]
        public static int TTTKarmaMax { get; set; } = 1250;

        [ServerVar("ttt_karma_min", Help = "The minimum amount of karma a player can have.")]
        public static int TTTKarmaMin { get; set; } = 500;

        [ServerVar("ttt_karma_ban", Help = "Should the player be banned once they reach minimum karma?")]
        public static bool TTTKarmaBan { get; set; } = true;

        [ServerVar("ttt_karma_gain", Help = "The amount of passive karma gain every round.")]
        public static int TTTKarmaGain { get; set; } = 50;

        [ServerVar("ttt_karma_loss", Help = "The amount of karma loss per damage dealt.")]
        public static float TTTKarmaLoss { get; set; } = 1f;

        [ServerVar("ttt_karma_penalty_max", Help = "The maximum amount of karma loss per player.")]
        public static int TTTKarmaPenaltyMax { get; set; } = 100;

        [Net] public Dictionary<string, int> Records => new Dictionary<string, int>();
        [Net] public bool IsTracking = false;

        public void RegisterPlayer(Player player)
        {
            if (Records.ContainsKey(player)) return;

            Records[player] = TTTKarmaDefault;
        }

        public void OnDamageDealt(Player attacker, float damage)
        {
            // Calculate karma loss
            int karmaLoss = (int)(damage * TTTKarmaLoss);
            karmaLoss = karmaLoss > TTTKarmaPenaltyMax ? TTTKarmaPenaltyMax : karmaLoss;

            UpdatePlayerKarma(attacker, -karmaLoss);
        }

        public void UpdatePlayerKarma(Player player, int delta)
        {
            if (!IsTracking) return;

            int updatedKarma = Records[player.SteamId];
            updatedKarma += delta;

            updatedKarma = updatedKarma > TTTKarmaMax ? TTTKarmaMax : updatedKarma;
            updatedKarma = updatedKarma < TTTKarmaMin ? TTTKarmaMin : updatedKarma;

            Records[player.SteamId] = updatedKarma;
        }

        public bool IsBanned(Player player)
        {
            return (Records[player.SteamId] < TTTKarmaMin && TTTKarmaBan);
        }
    }
}