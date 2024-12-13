#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using ECommons.Logging;
using WrathCombo.CustomComboNS.Functions;

#endregion

namespace WrathCombo.Services.IPC;

public partial class Helper
{
    private readonly Leasing _leasing;

    public Helper(ref Leasing leasing)
    {
        _leasing = leasing;
    }

    /// <summary>
    ///     Checks for typical bail conditions at the time of a set.
    /// </summary>
    /// <param name="lease">
    ///     Your lease ID from <see cref="Provider.RegisterForLease" />
    /// </param>
    /// <param name="setCost">The cost of the <c>set</c> method.</param>
    /// <returns>If the method should bail.</returns>
    internal bool CheckForBailConditionsAtSetTime
        (Guid? lease = null, int? setCost = null)
    {
        // Bail if IPC is disabled
        if (!IPCEnabled)
        {
            Logging.Warn(BailMessages.LiveDisabled);
            return true;
        }

        // Bail if the lease is not valid
        if (lease is not null &&
            !_leasing.CheckLeaseExists(lease.Value))
        {
            Logging.Warn(BailMessages.InvalidLease);
            return true;
        }

        // Bail if the lease is blacklisted
        if (lease is not null &&
            _leasing.CheckBlacklist(lease.Value))
        {
            Logging.Warn(BailMessages.BlacklistedLease);
            return true;
        }

        // Bail if the lease does not have enough configuration left for this set
        if (lease is not null &&
            setCost is not null &&
            _leasing.CheckLeaseConfigurationsAvailable(lease.Value) >= setCost.Value)
        {
            Logging.Warn(BailMessages.NotEnoughConfigurations);
            return true;
        }

        return false;
    }

    internal bool CheckCurrentJobModeIsEnabled
        (ComboTargetTypeKeys mode, ComboStateKeys enabledStateToCheck)
    {
        Search.ComboStatesByJobCategorized.TryGetValue(
            CustomComboFunctions.JobIDs.JobIDToShorthand(
                (byte)CustomComboFunctions.LocalPlayer!.ClassJob.RowId),
            out var comboStates);

        if (comboStates is null)
            return false;

        comboStates[mode]
            .TryGetValue(ComboSimplicityLevelKeys.Simple, out var simpleResults);
        var simple =
            simpleResults?.FirstOrDefault().Value;
        var advanced =
            comboStates[mode][ComboSimplicityLevelKeys.Advanced].First().Value;

        return simple is not null && simple[enabledStateToCheck] ||
               advanced[enabledStateToCheck];
    }

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
                Logging.Error(
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
                _leasing.SuspendLeases();

            return ipcStatus;
        }
    }

    #endregion
}

/// <summary>
///     Simple Wrapper for logging IPC events, to help keep things consistent.
/// </summary>
public static class Logging
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
