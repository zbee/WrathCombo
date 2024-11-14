#region

using XIVSlothCombo.Combos.PvP;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Window.Functions;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace XIVSlothCombo.Combos.PvE;

internal partial class DRK
{
    internal static class Config
    {
        private const float little = 100f;
        private const float medium = 150f;
        private const float bigger = 175f;
        private const float biggest = 200f;

        private const string stopUsingAtDescription =
            "Target HP% to stop using (0 = Use Always)";

        private const string startUsingAtDescription =
            "HP% to use at or below";

        public static readonly UserInt
            DRK_ST_DeliriumThreshold = new("DRK_ST_DeliriumThreshold", 0),
            DRK_ST_LivingDeadThreshold = new("DRK_ST_LivingDeadThreshold", 5),
            DRK_ST_ManaSpenderPooling = new("DRK_ST_ManaSpenderPooling", 3000),
            DRK_ST_TBNThreshold = new("DRK_ST_TBNThreshold", 25),
            DRK_ST_TBNBossRestriction = new("DRK_ST_TBNBossRestriction", 1), // Default to using on all enemies
            DRK_ST_ShadowedVigilThreshold = new("DRK_ST_ShadowedVigilThreshold", 40),
            DRK_ST_LivingDeadSelfThreshold =
                new("DRK_ST_LivingDeadSelfThreshold", 15),
            DRK_ST_LivingDeadTargetThreshold =
                new("DRK_ST_LivingDeadTargetThreshold", 1),
            DRK_ST_LivingDeadBossRestriction =
                new("DRK_ST_LivingDeadBossRestriction",
                    2), // Default to not using on bosses
            DRK_AoE_DeliriumThreshold = new("DRK_AoE_DeliriumThreshold", 25),
            DRK_AoE_LivingDeadThreshold = new("DRK_AoE_LivingDeadThreshold", 40),
            DRK_VariantCure = new("DRKVariantCure");

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                #region Advanced Single Target

                case CustomComboPreset.DRK_ST_Delirium:
                    UserConfig.DrawSliderInt(0, 25, DRK_ST_DeliriumThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_ST_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 30, DRK_ST_LivingDeadThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_ST_ManaSpenderPooling:
                    UserConfig.DrawSliderInt(0, 3000, DRK_ST_ManaSpenderPooling,
                        "Mana to save for TBN (0 = Use All)",
                        itemWidth: biggest,
                        sliderIncrement: SliderIncrements.Thousands);

                    break;

                case CustomComboPreset.DRK_ST_TBN:
                    UserConfig.DrawSliderInt(1, 40, DRK_ST_TBNThreshold,
                        startUsingAtDescription,
                        itemWidth: medium, sliderIncrement: SliderIncrements.Fives);

                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_TBNBossRestriction, "All Enemies",
                        "Will use Living Dead regardless of the type of enemy.",
                        outputValue: 1, itemWidth: 125f);
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_TBNBossRestriction, "Avoid Bosses",
                        "Will try not to use Living Dead when your target is a boss.\n" +
                        "(Note: don't rely on this 100%, square sometimes marks enemies inconsistently)",
                        outputValue: 2, itemWidth: 125f);

                    break;

                case CustomComboPreset.DRK_ST_ShadowedVigil:
                    UserConfig.DrawSliderInt(1, 55, DRK_ST_ShadowedVigilThreshold,
                        startUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_ST_LivingDead:
                    UserConfig.DrawSliderInt(1, 40, DRK_ST_LivingDeadSelfThreshold,
                        startUsingAtDescription,
                        itemWidth: medium, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawSliderInt(1, 10, DRK_ST_LivingDeadTargetThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_LivingDeadBossRestriction, "All Enemies",
                        "Will use Living Dead regardless of the type of enemy.",
                        outputValue: 1, itemWidth: 125f);
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_LivingDeadBossRestriction, "Avoid Bosses",
                        "Will try not to use Living Dead when your target is a boss.\n" +
                        "(Note: don't rely on this 100%, square sometimes marks enemies inconsistently)",
                        outputValue: 2, itemWidth: 125f);

                    break;

                #endregion

                #region Advanced AoE

                case CustomComboPreset.DRK_AoE_Delirium:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_DeliriumThreshold,
                        stopUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_AoE_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_LivingDeadThreshold,
                        stopUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_AoE_TBN:

                    break;

                case CustomComboPreset.DRK_AoE_ShadowedVigil:

                    break;

                case CustomComboPreset.DRK_AoE_LivingDead:

                    break;

                #endregion

                case CustomComboPreset.DRK_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, DRK_VariantCure,
                        startUsingAtDescription,
                        itemWidth: biggest, sliderIncrement: SliderIncrements.Fives);

                    break;

                #region PVP

                case CustomComboPreset.DRKPvP_Burst:
                    UserConfig.DrawSliderInt(1, 100,
                        DRKPvP.Config.ShadowbringerThreshold,
                        "HP% to be at or Above to use " +
                        "(0 = Use Always)",
                        itemWidth: 150f, sliderIncrement: SliderIncrements.Fives);

                    break;

                #endregion
            }
        }
    }
}
