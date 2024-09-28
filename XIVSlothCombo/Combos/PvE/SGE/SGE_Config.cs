using ImGuiNET;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Extensions;
using XIVSlothCombo.Window.Functions;

namespace XIVSlothCombo.Combos.PvE
{
    internal static partial class SGE
    {
        public static class Config
        {
            #region DPS
            public static UserBool
                SGE_ST_DPS_Adv = new("SGE_ST_DPS_Adv"),
                SGE_ST_DPS_EDosis_Adv = new("SGE_ST_Dosis_EDosis_Adv");
            public static UserBoolArray
                SGE_ST_DPS_Movement = new("SGE_ST_DPS_Movement");
            public static UserInt
                SGE_ST_DPS_EDosisHPPer = new("SGE_ST_DPS_EDosisHPPer", 10),
                SGE_ST_DPS_Lucid = new("SGE_ST_DPS_Lucid", 6500),
                SGE_ST_DPS_Rhizo = new("SGE_ST_DPS_Rhizo"),
                SGE_ST_DPS_AddersgallProtect = new("SGE_ST_DPS_AddersgallProtect", 3),
                SGE_AoE_DPS_Lucid = new("SGE_AoE_Phlegma_Lucid", 6500),
                SGE_AoE_DPS_Rhizo = new("SGE_AoE_DPS_Rhizo"),
                SGE_AoE_DPS_AddersgallProtect = new("SGE_AoE_DPS_AddersgallProtect", 3);
            public static UserFloat
                SGE_ST_DPS_EDosisThreshold = new("SGE_ST_Dosis_EDosisThreshold", 3.0f);
            #endregion

            #region Healing
            public static UserBool
                SGE_ST_Heal_Adv = new("SGE_ST_Heal_Adv"),
                SGE_ST_Heal_UIMouseOver = new("SGE_ST_Heal_UIMouseOver"),
                SGE_AoE_Heal_KeracholeTrait = new("SGE_AoE_Heal_KeracholeTrait");
            public static UserInt
                SGE_ST_Heal_Zoe = new("SGE_ST_Heal_Zoe"),
                SGE_ST_Heal_Haima = new("SGE_ST_Heal_Haima"),
                SGE_ST_Heal_Krasis = new("SGE_ST_Heal_Krasis"),
                SGE_ST_Heal_Pepsis = new("SGE_ST_Heal_Pepsis"),
                SGE_ST_Heal_Soteria = new("SGE_ST_Heal_Soteria"),
                SGE_ST_Heal_EDiagnosisHP = new("SGE_ST_Heal_EDiagnosisHP"),
                SGE_ST_Heal_Druochole = new("SGE_ST_Heal_Druochole"),
                SGE_ST_Heal_Taurochole = new("SGE_ST_Heal_Taurochole"),
                SGE_ST_Heal_Esuna = new("SGE_ST_Heal_Esuna");
            public static UserIntArray
                SGE_ST_Heals_Priority = new("SGE_ST_Heals_Priority"),
                SGE_AoE_Heals_Priority = new("SGE_AoE_Heals_Priority");
            public static UserBoolArray
                SGE_ST_Heal_EDiagnosisOpts = new("SGE_ST_Heal_EDiagnosisOpts");
            #endregion

            public static UserInt
                SGE_Eukrasia_Mode = new("SGE_Eukrasia_Mode");

            internal static void Draw(CustomComboPreset preset)
            {
                if (preset is CustomComboPreset.SGE_ST_DPS)
                    UserConfig.DrawAdditionalBoolChoice(SGE_ST_DPS_Adv, $"Apply all selected options to {Dosis2.ActionName()}", $"{Dosis.ActionName()} & {Dosis3.ActionName()} will behave normally.");

                if (preset is CustomComboPreset.SGE_ST_DPS_EDosis)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_DPS_EDosisHPPer, "Stop using at Enemy HP %. Set to Zero to disable this check");

                    UserConfig.DrawAdditionalBoolChoice(SGE_ST_DPS_EDosis_Adv, "Advanced Options", "", isConditionalChoice: true);
                    if (SGE_ST_DPS_EDosis_Adv)
                    {
                        ImGui.Indent();
                        UserConfig.DrawRoundedSliderFloat(0, 4, SGE_ST_DPS_EDosisThreshold, "Seconds remaining before reapplying the DoT. Set to Zero to disable this check.", digits: 1);
                        ImGui.Unindent();
                    }
                }

                if (preset is CustomComboPreset.SGE_ST_DPS_Lucid)
                    UserConfig.DrawSliderInt(4000, 9500, SGE_ST_DPS_Lucid, "MP Threshold", 150, SliderIncrements.Hundreds);

                if (preset is CustomComboPreset.SGE_ST_DPS_Rhizo)
                    UserConfig.DrawSliderInt(0, 1, SGE_ST_DPS_Rhizo, "Addersgall Threshold", 150, SliderIncrements.Ones);

                if (preset is CustomComboPreset.SGE_ST_DPS_AddersgallProtect)
                    UserConfig.DrawSliderInt(1, 3, SGE_ST_DPS_AddersgallProtect, "Addersgall Threshold", 150, SliderIncrements.Ones);

                if (preset is CustomComboPreset.SGE_ST_DPS_Movement)
                {
                    UserConfig.DrawHorizontalMultiChoice(SGE_ST_DPS_Movement, Toxikon.ActionName(), $"Use {Toxikon.ActionName()} when Addersting is available.", 4, 0);
                    UserConfig.DrawHorizontalMultiChoice(SGE_ST_DPS_Movement, Dyskrasia.ActionName(), $"Use {Dyskrasia.ActionName()} when in range of a selected enemy target.", 4, 1);
                    UserConfig.DrawHorizontalMultiChoice(SGE_ST_DPS_Movement, Eukrasia.ActionName(), $"Use {Eukrasia.ActionName()}.", 4, 2);
                    UserConfig.DrawHorizontalMultiChoice(SGE_ST_DPS_Movement, Psyche.ActionName(), $"Use {Psyche.ActionName()}.", 4, 3);
                }

                if (preset is CustomComboPreset.SGE_AoE_DPS_Lucid)
                    UserConfig.DrawSliderInt(4000, 9500, SGE_AoE_DPS_Lucid, "MP Threshold", 150, SliderIncrements.Hundreds);

                if (preset is CustomComboPreset.SGE_AoE_DPS_Rhizo)
                    UserConfig.DrawSliderInt(0, 1, SGE_AoE_DPS_Rhizo, "Addersgall Threshold", 150, SliderIncrements.Ones);

                if (preset is CustomComboPreset.SGE_AoE_DPS_AddersgallProtect)
                    UserConfig.DrawSliderInt(1, 3, SGE_AoE_DPS_AddersgallProtect, "Addersgall Threshold", 150, SliderIncrements.Ones);

                if (preset is CustomComboPreset.SGE_ST_Heal)
                {
                    UserConfig.DrawAdditionalBoolChoice(SGE_ST_Heal_Adv, "Advanced Options", "", isConditionalChoice: true);
                    if (SGE_ST_Heal_Adv)
                    {
                        ImGui.Indent();
                        UserConfig.DrawAdditionalBoolChoice(SGE_ST_Heal_UIMouseOver,
                            "Party UI Mouseover Checking",
                            "Check party member's HP & Debuffs by using mouseover on the party list.\n" +
                            "To be used in conjunction with Redirect/Reaction/etc");
                        ImGui.Unindent();
                    }
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Esuna)
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Esuna, "Stop using when below HP %. Set to Zero to disable this check");

                if (preset is CustomComboPreset.SGE_ST_Heal_Soteria)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Soteria, $"Use {Soteria.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 0, $"{Soteria.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Zoe)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Zoe, $"Use {Zoe.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 1, $"{Zoe.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Pepsis)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Pepsis, $"Use {Pepsis.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 2, $"{Pepsis.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Taurochole)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Taurochole, $"Use {Taurochole.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 3, $"{Taurochole.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Haima)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Haima, $"Use {Haima.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 4, $"{Haima.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Krasis)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Krasis, $"Use {Krasis.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 5, $"{Krasis.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_Druochole)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_Druochole, $"Use {Druochole.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawPriorityInput(SGE_ST_Heals_Priority, 7, 6, $"{Druochole.ActionName()} Priority: ");
                }

                if (preset is CustomComboPreset.SGE_ST_Heal_EDiagnosis)
                {
                    UserConfig.DrawSliderInt(0, 100, SGE_ST_Heal_EDiagnosisHP, $"Use {EukrasianDiagnosis.ActionName()} when Target HP is at or below set percentage");
                    UserConfig.DrawHorizontalMultiChoice(SGE_ST_Heal_EDiagnosisOpts, "Ignore Shield Check", $"Warning, will force the use of {EukrasianDiagnosis.ActionName()}, and normal {Diagnosis.ActionName()} will be unavailable.", 2, 0);
                    UserConfig.DrawHorizontalMultiChoice(SGE_ST_Heal_EDiagnosisOpts, "Check for Scholar Galvenize", "Enable to not override an existing Scholar's shield.", 2, 1);
                }

                if (preset is CustomComboPreset.SGE_AoE_Heal_Kerachole)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 0, $"{Kerachole.ActionName()} Priority: ");

                if (preset is CustomComboPreset.SGE_AoE_Heal_Ixochole)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 1, $"{Ixochole.ActionName()} Priority: ");

                if (preset is CustomComboPreset.SGE_AoE_Heal_Physis)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 2, $"{Physis.ActionName()} Priority: ");

                if (preset is CustomComboPreset.SGE_AoE_Heal_Holos)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 3, $"{Holos.ActionName()} Priority: ");

                if (preset is CustomComboPreset.SGE_AoE_Heal_Panhaima)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 4, $"{Panhaima.ActionName()} Priority: ");

                if (preset is CustomComboPreset.SGE_AoE_Heal_Pepsis)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 5, $"{Pepsis.ActionName()} Priority: ");

                if (preset is CustomComboPreset.SGE_AoE_Heal_Philosophia)
                    UserConfig.DrawPriorityInput(SGE_AoE_Heals_Priority, 7, 6, $"{Philosophia.ActionName()} Priority: ");


                if (preset is CustomComboPreset.SGE_AoE_Heal_Kerachole)
                    UserConfig.DrawAdditionalBoolChoice(SGE_AoE_Heal_KeracholeTrait,
                        "Check for Enhanced Kerachole Trait (Heal over Time)",
                        $"Enabling this will prevent {Kerachole.ActionName()} from being used when the Heal over Time trait is unavailable.");

                if (preset is CustomComboPreset.SGE_Eukrasia)
                {
                    UserConfig.DrawRadioButton(SGE_Eukrasia_Mode, $"{EukrasianDosis.ActionName()}", "", 0);
                    UserConfig.DrawRadioButton(SGE_Eukrasia_Mode, $"{EukrasianDiagnosis.ActionName()}", "", 1);
                    UserConfig.DrawRadioButton(SGE_Eukrasia_Mode, $"{EukrasianPrognosis.ActionName()}", "", 2);
                    UserConfig.DrawRadioButton(SGE_Eukrasia_Mode, $"{EukrasianDyskrasia.ActionName()}", "", 3);
                }
            }
        }
    }
}
