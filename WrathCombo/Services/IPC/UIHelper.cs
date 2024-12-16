#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using ECommons.ImGuiMethods;
using ImGuiNET;
using WrathCombo.Combos;
using WrathCombo.CustomComboNS.Functions;

#endregion

namespace WrathCombo.Services.IPC;

public class UIHelper(ref Leasing leasing, ref Search search)
{
    private readonly Leasing _leasing = leasing;
    private readonly Search _search = search;

    #region Checks for the UI

    #region Auto-Rotation

    private DateTime? _autoRotationUpdated;

    private (string controllers, bool state)
        AutoRotationControlled { get; set; } = (string.Empty, false);

    internal (string controllers, bool state)? AutoRotationStateControlled()
    {
        // Return the cached value if it is valid, fastest
        if (string.IsNullOrEmpty(AutoRotationControlled.controllers) &&
            _autoRotationUpdated is not null &&
            _autoRotationUpdated == _leasing.AutoRotationStateUpdated)
            return AutoRotationControlled;

        // Bail if the state is not controlled, fast
        var controlled = _leasing.CheckAutoRotationControlled();
        if (controlled is null)
            return null;

        // Re-populate the cache with the current state, slowest
        var controllers = _leasing.Registrations.Values
            .Where(l => l.AutoRotationControlled.Count != 0)
            .OrderByDescending(l => l.LastUpdated)
            .ToList();
        var controllingLeases = controllers
            .Select(l => l.PluginName);
        var controlledState = controllers
            .First().AutoRotationControlled[0];
        AutoRotationControlled =
            (string.Join(", ", controllingLeases), controlledState);
        _autoRotationUpdated = _leasing.AutoRotationStateUpdated;

        return AutoRotationControlled;
    }

    #endregion

    #region Jobs

    private DateTime? _jobsUpdated;

    private Dictionary<string, (string controllers, bool state)>
        JobsControlled { get; } = new();

    internal (string controllers, bool state)? JobControlled(uint job)
    {
        var jobName = CustomComboFunctions.JobIDs.JobIDToShorthand(job);

        // Return the cached value if it is valid, fastest
        if (_jobsUpdated is not null &&
            _jobsUpdated == _leasing.JobsUpdated &&
            JobsControlled.TryGetValue(jobName, out var jobControlled))
            return jobControlled;

        // Bail if the job is not controlled, fast
        var controlled = _leasing.CheckJobControlled();
        if (controlled is null)
            return null;

        // Re-populate the cache with the current set of controlled jobs, slowest
        JobsControlled.Clear();
        foreach (var controlledJob in _search.AllJobsControlled)
            JobsControlled[controlledJob.Key.ToString()] =
                (string.Join(", ", controlledJob.Value.Keys), true);
        _jobsUpdated = _search.LastCacheUpdateForAllJobsControlled;

        return JobsControlled[jobName];
    }

    #endregion

    #region Presets

    private DateTime? _presetsUpdated;

    private Dictionary<string, (string controllers, bool state)>
        PresetsControlled { get; } = new();

    internal (string controllers, bool state)? PresetControlled(
        CustomComboPreset preset)
    {
        var presetName = preset.ToString();

        var presetsUpdated = (DateTime)
            (_leasing.CombosUpdated > _leasing
                .OptionsUpdated
                ? _leasing.CombosUpdated
                : _leasing.OptionsUpdated ?? DateTime.MinValue);
        // Return the cached value if it is valid, fastest
        if (_presetsUpdated is not null &&
            _presetsUpdated == presetsUpdated &&
            PresetsControlled.TryGetValue(presetName, out var presetControlled))
            return presetControlled;

        // Bail if the preset is not controlled, fast-ish
        var controlledAsCombo = _leasing.CheckComboControlled(presetName);
        var controlledAsOption = _leasing.CheckComboOptionControlled(presetName);
        if (controlledAsCombo is null && controlledAsOption is null)
            return null;

        // Re-populate the cache with the current set of controlled presets, slowest
        PresetsControlled.Clear();
        foreach (var controlledPreset in _search.AllPresetsControlled)
            PresetsControlled[controlledPreset.Key.ToString()] =
                (string.Join(", ", controlledPreset.Value.Keys),
                    controlledPreset.Value.Values.First());

        _presetsUpdated = _search.LastCacheUpdateForAllPresetsControlled;

        return PresetsControlled[presetName];
    }

    #endregion

    #region Auto-Rotation Configs

    private DateTime? _autoRotationConfigsUpdated;

    private Dictionary<string, (string controllers, int state)>
        AutoRotationConfigsControlled { get; } = new();

    internal (string controllers, int state)? AutoRotationConfigControlled(
        string configName)
    {
        var configOption = Enum.Parse<AutoRotationConfigOption>(configName);

        // Return the cached value if it is valid, fastest
        if (_autoRotationConfigsUpdated is not null &&
            _autoRotationConfigsUpdated ==
            _leasing.AutoRotationConfigsUpdated &&
            AutoRotationConfigsControlled.TryGetValue(configName,
                out var configControlled))
            return configControlled;

        // Bail if the config is not controlled, fast-ish
        var controlled = _leasing.CheckAutoRotationConfigControlled(configOption);
        if (controlled is null)
            return null;

        // Re-populate the cache with the current set of controlled configs, slowest
        AutoRotationConfigsControlled.Clear();
        foreach (var controlledConfig in _search.AllAutoRotationConfigsControlled)
        {
            var controllers = string.Join(", ", controlledConfig.Value.Keys);
            var state = controlledConfig.Value.Values.First();
            AutoRotationConfigsControlled[controlledConfig.Key.ToString()] =
                (controllers, state);
        }

        _autoRotationConfigsUpdated = _search.LastCacheUpdateForAutoRotationConfigs;

        return AutoRotationConfigsControlled[configName];
    }

    #endregion

    #endregion

    #region Helper methods for the UI

    #region Style Variables

    private readonly Vector4 _backgroundColor = new(0.68f, 0.77f, 0.80f, 1);
    private readonly Vector4 _hoverColor = new(0.84f, 0.92f, 0.96f, 1);
    private readonly Vector4 _textColor = new(0.05f, 0.05f, 0.05f, 1);
    private readonly Vector2 _padding = new(14f.Scale(), 2f);
    private readonly Vector2 _spacing = new(0, 0);
    private readonly float _rounding = 8f;

    #endregion

    #region Tooltips

    private const string IndicatorTooltip =
        "This option is controlled by another plugin.\n" +
        "Click the X to revoke control.";

    private const string OptionTooltip =
        "This option is controlled by another plugin.\n" +
        "There is a 'Controlled by:' label above this option,\n" +
        "which lists the plugins controlling this option,\n" +
        "and a 'X' button to revoke control.";

    #endregion

    // Method to display the controlled indicator, which lists the plugins
    private bool ShowIPCControlledIndicator
    (bool? forAutoRotation = null,
        uint? forJob = null,
        CustomComboPreset? forPreset = null,
        string? forAutoRotationConfig = null)
    {
        (string controllers, object state)? controlled = null;

        #region Bail if not needed

        if (forAutoRotation is not null)
            if ((controlled = AutoRotationStateControlled()) is null)
                return false;
        if (forJob is not null)
            if ((controlled = JobControlled((uint)forJob)) is null)
                return false;
        if (forPreset is not null)
            if ((controlled =
                    PresetControlled((CustomComboPreset)forPreset)) is null)
                return false;
        if (forAutoRotationConfig is not null)
            if ((controlled =
                    AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return false;

        if (controlled is null)
            return false;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.Button, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, _padding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, _rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, _spacing);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _backgroundColor);
        ImGui.SmallButton($"Controlled by: {controlled.Value.controllers}");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - _rounding.Scale() - 3f.Scale());
        if (ImGui.SmallButton("X"))
            RevokeControl(controlled.Value.controllers);
        ImGui.PopStyleColor(3);

        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(2);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(IndicatorTooltip);
        return true;
    }

    // Method to display a differently-styled and disabled checkbox if controlled
    private bool ShowIPCControlledCheckbox
    (string label, bool? forAutoRotation = null,
        uint? forJob = null,
        CustomComboPreset? forPreset = null,
        string? forAutoRotationConfig = null)
    {
        (string controllers, bool state)? controlled = null;

        #region Bail if not needed

        if (forAutoRotation is not null)
            if ((controlled = AutoRotationStateControlled()) is null)
                return false;
        if (forJob is not null)
            if ((controlled = JobControlled((uint)forJob)) is null)
                return false;
        if (forPreset is not null)
            if ((controlled =
                    PresetControlled((CustomComboPreset)forPreset)) is null)
                return false;
        if (forAutoRotationConfig is not null)
        {
            var check = AutoRotationConfigControlled(forAutoRotationConfig);
            if (check is null)
                return false;
            controlled = (check.Value.controllers, check.Value.state == 1);
        }

        if (controlled is null)
            return false;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.FrameBg, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, _textColor);

        var _ = controlled.Value.state;
        ImGui.BeginDisabled();
        ImGui.Checkbox("", ref _);
        ImGui.EndDisabled();

        ImGui.SameLine();
        ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.DalamudGrey, label);

        ImGui.PopStyleColor(2);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(OptionTooltip);

        return false;
    }

    private bool ShowIPCControlledSlider
        (string label, string? forAutoRotationConfig = null)
    {
        (string controllers, int state)? controlled = null;

        #region Bail if not needed

        if (forAutoRotationConfig is not null)
            if ((controlled =
                    AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return false;

        if (controlled is null)
            return false;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.FrameBg, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, _backgroundColor);

        var _ = controlled.Value.state;
        ImGui.BeginDisabled();
        ImGui.SetNextItemWidth(200f.Scale());
        ImGuiEx.SliderInt("", ref _, 1, 99, "%d%%");
        ImGui.EndDisabled();

        ImGui.SameLine();
        ImGuiHelpers.SafeTextColoredWrapped(ImGuiColors.DalamudGrey, label);

        ImGui.PopStyleColor(3);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(OptionTooltip);

        return false;
    }

    private bool ShowIPCControlledCombo
        (string? forAutoRotationConfig = null)
    {
        (string controller, int state)? controlled = null;

        #region Bail if not needed

        if (forAutoRotationConfig is not null)
            if ((controlled =
                    AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return false;

        if (controlled is null)
            return false;

        #endregion

        ImGui.PushStyleColor(ImGuiCol.FrameBg, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);

        var option = Enum.Parse<AutoRotationConfigOption>(forAutoRotationConfig!);
        var valueType = option.GetType()
            .GetField(option.ToString())
            .GetCustomAttributes(typeof(ConfigValueTypeAttribute), false)
            .Cast<ConfigValueTypeAttribute>()
            .First().ValueType!;
        var value =
            Enum.Parse(valueType, controlled.Value.state.ToString());
        string valueString = value.ToString()!.Replace("_", " ");
        string[] values = [valueString];

        var _ = 0;
        ImGui.BeginDisabled();
        ImGui.Combo("", ref _, values, values.Length);
        ImGui.EndDisabled();

        ImGui.PopStyleColor(2);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(OptionTooltip);

        return false;
    }

    /// <summary>
    ///     Button click method for Indicator to cancel plugin control.
    /// </summary>
    /// <param name="controllers">The displayed list of plugins to revoke.</param>
    private void RevokeControl(string controllers)
    {
        var controllerNames = controllers.Split(", ");
        var leases = _leasing.Registrations.Values
            .Where(l => controllerNames.Contains(l.PluginName))
            .Select(l => l.ID)
            .ToList();
        foreach (var lease in leases)
            _leasing.RemoveRegistration(
                lease, CancellationReason.WrathUserManuallyCancelled);
    }

    #region Actual UI Method overloads

    #region Indicator

    public bool ShowIPCControlledIndicatorIfNeeded() =>
        ShowIPCControlledIndicator(forAutoRotation: true);

    public bool ShowIPCControlledIndicatorIfNeeded(uint job) =>
        ShowIPCControlledIndicator(forJob: job);

    public bool ShowIPCControlledIndicatorIfNeeded(CustomComboPreset preset) =>
        ShowIPCControlledIndicator(forPreset: preset);

    public bool ShowIPCControlledIndicatorIfNeeded(string configName) =>
        ShowIPCControlledIndicator(forAutoRotationConfig: configName);

    #endregion

    #region Disabled Inputs

    public bool ShowIPCControlledCheckboxIfNeeded
        (string label) =>
        ShowIPCControlledCheckbox(label, forAutoRotation: true);

    public bool ShowIPCControlledCheckboxIfNeeded
        (string label, uint job) =>
        ShowIPCControlledCheckbox(label, forJob: job);

    public bool ShowIPCControlledCheckboxIfNeeded
        (string label, CustomComboPreset preset) =>
        ShowIPCControlledCheckbox(label, forPreset: preset);

    public bool ShowIPCControlledCheckboxIfNeeded
        (string label, string configName) =>
        ShowIPCControlledCheckbox(label, forAutoRotationConfig: configName);

    public bool ShowIPCControlledSliderIfNeeded
        (string label, string configName) =>
        ShowIPCControlledSlider(label, forAutoRotationConfig: configName);

    public bool ShowIPCControlledComboIfNeeded
        (string configName) =>
        ShowIPCControlledCombo(forAutoRotationConfig: configName);

    #endregion

    #endregion

    #endregion
}
