#region

 using System;
 using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.CustomComboNS;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CheckNamespace

#endregion

namespace WrathCombo.Combos.PvE;

internal partial class DNC
{
    internal static WrathOpener Opener()
    {
        if (Opener1.LevelChecked)
            return Opener1;

        return WrathOpener.Dummy;
    }

    private static DNCGauge Gauge => GetJobGauge<DNCGauge>();

    #region Openers

    internal static DNCOpenerMaxLevel1 Opener1 = new();

    internal class DNCOpenerMaxLevel1 : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            StandardStep,
            Emboite,
            Emboite,
            Peloton,
            StandardFinish2, // 5
            TechnicalStep,
            Emboite,
            Emboite,
            Emboite,
            Emboite, // 10
            TechnicalFinish4,
            Devilment,
            Tillana,
            Flourish,
            DanceOfTheDawn, // 15
            FanDance4,
            LastDance,
            FanDance3,
            FinishingMove,
            StarfallDance, // 20
            ReverseCascade,
            ReverseCascade,
            ReverseCascade,
            ReverseCascade,
        ];

        public override List<(int[], uint, Func<bool>)> ProcSteps { get; set; } =
        [
            ([2, 3, 7, 8, 9, 10], Gauge.NextStep, () => Gauge.NextStep != Emboite),
            ([20], SaberDance, () => Gauge.Esprit >= 50),
            ([21, 22, 23, 24], SaberDance, () => Gauge.Esprit > 80),
            ([21, 22, 23, 24], StarfallDance, () =>
                HasEffect(Buffs.FlourishingStarfall)),
            ([21, 22, 23, 24], SaberDance  , () => Gauge.Esprit >= 50),
            ([21, 22, 23, 24], LastDance, () => HasEffect(Buffs.LastDanceReady)),
            ([21, 22, 23, 24], Fountainfall, () =>
                HasEffect(Buffs.SilkenFlow) || HasEffect(Buffs.FlourishingFlow)),
        ];

        public override bool HasCooldowns()
        {
            if (!(HasEffect(Buffs.StandardStep) && Gauge.CompletedSteps == 2))
                return false;

            if (!ActionReady(TechnicalStep))
                return false;

            if (!ActionReady(Devilment))
                return false;

            return true;
        }
    }

    #endregion
}
