using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Linq;
using WrathCombo.Combos.JobHelpers.Enums;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE;

internal partial class MCH
{
    // MCH Gauge & Extensions
    internal static MCHOpenerLogic MCHOpener = new();
    internal static MCHGauge Gauge = GetJobGauge<MCHGauge>();

    internal static bool reassembledExcavatorST =>
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[0] &&
         (HasEffect(Buffs.Reassembled) || !HasEffect(Buffs.Reassembled))) ||
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[0] &&
         !HasEffect(Buffs.Reassembled)) ||
        (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) ||
        !IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble);

    internal static bool reassembledChainsawST =>
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[1] &&
         (HasEffect(Buffs.Reassembled) || !HasEffect(Buffs.Reassembled))) ||
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[1] &&
         !HasEffect(Buffs.Reassembled)) ||
        (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) ||
        !IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble);

    internal static bool reassembledAnchorST =>
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[2] &&
         (HasEffect(Buffs.Reassembled) || !HasEffect(Buffs.Reassembled))) ||
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[2] &&
         !HasEffect(Buffs.Reassembled)) ||
        (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) ||
        !IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble);

    internal static bool reassembledDrillST =>
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[3] &&
         (HasEffect(Buffs.Reassembled) || !HasEffect(Buffs.Reassembled))) ||
        (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[3] &&
         !HasEffect(Buffs.Reassembled)) ||
        (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) ||
        !IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble);

    internal static float GCD => GetCooldown(OriginalHook(SplitShot)).CooldownTotal;

    internal static float heatblastRC => GetCooldown(Heatblast).CooldownTotal;

    internal static bool drillCD => !LevelChecked(Drill) || (!TraitLevelChecked(Traits.EnhancedMultiWeapon) &&
                                                             GetCooldownRemainingTime(Drill) > heatblastRC * 6) ||
                                    (TraitLevelChecked(Traits.EnhancedMultiWeapon) &&
                                     GetRemainingCharges(Drill) < GetMaxCharges(Drill) &&
                                     GetCooldownRemainingTime(Drill) > heatblastRC * 6);

    internal static bool anchorCD => !LevelChecked(AirAnchor) ||
                                     (LevelChecked(AirAnchor) && GetCooldownRemainingTime(AirAnchor) > heatblastRC * 6);

    internal static bool sawCD => !LevelChecked(Chainsaw) ||
                                  (LevelChecked(Chainsaw) && GetCooldownRemainingTime(Chainsaw) > heatblastRC * 6);

    internal static bool interruptReady => ActionReady(All.HeadGraze) && CanInterruptEnemy() &&
                                           CanDelayedWeave(ActionWatching.LastWeaponskill);

    internal static bool battery => Gauge.Battery >= 100;

    internal static bool HasNotWeaved => ActionWatching.GetAttackType(ActionWatching.LastAction) !=
                                         ActionWatching.ActionAttackType.Ability;

    public static int BSUsed => ActionWatching.CombatActions.Count(x => x == BarrelStabilizer);

    internal class MCHOpenerLogic : WrathOpener
    {
        public override int OpenerLevel => 100;
        public override int OpenerStepCount { get; } = 32;
        public override int PrePullStepCount { get; } = 1;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(CheckMate) < 3)
                return false;

            if (GetRemainingCharges(DoubleCheck) < 3)
                return false;

            if (!ActionReady(Chainsaw))
                return false;

            if (!ActionReady(Wildfire))
                return false;

            if (!ActionReady(BarrelStabilizer))
                return false;

            if (!ActionReady(Excavator))
                return false;

            if (!ActionReady(FullMetalField))
                return false;

            return true;
        }

        public override bool PrePullSteps(ref uint actionID)
        {
            if (HasEffect(Buffs.Reassembled)) CurrentState = OpenerState.InOpener;
            else actionID = Reassemble;

            if (ActionWatching.CombatActions.Count > 2 && InCombat())
                return false;

            return true;
        }

        public override bool Opener(ref uint actionID)
        {
            if (ActionWatching.TimeSinceLastAction.TotalSeconds >= 5)
                return false;

            if (OpenerStep == 1) actionID = AirAnchor;

            if (OpenerStep == 2) actionID = CheckMate;

            if (OpenerStep == 3) actionID = DoubleCheck;

            if (OpenerStep == 4) actionID = Drill;

            if (OpenerStep == 5) actionID = BarrelStabilizer;

            if (OpenerStep == 6) actionID = Chainsaw;

            if (OpenerStep == 7) actionID = Excavator;

            if (OpenerStep == 8) actionID = AutomatonQueen;

            if (OpenerStep == 9) actionID = Reassemble;

            if (OpenerStep == 10) actionID = Drill;

            if (OpenerStep == 11) actionID = CheckMate;

            if (OpenerStep == 12) actionID = Wildfire;

            if (OpenerStep == 13) actionID = FullMetalField;

            if (OpenerStep == 14) actionID = DoubleCheck;

            if (OpenerStep == 15) actionID = Hypercharge;

            if (OpenerStep == 16) actionID = BlazingShot;

            if (OpenerStep == 17) actionID = CheckMate;

            if (OpenerStep == 18) actionID = BlazingShot;

            if (OpenerStep == 19) actionID = DoubleCheck;

            if (OpenerStep == 20) actionID = BlazingShot;

            if (OpenerStep == 21) actionID = CheckMate;

            if (OpenerStep == 22) actionID = BlazingShot;

            if (OpenerStep == 23) actionID = DoubleCheck;

            if (OpenerStep == 24) actionID = BlazingShot;

            if (OpenerStep == 25) actionID = CheckMate;

            if (OpenerStep == 26) actionID = Drill;

            if (OpenerStep == 27) actionID = DoubleCheck;

            if (OpenerStep == 28) actionID = CheckMate;

            if (OpenerStep == 29) actionID = HeatedSplitShot;

            if (OpenerStep == 30) actionID = DoubleCheck;

            if (OpenerStep == 31) actionID = HeatedSlugShot;

            if (OpenerStep == 32) actionID = HeatedCleanShot;

            if (OpenerStep == 33) CurrentState = OpenerState.OpenerFinished;

            return true;

        }

        public override bool OpenerFailStates(ref uint actionID)
        {
            if (((actionID == CheckMate && GetRemainingCharges(CheckMate) < 3) ||
                  (actionID == Chainsaw && IsOnCooldown(Chainsaw)) ||
                  (actionID == Wildfire && IsOnCooldown(Wildfire)) ||
                  (actionID == BarrelStabilizer && IsOnCooldown(BarrelStabilizer)) ||
                  (actionID == BarrelStabilizer && IsOnCooldown(Excavator)) ||
                  (actionID == BarrelStabilizer && IsOnCooldown(FullMetalField)) ||
                  (actionID == DoubleCheck && GetRemainingCharges(DoubleCheck) < 3)) &&
                  ActionWatching.TimeSinceLastAction.TotalSeconds >= 3)
            {
                return true;
            }

            return false;
        }

        public override bool PrePullFailStates(ref uint actionId)
        {
            if (ActionWatching.CombatActions.Count > 2 && InCombat())
                return true;

            return false;
        }
    }

    internal static class MCHHelper
    {
        internal static unsafe bool IsComboExpiring(float Times)
        {
            float GCD = GetCooldown(OriginalHook(SplitShot)).CooldownTotal * Times;

            return ActionManager.Instance()->Combo.Timer != 0 && ActionManager.Instance()->Combo.Timer < GCD;
        }

        internal static bool UseQueen(MCHGauge gauge)
        {
            if (!ActionWatching.HasDoubleWeaved() && !HasEffect(Buffs.Wildfire) &&
                !JustUsed(OriginalHook(Heatblast)) && LevelChecked(OriginalHook(RookAutoturret)) &&
                gauge is { IsRobotActive: false, Battery: >= 50 })
            {
                if (LevelChecked(FullMetalField))
                {
                    //1min
                    if ((BSUsed == 1) & (gauge.Battery >= 90))
                        return true;

                    //even mins
                    if (BSUsed >= 2 && gauge.Battery == 100)
                        return true;

                    //odd mins 1st queen
                    if (BSUsed >= 2 && gauge is { Battery: 50, LastSummonBatteryPower: 100 })
                        return true;

                    //odd mins 2nd queen
                    if (BSUsed % 3 is 2 && gauge is { Battery: >= 60, LastSummonBatteryPower: 50 })
                        return true;

                    //odd mins 2nd queen
                    if (BSUsed % 3 is 0 && gauge is { Battery: >= 70, LastSummonBatteryPower: 50 })
                        return true;

                    //odd mins 2nd queen
                    if (BSUsed % 3 is 1 && gauge is { Battery: >= 80, LastSummonBatteryPower: 50 })
                        return true;
                }

                if (!LevelChecked(FullMetalField))
                    if (gauge.Battery == 100)
                        return true;

                if (!LevelChecked(BarrelStabilizer))
                    return true;
            }

            return false;
        }

        internal static bool Reassembled(MCHGauge gauge)
        {
            if (!JustUsed(OriginalHook(Heatblast)) &&
                !HasEffect(Buffs.Reassembled) && ActionReady(Reassemble))
            {
                if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                     (IsEnabled(CustomComboPreset.MCH_ST_AdvancedMode) && Config.MCH_ST_Reassembled[0])) &&
                    LevelChecked(Excavator) && HasEffect(Buffs.ExcavatorReady) &&
                    (BSUsed is 1 ||
                     (BSUsed % 3 is 2 && Gauge.Battery <= 40) ||
                     (BSUsed % 3 is 0 && Gauge.Battery <= 50) ||
                     (BSUsed % 3 is 1 && Gauge.Battery <= 60) ||
                     GetBuffRemainingTime(Buffs.ExcavatorReady) < 6))
                    return true;

                if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                     (IsEnabled(CustomComboPreset.MCH_ST_AdvancedMode) && Config.MCH_ST_Reassembled[1])) &&
                    LevelChecked(Chainsaw) && !LevelChecked(Excavator) &&
                    (GetCooldownRemainingTime(Chainsaw) <= GetCooldownRemainingTime(OriginalHook(SplitShot)) + 0.25 ||
                     ActionReady(Chainsaw)) && !battery)
                    return true;

                if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                     (IsEnabled(CustomComboPreset.MCH_ST_AdvancedMode) && Config.MCH_ST_Reassembled[2])) &&
                    LevelChecked(AirAnchor) &&
                    (GetCooldownRemainingTime(AirAnchor) <= GetCooldownRemainingTime(OriginalHook(SplitShot)) + 0.25 ||
                     ActionReady(AirAnchor)) && !battery)
                    return true;

                if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                     (IsEnabled(CustomComboPreset.MCH_ST_AdvancedMode) && Config.MCH_ST_Reassembled[3])) &&
                    LevelChecked(Drill) &&
                    ((!LevelChecked(AirAnchor) && Config.MCH_ST_Reassembled[2]) || !Config.MCH_ST_Reassembled[2]) &&
                    (GetCooldownRemainingTime(Drill) <= GetCooldownRemainingTime(OriginalHook(SplitShot)) + 0.25 ||
                     ActionReady(Drill)))
                    return true;
            }

            return false;
        }

        internal static bool Tools(ref uint actionID)
        {
            if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                 (IsEnabled(CustomComboPreset.MCH_ST_Adv_Excavator) && reassembledExcavatorST)) &&
                LevelChecked(Excavator) &&
                HasEffect(Buffs.ExcavatorReady) &&
                (BSUsed is 1 ||
                 (BSUsed % 3 is 2 && Gauge.Battery <= 40) ||
                 (BSUsed % 3 is 0 && Gauge.Battery <= 50) ||
                 (BSUsed % 3 is 1 && Gauge.Battery <= 60) ||
                 GetBuffRemainingTime(Buffs.ExcavatorReady) < 6))
            {
                actionID = Excavator;

                return true;
            }

            if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                 (IsEnabled(CustomComboPreset.MCH_ST_Adv_Chainsaw) && reassembledChainsawST)) &&
                LevelChecked(Chainsaw) &&
                !battery &&
                (GetCooldownRemainingTime(Chainsaw) <= GetCooldownRemainingTime(OriginalHook(SplitShot)) + 0.25 ||
                 ActionReady(Chainsaw)))
            {
                actionID = Chainsaw;

                return true;
            }

            if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                 (IsEnabled(CustomComboPreset.MCH_ST_Adv_AirAnchor) && reassembledAnchorST)) &&
                LevelChecked(OriginalHook(AirAnchor)) &&
                !battery &&
                (GetCooldownRemainingTime(OriginalHook(AirAnchor)) <=
                    GetCooldownRemainingTime(OriginalHook(SplitShot)) + 0.25 || ActionReady(OriginalHook(AirAnchor))))

            {
                actionID = OriginalHook(AirAnchor);

                return true;
            }

            if ((IsEnabled(CustomComboPreset.MCH_ST_SimpleMode) ||
                 (IsEnabled(CustomComboPreset.MCH_ST_Adv_Drill) && reassembledDrillST)) &&
                LevelChecked(Drill) &&
                !JustUsed(Drill) &&
                (GetCooldownRemainingTime(Drill) <= GetCooldownRemainingTime(OriginalHook(SplitShot)) + 0.25 ||
                 ActionReady(Drill)) && GetCooldownRemainingTime(Wildfire) is >= 20 or <= 10)
            {
                actionID = Drill;

                return true;
            }

            return false;
        }
    }
}