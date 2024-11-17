using XIVSlothCombo.Combos.PvP;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Window.Functions;

namespace XIVSlothCombo.Combos.PvE;

internal partial class DRK
{
    internal static class Config
    {
        public static readonly UserInt
                DRK_ST_ManaSpenderPooling = new("DRK_ST_ManaSpenderPooling", 3000),
                DRK_ST_LivingDeadThreshold = new("DRK_ST_LivingDeadThreshold", 10),
                DRK_AoE_LivingDeadThreshold = new("DRK_AoE_LivingDeadThreshold", 40),
                DRK_ST_DeliriumThreshold = new("DRK_ST_DeliriumThreshold", 0),
                DRK_AoE_DeliriumThreshold = new("DRK_AoE_DeliriumThreshold", 25),
                DRK_VariantCure = new("DRKVariantCure");

        internal static void Draw(CustomComboPreset preset)
        {
            #region DARK KNIGHT

            switch (preset)
            {
                case CustomComboPreset.DRK_ST_ManaSpenderPooling:
                    UserConfig.DrawSliderInt(0, 3000, DRK_ST_ManaSpenderPooling,
                        "How much MP to save (0 = Use All)", 150, SliderIncrements.Thousands);

                    break;

                case CustomComboPreset.DRK_ST_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 30, DRK_ST_LivingDeadThreshold,
                        "Stop Using When Target HP% is at or Below (Set to 0 to Disable This Check)", 150,
                        SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_AoE_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_LivingDeadThreshold,
                        "Stop Using When Target HP% is at or Below (Set to 0 to Disable This Check)", 150,
                        SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRKPvP_Burst:
                    UserConfig.DrawSliderInt(1, 100, DRKPvP.Config.ShadowbringerThreshold,
                        "HP% to be at or above to use Shadowbringer");

                    break;

                case CustomComboPreset.DRK_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, DRK_VariantCure, "HP% to be at or under", 200);

                    break;
            }

            #endregion
        }
    }
}