#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using ECommons.ExcelServices;
using ECommons.Logging;
using WrathCombo.Attributes;
using WrathCombo.Combos;

#endregion

namespace WrathCombo.Services;

#region Standardized Dictionary Keys

/// <summary>
///     The keys for the states of a combo, enabled and enabled in Auto-Mode.
/// </summary>
public enum ComboState
{
    /// <summary>
    ///     The key for whether a combo is enabled.
    /// </summary>
    Enabled,

    /// <summary>
    ///     The key for whether a combo is enabled in Auto-Mode.
    /// </summary>
    AutoMode
}

/// <summary>
///     The keys for the types of target a combo is designed for, Single-Target or
///     Multi-Target.
/// </summary>
public enum ComboTargetType
{
    /// <summary>
    ///     The key for conveying data about the Single-Target portion of a job
    ///     configuration.
    /// </summary>
    SingleTarget,

    /// <summary>
    ///     The key for conveying data about the Multi-Target portion of a job
    ///     configuration.
    /// </summary>
    MultiTarget
}

#endregion

/// <summary>
///     Why a lease was cancelled.
/// </summary>
public enum CancellationReason
{
    [Description("The Wrath user manually elected to revoke your lease.")]
    WrathUserManuallyCancelled,

    [Description("Another plugin leasing Wrath options overwrote your lease, " +
                 "see additional information for details.")]
    OtherLeaseeOverwrote,

    [Description("Your plugin was detected as having been disabled, " +
                 "not that you're likely to see this.")]
    LeaseePluginDisabled,

    [Description("The Wrath plugin is being disabled.")]
    WrathPluginDisabled,

    [Description("Your lease was released by IPC call, " +
                 "theoretically this was done by you.")]
    LeaseeReleased,

    [Description("IPC Services have been disabled remotely. " +
                 "Please see the commit history for /res/ipc_status.txt. \n " +
                 "https://github.com/PunishXIV/WrathCombo/commits/main/res/ipc_status.txt")]
    AllServicesSuspended,
}

public class IPCRegistration(
    string pluginName,
    Action<CancellationReason, string>? callback)
{
    public Guid ID { get; } = Guid.NewGuid();
    public string PluginName { get; } = pluginName;
    public Action<CancellationReason, string>? Callback { get; } = callback;

    private DateTime Created { get; } = DateTime.Now;
    private DateTime LastUpdated { get; } = DateTime.Now;

    /// <summary>
    ///     The number of sets leased by this registration currently.
    ///     Maximum is <c>40</c>.
    /// </summary>
    /// <seealso cref="IPCService.RegisterForLease" />
    public int SetsLeased =>
        AutoRotationControlled.Count +
        JobsControlled.Count * 2 +
        CombosControlled.Count * 2 +
        OptionsControlled.Count;

    internal Dictionary<byte, bool> AutoRotationControlled { get; set; } = new();

    internal Dictionary<AutoRotationConfigOption, int> AutoRotationConfigsControlled
    {
        get;
        set;
    } = new();

    internal Dictionary<Job, bool> JobsControlled { get; set; } = new();

    internal Dictionary<CustomComboPreset, bool> CombosControlled { get; set; } =
        new();

    internal Dictionary<CustomComboPreset, bool> OptionsControlled { get; set; } =
        new();

    public void Cancel
        (CancellationReason cancellationReason, string additionalInfo = "")
    {
        IPCLogging.Log(
            "Cancelling Lease for: "
            + pluginName
            + " (" + cancellationReason + ")" +
            (additionalInfo != ""
                ? "\n" + additionalInfo
                : "")
        );
        Callback?.Invoke(cancellationReason, additionalInfo);
    }
}

public partial class IPCHelper
{
    private Dictionary<Guid, IPCRegistration> _registrations = new();

    internal Guid? CreateRegistration(string pluginName, Action callback)
    {
        throw new NotImplementedException();
    }

    internal void AddRegistrationForCurrentJob(Guid lease)
    {
        throw new NotImplementedException();
    }

    internal void AddRegistrationForAutoRotation(Guid lease, bool newState)
    {
        throw new NotImplementedException();
    }

    internal void AddRegistrationForCombo
        (Guid lease, string combo, bool newState, bool newAutoState)
    {
        throw new NotImplementedException();
    }

    internal void AddRegistrationForOption
        (Guid lease, string combo, bool newState)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Removes a registration from the IPC service, cancelling the lease.
    /// </summary>
    /// <param name="lease">
    ///     Your lease ID from <see cref="IPCService.RegisterForLease" />
    /// </param>
    /// <param name="cancellationReason">
    ///     The <see cref="CancellationReason" /> for cancelling the lease.
    /// </param>
    /// <param name="additionalInfo">
    ///     Any additional information to log and provide with the cancellation.
    /// </param>
    /// <remarks>
    ///     Will call the <see cref="IPCRegistration.Callback" /> method if one was
    ///     provided.
    /// </remarks>
    internal void RemoveRegistration
    (Guid lease, CancellationReason cancellationReason,
        string additionalInfo = "")
    {
        _registrations[lease].Cancel(cancellationReason, additionalInfo);
        _registrations.Remove(lease);
    }

    internal string? CheckJobControlled()
    {
        throw new NotImplementedException();
    }

    internal string? CheckAutoRotationControlled()
    {
        throw new NotImplementedException();
    }

    internal string? CheckComboControlled(string combo)
    {
        throw new NotImplementedException();
    }

    internal string? CheckOptionControlled(string option)
    {
        throw new NotImplementedException();
    }

    // job name, internal name
    internal static Dictionary<string, string> SearchForCombosInAutoMode() =>
        Service.Configuration.AutoActions
            // Only get combos with their auto-modes turned on
            .Where(action => action.Value)
            // Get the enum value of those combos
            .Select(action =>
                Enum.Parse(typeof(CustomComboPreset), action.Key.ToString()))
            // Get the actual enum value
            .Cast<CustomComboPreset>()
            // Select the internal name and job ID of the combo
            .Select(preset => new
            {
                InternalName = preset.ToString(),
                ((CustomComboInfoAttribute)Attribute.GetCustomAttribute(
                    typeof(CustomComboPreset).GetField(preset.ToString())!,
                    typeof(CustomComboInfoAttribute)
                )!).JobID
            })
            // Output a dictionary of `job abbr` -> `internal name`
            .ToDictionary(
                combo => CustomComboInfoAttribute.JobIDToShorthand(combo.JobID),
                combo => combo.InternalName
            );

    #region Checking the repo for live IPC status

    private readonly HttpClient _httpClient = new();

    /// <summary>
    ///     The endpoint for checking the IPC status straight from the repo,
    ///     so it can be disabled without a plugin update if for some reason
    ///     necessary.
    /// </summary>
    private const string IPCStatusEndpoint =
        "https://raw.githubusercontent.com/PunishXIV/WrathCombo/main/res/ipc_status.txt";

    /// <summary>
    ///     The cached backing field for the IPC status.
    /// </summary>
    /// <seealso cref="_ipcStatusLastUpdated" />
    /// <seealso cref="IPCEnabled" />
    private bool? _ipcEnabled;

    /// <summary>
    ///     The time the IPC status was last checked.
    /// </summary>
    /// <seealso cref="_ipcEnabled" />
    /// <seealso cref="IPCEnabled" />
    private DateTime? _ipcStatusLastUpdated;

    /// <summary>
    ///     The lightly-cached live IPC status.<br />
    ///     Backed by <see cref="_ipcEnabled" />.
    /// </summary>
    /// <seealso cref="IPCStatusEndpoint" />
    /// <seealso cref="_ipcEnabled" />
    /// <seealso cref="_ipcStatusLastUpdated" />
    public bool IPCEnabled
    {
        get
        {
            // If the IPC status was checked within the last 5 minutes:
            // return the cached value
            if (_ipcEnabled is not null &&
                DateTime.Now - _ipcStatusLastUpdated < TimeSpan.FromMinutes(5))
                return _ipcEnabled!.Value;

            // Otherwise, check the status and cache the result
            var data = string.Empty;
            // Check the status
            try
            {
                using var ipcStatusQuery =
                    _httpClient.GetAsync(IPCStatusEndpoint).Result;
                ipcStatusQuery.EnsureSuccessStatusCode();
                data = ipcStatusQuery.Content.ReadAsStringAsync()
                    .Result.Trim().ToLower();
            }
            catch (Exception e)
            {
                IPCLogging.Error(
                    "Failed to check IPC status. Assuming it is enabled.\n" +
                    e.Message
                );
            }

            // Read the status
            var ipcStatus = data.StartsWith("enabled");
            // Cache the status
            _ipcEnabled = ipcStatus;
            _ipcStatusLastUpdated = DateTime.Now;

            // Handle suspended status
            if (!ipcStatus)
                SuspendLeases();

            return ipcStatus;
        }
    }

    /// <summary>
    ///     Suspend all leases. Called when IPC is disabled remotely.
    /// </summary>
    /// <seealso cref="IPCEnabled"/>
    /// <seealso cref="RemoveRegistration"/>
    private void SuspendLeases()
    {
        IPCLogging.Warn(
            "IPC has been disabled remotely.\n" +
            "Suspending all leases."
        );

        // dispose every lease in _registrations
        foreach (var registration in _registrations.Values)
            RemoveRegistration(
                registration.ID, CancellationReason.AllServicesSuspended
            );
    }

    #endregion

    #region Aggregations of Sets

    internal Dictionary<AutoRotationConfigOption, int>
        AllAutoRotationConfigsControlled =>
        _registrations.Values
            .SelectMany(registration => registration.AutoRotationConfigsControlled)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

    internal Dictionary<Job, bool> AllJobsControlled =>
        _registrations.Values
            .SelectMany(registration => registration.JobsControlled)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

    internal Dictionary<CustomComboPreset, bool> AllPresetsControlled =>
        _registrations.Values
            .SelectMany(registration => registration.CombosControlled)
            .ToDictionary(pair => pair.Key, pair => pair.Value)
            .Concat(
                _registrations.Values
                    .SelectMany(registration => registration.OptionsControlled)
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
            )
            .ToDictionary(pair => pair.Key, pair => pair.Value);

    #endregion
}

/// <summary>
///     Simple Wrapper for logging IPC events, to help keep the logs consistent.
/// </summary>
public static class IPCLogging
{
    private const string Prefix = "[Wrath IPC] ";

    private static readonly StackTrace StackTrace = new();

    private static string PrefixMethod
    {
        get
        {
            var frame = StackTrace.GetFrame(1); // Get the calling method frame
            var method = frame.GetMethod();
            var className = method.DeclaringType.Name;
            className = className[3..]; // Strip the "IPC" prefix
            var methodName = method.Name;
            return $"[{className}.{methodName}] ";
        }
    }

    public static void Log(string message) =>
        PluginLog.Verbose(Prefix + PrefixMethod + message);

    public static void Warn(string message) =>
        PluginLog.Warning(Prefix + PrefixMethod + message
#if DEBUG
                          + "\n" + (StackTrace)
#endif
        );

    public static void Error(string message) =>
        PluginLog.Error(Prefix + PrefixMethod + message + "\n" + (StackTrace));
}
