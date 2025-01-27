#region

using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.Combos.PvE.Content;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
using Preset = WrathCombo.Combos.CustomComboPreset;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ReturnTypeCanBeNotNullable
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

#endregion

namespace WrathCombo.Combos.PvE;

internal partial class DRK
{
    /// <summary>
    ///     DRK's job gauge.
    /// </summary>
    private static DRKGauge Gauge => GetJobGauge<DRKGauge>();

    /// <summary>
    ///     Select the opener to use.
    /// </summary>
    /// <returns>A valid <see cref="WrathOpener">Opener</see>.</returns>
    internal static WrathOpener Opener()
    {
        if (Opener1.LevelChecked)
            return Opener1;

        return WrathOpener.Dummy;
    }

    #region Action Logic

    /// <summary>
    ///     Flags to combine to provide to the `TryGet...Action` methods.
    /// </summary>
    [Flags]
    private enum Combo
    {
        // Target-type for combo
        ST = 1 << 0, // 1
        AoE = 1 << 1, // 2

        // Complexity of combo
        Adv = 1 << 2, // 4
        Simple = 1 << 3, // 8
    }

    /*
     * The following methods all follow the signature of:
     *     private static bool TryGet...Action(Combo flags, ref uint action)
     * Where the `...` is replaced by as small of a word describing the group of
     * actions as possible.
     * All return `false` at the end, the only usual indicator that the `action` has
     * not been changed.
     * The only other return of `false` is when a piece of logic would apply to
     * multiple following actions; Other cases of this, i.e. where such logic would
     * apply to all such actions, is often done before the method call.
     *
     * All should be passed a `newAction` reference to overwrite when the return is
     * true. If the return is false, the `newAction` should be ignored.
     *
     * All have a return pattern of:
     *     return (action = ActionIDToExecute) != 0;
     * Where `ActionIDToExecute` is the action to execute.
     * The return is always true, the logic doesn't actually matter; The reason for
     * this pattern is to allow for a return and an `action` assignment
     * simultaneously.
     */

    /// <summary>
    ///     Tests if any of the Variant actions can be used.
    /// </summary>
    /// <param name="flags">
    ///     The flags to describe the combo executing this method.
    /// </param>
    /// <param name="action">The action to execute.</param>
    /// <returns>Whether the <c>action</c> was changed.</returns>
    private static bool TryGetVariantAction(Combo flags, ref uint action)
    {
        // Heal
        if ((flags.HasFlag(Combo.Simple) ||
             (flags.HasFlag(Combo.Adv) && IsEnabled(Preset.DRK_Var_Cure))) &&
            IsEnabled(Variant.VariantCure) &&
            ActionReady(Variant.VariantCure) &&
            PlayerHealthPercentageHp() <= GetOptionValue(Config.DRK_VariantCure))
            return (action = Variant.VariantCure) != 0;

        if (!CanWeave()) return false;

        // Aggro + Stun
        if ((flags.HasFlag(Combo.Simple) ||
             (flags.HasFlag(Combo.Adv) && IsEnabled(Preset.DRK_Var_Ulti))) &&
            IsEnabled(Variant.VariantUltimatum) &&
            ActionReady(Variant.VariantUltimatum))
            return (action = Variant.VariantUltimatum) != 0;

        // Damage over Time
        var DoTStatus = FindTargetEffect(Variant.Debuffs.SustainedDamage);
        if ((flags.HasFlag(Combo.Simple) ||
             (flags.HasFlag(Combo.Adv) && IsEnabled(Preset.DRK_Var_Dart))) &&
            IsEnabled(Variant.VariantSpiritDart) &&
            ActionReady(Variant.VariantSpiritDart) &&
            DoTStatus?.RemainingTime <= 3)
            return (action = Variant.VariantSpiritDart) != 0;

        return false;
    }

    #region JustUsedMit

    /// <summary>
    ///     Whether mitigation was recently used, depending on the duration and
    ///     strength of the mitigation.
    /// </summary>
    private static readonly bool JustUsedMitigation =
        JustUsed(BlackestNight, 2f) ||
        JustUsed(Oblation, 2f) ||
        JustUsed(All.Reprisal, 4f) ||
        JustUsed(DarkMissionary, 5f) ||
        JustUsed(All.Rampart, 6f) ||
        JustUsed(All.ArmsLength, 2f) ||
        JustUsed(ShadowedVigil, 6f) ||
        JustUsed(LivingDead, 7f);

    #endregion

    /// <summary>
    ///     Tests if any of the Mitigation actions can be used.
    /// </summary>
    /// <param name="flags">
    ///     The flags to describe the combo executing this method.
    /// </param>
    /// <param name="action">The action to execute.</param>
    /// <returns>Whether the <c>action</c> was changed.</returns>
    private static bool TryGetMitigationAction(Combo flags, ref uint action)
    {
        if (JustUsedMitigation) return false;

        // Living Dead
        #region Variables
        var bossRestrictionLivingDead = flags.HasFlag(Combo.Adv)
            ? (int)Config.DRK_ST_LivingDeadBossRestriction
            : (int)Config.BossAvoidance.Off;
        var livingDeadSelfThreshold = flags.HasFlag(Combo.Adv) ?
            flags.HasFlag(Combo.ST)
                ? Config.DRK_ST_LivingDeadSelfThreshold
                : Config.DRK_AoE_LivingDeadSelfThreshold :
            flags.HasFlag(Combo.ST) ? 15 : 20;
        var livingDeadTargetThreshold = flags.HasFlag(Combo.Adv) ?
            flags.HasFlag(Combo.ST)
                ? Config.DRK_ST_LivingDeadTargetThreshold
                : Config.DRK_AoE_LivingDeadTargetThreshold :
            flags.HasFlag(Combo.ST) ? 1 : 15;
        #endregion
        if ((flags.HasFlag(Combo.Simple) ||
             ((flags.HasFlag(Combo.ST) && IsEnabled(Preset.DRK_ST_LivingDead)) ||
              flags.HasFlag(Combo.AoE) && IsEnabled(Preset.DRK_AoE_LivingDead))) &&
            ActionReady(LivingDead) &&
            PlayerHealthPercentageHp() <= livingDeadSelfThreshold &&
            GetTargetHPPercent() >= livingDeadTargetThreshold &&
            // Checking if the target matches the boss avoidance option
            ((bossRestrictionLivingDead is (int)Config.BossAvoidance.On &&
              InBossEncounter()) ||
             bossRestrictionLivingDead is (int)Config.BossAvoidance.Off))
            return (action = LivingDead) != 0;

        // TBN
        if ((flags.HasFlag(Combo.Simple) ||
             ((flags.HasFlag(Combo.ST) && IsEnabled(Preset.DRK_ST_TBN)) ||
              flags.HasFlag(Combo.AoE) && IsEnabled(Preset.DRK_AoE_TBN))) &&
            ActionReady(BlackestNight) &&
            LocalPlayer.CurrentMp >= 3000 &&
            ShouldTBNSelf(flags.HasFlag(Combo.AoE)))
            return (action = BlackestNight) != 0;

        // Oblation
        var oblationCharges = flags.HasFlag(Combo.Adv) && flags.HasFlag(Combo.ST)
            ? Config.DRK_ST_OblationCharges
            : 0;
        if ((flags.HasFlag(Combo.Simple) ||
             ((flags.HasFlag(Combo.ST) && IsEnabled(Preset.DRK_ST_Oblation)) ||
              flags.HasFlag(Combo.AoE) && IsEnabled(Preset.DRK_AoE_Oblation))) &&
            ActionReady(Oblation) &&
            !HasEffectAny(Buffs.Oblation) &&
            GetRemainingCharges(Oblation) > oblationCharges)
            return (action = BlackestNight) != 0;

        // Reprisal
        #region Variables
        var reprisalRange = flags.HasFlag(Combo.AoE)
            ? 7
            : 5;
        var reprisalTargetCount = flags.HasFlag(Combo.Adv) && flags.HasFlag(Combo.AoE)
            ? Config.DRK_AoE_ReprisalEnemyCount
            : 1;
        var reprisalUseForRaidwides =
            flags.HasFlag(Combo.AoE) || RaidWideCasting();
        var reprisalTargetHasNoDebuff =
            flags.HasFlag(Combo.AoE) || !TargetHasEffectAny(All.Debuffs.Reprisal);
        #endregion
        if ((flags.HasFlag(Combo.Simple) ||
             ((flags.HasFlag(Combo.ST) && IsEnabled(Preset.DRK_ST_Reprisal)) ||
              flags.HasFlag(Combo.AoE) && IsEnabled(Preset.DRK_AoE_Reprisal))) &&
            ActionReady(All.Reprisal) &&
            reprisalTargetHasNoDebuff &&
            reprisalUseForRaidwides &&
            CanCircleAoe(reprisalRange) >= reprisalTargetCount)
            return (action = All.Reprisal) != 0;

        // Dark Missionary (ST only)
        if (flags.HasFlag(Combo.ST) &&
            (flags.HasFlag(Combo.Simple) ||
             (IsEnabled(Preset.DRK_ST_Missionary))) &&
            ActionReady(DarkMissionary) &&
            RaidWideCasting())
            return (action = DarkMissionary) != 0;

        // Rampart (AoE only)
        // todo

        // Arms Length (AoE only)
        // todo

        // Shadowed Vigil
        #region Variables
        var vigilHealthThreshold = flags.HasFlag(Combo.Adv) ?
            flags.HasFlag(Combo.ST)
                ? Config.DRK_ST_ShadowedVigilThreshold
                : Config.DRK_AoE_ShadowedVigilThreshold :
            flags.HasFlag(Combo.ST) ? 40 : 50;
        #endregion
        if ((flags.HasFlag(Combo.Simple) ||
             ((flags.HasFlag(Combo.ST) && IsEnabled(Preset.DRK_ST_Vigil)) ||
              flags.HasFlag(Combo.AoE) && IsEnabled(Preset.DRK_AoE_Vigil))) &&
            ActionReady(ShadowedVigil) &&
            PlayerHealthPercentageHp() <= vigilHealthThreshold)
            return (action = OriginalHook(ShadowWall)) != 0;

        return false;
    }

    #region TBN

    /// <summary>
    ///     Whether the player has a shield from TBN from themselves.
    /// </summary>
    /// <seealso cref="Buffs.BlackestNightShield" />
    private static bool HasOwnTBN
    {
        get
        {
            var has = false;
            if (LocalPlayer is not null)
                has = FindEffect(
                    Buffs.BlackestNightShield,
                    LocalPlayer,
                    LocalPlayer.GameObjectId
                ) is not null;

            return has;
        }
    }

    /// <summary>
    ///     Whether the player has a shield from TBN from anyone.
    /// </summary>
    /// <seealso cref="Buffs.BlackestNightShield" />
    private static bool HasAnyTBN
    {
        get
        {
            var has = false;
            if (LocalPlayer is not null)
                has = FindEffect(Buffs.BlackestNightShield) is not null;

            return has;
        }
    }

    /// <summary>
    ///     Decides if the player should use TBN on themselves,
    ///     based on general rules and the player's configuration.
    /// </summary>
    /// <param name="aoe">Whether AoE or ST options should be checked.</param>
    /// <returns>Whether TBN should be used on self.</returns>
    /// <seealso cref="BlackestNight" />
    /// <seealso cref="Buffs.BlackestNightShield" />
    /// <seealso cref="CustomComboPreset.DRK_ST_TBN" />
    /// <seealso cref="Config.DRK_ST_TBNThreshold" />
    /// <seealso cref="Config.DRK_ST_TBNBossRestriction" />
    /// <seealso cref="CustomComboPreset.DRK_AoE_TBN" />
    private static bool ShouldTBNSelf(bool aoe = false)
    {
        // Bail if we're dead or unloaded
        if (LocalPlayer is null)
            return false;

        // Bail if TBN is disabled
        if ((!aoe
             && (!IsEnabled(Preset.DRK_ST_Mitigation)
                 || !IsEnabled(Preset.DRK_ST_TBN)))
            || (aoe
                && (!IsEnabled(Preset.DRK_AoE_Mitigation)
                    || !IsEnabled(Preset.DRK_AoE_TBN))))
            return false;

        // Bail if we already have TBN
        if (HasOwnTBN)
            return false;

        // Bail if we have no target
        if (!HasBattleTarget())
            return false;

        var hpRemaining = PlayerHealthPercentageHp();
        var hpThreshold = !aoe ? (float)Config.DRK_ST_TBNThreshold : 90f;

        // Bail if we're above the threshold
        if (hpRemaining > hpThreshold)
            return false;

        var targetIsBoss = TargetIsBoss();
        var bossRestriction = !aoe
            ? (int)Config.DRK_ST_TBNBossRestriction
            : (int)Config.BossAvoidance.Off; // Don't avoid bosses in AoE

        // Bail if we're trying to avoid bosses and the target is one
        if (bossRestriction is (int)Config.BossAvoidance.On
            && targetIsBoss)
            return false;

        // Bail if we have a TBN and burst is >30s away ()
        if (GetCooldownRemainingTime(LivingShadow) > 30
            && HasAnyTBN)
            return false;

        return true;
    }

    #endregion

    #region One-Button Mitigation

    /// <summary>
    ///     The list of Mitigations to use in the One-Button Mitigation combo.<br />
    ///     The order of the list needs to match the order in
    ///     <see cref="CustomComboPreset" />.
    /// </summary>
    /// <value>
    ///     <c>Action</c> is the action to use.<br />
    ///     <c>Preset</c> is the preset to check if the action is enabled.<br />
    ///     <c>Logic</c> is the logic for whether to use the action.
    /// </value>
    /// <remarks>
    ///     Each logic check is already combined with checking if the preset
    ///     <see cref="IsEnabled(uint)">is enabled</see>
    ///     and if the action is <see cref="ActionReady(uint)">ready</see> and
    ///     <see cref="LevelChecked(uint)">level-checked</see>.<br />
    ///     Do not add any of these checks to <c>Logic</c>.
    /// </remarks>
    private static (uint Action, Preset Preset, System.Func<bool> Logic)[]
        PrioritizedMitigation =>
    [
        (BlackestNight, Preset.DRK_Mit_TheBlackestNight,
            () => !HasAnyTBN && LocalPlayer.CurrentMp > 3000 &&
                  PlayerHealthPercentageHp() <= Config.DRK_Mit_TBN_Health),
        (Oblation, Preset.DRK_Mit_Oblation,
            () => (!((HasFriendlyTarget() && TargetHasEffectAny(Buffs.Oblation)) ||
                     (!HasFriendlyTarget() && HasEffectAny(Buffs.Oblation)))) &&
                  GetRemainingCharges(Oblation) > Config.DRK_Mit_Oblation_Charges),
        (All.Reprisal, Preset.DRK_Mit_Reprisal,
            () => InActionRange(All.Reprisal)),
        (DarkMissionary, Preset.DRK_Mit_DarkMissionary,
            () => Config.DRK_Mit_DarkMissionary_PartyRequirement ==
                  (int)Config.PartyRequirement.No ||
                  IsInParty()),
        (All.Rampart, Preset.DRK_Mit_Rampart,
            () => PlayerHealthPercentageHp() <= Config.DRK_Mit_Rampart_Health),
        (DarkMind, Preset.DRK_Mit_DarkMind, () => true),
        (All.ArmsLength, Preset.DRK_Mit_ArmsLength,
            () => CanCircleAoe(7) >= Config.DRK_Mit_ArmsLength_EnemyCount &&
                  (Config.DRK_Mit_ArmsLength_Boss == (int)Config.BossAvoidance.Off ||
                   InBossEncounter())),
        (OriginalHook(ShadowWall), Preset.DRK_Mit_ShadowWall,
            () => PlayerHealthPercentageHp() <= Config.DRK_Mit_ShadowWall_Health),
    ];

    /// <summary>
    ///     Given the index of a mitigation in <see cref="PrioritizedMitigation" />,
    ///     checks if the mitigation is ready and meets the provided requirements.
    /// </summary>
    /// <param name="index">
    ///     The index of the mitigation in <see cref="PrioritizedMitigation" />,
    ///     which is the order of the mitigation in <see cref="CustomComboPreset" />.
    /// </param>
    /// <param name="action">
    ///     The variable to set to the action to, if the mitigation is set to be
    ///     used.
    /// </param>
    /// <returns>
    ///     Whether the mitigation is ready, enabled, and passes the provided logic
    ///     check.
    /// </returns>
    private static bool CheckMitigationConfigMeetsRequirements
        (int index, out uint action)
    {
        action = PrioritizedMitigation[index].Action;
        return ActionReady(action) &&
               PrioritizedMitigation[index].Logic() &&
               IsEnabled(PrioritizedMitigation[index].Preset);
    }

    #endregion

    #endregion

    #region Openers

    internal static DRKOpenerMaxLevel1 Opener1 = new();

    internal class DRKOpenerMaxLevel1 : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            HardSlash,
            EdgeOfShadow,
            LivingShadow,
            SyphonStrike,
            Souleater, // 5
            Delirium,
            Disesteem,
            SaltedEarth,
            EdgeOfShadow, // Depends on TBN pop
            ScarletDelirium, // 10
            Shadowbringer,
            EdgeOfShadow,
            Comeuppance,
            CarveAndSpit,
            EdgeOfShadow, // 15
            Torcleaver,
            Shadowbringer,
            EdgeOfShadow,
            Bloodspiller,
            SaltAndDarkness, // 20
        ];

        internal override UserData? ContentCheckConfig =>
            Config.DRK_ST_OpenerDifficulty;

        public override bool HasCooldowns()
        {
            if (LocalPlayer.CurrentMp < 7000)
                return false;

            if (!IsOffCooldown(LivingShadow))
                return false;

            if (!IsOffCooldown(Delirium))
                return false;

            if (!IsOffCooldown(CarveAndSpit))
                return false;

            if (!IsOffCooldown(SaltedEarth))
                return false;

            if (GetRemainingCharges(Shadowbringer) < 2)
                return false;

            return true;
        }
    }

    #endregion

    #region IDs

    public const byte JobID = 32;

    #region Actions

    public const uint

        #region Single-Target 1-2-3 Combo

        HardSlash = 3617,
        SyphonStrike = 3623,
        Souleater = 3632,

        #endregion

        #region AoE 1-2-3 Combo

        Unleash = 3621,
        StalwartSoul = 16468,

        #endregion

        #region Single-Target oGCDs

        CarveAndSpit = 3643, // With AbyssalDrain
        EdgeOfDarkness = 16467, // For MP
        EdgeOfShadow = 16470, // For MP // Upgrade of EdgeOfDarkness
        Bloodspiller = 7392, // For Blood
        ScarletDelirium = 36928, // Under Enhanced Delirium
        Comeuppance = 36929, // Under Enhanced Delirium
        Torcleaver = 36930, // Under Enhanced Delirium

        #endregion

        #region AoE oGCDs

        AbyssalDrain = 3641, // Cooldown shared with CarveAndSpit
        FloodOfDarkness = 16466, // For MP
        FloodOfShadow = 16469, // For MP // Upgrade of FloodOfDarkness
        Quietus = 7391, // For Blood
        SaltedEarth = 3639,
        SaltAndDarkness = 25755, // Recast of Salted Earth
        Impalement = 36931, // Under Delirium

        #endregion

        #region Buffing oGCDs

        BloodWeapon = 3625,
        Delirium = 7390,

        #endregion

        #region Burst Window

        LivingShadow = 16472,
        Shadowbringer = 25757,
        Disesteem = 36932,

        #endregion

        #region Ranged Option

        Unmend = 3624,

        #endregion

        #region Mitigation

        Grit =
            3629, // Lv10, instant, 2.0s CD (group 1), range 0, single-target, targets=Self
        ReleaseGrit =
            32067, // Lv10, instant, 1.0s CD (group 1), range 0, single-target, targets=Self
        ShadowWall =
            3636, // Lv38, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
        DarkMind =
            3634, // Lv45, instant, 60.0s CD (group 8), range 0, single-target, targets=Self
        LivingDead =
            3638, // Lv50, instant, 300.0s CD (group 24), range 0, single-target, targets=Self
        DarkMissionary =
            16471, // Lv66, instant, 90.0s CD (group 14), range 0, AOE 30 circle, targets=Self
        BlackestNight =
            7393, // Lv70, instant, 15.0s CD (group 2), range 30, single-target, targets=Self/Party
        Oblation =
            25754, // Lv82, instant, 60.0s CD (group 18/71) (2 charges), range 30, single-target, targets=Self/Party
        ShadowedVigil =
            36927; // Lv92, instant, 120.0s CD (group 20), range 0, single-target, targets=Self, animLock=???

    #endregion

    #endregion

    public static class Buffs
    {
        #region Main Buffs

        /// Tank Stance
        public const ushort Grit = 743;

        /// The lowest level buff, before Delirium
        public const ushort BloodWeapon = 742;

        /// The lower Delirium buff, with just the blood ability usage
        public const ushort Delirium = 1972;

        /// Different from Delirium, to do the Scarlet Delirium chain
        public const ushort EnhancedDelirium = 3836;

        /// The increased damage buff that should always be up - checked through gauge
        public const ushort Darkside = 741;

        #endregion

        #region "DoT" or Burst

        /// Ground DoT active status
        public const ushort SaltedEarth = 749;

        /// Charge to be able to use Disesteem
        public const ushort Scorn = 3837;

        #endregion

        #region Mitigation

        /// TBN Active - Dark arts checked through gauge
        public const ushort BlackestNightShield = 1178;

        /// The initial Invuln that needs procc'd
        public const ushort LivingDead = 810;

        /// The real, triggered Invuln that gives heals
        public const ushort WalkingDead = 811;

        /// Damage Reduction part of Vigil
        public const ushort ShadowedVigil = 3835;

        /// The triggered part of Vigil that needs procc'd to heal (happens below 50%)
        public const ushort ShadowedVigilant = 3902;

        /// Oblation Active
        public const ushort Oblation = 2682;

        #endregion
    }

    public static class Traits
    {
        public const uint
            EnhancedDelirium = 572;
    }

    #endregion
}
