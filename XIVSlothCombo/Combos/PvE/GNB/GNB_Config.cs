using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Window.Functions;

namespace XIVSlothCombo.Combos.PvE;

internal partial class GNB
{
    internal static class Config
    {
        public const string
            GNB_VariantCure = "GNB_VariantCure";

        public static UserInt
            GNB_ST_NoMercyStop = new("GNB_ST_NoMercyStop"),
            GNB_AoE_NoMercyStop = new("GNB_AoE_NoMercyStop");

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.GNB_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, GNB_VariantCure, "HP% to be at or under", 200);

                    break;

                case CustomComboPreset.GNB_ST_NoMercy:
                    UserConfig.DrawSliderInt(0, 25, GNB_ST_NoMercyStop,
                        "Stop Usage if Target HP% is below set value.\nTo Disable this option, set to 0.");

                    break;

                case CustomComboPreset.GNB_AoE_NoMercy:
                    UserConfig.DrawSliderInt(0, 25, GNB_AoE_NoMercyStop,
                        "Stop Usage if Target HP% is below set value.\nTo Disable this option, set to 0.");

                    break;
            }
        }
    }
}