using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Window.Functions;

namespace XIVSlothCombo.Combos.PvE;

internal partial class PLD
{
    internal static class Config
    {
        public static UserInt
            PLD_ST_FoF_Trigger = new("PLD_ST_FoF_Trigger", 0),
            PLD_AoE_FoF_Trigger = new("PLD_AoE_FoF_Trigger", 0),
            PLD_ST_SheltronOption = new("PLD_ST_SheltronOption", 50),
            PLD_AoE_SheltronOption = new("PLD_AoE_SheltronOption", 50),
            PLD_Intervene_HoldCharges = new("PLD_Intervene_HoldCharges", 1),
            PLD_AoE_Intervene_HoldCharges = new("PLD_AoE_Intervene_HoldCharges", 1),
            PLD_Intervene_MeleeOnly = new("PLD_Intervene_MeleeOnly", 1),
            PLD_AoE_Intervene_MeleeOnly = new("PLD_AoE_Intervene_MeleeOnly", 1),
            PLD_ST_MP_Reserve = new("PLD_ST_MP_Reserve", 1000),
            PLD_AoE_MP_Reserve = new("PLD_AoE_MP_Reserve", 1000),
            PLD_ShieldLob_SubOption = new("PLD_ShieldLob_SubOption", 1),
            PLD_RequiescatOption = new("PLD_RequiescatOption"),
            PLD_SpiritsWithinOption = new("PLD_SpiritsWithinOption"),
            PLD_VariantCure = new("PLD_VariantCure");

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                // Fight or Flight
                case CustomComboPreset.PLD_ST_AdvancedMode_FoF:
                    UserConfig.DrawSliderInt(0, 50, PLD_ST_FoF_Trigger, "Target HP%", 200);

                    break;

                case CustomComboPreset.PLD_AoE_AdvancedMode_FoF:
                    UserConfig.DrawSliderInt(0, 50, PLD_AoE_FoF_Trigger, "Target HP%", 200);

                    break;

                // Sheltron
                case CustomComboPreset.PLD_ST_AdvancedMode_Sheltron:
                    UserConfig.DrawSliderInt(50, 100, PLD_ST_SheltronOption, "Oath Gauge", 200, 5);

                    break;

                case CustomComboPreset.PLD_AoE_AdvancedMode_Sheltron:
                    UserConfig.DrawSliderInt(50, 100, PLD_AoE_SheltronOption, "Oath Gauge", 200, 5);

                    break;

                // Intervene
                case CustomComboPreset.PLD_ST_AdvancedMode_Intervene:
                    UserConfig.DrawSliderInt(0, 1, PLD_Intervene_HoldCharges, "Charges", 200);

                    UserConfig.DrawHorizontalRadioButton(PLD_Intervene_MeleeOnly, "Melee Range",
                        "Uses Intervene while within melee range.\nMay result in minor movement.", 1);

                    UserConfig.DrawHorizontalRadioButton(PLD_Intervene_MeleeOnly, "No Movement",
                        "Only uses Intervene when it would not result in movement (zero distance).", 2);

                    break;

                case CustomComboPreset.PLD_AoE_AdvancedMode_Intervene:
                    UserConfig.DrawSliderInt(0, 1, PLD_AoE_Intervene_HoldCharges, "Charges", 200);

                    UserConfig.DrawHorizontalRadioButton(PLD_AoE_Intervene_MeleeOnly, "Melee Range",
                        "Uses Intervene while within melee range.\nMay result in minor movement.", 1);

                    UserConfig.DrawHorizontalRadioButton(PLD_AoE_Intervene_MeleeOnly, "No Movement",
                        "Only uses Intervene when it would not result in movement (zero distance).", 2);

                    break;

                // Shield Lob
                case CustomComboPreset.PLD_ST_AdvancedMode_ShieldLob:
                    UserConfig.DrawHorizontalRadioButton(PLD_ShieldLob_SubOption, "Shield Lob Only",
                        "Uses only Shield Lob.", 1);

                    UserConfig.DrawHorizontalRadioButton(PLD_ShieldLob_SubOption, "Hardcast Holy Spirit",
                        "Attempts to hardcast Holy Spirit when not moving.\nOtherwise uses Shield Lob.", 2);

                    break;

                // MP Reservation
                case CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve:
                    UserConfig.DrawSliderInt(1000, 5000, PLD_ST_MP_Reserve, "Minimum MP", sliderIncrement: 100);

                    break;

                case CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve:
                    UserConfig.DrawSliderInt(1000, 5000, PLD_AoE_MP_Reserve, "Minimum MP", sliderIncrement: 100);

                    break;

                // Requiescat Spender Feature
                case CustomComboPreset.PLD_Requiescat_Options:
                    UserConfig.DrawRadioButton(PLD_RequiescatOption, "Confiteor", "", 1);
                    UserConfig.DrawRadioButton(PLD_RequiescatOption, "Blade of Faith/Truth/Valor", "", 2);
                    UserConfig.DrawRadioButton(PLD_RequiescatOption, "Confiteor & Blade of Faith/Truth/Valor", "", 3);
                    UserConfig.DrawRadioButton(PLD_RequiescatOption, "Holy Spirit", "", 4);
                    UserConfig.DrawRadioButton(PLD_RequiescatOption, "Holy Circle", "", 5);

                    break;

                // Spirits Within / Circle of Scorn Feature
                case CustomComboPreset.PLD_SpiritsWithin:
                    UserConfig.DrawRadioButton(PLD_SpiritsWithinOption, "Prioritize Circle of Scorn", "", 1);
                    UserConfig.DrawRadioButton(PLD_SpiritsWithinOption, "Prioritize Spirits Within / Expiacion", "", 2);

                    break;

                // Variant Cure Feature
                case CustomComboPreset.PLD_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, PLD_VariantCure, "Player HP%", 200);

                    break;
            }
        }
    }
}