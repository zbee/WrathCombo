using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using System;
using System.Linq;

namespace XIVSlothCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        private static DateTime combatStart = DateTime.Now;
        private static DateTime partyCombat = DateTime.Now;
        private static bool partyInCombat = false;

        /// <summary> Tells the elapsed time since the combat started. </summary>
        /// <returns> Combat time in seconds. </returns>
        public static TimeSpan CombatEngageDuration() => InCombat() ? DateTime.Now - combatStart : TimeSpan.Zero;

        public unsafe static TimeSpan PartyEngageDuration() => partyInCombat ? DateTime.Now - partyCombat : TimeSpan.Zero;

        public static void TimerSetup()
        {
            Svc.Condition.ConditionChange += OnCombat;
            Svc.Framework.Update += UpdatePartyTimer;
        }

        private unsafe static void UpdatePartyTimer(IFramework framework)
        {
            if (GetPartyMembers().Any(x => x.Struct()->InCombat) && !partyInCombat)
            {
                partyInCombat = true;
                partyCombat = DateTime.Now;
            }
            else if (!GetPartyMembers().Any(x => x.Struct()->InCombat))
            {
                partyInCombat = false;
            }
        }

        public static void TimerDispose()
        {
            Svc.Condition.ConditionChange -= OnCombat;
            Svc.Framework.Update -= UpdatePartyTimer;
        }

        internal static void OnCombat(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.InCombat && value)
                combatStart = DateTime.Now;
        }
    }
}
