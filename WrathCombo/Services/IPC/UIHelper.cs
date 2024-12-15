#region

using System;
using System.Collections.Generic;
using System.Linq;
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

    private string AutoRotationControlled { get; set; } = string.Empty;

    private string? AutoRotationStateControlled()
    {
        // Return the cached value if it is valid, fastest
        if (string.IsNullOrEmpty(AutoRotationControlled) &&
            _autoRotationUpdated is not null &&
            _autoRotationUpdated == _leasing.AutoRotationStateUpdated)
            return AutoRotationControlled;

        // Bail if the state is not controlled, fast
        var controlled = _leasing.CheckAutoRotationControlled();
        if (controlled is null)
            return null;

        // Re-populate the cache with the current state, slowest
        var controllingLeases = _leasing.Registrations.Values
            .Where(l => l.AutoRotationControlled.Count != 0)
            .OrderByDescending(l => l.LastUpdated)
            .Select(l => l.PluginName);
        AutoRotationControlled = string.Join(", ", controllingLeases);
        _autoRotationUpdated = _leasing.AutoRotationStateUpdated;

        return AutoRotationControlled;
    }

    #endregion

    #region Jobs

    private DateTime? _jobsUpdated;

    private Dictionary<string, string> JobsControlled { get; } = new();

    private string? JobControlled(uint job)
    {
        var jobName = CustomComboFunctions.JobIDs.JobIDToShorthand(job);

        // Return the cached value if it is valid, fastest
        if (_jobsUpdated is not null &&
            _jobsUpdated == _search.LastCacheUpdateForAllJobsControlled &&
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
                string.Join(", ", controlledJob.Value.Keys);
        _jobsUpdated = _search.LastCacheUpdateForAllJobsControlled;

        return JobsControlled[jobName];
    }

    #endregion

    #region Presets

    private DateTime? _presetsUpdated;

    private Dictionary<string, string> PresetsControlled { get; } = new();

    private string? PresetControlled(CustomComboPreset preset)
    {
        var presetName = preset.ToString();

        // Return the cached value if it is valid, fastest
        if (_presetsUpdated is not null &&
            _presetsUpdated == _search.LastCacheUpdateForAllPresetsControlled &&
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
                string.Join(", ", controlledPreset.Value.Keys);
        _presetsUpdated = _search.LastCacheUpdateForAllPresetsControlled;

        return PresetsControlled[presetName];
    }

    #endregion

    #region Auto-Rotation Configs

    private DateTime? _autoRotationConfigsUpdated;

    private Dictionary<string, string> AutoRotationConfigsControlled { get; } =
        new();

    private string? AutoRotationConfigControlled(string configName)
    {
        var configOption = Enum.Parse<AutoRotationConfigOption>(configName);

        // Return the cached value if it is valid, fastest
        if (_autoRotationConfigsUpdated is not null &&
            _autoRotationConfigsUpdated ==
            _search.LastCacheUpdateForAutoRotationConfigs &&
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
            AutoRotationConfigsControlled[controlledConfig.Key.ToString()] =
                string.Join(", ", controlledConfig.Value.Keys);
        _autoRotationConfigsUpdated = _search.LastCacheUpdateForAutoRotationConfigs;

        return AutoRotationConfigsControlled[configName];
    }

    #endregion

    #endregion

    #region Helper methods for the UI

    // Method to display the controlled indicator, which lists the plugins

    // Method to display a differently-styled and disabled checkbox if controlled

    #region Actual UI Method overloads

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

    #region Indicator

    public void ShowIPCControlledIndicatorIfNeeded() {}
    public void ShowIPCControlledIndicatorIfNeeded(uint job) {}
    public void ShowIPCControlledIndicatorIfNeeded(CustomComboPreset preset) {}
    public void ShowIPCControlledIndicatorIfNeeded(string configName) {}

    #endregion

    #region Disabled Inputs

    // todo: these require me to also cache the controlled state, not just the controlling leases

    public void ShowIPCControlledCheckboxIfNeeded() {}
    public void ShowIPCControlledCheckboxIfNeeded(CustomComboPreset preset) {}
    public void ShowIPCControlledCheckboxIfNeeded(string configName) {}
    public void ShowIPCControlledComboIfNeeded(string configName) {}

    #endregion

    #endregion

    #endregion
}
