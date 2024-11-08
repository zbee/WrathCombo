using ECommons.DalamudServices;
using XIVSlothCombo.CustomComboNS;
using XIVSlothCombo.CustomComboNS.Functions;

namespace XIVSlothCombo.Combos.PvE;

internal partial class All
{
    public const byte JobID = 0;

    public const uint
        Rampart = 7531,
        SecondWind = 7541,
        TrueNorth = 7546,
        Addle = 7560,
        Swiftcast = 7561,
        LucidDreaming = 7562,
        Resurrection = 173,
        Raise = 125,
        Provoke = 7533,
        Shirk = 7537,
        Reprisal = 7535,
        Esuna = 7568,
        Rescue = 7571,
        SolidReason = 232,
        AgelessWords = 215,
        Sleep = 25880,
        WiseToTheWorldMIN = 26521,
        WiseToTheWorldBTN = 26522,
        LowBlow = 7540,
        Bloodbath = 7542,
        HeadGraze = 7551,
        FootGraze = 7553,
        LegGraze = 7554,
        Feint = 7549,
        Interject = 7538,
        Peloton = 7557,
        LegSweep = 7863,
        Repose = 16560,
        Sprint = 3;

    private const uint
        IsleSprint = 31314;

    /// <summary>
    ///     Quick Level, Offcooldown, spellweave, and MP check of Lucid Dreaming
    /// </summary>
    /// <param name="actionID">action id to check weave</param>
    /// <param name="MPThreshold">Player MP less than Threshold check</param>
    /// <param name="weave">Spell Weave check by default</param>
    /// <returns></returns>
    public static bool CanUseLucid(uint actionID, int MPThreshold, bool weave = true)
    {
        return CustomComboFunctions.ActionReady(LucidDreaming)
               && CustomComboFunctions.LocalPlayer.CurrentMp <= MPThreshold
               && (!weave || CustomComboFunctions.CanSpellWeave(actionID));
    }

    public static class Buffs
    {
        public const ushort
            Weakness = 43,
            Medicated = 49,
            Bloodbath = 84,
            Swiftcast = 167,
            Rampart = 1191,
            Peloton = 1199,
            LucidDreaming = 1204,
            TrueNorth = 1250,
            Sprint = 50;
    }

    public static class Debuffs
    {
        public const ushort
            Sleep = 3,
            Bind = 13,
            Heavy = 14,
            Addle = 1203,
            Reprisal = 1193,
            Feint = 1195;
    }

    internal class ALL_IslandSanctuary_Sprint : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_IslandSanctuary_Sprint;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is Sprint && Svc.ClientState.TerritoryType is 1055
                ? IsleSprint
                : actionID;
        }
    }

    //Tank Features
    internal class ALL_Tank_Interrupt : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Tank_Interrupt;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            switch (actionID)
            {
                case LowBlow or PLD.ShieldBash when CanInterruptEnemy() && ActionReady(Interject):
                    return Interject;

                case LowBlow or PLD.ShieldBash when ActionReady(LowBlow):
                    return LowBlow;

                case LowBlow or PLD.ShieldBash when actionID == PLD.ShieldBash && IsOnCooldown(LowBlow):
                    return actionID;

                default:
                    return actionID;
            }
        }
    }

    internal class ALL_Tank_Reprisal : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Tank_Reprisal;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is Reprisal && TargetHasEffectAny(Debuffs.Reprisal) && IsOffCooldown(Reprisal)
                ? OriginalHook(11)
                : actionID;
        }
    }

    //Healer Features
    internal class ALL_Healer_Raise : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Healer_Raise;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            switch (actionID)
            {
                case WHM.Raise or AST.Ascend or SGE.Egeiro:
                case SCH.Resurrection when LocalPlayer.ClassJob.Id is SCH.JobID:
                {
                    if (ActionReady(Swiftcast))
                        return Swiftcast;

                    if (actionID == WHM.Raise && IsEnabled(CustomComboPreset.WHM_ThinAirRaise) &&
                        ActionReady(WHM.ThinAir) && !HasEffect(WHM.Buffs.ThinAir))
                        return WHM.ThinAir;

                    return actionID;
                }

                default:
                    return actionID;
            }
        }
    }

    //Caster Features
    internal class ALL_Caster_Addle : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Caster_Addle;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is Addle && TargetHasEffectAny(Debuffs.Addle) && IsOffCooldown(Addle)
                ? OriginalHook(11)
                : actionID;
        }
    }

    internal class ALL_Caster_Raise : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Caster_Raise;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            switch (actionID)
            {
                case BLU.AngelWhisper or RDM.Verraise:
                case SMN.Resurrection when LocalPlayer.ClassJob.Id is SMN.JobID:
                {
                    if (HasEffect(Buffs.Swiftcast) || HasEffect(RDM.Buffs.Dualcast))
                        return actionID;

                    if (IsOffCooldown(Swiftcast))
                        return Swiftcast;

                    if (LocalPlayer.ClassJob.Id is RDM.JobID &&
                        ActionReady(RDM.Vercure))
                        return RDM.Vercure;

                    break;
                }
            }

            return actionID;
        }
    }

    //Melee DPS Features
    internal class ALL_Melee_Feint : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Melee_Feint;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is Feint && TargetHasEffectAny(Debuffs.Feint) && IsOffCooldown(Feint)
                ? OriginalHook(11)
                : actionID;
        }
    }

    internal class ALL_Melee_TrueNorth : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Melee_TrueNorth;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is TrueNorth && HasEffect(Buffs.TrueNorth)
                ? OriginalHook(11)
                : actionID;
        }
    }

    //Ranged Physical Features
    internal class ALL_Ranged_Mitigation : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Ranged_Mitigation;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is BRD.Troubadour or MCH.Tactician or DNC.ShieldSamba &&
                   (HasEffectAny(BRD.Buffs.Troubadour) || HasEffectAny(MCH.Buffs.Tactician) ||
                    HasEffectAny(DNC.Buffs.ShieldSamba)) && IsOffCooldown(actionID)
                ? OriginalHook(11)
                : actionID;
        }
    }

    internal class ALL_Ranged_Interrupt : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ALL_Ranged_Interrupt;

        protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
        {
            return actionID is FootGraze && CanInterruptEnemy() && ActionReady(HeadGraze)
                ? HeadGraze
                : actionID;
        }
    }
}