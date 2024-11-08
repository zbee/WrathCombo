using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using System;

namespace XIVSlothCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        private static DateTime combatStart = DateTime.Now;

        /// <summary> Tells the elapsed time since the combat started. </summary>
        /// <returns> Combat time in seconds. </returns>
        public static TimeSpan CombatEngageDuration() => InCombat() ? DateTime.Now - combatStart : TimeSpan.Zero;

        public static void TimerSetup()
        {
            Svc.Condition.ConditionChange += OnCombat;
        }

        public static void TimerDispose()
        {
            Svc.Condition.ConditionChange -= OnCombat;
        }

        internal static void OnCombat(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.InCombat && value)
                combatStart = DateTime.Now;
        }
    }
}
