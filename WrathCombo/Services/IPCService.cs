#region

using System;
using System.Collections.Generic;
using ECommons.Logging;
using WrathCombo.Attributes;
using WrathCombo.Combos;
using Helper = WrathCombo.Services.IPCHelper;

// ReSharper disable UnusedMember.Global

#endregion

namespace WrathCombo.Services;

/// <summary>
///     TODO:
///     <list type="bullet">
///         <item>
///             <description>
///                 Convert <see cref="IsCurrentJobConfiguredOn" />,
///                 <see cref="IsCurrentJobAutoModeOn" />,
///                 <see cref="GetComboState" /> to use enum keys.
///             </description>
///         </item>
///     </list>
/// </summary>
public partial class IPCService
{
    /// <summary>
    ///     Method to test IPC.
    /// </summary>
    public void Test() => PluginLog.Debug("IPC connection successful.");

    #region Normal IPC Flow

    /// <summary>
    ///     Register your plugin for control of Wrath Combo.
    /// </summary>
    /// <param name="pluginName">
    ///     The name you want shown to Wrath users for options your plugin controls.
    /// </param>
    /// <param name="leaseCancelledCallback">
    ///     Your method to be called when your lease is cancelled, usually
    ///     by the user.
    /// </param>
    /// <returns>Your lease ID to be used in <c>set</c> calls.</returns>
    /// <remarks>
    ///     Each lease is limited to controlling <c>40</c> configurations.
    /// </remarks>
    public Guid RegisterForLease
        (string pluginName, Action? leaseCancelledCallback = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Get the current state of the Auto-Rotation setting in Wrath Combo.
    /// </summary>
    /// <returns>Whether Auto-Rotation is enabled or disabled</returns>
    /// <remarks>
    ///     This is only the state of Auto-Rotation, not whether any combos are
    ///     enabled in Auto-Mode.
    /// </remarks>
    public bool GetAutoRotationState() =>
        Service.Configuration.RotationConfig.Enabled;

    /// <summary>
    ///     Set the state of Auto-Rotation in Wrath Combo.
    /// </summary>
    /// <param name="lease">Your lease ID from <see cref="RegisterForLease" /></param>
    /// <param name="enable">
    ///     Optionally whether to enable Auto-Rotation.<br />
    ///     Only used to disable Auto-Rotation, as enabling it is the default.
    /// </param>
    /// <seealso cref="GetAutoRotationState" />
    /// <remarks>
    ///     This is only the state of Auto-Rotation, not whether any combos are
    ///     enabled in Auto-Mode.
    /// </remarks>
    /// <value>+1 <c>set</c></value>
    public void SetAutoRotationState(Guid lease, bool enable = true)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Checks if the current job has a Single and Multi-Target combo configured
    ///     that are enabled in Auto-Mode.
    /// </summary>
    /// <returns>
    ///     If the user's current job is fully ready for Auto-Rotation.
    /// </returns>
    /// <seealso cref="IsCurrentJobConfiguredOn" />
    /// <seealso cref="IsCurrentJobAutoModeOn" />
    public bool IsCurrentJobAutoRotationReady()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sets up the user's current job for Auto-Rotation.<br />
    ///     This will enable the Single and Multi-Target combos, and enable them in
    ///     Auto-Mode.<br />
    ///     This will try to use the user's existing settings, only enabling default
    ///     states for jobs that are not configured.
    /// </summary>
    /// <value>+1 <c>set</c></value>
    /// <param name="lease">Your lease ID from <see cref="RegisterForLease" /></param>
    public void SetCurrentJobAutoRotationReady(Guid lease)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     This cancels your lease, removing your control of Wrath Combo.
    /// </summary>
    /// <param name="lease">Your lease ID from <see cref="RegisterForLease" /></param>
    /// <remarks>
    ///     Will call your <c>leaseCancelledCallback</c> method if you provided one.
    /// </remarks>
    public void ReleaseControl(Guid lease)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Extra Job State Checks

    /// <summary>
    ///     Checks if the user's current job has a Single-Target and Multi-Target
    ///     combo configured.
    /// </summary>
    /// <returns>
    ///     <b>Single-Target</b> - a <c>bool</c> indicating if a Single-Target combo
    ///     is configured.<br />
    ///     <b>Multi-Target</b> - a <c>bool</c> indicating if a Multi-Target combo
    ///     is configured.
    /// </returns>
    public Dictionary<string, bool> IsCurrentJobConfiguredOn()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Checks if the user's current job has a Single-Target and Multi-Target
    ///     combo enabled in Auto-Mode.
    /// </summary>
    /// <returns>
    ///     <b>Single-Target</b> - a <c>bool</c> indicating if a Single-Target combo
    ///     is enabled in Auto-Mode.<br />
    ///     <b>Multi-Target</b> - a <c>bool</c> indicating if a Multi-Target combo
    ///     is enabled in Auto-Mode.
    /// </returns>
    public Dictionary<string, bool> IsCurrentJobAutoModeOn()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Fine-Grained Combo Methods

    /// <summary>
    ///     Gets the internal names of all combos and options for the given job.
    /// </summary>
    /// <param name="jobAbbreviation">
    ///     The all-caps, 3-letter abbreviation for the job you want to search for.
    ///     <br />
    ///     Defaults to the user's current job if not provided.<br />
    ///     See <see cref="CustomComboInfoAttribute.JobIDToShorthand" />.
    /// </param>
    /// <returns>
    ///     A list of internal names for all combos and options for the given job.
    /// </returns>
    /// <seealso cref="Helper.SearchForCombosInAutoMode" />
    public List<string> GetComboNamesForJob(string? jobAbbreviation)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Get the current state of a combo in Wrath Combo.
    /// </summary>
    /// <param name="comboInternalName">
    ///     The internal name of the combo you want to check.<br />
    ///     See <see cref="CustomComboPreset" /> or <see cref="GetComboNamesForJob" />.
    /// </param>
    /// <returns>
    ///     <b>Enabled</b> - a <c>bool</c> indicating if the combo is enabled.<br />
    ///     <b>Auto-Mode</b> - a <c>bool</c> indicating if the combo is enabled in
    ///     Auto-Mode.
    /// </returns>
    public Dictionary<string, bool> GetComboState(string comboInternalName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Set the state of a combo in Wrath Combo.
    /// </summary>
    /// <value>+2 <c>set</c></value>
    /// <param name="lease">Your lease ID from <see cref="RegisterForLease" /></param>
    /// <param name="comboInternalName">
    ///     The internal name of the combo you want to set.<br />
    ///     See <see cref="CustomComboPreset" /> or <see cref="GetComboNamesForJob" />.
    /// </param>
    /// <param name="comboState">
    ///     Optionally whether to enable combo.<br />
    ///     Only used to disable the combo, as enabling it is the default.
    /// </param>
    /// <param name="autoState">
    ///     Optionally whether to enable the combo in Auto-Mode.<br />
    ///     Only used to disable the combo in Auto-Mode, as enabling it is the
    ///     default.
    /// </param>
    public void SetComboState
    (Guid lease, string comboInternalName,
        bool comboState = true, bool autoState = true)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Gets the current state of a combo option in Wrath Combo.
    /// </summary>
    /// <param name="optionName">
    ///     The name of the combo option you want to check.
    /// </param>
    /// <returns>
    ///     A <c>bool</c> indicating if the combo option is enabled.
    /// </returns>
    public bool GetComboOptionState(string optionName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sets the state of a combo option in Wrath Combo.
    /// </summary>
    /// <value>+1 <c>set</c></value>
    /// <param name="lease">
    ///     Your lease ID from <see cref="RegisterForLease" />.
    /// </param>
    /// <param name="optionName">
    ///     The name of the combo option you want to set.
    /// </param>
    /// <param name="state">
    ///     Optionally whether to enable the combo option.<br />
    ///     Only used to disable the combo option, as enabling it is the default.
    /// </param>
    public void SetComboOptionState(Guid lease, string optionName, bool state = true)
    {
        throw new NotImplementedException();
    }

    #endregion
}
