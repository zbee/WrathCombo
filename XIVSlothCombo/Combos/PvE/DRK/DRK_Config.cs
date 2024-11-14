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
        private const string stopUsingAtDescription =
            "Target HP% to stop using (0 = Use Always)";

        public static readonly UserInt
            DRK_ST_DeliriumThreshold = new("DRK_ST_DeliriumThreshold", 0),
            DRK_ST_LivingDeadThreshold = new("DRK_ST_LivingDeadThreshold", 5),
            DRK_ST_ManaSpenderPooling = new("DRK_ST_ManaSpenderPooling", 3000),
            DRK_AoE_DeliriumThreshold = new("DRK_AoE_DeliriumThreshold", 25),
            DRK_AoE_LivingDeadThreshold = new("DRK_AoE_LivingDeadThreshold", 40),
            DRK_VariantCure = new("DRKVariantCure");

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.DRK_ST_Delirium:
                    UserConfig.DrawSliderInt(0, 25, DRK_ST_DeliriumThreshold,
                        stopUsingAtDescription,
                        itemWidth: 100f, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_ST_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 30, DRK_ST_LivingDeadThreshold,
                        stopUsingAtDescription,
                        itemWidth: 100f, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_ST_ManaSpenderPooling:
                    UserConfig.DrawSliderInt(0, 3000, DRK_ST_ManaSpenderPooling,
                        stopUsingAtDescription,
                        itemWidth: 200f,
                        sliderIncrement: SliderIncrements.Thousands);

                    break;

                case CustomComboPreset.DRK_AoE_Delirium:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_DeliriumThreshold,
                        stopUsingAtDescription,
                        itemWidth: 150f, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_AoE_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_LivingDeadThreshold,
                        stopUsingAtDescription,
                        itemWidth: 150f, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, DRK_VariantCure,
                        "HP% to be at or Below",
                        itemWidth: 175f, sliderIncrement: SliderIncrements.Fives);

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
