using Dalamud.Interface.Components;
using ECommons.ImGuiMethods;
using ImGuiNET;
using XIVSlothCombo.Services;

namespace XIVSlothCombo.Window.Tabs
{
    internal class AutoRotationTab : ConfigWindow
    {
        internal static new void Draw()
        {
            ImGui.TextWrapped($"This is where you can configure the parameters in which Auto-Rotation will operate. " +
                $"Features marked with an 'Auto-Mode' checkbox are able to be used with Auto-Rotation.");
            ImGui.Separator();

            var cfg = Service.Configuration.RotationConfig;
            bool changed = false;

            changed |= ImGui.Checkbox($"Enable Auto-Rotation", ref cfg.Enabled);
            if (cfg.Enabled)
            {
                changed |= ImGui.Checkbox("Only in Combat", ref cfg.InCombatOnly);
            }

            if (ImGui.CollapsingHeader("DPS Settings"))
            {
                ImGui.Text($"Targeting Mode");
                changed |= ImGuiEx.EnumCombo("###DPSTargetingMode", ref cfg.DPSRotationMode);
                ImGuiComponents.HelpMarker("Manual - Leaves all targeting decisions to you.\n" +
                    "Highest Max - Prioritises enemies with the highest max HP.\n" +
                    "Lowest Max - Prioritises enemies with the lowest max HP.\n" +
                    "Highest Current - Prioritises the enemy with the highest current HP.\n" +
                    "Lowest Current - Prioritises the enemy with the lowest current HP.\n" +
                    "Tank Target - Prioritises the same target as the first tank in your group.");
            }
            ImGui.Spacing();
            if (ImGui.CollapsingHeader("Healing Settings"))
            {
                ImGui.Text($"Targeting Mode");
                changed |= ImGuiEx.EnumCombo("###HealerTargetingMode", ref cfg.HealerRotationMode);
                ImGuiComponents.HelpMarker("Manual - Leaves all targeting decisions to you.\n" +
                    "Highest Current - Prioritises the party member with the highest current HP.\n" +
                    "Lowest Current - Prioritises the party member with the lowest current HP.\n" +
                    "Self Priority - Heal yourself before others.\n" +
                    "Tank Priority - Heal the tank(s) before others.\n" +
                    "Healer Priority - Heal the healer(s) before others.\n" +
                    "DPS Priority - Heal the DPS before others.");

                ImGui.SetNextItemWidth(200f.Scale());
                changed |= ImGuiEx.SliderInt("Single Target HP% Threshold", ref cfg.HealerSettings.SingleTargetHPP, 1, 99, "%d%%");
                ImGui.SetNextItemWidth(200f.Scale());
                changed |= ImGuiEx.SliderInt("AoE HP% Threshold", ref cfg.HealerSettings.AoETargetHPP, 1, 99, "%d%%");
                ImGuiComponents.HelpMarker("");
            }
            ImGui.Spacing();
            if (ImGui.CollapsingHeader("Tank Settings"))
            {
                ImGui.Text($"Targeting Mode");
                changed |= ImGuiEx.EnumCombo("###TankTargetingMode", ref cfg.TankRotationMode);
            }

            if (changed)
                Service.Configuration.Save();

        }
    }
}
