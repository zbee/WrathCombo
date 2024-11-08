using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using ECommons.DalamudServices;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using XIVSlothCombo.Combos;
using XIVSlothCombo.Combos.PvE;
using XIVSlothCombo.Combos.PvP;
using XIVSlothCombo.Core;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Data;
using XIVSlothCombo.Extensions;
using XIVSlothCombo.Services;


namespace XIVSlothCombo.Window.Functions
{
    public static class UserConfig
    {
        /// <summary> Draws a slider that lets the user set a given value for their feature. </summary>
        /// <param name="minValue"> The absolute minimum value you'll let the user pick. </param>
        /// <param name="maxValue"> The absolute maximum value you'll let the user pick. </param>
        /// <param name="config"> The config ID. </param>
        /// <param name="sliderDescription"> Description of the slider. Appends to the right of the slider. </param>
        /// <param name="itemWidth"> How long the slider should be. </param>
        /// <param name="sliderIncrement"> How much you want the user to increment the slider by. Uses SliderIncrements as a preset. </param>
        /// <param name="hasAdditionalChoice">True if this config can trigger additional configs depending on value.</param>
        /// <param name="additonalChoiceCondition">What the condition is to convey to the user what triggers it.</param>
        public static void DrawSliderInt(int minValue, int maxValue, string config, string sliderDescription, float itemWidth = 150, uint sliderIncrement = SliderIncrements.Ones, bool hasAdditionalChoice = false, string additonalChoiceCondition = "")
        {
            ImGui.Indent();
            int output = PluginConfiguration.GetCustomIntValue(config, minValue);
            if (output < minValue)
            {
                output = minValue;
                PluginConfiguration.SetCustomIntValue(config, output);
                Service.Configuration.Save();
            }

            sliderDescription = sliderDescription.Replace("%", "%%");
            float contentRegionMin = ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y;
            float wrapPos = ImGui.GetContentRegionMax().X - 35f;

            InfoBox box = new()
            {
                Color = Colors.White,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    bool inputChanged = false;
                    Vector2 currentPos = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(currentPos.X + itemWidth);
                    ImGui.PushTextWrapPos(wrapPos);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{sliderDescription}");
                    Vector2 height = ImGui.GetItemRectSize();
                    float lines = height.Y / ImGui.GetFontSize();
                    Vector2 textLength = ImGui.CalcTextSize(sliderDescription);
                    string newLines = "";
                    for (int i = 1; i < lines; i++)
                    {
                        if (i % 2 == 0)
                        {
                            newLines += "\n";
                        }
                        else
                        {
                            newLines += "\n\n";
                        }

                    }

                    if (hasAdditionalChoice)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.Dummy(new Vector2(5, 0));
                        ImGui.SameLine();
                        ImGui.TextWrapped($"{FontAwesomeIcon.Search.ToIconString()}");
                        ImGui.PopFont();
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"This setting has additional options depending on its value.{(string.IsNullOrEmpty(additonalChoiceCondition) ? "" : $"\nCondition: {additonalChoiceCondition}")}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(currentPos.X);
                    ImGui.PushItemWidth(itemWidth);
                    inputChanged |= ImGui.SliderInt($"{newLines}###{config}", ref output, minValue, maxValue);

                    if (inputChanged)
                    {
                        if (output % sliderIncrement != 0)
                        {
                            output = output.RoundOff(sliderIncrement);
                            if (output < minValue) output = minValue;
                            if (output > maxValue) output = maxValue;
                        }

                        PluginConfiguration.SetCustomIntValue(config, output);
                        Service.Configuration.Save();
                    }
                }
            };

            box.Draw();
            ImGui.Spacing();
            ImGui.Unindent();
        }

        /// <summary> Draws a slider that lets the user set a given value for their feature. </summary>
        /// <param name="minValue"> The absolute minimum value you'll let the user pick. </param>
        /// <param name="maxValue"> The absolute maximum value you'll let the user pick. </param>
        /// <param name="config"> The config ID. </param>
        /// <param name="sliderDescription"> Description of the slider. Appends to the right of the slider. </param>
        /// <param name="itemWidth"> How long the slider should be. </param>
        /// <param name="hasAdditionalChoice"></param>
        /// <param name="additonalChoiceCondition"></param>
        public static void DrawSliderFloat(float minValue, float maxValue, string config, string sliderDescription, float itemWidth = 150, bool hasAdditionalChoice = false, string additonalChoiceCondition = "")
        {
            float output = PluginConfiguration.GetCustomFloatValue(config, minValue);
            if (output < minValue)
            {
                output = minValue;
                PluginConfiguration.SetCustomFloatValue(config, output);
                Service.Configuration.Save();
            }

            sliderDescription = sliderDescription.Replace("%", "%%");
            float contentRegionMin = ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y;
            float wrapPos = ImGui.GetContentRegionMax().X - 35f;


            InfoBox box = new()
            {
                Color = Colors.White,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    bool inputChanged = false;
                    Vector2 currentPos = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(currentPos.X + itemWidth);
                    ImGui.PushTextWrapPos(wrapPos);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{sliderDescription}");
                    Vector2 height = ImGui.GetItemRectSize();
                    float lines = height.Y / ImGui.GetFontSize();
                    Vector2 textLength = ImGui.CalcTextSize(sliderDescription);
                    string newLines = "";
                    for (int i = 1; i < lines; i++)
                    {
                        if (i % 2 == 0)
                        {
                            newLines += "\n";
                        }
                        else
                        {
                            newLines += "\n\n";
                        }

                    }

                    if (hasAdditionalChoice)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.Dummy(new Vector2(5, 0));
                        ImGui.SameLine();
                        ImGui.TextWrapped($"{FontAwesomeIcon.Search.ToIconString()}");
                        ImGui.PopFont();
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"This setting has additional options depending on its value.{(string.IsNullOrEmpty(additonalChoiceCondition) ? "" : $"\nCondition: {additonalChoiceCondition}")}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(currentPos.X);
                    ImGui.PushItemWidth(itemWidth);
                    inputChanged |= ImGui.SliderFloat($"{newLines}###{config}", ref output, minValue, maxValue);

                    if (inputChanged)
                    {
                        PluginConfiguration.SetCustomFloatValue(config, output);
                        Service.Configuration.Save();
                    }
                }
            };

            box.Draw();
            ImGui.Spacing();
        }

        /// <summary> Draws a slider that lets the user set a given value for their feature. </summary>
        /// <param name="minValue"> The absolute minimum value you'll let the user pick. </param>
        /// <param name="maxValue"> The absolute maximum value you'll let the user pick. </param>
        /// <param name="config"> The config ID. </param>
        /// <param name="sliderDescription"> Description of the slider. Appends to the right of the slider. </param>
        /// <param name="itemWidth"> How long the slider should be. </param>
        /// <param name="hasAdditionalChoice"></param>
        /// <param name="additonalChoiceCondition"></param>
        /// <param name="digits"></param>
        public static void DrawRoundedSliderFloat(float minValue, float maxValue, string config, string sliderDescription, float itemWidth = 150, bool hasAdditionalChoice = false, string additonalChoiceCondition = "", int digits = 1)
        {
            float output = PluginConfiguration.GetCustomFloatValue(config, minValue);
            if (output < minValue)
            {
                output = minValue;
                PluginConfiguration.SetCustomFloatValue(config, output);
                Service.Configuration.Save();
            }

            sliderDescription = sliderDescription.Replace("%", "%%");
            float contentRegionMin = ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y;
            float wrapPos = ImGui.GetContentRegionMax().X - 35f;


            InfoBox box = new()
            {
                Color = Colors.White,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    bool inputChanged = false;
                    Vector2 currentPos = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(currentPos.X + itemWidth);
                    ImGui.PushTextWrapPos(wrapPos);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{sliderDescription}");
                    Vector2 height = ImGui.GetItemRectSize();
                    float lines = height.Y / ImGui.GetFontSize();
                    Vector2 textLength = ImGui.CalcTextSize(sliderDescription);
                    string newLines = "";
                    for (int i = 1; i < lines; i++)
                    {
                        if (i % 2 == 0)
                        {
                            newLines += "\n";
                        }
                        else
                        {
                            newLines += "\n\n";
                        }

                    }

                    if (hasAdditionalChoice)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.Dummy(new Vector2(5, 0));
                        ImGui.SameLine();
                        ImGui.TextWrapped($"{FontAwesomeIcon.Search.ToIconString()}");
                        ImGui.PopFont();
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"This setting has additional options depending on its value.{(string.IsNullOrEmpty(additonalChoiceCondition) ? "" : $"\nCondition: {additonalChoiceCondition}")}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(currentPos.X);
                    ImGui.PushItemWidth(itemWidth);
                    inputChanged |= ImGui.SliderFloat($"{newLines}###{config}", ref output, minValue, maxValue, $"%.{digits}f");

                    if (inputChanged)
                    {
                        PluginConfiguration.SetCustomFloatValue(config, output);
                        Service.Configuration.Save();
                    }
                }
            };

            box.Draw();
            ImGui.Spacing();
        }

        /// <summary> Draws a checkbox intended to be linked to other checkboxes sharing the same config value. </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// <param name="outputValue"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="itemWidth"></param>
        /// <param name="descriptionColor"></param>
        public static void DrawRadioButton(string config, string checkBoxName, string checkboxDescription, int outputValue, float itemWidth = 150, Vector4 descriptionColor = new Vector4())
        {
            ImGui.Indent();
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudYellow;
            int output = PluginConfiguration.GetCustomIntValue(config, outputValue);
            ImGui.PushItemWidth(itemWidth);
            ImGui.SameLine();
            ImGui.Dummy(new Vector2(21, 0));
            ImGui.SameLine();
            bool enabled = output == outputValue;

            if (ImGui.RadioButton($"{checkBoxName}###{config}{outputValue}", enabled))
            {
                PluginConfiguration.SetCustomIntValue(config, outputValue);
                Service.Configuration.Save();
            }

            if (!checkboxDescription.IsNullOrEmpty())
            {
                ImGui.PushStyleColor(ImGuiCol.Text, descriptionColor);
                ImGui.TextWrapped(checkboxDescription);
                ImGui.PopStyleColor();
            }

            ImGui.Unindent();
            ImGui.Spacing();
        }

        /// <summary> Draws a checkbox in a horizontal configuration intended to be linked to other checkboxes sharing the same config value. </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// <param name="outputValue"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="itemWidth"></param>
        /// <param name="descriptionColor"></param>
        public static void DrawHorizontalRadioButton(string config, string checkBoxName, string checkboxDescription, int outputValue, float itemWidth = 150, Vector4 descriptionColor = new Vector4())
        {
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudYellow;
            int output = PluginConfiguration.GetCustomIntValue(config);
            ImGui.SameLine();
            ImGui.PushItemWidth(itemWidth);
            bool enabled = output == outputValue;

            ImGui.PushStyleColor(ImGuiCol.Text, descriptionColor);
            if (ImGui.RadioButton($"{checkBoxName}###{config}{outputValue}", enabled))
            {
                PluginConfiguration.SetCustomIntValue(config, outputValue);
                Service.Configuration.Save();
            }

            if (!checkboxDescription.IsNullOrEmpty() && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(checkboxDescription);
                ImGui.EndTooltip();
            }
            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.Dummy(new Vector2(16f, 0));
        }

        /// <summary>A true or false configuration. Similar to presets except can be used as part of a condition on another config.</summary>
        /// <param name="config">The config ID.</param>
        /// <param name="checkBoxName">The name of the feature.</param>
        /// <param name="checkboxDescription">The description of the feature</param>
        /// <param name="itemWidth"></param>
        /// <param name="isConditionalChoice"></param>
        public static void DrawAdditionalBoolChoice(string config, string checkBoxName, string checkboxDescription, float itemWidth = 150, bool isConditionalChoice = false)
        {
            bool output = PluginConfiguration.GetCustomBoolValue(config);
            ImGui.PushItemWidth(itemWidth);
            if (!isConditionalChoice)
                ImGui.Indent();
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.AlignTextToFramePadding();
                ImGui.TextWrapped($"{FontAwesomeIcon.Plus.ToIconString()}");
                ImGui.PopFont();
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(3));
                ImGui.SameLine();
                if (isConditionalChoice) ImGui.Indent(); //Align checkbox after the + symbol
            }
            if (ImGui.Checkbox($"{checkBoxName}###{config}", ref output))
            {
                PluginConfiguration.SetCustomBoolValue(config, output);
                Service.Configuration.Save();
            }

            if (!checkboxDescription.IsNullOrEmpty())
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
                ImGui.TextWrapped(checkboxDescription);
                ImGui.PopStyleColor();
            }

            //!isConditionalChoice
            ImGui.Unindent();
            ImGui.Spacing();
        }

        /// <summary> Draws multi choice checkboxes in a horizontal configuration. </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// <param name="totalChoices"> The total number of options for the feature </param>
        /// /// <param name="choice"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="descriptionColor"></param>
        public static void DrawHorizontalMultiChoice(string config, string checkBoxName, string checkboxDescription, int totalChoices, int choice, Vector4 descriptionColor = new Vector4())
        {
            ImGui.Indent();
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudWhite;
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);

            //If new saved options or amount of choices changed, resize and save
            if (values.Length == 0 || values.Length != totalChoices)
            {
                Array.Resize(ref values, totalChoices);
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.PushStyleColor(ImGuiCol.Text, descriptionColor);
            if (choice > 0)
            {
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(12f, 0));
                ImGui.SameLine();
            }

            if (ImGui.Checkbox($"{checkBoxName}###{config}{choice}", ref values[choice]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }
            if (!checkboxDescription.IsNullOrEmpty() && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(checkboxDescription);
                ImGui.EndTooltip();
            }


            ImGui.PopStyleColor();
            ImGui.Unindent();
        }

        public static void DrawGridMultiChoice(string config, byte columns, string[,] nameAndDesc, Vector4 descriptionColor = new Vector4())
        {
            int totalChoices = nameAndDesc.GetLength(0);
            if (totalChoices > 0)
            {
                ImGui.Indent();
                if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudWhite;
                //ImGui.PushItemWidth(itemWidth);
                //ImGui.SameLine();
                //ImGui.Dummy(new Vector2(21, 0));
                //ImGui.SameLine();
                bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);

                //If new saved options or amount of choices changed, resize and save
                if (values.Length == 0 || values.Length != totalChoices)
                {
                    Array.Resize(ref values, totalChoices);
                    PluginConfiguration.SetCustomBoolArrayValue(config, values);
                    Service.Configuration.Save();
                }

                ImGui.BeginTable($"Grid###{config}", columns);
                ImGui.TableNextRow();
                //Convert the 2D array of names and descriptions into radio buttons
                for (int idx = 0; idx < totalChoices; idx++)
                {
                    ImGui.TableNextColumn();
                    string checkBoxName = nameAndDesc[idx, 0];
                    string checkboxDescription = nameAndDesc[idx, 1];

                    ImGui.PushStyleColor(ImGuiCol.Text, descriptionColor);
                    if (ImGui.Checkbox($"{checkBoxName}###{config}{idx}", ref values[idx]))
                    {
                        PluginConfiguration.SetCustomBoolArrayValue(config, values);
                        Service.Configuration.Save();
                    }

                    if (!checkboxDescription.IsNullOrEmpty() && ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted(checkboxDescription);
                        ImGui.EndTooltip();
                    }

                    ImGui.PopStyleColor();

                    if (((idx + 1) % columns) == 0)
                        ImGui.TableNextRow();
                }
                ImGui.EndTable();
                ImGui.Unindent();
            }
        }

        public static void DrawPvPStatusMultiChoice(string config)
        {
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);

            ImGui.Columns(4, $"{config}", false);

            if (values.Length == 0) Array.Resize(ref values, 7);

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedPink);

            if (ImGui.Checkbox($"Stun###{config}0", ref values[0]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Deep Freeze###{config}1", ref values[1]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Half Asleep###{config}2", ref values[2]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Sleep###{config}3", ref values[3]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Bind###{config}4", ref values[4]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Heavy###{config}5", ref values[5]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Silence###{config}6", ref values[6]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.Columns(1);
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }

        public static void DrawRoleGridMultiChoice(string config)
        {
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);

            ImGui.Columns(5, $"{config}", false);

            if (values.Length == 0) Array.Resize(ref values, 5);

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);

            if (ImGui.Checkbox($"Tanks###{config}0", ref values[0]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);

            if (ImGui.Checkbox($"Healers###{config}1", ref values[1]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DPSRed);

            if (ImGui.Checkbox($"Melee###{config}2", ref values[2]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);

            if (ImGui.Checkbox($"Ranged###{config}3", ref values[3]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedPurple);

            if (ImGui.Checkbox($"Casters###{config}4", ref values[4]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.Columns(1);
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }

        public static void DrawRoleGridSingleChoice(string config)
        {
            int value = PluginConfiguration.GetCustomIntValue(config);
            bool[] values = new bool[20];

            for (int i = 0; i <= 4; i++)
            {
                if (value == i) values[i] = true;
                else
                    values[i] = false;
            }

            ImGui.Columns(5, $"{config}", false);

            if (values.Length == 0) Array.Resize(ref values, 5);

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);

            if (ImGui.Checkbox($"Tanks###{config}0", ref values[0]))
            {
                PluginConfiguration.SetCustomIntValue(config, 0);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);

            if (ImGui.Checkbox($"Healers###{config}1", ref values[1]))
            {
                PluginConfiguration.SetCustomIntValue(config, 1);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DPSRed);

            if (ImGui.Checkbox($"Melee###{config}2", ref values[2]))
            {
                PluginConfiguration.SetCustomIntValue(config, 2);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);

            if (ImGui.Checkbox($"Ranged###{config}3", ref values[3]))
            {
                PluginConfiguration.SetCustomIntValue(config, 3);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedPurple);

            if (ImGui.Checkbox($"Casters###{config}4", ref values[4]))
            {
                PluginConfiguration.SetCustomIntValue(config, 4);
                Service.Configuration.Save();
            }

            ImGui.Columns(1);
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }

        public static void DrawJobGridMultiChoice(string config)
        {
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);

            ImGui.Columns(5, $"{config}", false);

            if (values.Length == 0) Array.Resize(ref values, 20);

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);

            if (ImGui.Checkbox($"Paladin###{config}0", ref values[0]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Warrior###{config}1", ref values[1]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Dark Knight###{config}2", ref values[2]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Gunbreaker###{config}3", ref values[3]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.NextColumn();

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);

            if (ImGui.Checkbox($"White Mage###{config}", ref values[4]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Scholar###{config}5", ref values[5]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Astrologian###{config}6", ref values[6]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Sage###{config}7", ref values[7]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.NextColumn();

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DPSRed);

            if (ImGui.Checkbox($"Monk###{config}8", ref values[8]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Dragoon###{config}9", ref values[9]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Ninja###{config}10", ref values[10]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Samurai###{config}11", ref values[11]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Reaper###{config}12", ref values[12]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.NextColumn();

            if (ImGui.Checkbox($"Bard###{config}13", ref values[13]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Machinist###{config}14", ref values[14]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Dancer###{config}15", ref values[15]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedPurple);

            if (ImGui.Checkbox($"Black Mage###{config}16", ref values[16]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Summoner###{config}17", ref values[17]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Red Mage###{config}18", ref values[18]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Blue Mage###{config}19", ref values[19]))
            {
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            ImGui.PopStyleColor();
            ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.Spacing();
        }

        public static void DrawJobGridSingleChoice(string config)
        {
            int value = PluginConfiguration.GetCustomIntValue(config);
            bool[] values = new bool[20];

            for (int i = 0; i <= 19; i++)
            {
                if (value == i) values[i] = true;
                else
                    values[i] = false;
            }

            ImGui.Columns(5, null, false);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);

            if (ImGui.Checkbox($"Paladin###{config}0", ref values[0]))
            {
                PluginConfiguration.SetCustomIntValue(config, 0);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Warrior###{config}1", ref values[1]))
            {
                PluginConfiguration.SetCustomIntValue(config, 1);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Dark Knight###{config}2", ref values[2]))
            {
                PluginConfiguration.SetCustomIntValue(config, 2);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Gunbreaker###{config}3", ref values[3]))
            {
                PluginConfiguration.SetCustomIntValue(config, 3);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.NextColumn();

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);

            if (ImGui.Checkbox($"White Mage###{config}4", ref values[4]))
            {
                PluginConfiguration.SetCustomIntValue(config, 4);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Scholar###{config}5", ref values[5]))
            {
                PluginConfiguration.SetCustomIntValue(config, 5);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Astrologian###{config}6", ref values[6]))
            {
                PluginConfiguration.SetCustomIntValue(config, 6);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Sage###{config}7", ref values[7]))
            {
                PluginConfiguration.SetCustomIntValue(config, 7);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.NextColumn();

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DPSRed);

            if (ImGui.Checkbox($"Monk###{config}8", ref values[8]))
            {
                PluginConfiguration.SetCustomIntValue(config, 8);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Dragoon###{config}9", ref values[9]))
            {
                PluginConfiguration.SetCustomIntValue(config, 9);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Ninja###{config}10", ref values[10]))
            {
                PluginConfiguration.SetCustomIntValue(config, 10);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Samurai###{config}11", ref values[11]))
            {
                PluginConfiguration.SetCustomIntValue(config, 11);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Reaper###{config}12", ref values[12]))
            {
                PluginConfiguration.SetCustomIntValue(config, 12);
                Service.Configuration.Save();
            }

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.NextColumn();

            if (ImGui.Checkbox($"Bard###{config}13", ref values[13]))
            {
                PluginConfiguration.SetCustomIntValue(config, 13);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Machinist###{config}14", ref values[14]))
            {
                PluginConfiguration.SetCustomIntValue(config, 14);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Dancer###{config}15", ref values[15]))
            {
                PluginConfiguration.SetCustomIntValue(config, 15);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();

            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedPurple);

            if (ImGui.Checkbox($"Black Mage###{config}16", ref values[16]))
            {
                PluginConfiguration.SetCustomIntValue(config, 16);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Summoner###{config}17", ref values[17]))
            {
                PluginConfiguration.SetCustomIntValue(config, 17);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Red Mage###{config}18", ref values[18]))
            {
                PluginConfiguration.SetCustomIntValue(config, 18);
                Service.Configuration.Save();
            }

            ImGui.NextColumn();

            if (ImGui.Checkbox($"Blue Mage###{config}19", ref values[19]))
            {
                PluginConfiguration.SetCustomIntValue(config, 19);
                Service.Configuration.Save();
            }

            ImGui.PopStyleColor();
            ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.Spacing();
        }

        internal static void DrawPriorityInput(UserIntArray config, int maxValues, int currentItem, string customLabel = "")
        {
            if (config.Count != maxValues || config.Any(x => x == 0))
            {
                config.Clear(maxValues);
                for (int i = 1; i <= maxValues; i++)
                {
                    config[i - 1] = i;
                }
            }

            int curVal = config[currentItem];
            int oldVal = config[currentItem];

            InfoBox box = new()
            {
                Color = Colors.Blue,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    if (customLabel.IsNullOrEmpty())
                    {
                        ImGui.TextUnformatted($"Priority: ");
                    }
                    else
                    {
                        ImGui.TextUnformatted(customLabel);
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100f);

                    if (ImGui.InputInt($"###Priority{config.Name}{currentItem}", ref curVal))
                    {
                        for (int i = 0; i < maxValues; i++)
                        {
                            if (i == currentItem)
                                continue;

                            if (config[i] == curVal)
                            {
                                config[i] = oldVal;
                                config[currentItem] = curVal;
                                break;
                            }
                        }
                    }
                }
            };

            ImGui.Indent();
            box.Draw();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Smaller Number = Higher Priority");
                ImGui.EndTooltip();
            }
            ImGui.Unindent();
            ImGui.Spacing();
        }

        public static int RoundOff(this int i, uint sliderIncrement)
        {
            double sliderAsDouble = Convert.ToDouble(sliderIncrement);
            return ((int)Math.Round(i / sliderAsDouble)) * (int)sliderIncrement;
        }
    }

    public static class UserConfigItems
    {
        /// <summary> Draws the User Configurable settings. </summary>
        /// <param name="preset"> The preset it's attached to. </param>
        /// <param name="enabled"> If it's enabled or not. </param>
        internal static void Draw(CustomComboPreset preset, bool enabled)
        {
            if (!enabled) return;

            // ====================================================================================
            #region Misc

            #endregion
            // ====================================================================================
            #region ADV

            #endregion
            // ====================================================================================
            
            // ====================================================================================
            #region BLACK MAGE

            if (preset is CustomComboPreset.BLM_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, BLM.Config.BLM_VariantCure, "HP% to be at or under", 200);

            #endregion
            // ====================================================================================
            #region BLUE MAGE

            #endregion
            // ====================================================================================
            #region BARD

            if (preset == CustomComboPreset.BRD_Adv_RagingJaws)
                UserConfig.DrawSliderInt(3, 10, BRD.Config.BRD_RagingJawsRenewTime, "Remaining time (In seconds). Recommended 5, increase little by little if refresh is outside of radiant window");

            if (preset == CustomComboPreset.BRD_Adv_NoWaste)
                UserConfig.DrawSliderInt(1, 10, BRD.Config.BRD_NoWasteHPPercentage, "Remaining target HP percentage");

            if (preset == CustomComboPreset.BRD_AoE_Adv_NoWaste)
                UserConfig.DrawSliderInt(1, 10, BRD.Config.BRD_AoENoWasteHPPercentage, "Remaining target HP percentage");

            if (preset == CustomComboPreset.BRD_ST_SecondWind)
                UserConfig.DrawSliderInt(0, 100, BRD.Config.BRD_STSecondWindThreshold, "HP percent threshold to use Second Wind below.", 150, SliderIncrements.Ones);

            if (preset == CustomComboPreset.BRD_AoE_SecondWind)
                UserConfig.DrawSliderInt(0, 100, BRD.Config.BRD_AoESecondWindThreshold, "HP percent threshold to use Second Wind below.", 150, SliderIncrements.Ones);

            if (preset == CustomComboPreset.BRD_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, BRD.Config.BRD_VariantCure, "HP% to be at or under", 200);

            #endregion
            // ====================================================================================
            #region DANCER

            if (preset == CustomComboPreset.DNC_DanceComboReplacer)
            {
                //int[]? actions = Service.Configuration.DancerDanceCompatActionIDs.Cast<int>().ToArray();
                int[]? actions = Service.Configuration.DancerDanceCompatActionIDs.Select(x => (int)x).ToArray();


                bool inputChanged = false;

                inputChanged |= ImGui.InputInt("Emboite (Red) ActionID", ref actions[0], 0);
                inputChanged |= ImGui.InputInt("Entrechat (Blue) ActionID", ref actions[1], 0);
                inputChanged |= ImGui.InputInt("Jete (Green) ActionID", ref actions[2], 0);
                inputChanged |= ImGui.InputInt("Pirouette (Yellow) ActionID", ref actions[3], 0);

                if (inputChanged)
                {
                    //Service.Configuration.DancerDanceCompatActionIDs = actions.Cast<uint>().ToArray();
                    Service.Configuration.DancerDanceCompatActionIDs = actions.Select(x => (uint)x).ToArray();
                    Service.Configuration.Save();
                }

                ImGui.Spacing();
            }

            if (preset == CustomComboPreset.DNC_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, DNC.Config.DNCVariantCurePercent, "HP% to be at or under", 200);

            #region Multi-Button Sliders

            if (preset == CustomComboPreset.DNC_ST_EspritOvercap)
                UserConfig.DrawSliderInt(50, 100, DNC.Config.DNCEspritThreshold_ST, "Esprit", 150, SliderIncrements.Fives);

            if (preset == CustomComboPreset.DNC_AoE_EspritOvercap)
                UserConfig.DrawSliderInt(50, 100, DNC.Config.DNCEspritThreshold_AoE, "Esprit", 150, SliderIncrements.Fives);

            #endregion

            #region Advanced ST Sliders

            if (preset == CustomComboPreset.DNC_ST_Adv_SS)
                UserConfig.DrawSliderInt(0, 5, DNC.Config.DNC_ST_Adv_SSBurstPercent, "Target HP% to stop using Standard Step below", 75);

            if (preset == CustomComboPreset.DNC_ST_Adv_TS)
                UserConfig.DrawSliderInt(0, 5, DNC.Config.DNC_ST_Adv_TSBurstPercent, "Target HP% to stop using Technical Step below", 75);

            if (preset == CustomComboPreset.DNC_ST_Adv_Feathers)
                UserConfig.DrawSliderInt(0, 5, DNC.Config.DNC_ST_Adv_FeatherBurstPercent, "Target HP% to dump all pooled feathers below", 75);

            if (preset == CustomComboPreset.DNC_ST_Adv_SaberDance)
                UserConfig.DrawSliderInt(50, 100, DNC.Config.DNC_ST_Adv_SaberThreshold, "Esprit", 150, SliderIncrements.Fives);

            if (preset == CustomComboPreset.DNC_ST_Adv_PanicHeals)
                UserConfig.DrawSliderInt(0, 100, DNC.Config.DNC_ST_Adv_PanicHealWaltzPercent, "Curing Waltz HP%", 200);

            if (preset == CustomComboPreset.DNC_ST_Adv_PanicHeals)
                UserConfig.DrawSliderInt(0, 100, DNC.Config.DNC_ST_Adv_PanicHealWindPercent, "Second Wind HP%", 200);

            #endregion

            #region Advanced AoE Sliders

            if (preset == CustomComboPreset.DNC_AoE_Adv_SS)
                UserConfig.DrawSliderInt(0, 10, DNC.Config.DNC_AoE_Adv_SSBurstPercent, "Target HP% to stop using Standard Step below", 75);

            if (preset == CustomComboPreset.DNC_AoE_Adv_TS)
                UserConfig.DrawSliderInt(0, 10, DNC.Config.DNC_AoE_Adv_TSBurstPercent, "Target HP% to stop using Technical Step below", 75);

            if (preset == CustomComboPreset.DNC_AoE_Adv_SaberDance)
                UserConfig.DrawSliderInt(50, 100, DNC.Config.DNC_AoE_Adv_SaberThreshold, "Esprit", 150, SliderIncrements.Fives);

            if (preset == CustomComboPreset.DNC_AoE_Adv_PanicHeals)
                UserConfig.DrawSliderInt(0, 100, DNC.Config.DNC_AoE_Adv_PanicHealWaltzPercent, "Curing Waltz HP%", 200);

            if (preset == CustomComboPreset.DNC_AoE_Adv_PanicHeals)
                UserConfig.DrawSliderInt(0, 100, DNC.Config.DNC_AoE_Adv_PanicHealWindPercent, "Second Wind HP%", 200);

            #endregion

            #region PvP Sliders

            if (preset == CustomComboPreset.DNCPvP_BurstMode_CuringWaltz)
                UserConfig.DrawSliderInt(0, 90, DNCPvP.Config.DNCPvP_WaltzThreshold, "Curing Waltz HP% - caps at 90 to prevent waste.", 150, SliderIncrements.Ones);

            #endregion

            #endregion
            // ====================================================================================
            #region DARK KNIGHT

            if (preset == CustomComboPreset.DRK_ST_ManaSpenderPooling && enabled)
                UserConfig.DrawSliderInt(0, 3000, DRK.Config.DRK_ST_ManaSpenderPooling, "How much MP to save (0 = Use All)", 150, SliderIncrements.Thousands);

            if (preset == CustomComboPreset.DRK_ST_CDs_LivingShadow && enabled)
                UserConfig.DrawSliderInt(0, 30, DRK.Config.DRK_ST_LivingDeadThreshold, "Stop Using When Target HP% is at or Below (Set to 0 to Disable This Check)", 150, SliderIncrements.Fives);

            if (preset == CustomComboPreset.DRK_AoE_CDs_LivingShadow && enabled)
                UserConfig.DrawSliderInt(0, 60, DRK.Config.DRK_AoE_LivingDeadThreshold, "Stop Using When Target HP% is at or Below (Set to 0 to Disable This Check)", 150, SliderIncrements.Fives);

            if (preset == CustomComboPreset.DRKPvP_Burst)
                UserConfig.DrawSliderInt(1, 100, DRKPvP.Config.ShadowbringerThreshold, "HP% to be at or above to use Shadowbringer");

            if (preset == CustomComboPreset.DRK_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, DRK.Config.DRK_VariantCure, "HP% to be at or under", 200);

            #endregion
            // ====================================================================================
            // ====================================================================================
            #region GUNBREAKER

            if (preset == CustomComboPreset.GNB_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, GNB.Config.GNB_VariantCure, "HP% to be at or under", 200);
            if (preset == CustomComboPreset.GNB_ST_NoMercy)
                UserConfig.DrawSliderInt(0, 25, GNB.Config.GNB_ST_NoMercyStop, "Stop Usage if Target HP% is below set value.\nTo Disable this option, set to 0.");
            if (preset == CustomComboPreset.GNB_AoE_NoMercy)
                UserConfig.DrawSliderInt(0, 25, GNB.Config.GNB_AoE_NoMercyStop, "Stop Usage if Target HP% is below set value.\nTo Disable this option, set to 0.");

            #endregion
            // ====================================================================================
            #region MACHINIST

            if (preset == CustomComboPreset.MCH_ST_Adv_Reassemble)
                UserConfig.DrawSliderInt(0, 1, MCH.Config.MCH_ST_ReassemblePool, "Number of Charges to Save for Manual Use");

            if (preset == CustomComboPreset.MCH_AoE_Adv_Reassemble)
                UserConfig.DrawSliderInt(0, 1, MCH.Config.MCH_AoE_ReassemblePool, "Number of Charges to Save for Manual Use");

            if (preset is CustomComboPreset.MCH_ST_Adv_Reassemble)
            {
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_ST_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.Excavator)}", "", 5, 0);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_ST_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.Chainsaw)}", "", 5, 1);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_ST_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.AirAnchor)}", "", 5, 2);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_ST_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.Drill)}", "", 5, 3);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_ST_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.CleanShot)}", "", 5, 4);
            }

            if (preset is CustomComboPreset.MCH_AoE_Adv_Reassemble)
            {
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_AoE_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.SpreadShot)}/{ActionWatching.GetActionName(MCH.Scattergun)}", "", 4, 0);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_AoE_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.AutoCrossbow)}", "", 4, 1);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_AoE_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.Chainsaw)}", "", 4, 2);
                UserConfig.DrawHorizontalMultiChoice(MCH.Config.MCH_AoE_Reassembled, $"Use on {ActionWatching.GetActionName(MCH.Excavator)}", "", 4, 3);
            }

            if (preset == CustomComboPreset.MCH_ST_Adv_SecondWind)
                UserConfig.DrawSliderInt(0, 100, MCH.Config.MCH_ST_SecondWindThreshold, $"{ActionWatching.GetActionName(All.SecondWind)} HP percentage threshold", 150, SliderIncrements.Ones);

            if (preset == CustomComboPreset.MCH_AoE_Adv_SecondWind)
                UserConfig.DrawSliderInt(0, 100, MCH.Config.MCH_AoE_SecondWindThreshold, $"{ActionWatching.GetActionName(All.SecondWind)} HP percentage threshold", 150, SliderIncrements.Ones);

            if (preset == CustomComboPreset.MCH_AoE_Adv_Queen)
                UserConfig.DrawSliderInt(50, 100, MCH.Config.MCH_AoE_TurretUsage, "Battery threshold", sliderIncrement: 5);

            if (preset == CustomComboPreset.MCH_AoE_Adv_GaussRicochet)
                UserConfig.DrawAdditionalBoolChoice(MCH.Config.MCH_AoE_Hypercharge, $"Use Outwith {ActionWatching.GetActionName(MCH.Hypercharge)}", "");

            if (preset == CustomComboPreset.MCH_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, MCH.Config.MCH_VariantCure, "HP% to be at or under", 200);

            if (preset == CustomComboPreset.MCH_ST_Adv_QueenOverdrive)
                UserConfig.DrawSliderInt(1, 10, MCH.Config.MCH_ST_QueenOverDrive, "HP% for the target to be at or under");

            if (preset == CustomComboPreset.MCH_ST_Adv_WildFire)
                UserConfig.DrawSliderInt(0, 15, MCH.Config.MCH_ST_WildfireHP, "Stop Using When Target HP% is at or Below (Set to 0 to Disable This Check)");

            if (preset == CustomComboPreset.MCH_ST_Adv_Hypercharge)
                UserConfig.DrawSliderInt(0, 15, MCH.Config.MCH_ST_HyperchargeHP, "Stop Using When Target HP% is at or Below (Set to 0 to Disable This Check)");

            #endregion
            // ====================================================================================

            #region MONK

            if (preset == CustomComboPreset.MNK_ST_ComboHeals)
            {
                UserConfig.DrawSliderInt(0, 100, MNK.Config.MNK_ST_SecondWind_Threshold,
                    "Second Wind HP percentage threshold (0 = Disabled)", 150, SliderIncrements.Ones);

                UserConfig.DrawSliderInt(0, 100, MNK.Config.MNK_ST_Bloodbath_Threshold,
                    "Bloodbath HP percentage threshold (0 = Disabled)", 150, SliderIncrements.Ones);
            }

            if (preset == CustomComboPreset.MNK_AoE_ComboHeals)
            {
                UserConfig.DrawSliderInt(0, 100, MNK.Config.MNK_AoE_SecondWind_Threshold,
                    "Second Wind HP percentage threshold (0 = Disabled)", 150, SliderIncrements.Ones);

                UserConfig.DrawSliderInt(0, 100, MNK.Config.MNK_AoE_Bloodbath_Threshold,
                    "Bloodbath HP percentage threshold (0 = Disabled)", 150, SliderIncrements.Ones);
            }

            if (preset == CustomComboPreset.MNK_STUseOpener && enabled)
            {
                UserConfig.DrawHorizontalRadioButton(MNK.Config.MNK_SelectedOpener, "Double Lunar",
                    "Uses Lunar/Lunar opener", 0);

                UserConfig.DrawHorizontalRadioButton(MNK.Config.MNK_SelectedOpener, "Solar Lunar",
                    "Uses Solar/Lunar opener", 1);
            }

            if (preset == CustomComboPreset.MNK_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, MNK.Config.MNK_VariantCure, "HP% to be at or under", 200);

            #endregion
            // ====================================================================================
            #region NINJA

            if (preset == CustomComboPreset.NIN_Simple_Mudras)
            {
                UserConfig.DrawRadioButton(NIN.Config.NIN_SimpleMudra_Choice, "Mudra Path Set 1", $"1. Ten Mudras -> Fuma Shuriken, Raiton/Hyosho Ranryu, Suiton (Doton under Kassatsu).\nChi Mudras -> Fuma Shuriken, Hyoton, Huton.\nJin Mudras -> Fuma Shuriken, Katon/Goka Mekkyaku, Doton", 1);
                UserConfig.DrawRadioButton(NIN.Config.NIN_SimpleMudra_Choice, "Mudra Path Set 2", $"2. Ten Mudras -> Fuma Shuriken, Hyoton/Hyosho Ranryu, Doton.\nChi Mudras -> Fuma Shuriken, Katon, Suiton.\nJin Mudras -> Fuma Shuriken, Raiton/Goka Mekkyaku, Huton (Doton under Kassatsu).", 2);
            }
            if (preset == CustomComboPreset.NIN_ST_AdvancedMode)
                UserConfig.DrawSliderInt(0, 10, NIN.Config.BurnKazematoi, "Target HP% to dump all pooled Kazematoi below");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_Bhavacakra)
                UserConfig.DrawSliderInt(50, 100, NIN.Config.Ninki_BhavaPooling, "Set the minimal amount of Ninki required to have before spending on Bhavacakra.");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack)
                UserConfig.DrawSliderInt(0, 21, NIN.Config.Trick_CooldownRemaining, "Set the amount of time remaining on Trick Attack cooldown before trying to set up with Suiton.");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_Bunshin)
                UserConfig.DrawSliderInt(50, 100, NIN.Config.Ninki_BunshinPoolingST, "Set the amount of Ninki required to have before spending on Bunshin.");

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_Bunshin)
                UserConfig.DrawSliderInt(50, 100, NIN.Config.Ninki_BunshinPoolingAoE, "Set the amount of Ninki required to have before spending on Bunshin.");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack_Cooldowns)
                UserConfig.DrawSliderInt(0, 21, NIN.Config.Advanced_Trick_Cooldown, "Set the amount of time remaining on Trick Attack cooldown to start saving cooldowns.");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_SecondWind)
                UserConfig.DrawSliderInt(0, 100, NIN.Config.SecondWindThresholdST, "Set a HP% threshold for when Second Wind will be used.");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_ShadeShift)
                UserConfig.DrawSliderInt(0, 100, NIN.Config.ShadeShiftThresholdST, "Set a HP% threshold for when Shade Shift will be used.");

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_Bloodbath)
                UserConfig.DrawSliderInt(0, 100, NIN.Config.BloodbathThresholdST, "Set a HP% threshold for when Bloodbath will be used.");

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_SecondWind)
                UserConfig.DrawSliderInt(0, 100, NIN.Config.SecondWindThresholdAoE, "Set a HP% threshold for when Second Wind will be used.");

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_ShadeShift)
                UserConfig.DrawSliderInt(0, 100, NIN.Config.ShadeShiftThresholdAoE, "Set a HP% threshold for when Shade Shift will be used.");

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_Bloodbath)
                UserConfig.DrawSliderInt(0, 100, NIN.Config.BloodbathThresholdAoE, "Set a HP% threshold for when Bloodbath will be used.");

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_HellfrogMedium)
                UserConfig.DrawSliderInt(50, 100, NIN.Config.Ninki_HellfrogPooling, "Set the amount of Ninki required to have before spending on Hellfrog Medium.");

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton)
            {
                UserConfig.DrawSliderInt(0, 18, NIN.Config.Advanced_DotonTimer, "Sets the amount of time remaining on Doton before casting again.");
                UserConfig.DrawSliderInt(0, 100, NIN.Config.Advanced_DotonHP, "Sets the max remaining HP percentage of the current target to cast Doton.");
            }

            if (preset == CustomComboPreset.NIN_AoE_AdvancedMode_TCJ)
            {
                UserConfig.DrawRadioButton(NIN.Config.Advanced_TCJEnderAoE, "Ten Chi Jin Ender 1", "Ends Ten Chi Jin with Suiton.", 0);
                UserConfig.DrawRadioButton(NIN.Config.Advanced_TCJEnderAoE, $"Ten Chi Jin Ender 2", "Ends Ten Chi Jin with Doton.\nIf you have Doton enabled, Ten Chi Jin will be delayed according to the settings in that feature.", 1);
            }

            if (preset == CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Raiton)
            {
                UserConfig.DrawAdditionalBoolChoice(NIN.Config.Advanced_ChargePool, "Pool Charges", "Waits until at least 2 seconds before your 2nd charge or if Trick Attack debuff is on your target before spending.");
            }

            if (preset == CustomComboPreset.NIN_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, NIN.Config.NIN_VariantCure, "HP% to be at or under", 200);

            #endregion
            // ====================================================================================
            #region PICTOMANCER
            if (preset == CustomComboPreset.CombinedAetherhues)
            {
                UserConfig.DrawRadioButton(PCT.Config.CombinedAetherhueChoices, "Both Single Target & AoE", $"Replaces both {PCT.FireInRed.ActionName()} & {PCT.FireIIinRed.ActionName()}", 0);
                UserConfig.DrawRadioButton(PCT.Config.CombinedAetherhueChoices, "Single Target Only", $"Replace only {PCT.FireInRed.ActionName()}", 1);
                UserConfig.DrawRadioButton(PCT.Config.CombinedAetherhueChoices, "AoE Only", $"Replace only {PCT.FireIIinRed.ActionName()}", 2);
            }

            if (preset == CustomComboPreset.CombinedMotifs)
            {
                UserConfig.DrawAdditionalBoolChoice(PCT.Config.CombinedMotifsMog, $"{PCT.MogoftheAges.ActionName()} Feature", $"Add {PCT.MogoftheAges.ActionName()} when fully drawn and off cooldown.");
                UserConfig.DrawAdditionalBoolChoice(PCT.Config.CombinedMotifsMadeen, $"{PCT.RetributionoftheMadeen.ActionName()} Feature", $"Add {PCT.RetributionoftheMadeen.ActionName()} when fully drawn and off cooldown.");
                UserConfig.DrawAdditionalBoolChoice(PCT.Config.CombinedMotifsWeapon, $"{PCT.HammerStamp.ActionName()} Feature", $"Add {PCT.HammerStamp.ActionName()} when under the effect of {PCT.Buffs.HammerTime.StatusName()}.");
            }

            if(preset == CustomComboPreset.PCT_ST_AdvancedMode_LucidDreaming )
            {
                UserConfig.DrawSliderInt(0, 10000, PCT.Config.PCT_ST_AdvancedMode_LucidOption, "Add Lucid Dreaming when below this MP", sliderIncrement: SliderIncrements.Hundreds);
            }

            if (preset == CustomComboPreset.PCT_AoE_AdvancedMode_HolyinWhite)
            {
                UserConfig.DrawSliderInt(0, 5, PCT.Config.PCT_AoE_AdvancedMode_HolyinWhiteOption, "How many charges to keep ready? (0 = Use all)");
            }

            if(preset == CustomComboPreset.PCT_AoE_AdvancedMode_LucidDreaming)
            {
                UserConfig.DrawSliderInt(0, 10000, PCT.Config.PCT_AoE_AdvancedMode_LucidOption, "Add Lucid Dreaming when below this MP", sliderIncrement: SliderIncrements.Hundreds);
            }
            if (preset == CustomComboPreset.PCT_ST_AdvancedMode_LandscapeMotif)
                UserConfig.DrawSliderInt(0, 10, PCT.Config.PCT_ST_LandscapeStop, "Health % to stop Drawing Motif");
            if (preset == CustomComboPreset.PCT_ST_AdvancedMode_CreatureMotif)
                UserConfig.DrawSliderInt(0, 10, PCT.Config.PCT_ST_CreatureStop, "Health % to stop Drawing Motif");
            if (preset == CustomComboPreset.PCT_ST_AdvancedMode_WeaponMotif)
                UserConfig.DrawSliderInt(0, 10, PCT.Config.PCT_ST_WeaponStop, "Health % to stop Drawing Motif");
            if (preset == CustomComboPreset.PCT_AoE_AdvancedMode_LandscapeMotif)
                UserConfig.DrawSliderInt(0, 10, PCT.Config.PCT_AoE_LandscapeStop, "Health % to stop Drawing Motif");
            if (preset == CustomComboPreset.PCT_AoE_AdvancedMode_CreatureMotif)
                UserConfig.DrawSliderInt(0, 10, PCT.Config.PCT_AoE_CreatureStop, "Health % to stop Drawing Motif");
            if (preset == CustomComboPreset.PCT_AoE_AdvancedMode_WeaponMotif)
                UserConfig.DrawSliderInt(0, 10, PCT.Config.PCT_AoE_WeaponStop, "Health % to stop Drawing Motif");

            if (preset == CustomComboPreset.PCT_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, PCT.Config.PCT_VariantCure, "HP% to be at or under", 200);

            // PvP
            if (preset == CustomComboPreset.PCTPvP_BurstControl)
                UserConfig.DrawSliderInt(1, 100, PCTPvP.Config.PCTPvP_BurstHP, "Target HP%", 200);

            if (preset == CustomComboPreset.PCTPvP_TemperaCoat)
                UserConfig.DrawSliderInt(1, 100, PCTPvP.Config.PCTPvP_TemperaHP, "Player HP%", 200);
            #endregion
            // ====================================================================================
            #region PALADIN

            // Fight or Flight
            if (preset == CustomComboPreset.PLD_ST_AdvancedMode_FoF)
                UserConfig.DrawSliderInt(0, 50, PLD.Config.PLD_ST_FoF_Trigger, "Target HP%", 200);

            if (preset == CustomComboPreset.PLD_AoE_AdvancedMode_FoF)
                UserConfig.DrawSliderInt(0, 50, PLD.Config.PLD_AoE_FoF_Trigger, "Target HP%", 200);

            // Sheltron
            if (preset == CustomComboPreset.PLD_ST_AdvancedMode_Sheltron)
                UserConfig.DrawSliderInt(50, 100, PLD.Config.PLD_ST_SheltronOption, "Oath Gauge", 200, 5);

            if (preset == CustomComboPreset.PLD_AoE_AdvancedMode_Sheltron)
                UserConfig.DrawSliderInt(50, 100, PLD.Config.PLD_AoE_SheltronOption, "Oath Gauge", 200, 5);

            // Intervene
            if (preset == CustomComboPreset.PLD_ST_AdvancedMode_Intervene)
            {
                UserConfig.DrawSliderInt(0, 1, PLD.Config.PLD_Intervene_HoldCharges, "Charges", 200);
                UserConfig.DrawHorizontalRadioButton(PLD.Config.PLD_Intervene_MeleeOnly, "Melee Range", "Uses Intervene while within melee range.\nMay result in minor movement.", 1);
                UserConfig.DrawHorizontalRadioButton(PLD.Config.PLD_Intervene_MeleeOnly, "No Movement", "Only uses Intervene when it would not result in movement (zero distance).", 2);
            }

            if (preset == CustomComboPreset.PLD_AoE_AdvancedMode_Intervene)
            {
                UserConfig.DrawSliderInt(0, 1, PLD.Config.PLD_AoE_Intervene_HoldCharges, "Charges", 200);
                UserConfig.DrawHorizontalRadioButton(PLD.Config.PLD_AoE_Intervene_MeleeOnly, "Melee Range", "Uses Intervene while within melee range.\nMay result in minor movement.", 1);
                UserConfig.DrawHorizontalRadioButton(PLD.Config.PLD_AoE_Intervene_MeleeOnly, "No Movement", "Only uses Intervene when it would not result in movement (zero distance).", 2);
            }

            // Shield Lob
            if (preset == CustomComboPreset.PLD_ST_AdvancedMode_ShieldLob)
            {
                UserConfig.DrawHorizontalRadioButton(PLD.Config.PLD_ShieldLob_SubOption, "Shield Lob Only", "Uses only Shield Lob.", 1);
                UserConfig.DrawHorizontalRadioButton(PLD.Config.PLD_ShieldLob_SubOption, "Hardcast Holy Spirit", "Attempts to hardcast Holy Spirit when not moving.\nOtherwise uses Shield Lob.", 2);
            }

            // MP Reservation
            if (preset == CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve)
                UserConfig.DrawSliderInt(1000, 5000, PLD.Config.PLD_ST_MP_Reserve, "Minimum MP", sliderIncrement: 100);

            if (preset == CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve)
                UserConfig.DrawSliderInt(1000, 5000, PLD.Config.PLD_AoE_MP_Reserve, "Minimum MP", sliderIncrement: 100);

            // Requiescat Spender Feature
            if (preset == CustomComboPreset.PLD_Requiescat_Options)
            {
                UserConfig.DrawRadioButton(PLD.Config.PLD_RequiescatOption, "Confiteor", "", 1);
                UserConfig.DrawRadioButton(PLD.Config.PLD_RequiescatOption, "Blade of Faith/Truth/Valor", "", 2);
                UserConfig.DrawRadioButton(PLD.Config.PLD_RequiescatOption, "Confiteor & Blade of Faith/Truth/Valor", "", 3);
                UserConfig.DrawRadioButton(PLD.Config.PLD_RequiescatOption, "Holy Spirit", "", 4);
                UserConfig.DrawRadioButton(PLD.Config.PLD_RequiescatOption, "Holy Circle", "", 5);
            }

            // Spirits Within / Circle of Scorn Feature
            if (preset == CustomComboPreset.PLD_SpiritsWithin)
            {
                UserConfig.DrawRadioButton(PLD.Config.PLD_SpiritsWithinOption, "Prioritize Circle of Scorn", "", 1);
                UserConfig.DrawRadioButton(PLD.Config.PLD_SpiritsWithinOption, "Prioritize Spirits Within / Expiacion", "", 2);
            }

            // Variant Cure Feature
            if (preset == CustomComboPreset.PLD_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, PLD.Config.PLD_VariantCure, "Player HP%", 200);

            #endregion
            // ====================================================================================

            // ====================================================================================

            // ====================================================================================

            // ====================================================================================
            #region SAMURAI

            if (preset == CustomComboPreset.SAM_ST_CDs_Iaijutsu)
            {
                UserConfig.DrawSliderInt(0, 100, SAM.Config.SAM_ST_Higanbana_Threshold, "Stop using Higanbana on targets below this HP % (0% = always use).", 150, SliderIncrements.Ones);
            }
            if (preset == CustomComboPreset.SAM_ST_ComboHeals)
            {
                UserConfig.DrawSliderInt(0, 100, SAM.Config.SAM_STSecondWindThreshold, "HP percent threshold to use Second Wind below (0 = Disabled)", 150, SliderIncrements.Ones);
                UserConfig.DrawSliderInt(0, 100, SAM.Config.SAM_STBloodbathThreshold, "HP percent threshold to use Bloodbath (0 = Disabled)", 150, SliderIncrements.Ones);
            }

            if (preset == CustomComboPreset.SAM_AoE_ComboHeals)
            {
                UserConfig.DrawSliderInt(0, 100, SAM.Config.SAM_AoESecondWindThreshold, "HP percent threshold to use Second Wind below (0 = Disabled)", 150, SliderIncrements.Ones);
                UserConfig.DrawSliderInt(0, 100, SAM.Config.SAM_AoEBloodbathThreshold, "HP percent threshold to use Bloodbath below (0 = Disabled)", 150, SliderIncrements.Ones);
            }

            if (preset == CustomComboPreset.SAM_ST_Shinten)
            {
                UserConfig.DrawSliderInt(50, 85, SAM.Config.SAM_ST_KenkiOvercapAmount, "Set the Kenki overcap amount for ST combos.");
                UserConfig.DrawSliderInt(0, 100, SAM.Config.SAM_ST_ExecuteThreshold, "HP percent threshold to not save Kenki", 150, SliderIncrements.Ones);
            }

            if (preset == CustomComboPreset.SAM_AoE_Kyuten)
                UserConfig.DrawSliderInt(50, 85, SAM.Config.SAM_AoE_KenkiOvercapAmount, "Set the Kenki overcap amount for AOE combos.");

            if (preset == CustomComboPreset.SAM_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, SAM.Config.SAM_VariantCure, "HP% to be at or under", 200);

            //PvP
            if (preset == CustomComboPreset.SAMPvP_BurstMode && enabled)
                UserConfig.DrawSliderInt(0, 2, SAMPvP.Config.SAMPvP_SotenCharges, "How many charges of Soten to keep ready? (0 = Use All).");

            if (preset == CustomComboPreset.SAMPvP_KashaFeatures_GapCloser && enabled)
                UserConfig.DrawSliderInt(0, 100, SAMPvP.Config.SAMPvP_SotenHP, "Use Soten on enemies below selected HP.");

            if (preset == CustomComboPreset.SAM_ST_KashaCombo)
            {
                UserConfig.DrawAdditionalBoolChoice(SAM.Config.SAM_Kasha_KenkiOvercap, "Kenki Overcap Protection", "Spends Kenki when at the set value or above.");
                if (SAM.Config.SAM_Kasha_KenkiOvercap)
                    UserConfig.DrawSliderInt(25, 100, SAM.Config.SAM_Kasha_KenkiOvercapAmount, "Kenki Amount", sliderIncrement: SliderIncrements.Fives);
            }

            if (preset == CustomComboPreset.SAM_ST_YukikazeCombo)
            {
                UserConfig.DrawAdditionalBoolChoice(SAM.Config.SAM_Yukaze_KenkiOvercap, "Kenki Overcap Protection", "Spends Kenki when at the set value or above.");
                if (SAM.Config.SAM_Yukaze_KenkiOvercap)
                    UserConfig.DrawSliderInt(25, 100, SAM.Config.SAM_Yukaze_KenkiOvercapAmount, "Kenki Amount", sliderIncrement: SliderIncrements.Fives);
            }

            if (preset == CustomComboPreset.SAM_ST_GekkoCombo)
            {
                UserConfig.DrawAdditionalBoolChoice(SAM.Config.SAM_Gekko_KenkiOvercap, "Kenki Overcap Protection", "Spends Kenki when at the set value or above.");
                if (SAM.Config.SAM_Gekko_KenkiOvercap)
                    UserConfig.DrawSliderInt(25, 100, SAM.Config.SAM_Gekko_KenkiOvercapAmount, "Kenki Amount", sliderIncrement: SliderIncrements.Fives);
            }

            if (preset == CustomComboPreset.SAM_AoE_OkaCombo)
            {
                UserConfig.DrawAdditionalBoolChoice(SAM.Config.SAM_Oka_KenkiOvercap, "Kenki Overcap Protection", "Spends Kenki when at the set value or above.");
                if (SAM.Config.SAM_Oka_KenkiOvercap)
                    UserConfig.DrawSliderInt(25, 100, SAM.Config.SAM_Oka_KenkiOvercapAmount, "Kenki Amount", sliderIncrement: SliderIncrements.Fives);
            }

            if (preset == CustomComboPreset.SAM_AoE_MangetsuCombo)
            {
                UserConfig.DrawAdditionalBoolChoice(SAM.Config.SAM_Mangetsu_KenkiOvercap, "Kenki Overcap Protection", "Spends Kenki when at the set value or above.");
                if (SAM.Config.SAM_Mangetsu_KenkiOvercap)
                    UserConfig.DrawSliderInt(25, 100, SAM.Config.SAM_Mangetsu_KenkiOvercapAmount, "Kenki Amount", sliderIncrement: SliderIncrements.Fives);
            }

            #endregion
            // ====================================================================================

            // ====================================================================================
            #region SUMMONER

            #region PvE
            if (preset == CustomComboPreset.SMN_DemiEgiMenu_EgiOrder)
            {
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_PrimalChoice, "Titan first", "Summons Titan, Garuda then Ifrit.", 1);
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_PrimalChoice, "Garuda first", "Summons Garuda, Titan then Ifrit.", 2);
            }

            if (preset == CustomComboPreset.SMN_DemiEgiMenu_oGCDPooling)
                UserConfig.DrawSliderInt(0, 3, SMN.Config.SMN_Burst_Delay, "Sets the amount of GCDs under Demi summon to wait for oGCD use.", 150, SliderIncrements.Ones);

            if (preset == CustomComboPreset.SMN_DemiEgiMenu_oGCDPooling)
            {
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_BurstPhase, "Solar Bahamut/Bahamut", "Bursts during Bahamut phase.\nBahamut burst phase becomes Solar Bahamut at Lv100.", 1);
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_BurstPhase, "Phoenix", "Bursts during Phoenix phase.", 2);
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_BurstPhase, "Any Demi Phase", "Bursts during any Demi Summon phase.", 3);
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_BurstPhase, "Flexible (SpS) Option", "Bursts when Searing Light is ready, regardless of phase.", 4);
            }

            if (preset == CustomComboPreset.SMN_DemiEgiMenu_SwiftcastEgi)
            {
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_SwiftcastPhase, "Garuda", "Swiftcasts Slipstream", 1);
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_SwiftcastPhase, "Ifrit", "Swiftcasts Ruby Ruin/Ruby Rite", 2);
                UserConfig.DrawHorizontalRadioButton(SMN.Config.SMN_SwiftcastPhase, "Flexible (SpS) Option", "Swiftcasts the first available Egi when Swiftcast is ready.", 3);
            }

            if (preset == CustomComboPreset.SMN_Lucid)
                UserConfig.DrawSliderInt(4000, 9500, SMN.Config.SMN_Lucid, "Set value for your MP to be at or under for this feature to take effect.", 150, SliderIncrements.Hundreds);

            if (preset == CustomComboPreset.SMN_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, SMN.Config.SMN_VariantCure, "HP% to be at or under", 200);

            if (preset == CustomComboPreset.SMN_ST_Egi_AstralFlow)
            {
                UserConfig.DrawHorizontalMultiChoice(SMN.Config.SMN_ST_Egi_AstralFlow, "Add Mountain Buster", "", 3, 0);
                UserConfig.DrawHorizontalMultiChoice(SMN.Config.SMN_ST_Egi_AstralFlow, "Add Crimson Cyclone", "", 3, 1);
                UserConfig.DrawHorizontalMultiChoice(SMN.Config.SMN_ST_Egi_AstralFlow, "Add Slipstream", "", 3, 2);

                if (SMN.Config.SMN_ST_Egi_AstralFlow[1])
                    UserConfig.DrawAdditionalBoolChoice(SMN.Config.SMN_ST_CrimsonCycloneMelee, "Enforced Crimson Cyclone Melee Check", "Only uses Crimson Cyclone within melee range.");
            }



            #endregion

            #region PvP

            if (preset == CustomComboPreset.SMNPvP_BurstMode)
                UserConfig.DrawSliderInt(50, 100, SMNPvP.Config.SMNPvP_FesterThreshold, "Target HP% to cast Fester below.\nSet to 100 use Fester as soon as it's available.", 150, SliderIncrements.Ones);

            if (preset == CustomComboPreset.SMNPvP_BurstMode_RadiantAegis)
                UserConfig.DrawSliderInt(0, 90, SMNPvP.Config.SMNPvP_RadiantAegisThreshold, "Caps at 90 to prevent waste.", 150, SliderIncrements.Ones);

            #endregion

            #endregion
            // ====================================================================================

            // ====================================================================================
            #region WARRIOR

            if (preset == CustomComboPreset.WAR_ST_Advanced_StormsEye && enabled)
                UserConfig.DrawSliderInt(0, 30, WAR.Config.WAR_SurgingRefreshRange, "Seconds remaining before refreshing Surging Tempest.");

            if (preset == CustomComboPreset.WAR_InfuriateFellCleave && enabled)
                UserConfig.DrawSliderInt(0, 50, WAR.Config.WAR_InfuriateRange, "Set how much rage to be at or under to use this feature.");

            if (preset == CustomComboPreset.WAR_ST_Advanced_Onslaught && enabled)
                UserConfig.DrawSliderInt(0, 2, WAR.Config.WAR_KeepOnslaughtCharges, "How many charges to keep ready? (0 = Use All)");

            if (preset == CustomComboPreset.WAR_ST_Advanced_Infuriate && enabled)
                UserConfig.DrawSliderInt(0, 2, WAR.Config.WAR_KeepInfuriateCharges, "How many charges to keep ready? (0 = Use All)");

            if (preset == CustomComboPreset.WAR_Variant_Cure)
                UserConfig.DrawSliderInt(1, 100, WAR.Config.WAR_VariantCure, "HP% to be at or under", 200);

            if (preset == CustomComboPreset.WAR_ST_Advanced_FellCleave)
                UserConfig.DrawSliderInt(50, 100, WAR.Config.WAR_FellCleaveGauge, "Minimum gauge to spend");

            if (preset == CustomComboPreset.WAR_AoE_Advanced_Decimate)
                UserConfig.DrawSliderInt(50, 100, WAR.Config.WAR_DecimateGauge, "Minimum gauge to spend");

            if (preset == CustomComboPreset.WAR_ST_Advanced_Infuriate)
                UserConfig.DrawSliderInt(0, 50, WAR.Config.WAR_InfuriateSTGauge, "Use when gauge is under or equal to");

            if (preset == CustomComboPreset.WAR_AoE_Advanced_Infuriate)
                UserConfig.DrawSliderInt(0, 50, WAR.Config.WAR_InfuriateAoEGauge, "Use when gauge is under or equal to");

            if (preset == CustomComboPreset.WARPvP_BurstMode_Blota)
            {
                UserConfig.DrawHorizontalRadioButton(WARPvP.Config.WARPVP_BlotaTiming, "Before Primal Rend", "", 0);
                UserConfig.DrawHorizontalRadioButton(WARPvP.Config.WARPVP_BlotaTiming, "After Primal Rend", "", 1);
            }

            #endregion
            // ====================================================================================

            // ====================================================================================
            #region DOH

            #endregion
            // ====================================================================================
            #region DOL

            #endregion
            // ====================================================================================
            #region PvP VALUES

            IPlayerCharacter? pc = Svc.ClientState.LocalPlayer;

            if (preset == CustomComboPreset.PvP_EmergencyHeals)
            {
                if (pc != null)
                {
                    uint maxHP = Svc.ClientState.LocalPlayer?.MaxHp <= 15000 ? 0 : Svc.ClientState.LocalPlayer.MaxHp - 15000;

                    if (maxHP > 0)
                    {
                        int setting = PluginConfiguration.GetCustomIntValue(PvPCommon.Config.EmergencyHealThreshold);
                        float hpThreshold = (float)maxHP / 100 * setting;

                        UserConfig.DrawSliderInt(1, 100, PvPCommon.Config.EmergencyHealThreshold, $"Set the percentage to be at or under for the feature to kick in.\n100% is considered to start at 15,000 less than your max HP to prevent wastage.\nHP Value to be at or under: {hpThreshold}");
                    }

                    else
                    {
                        UserConfig.DrawSliderInt(1, 100, PvPCommon.Config.EmergencyHealThreshold, "Set the percentage to be at or under for the feature to kick in.\n100% is considered to start at 15,000 less than your max HP to prevent wastage.");
                    }
                }

                else
                {
                    UserConfig.DrawSliderInt(1, 100, PvPCommon.Config.EmergencyHealThreshold, "Set the percentage to be at or under for the feature to kick in.\n100% is considered to start at 15,000 less than your max HP to prevent wastage.");
                }
            }

            if (preset == CustomComboPreset.PvP_EmergencyGuard)
                UserConfig.DrawSliderInt(1, 100, PvPCommon.Config.EmergencyGuardThreshold, "Set the percentage to be at or under for the feature to kick in.");

            if (preset == CustomComboPreset.PvP_QuickPurify)
                UserConfig.DrawPvPStatusMultiChoice(PvPCommon.Config.QuickPurifyStatuses);

            if (preset == CustomComboPreset.NINPvP_ST_Meisui)
            {
                string description = "Set the HP percentage to be at or under for the feature to kick in.\n100% is considered to start at 8,000 less than your max HP to prevent wastage.";

                if (pc != null)
                {
                    uint maxHP = pc.MaxHp <= 8000 ? 0 : pc.MaxHp - 8000;
                    if (maxHP > 0)
                    {
                        int setting = PluginConfiguration.GetCustomIntValue(NINPvP.Config.NINPvP_Meisui_ST);
                        float hpThreshold = (float)maxHP / 100 * setting;

                        description += $"\nHP Value to be at or under: {hpThreshold}";
                    }
                }

                UserConfig.DrawSliderInt(1, 100, NINPvP.Config.NINPvP_Meisui_ST, description);
            }

            if (preset == CustomComboPreset.NINPvP_AoE_Meisui)
            {
                string description = "Set the HP percentage to be at or under for the feature to kick in.\n100% is considered to start at 8,000 less than your max HP to prevent wastage.";

                if (pc != null)
                {
                    uint maxHP = pc.MaxHp <= 8000 ? 0 : pc.MaxHp - 8000;
                    if (maxHP > 0)
                    {
                        int setting = PluginConfiguration.GetCustomIntValue(NINPvP.Config.NINPvP_Meisui_AoE);
                        float hpThreshold = (float)maxHP / 100 * setting;

                        description += $"\nHP Value to be at or under: {hpThreshold}";
                    }
                }

                UserConfig.DrawSliderInt(1, 100, NINPvP.Config.NINPvP_Meisui_AoE, description);
            }


            #endregion
        }
    }

    public static class SliderIncrements
    {
        public const uint
            Ones = 1,
            Fives = 5,
            Tens = 10,
            Hundreds = 100,
            Thousands = 1000;
    }
}
