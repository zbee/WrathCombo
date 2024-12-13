#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using WrathCombo.Attributes;
using WrathCombo.Combos;
using WrathCombo.CustomComboNS.Functions;

#endregion

namespace WrathCombo.Services.IPC;

public class Search(ref Leasing leasing)
{
    private const StringComparison ToLower =
        StringComparison.CurrentCultureIgnoreCase;

    private readonly Leasing _leasing = leasing;

    #region Aggregations of Leasing Configurations

    internal Dictionary<AutoRotationConfigOption, Dictionary<string, int>>
        AllAutoRotationConfigsControlled =>
        _leasing.Registrations.Values
            .SelectMany(registration => registration.AutoRotationConfigsControlled
                .Select(pair => new
                {
                    pair.Key, registration.PluginName, pair.Value,
                    registration.LastUpdated
                }))
            .GroupBy(x => x.Key)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.LastUpdated)
                    .ToDictionary(x => x.PluginName, x => x.Value)
            );

    internal Dictionary<Job, Dictionary<string, bool>> AllJobsControlled =>
        _leasing.Registrations.Values
            .SelectMany(registration => registration.JobsControlled
                .Select(pair => new
                {
                    pair.Key, registration.PluginName, pair.Value,
                    registration.LastUpdated
                }))
            .GroupBy(x => x.Key)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.LastUpdated)
                    .ToDictionary(x => x.PluginName, x => x.Value)
            );

    internal Dictionary<CustomComboPreset, Dictionary<string, bool>>
        AllPresetsControlled =>
        _leasing.Registrations.Values
            .SelectMany(registration => registration.CombosControlled
                .Select(pair => new
                {
                    pair.Key, registration.PluginName, pair.Value,
                    registration.LastUpdated
                }))
            .GroupBy(x => x.Key)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.LastUpdated)
                    .ToDictionary(x => x.PluginName, x => x.Value)
            )
            .Concat(
                _leasing.Registrations.Values
                    .SelectMany(registration => registration.OptionsControlled
                        .Select(pair => new
                        {
                            pair.Key, registration.PluginName, pair.Value,
                            registration.LastUpdated
                        }))
                    .GroupBy(x => x.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.LastUpdated)
                            .ToDictionary(x => x.PluginName, x => x.Value)
                    )
            )
            .ToDictionary(pair => pair.Key, pair => pair.Value);

    #endregion

    #region Presets Information

    #region Cached Preset Info

    /// <summary>
    ///     The path to the configuration file for Wrath Combo.
    /// </summary>
    private static string ConfigFilePath
    {
        get
        {
            var pluginConfig = Svc.PluginInterface.GetPluginConfigDirectory();
            if (Path.EndsInDirectorySeparator(pluginConfig))
                pluginConfig = Path.TrimEndingDirectorySeparator(pluginConfig);
            pluginConfig =
                pluginConfig
                    [..pluginConfig.LastIndexOf(Path.DirectorySeparatorChar)];
            pluginConfig = Path.Combine(pluginConfig, "WrathCombo.json");
            return pluginConfig;
        }
    }

    /// <summary>
    ///     When <see cref="PresetStates" /> was last built.
    /// </summary>
    private static DateTime _lastCacheUpdateForPresetStates = DateTime.MinValue;

    /// <summary>
    ///     Cached list of <see cref="CustomComboPreset">Presets</see>, and most of
    ///     their attribute-based information.
    /// </summary>
    [field: AllowNull, MaybeNull]
    // ReSharper disable once MemberCanBePrivate.Global
    internal static Dictionary<string, (CustomComboPreset ID,
        CustomComboInfoAttribute Info, bool HasParentCombo, bool IsVariant, string
        ParentComboName)> Presets
    {
        get
        {
            return field ??= Enum.GetValues(typeof(CustomComboPreset))
                .Cast<CustomComboPreset>()
                .Select(preset => new
                {
                    ID = preset,
                    InternalName = preset.ToString(),
                    Info = (CustomComboInfoAttribute)Attribute.GetCustomAttribute(
                        typeof(CustomComboPreset).GetField(
                            preset.ToString())!,
                        typeof(CustomComboInfoAttribute)
                    )!,
                    HasParentCombo = Attribute.IsDefined(
                        typeof(CustomComboPreset).GetField(
                            preset.ToString())!,
                        typeof(ParentComboAttribute)
                    ),
                    IsVariant = Attribute.IsDefined(
                        typeof(CustomComboPreset).GetField(
                            preset.ToString())!,
                        typeof(VariantAttribute)
                    ),
                    ParentComboName = Attribute.IsDefined(
                        typeof(CustomComboPreset).GetField(
                            preset.ToString())!,
                        typeof(ParentComboAttribute)
                    )
                        ? ((CustomComboInfoAttribute)Attribute.GetCustomAttribute(
                            typeof(CustomComboPreset).GetField(
                                ((ParentComboAttribute)Attribute.GetCustomAttribute(
                                    typeof(CustomComboPreset).GetField(
                                        preset.ToString())!,
                                    typeof(ParentComboAttribute)
                                )!).ParentPreset.ToString())!,
                            typeof(CustomComboInfoAttribute)
                        )!).Name
                        : string.Empty
                })
                .Where(combo =>
                    !combo.InternalName.EndsWith("any", ToLower))
                .ToDictionary(
                    combo => combo.InternalName,
                    combo => (combo.ID, combo.Info, combo.HasParentCombo,
                        combo.IsVariant, combo.ParentComboName)
                );
        }
    }

    /// <summary>
    ///     Cached list of <see cref="CustomComboPreset">Presets</see>, and the
    ///     state and Auto-Mode state of each.
    /// </summary>
    /// <remarks>
    ///     Rebuilt if the <see cref="ConfigFilePath">Config File</see> has been
    ///     updated since
    ///     <see cref="_lastCacheUpdateForPresetStates">last cached</see>.
    /// </remarks>
    [field: AllowNull, MaybeNull]
    // ReSharper disable once MemberCanBePrivate.Global
    internal static Dictionary<string, Dictionary<ComboStateKeys, bool>> PresetStates
    {
        get
        {
            if (field != null &&
                File.GetLastWriteTime(ConfigFilePath) <=
                _lastCacheUpdateForPresetStates)
                return field;

            field = Presets
                .ToDictionary(
                    preset => preset.Key,
                    preset =>
                    {
                        var isEnabled =
                            CustomComboFunctions.IsEnabled(preset.Value.ID);
                        var isAutoMode =
                            Service.Configuration.AutoActions.TryGetValue(
                                preset.Value.ID, out bool autoMode) && autoMode;
                        return new Dictionary<ComboStateKeys, bool>
                        {
                            { ComboStateKeys.Enabled, isEnabled },
                            { ComboStateKeys.AutoMode, isAutoMode }
                        };
                    }
                );
            _lastCacheUpdateForPresetStates = DateTime.Now;

            return field;
        }
    }

    #endregion

    #region Combo Information

    internal static Dictionary<string, List<string>> ComboNamesByJob =>
        Presets
            .Where(preset =>
                preset.Value is { IsVariant: false, HasParentCombo: false } &&
                !preset.Key.Contains("pvp", ToLower))
            .GroupBy(preset =>
                CustomComboFunctions.JobIDs.JobIDToShorthand(preset.Value.Info.JobID))
            .ToDictionary(
                g => g.Key,
                g => g.Select(preset => preset.Key).ToList()
            );

    internal static Dictionary<string,
            Dictionary<string, Dictionary<ComboStateKeys, bool>>>
        ComboStatesByJob =>
        ComboNamesByJob
            .ToDictionary(
                job => job.Key,
                job => job.Value
                    .ToDictionary(
                        combo => combo,
                        combo => PresetStates[combo]
                    )
            );

    internal static Dictionary<string,
            Dictionary<ComboTargetTypeKeys,
                Dictionary<ComboSimplicityLevelKeys,
                    Dictionary<string, Dictionary<ComboStateKeys, bool>>>>>
        ComboStatesByJobCategorized =>
        Presets
            .Where(preset =>
                preset.Value is { IsVariant: false, HasParentCombo: false } &&
                !preset.Key.Contains("pvp", ToLower))
            .SelectMany(preset => new[]
            {
                new
                {
                    Job = CustomComboFunctions.JobIDs.JobIDToShorthand(preset.Value.Info
                        .JobID),
                    Combo = preset.Key,
                    preset.Value.Info
                }
            })
            .GroupBy(x => x.Job)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x =>
                        x.Info.Name.Contains("- aoe", ToLower) ||
                        x.Info.Name.Contains("aoe dps feature", ToLower)
                            ? ComboTargetTypeKeys.MultiTarget
                            : x.Info.Name.Contains("- single target", ToLower) ||
                              x.Info.Name.Contains("single target dps feature",
                                  ToLower)
                                ? ComboTargetTypeKeys.SingleTarget
                                : ComboTargetTypeKeys.Other
                    )
                    .ToDictionary(
                        g2 => g2.Key,
                        g2 => g2.GroupBy(x =>
                                x.Info.Name.Contains("advanced mode -", ToLower) ||
                                x.Info.Name.Contains("dps feature", ToLower)
                                    ? ComboSimplicityLevelKeys.Advanced
                                    : x.Info.Name.Contains("simple mode -", ToLower)
                                        ? ComboSimplicityLevelKeys.Simple
                                        : ComboSimplicityLevelKeys.Other
                            )
                            .ToDictionary(
                                g3 => g3.Key,
                                g3 => g3.ToDictionary(
                                    x => x.Combo,
                                    x => ComboStatesByJob[x.Job][x.Combo]
                                )
                            )
                    )
            );

    #endregion

    #region Options Information

    internal static Dictionary<string, List<string>> OptionNamesByJob =>
        Presets
            .Where(preset =>
                preset.Value is { IsVariant: false, HasParentCombo: true } &&
                !preset.Key.Contains("pvp", ToLower))
            .GroupBy(preset =>
                CustomComboFunctions.JobIDs.JobIDToShorthand(preset.Value.Info.JobID))
            .ToDictionary(
                g => g.Key,
                g => g.Select(preset => preset.Key).ToList()
            );

    internal static Dictionary<string,
            Dictionary<string, Dictionary<ComboStateKeys, bool>>>
        OptionStatesByJob =>
        OptionNamesByJob
            .ToDictionary(
                job => job.Key,
                job => job.Value
                    .ToDictionary(
                        option => option,
                        option => new Dictionary<ComboStateKeys, bool>
                        {
                            {
                                ComboStateKeys.Enabled,
                                PresetStates[option][ComboStateKeys.Enabled]
                            }
                        }
                    )
            );

    #endregion

    #endregion
}
