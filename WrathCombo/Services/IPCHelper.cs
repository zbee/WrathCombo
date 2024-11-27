#region

using System;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.Attributes;
using WrathCombo.Combos;

#endregion

namespace WrathCombo.Services;

public class IPCRegistration
{
    // Organizing data about a lease, and everything it controls
}

public class IPCHelper
{
    private Dictionary<Guid, IPCRegistration> _registrations = new();

    internal Guid CreateRegistration(string pluginName, Action callback)
    {
        throw new NotImplementedException();
    }

    internal void AddRegistrationForCurrentJob(Guid lease)
    {
        throw new NotImplementedException();
    }

    internal void AddRegistrationForAutoRotation (Guid lease, bool newState)
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

    internal void RemoveRegistration(Guid lease)
    {
        throw new NotImplementedException();
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
}
