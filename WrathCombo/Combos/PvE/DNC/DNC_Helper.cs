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
            StandardFinish2,
            TechnicalStep,
            Emboite,
            Emboite,
            Emboite, // 5
            Emboite,
            TechnicalFinish4,
            Devilment,
            Tillana,
            Flourish, // 10
            DanceOfTheDawn,
            FanDance4,
            LastDance,
            FanDance3,
            FinishingMove, // 15
            StarfallDance,
            ReverseCascade,
            ReverseCascade,
            ReverseCascade,
            ReverseCascade, // 20
        ];

        public override List<(int[], uint, Func<bool>)> ProcSteps { get; set; } =
        [
            ([3, 4, 5, 6], Gauge.NextStep, () => Gauge.NextStep != Emboite),
            ([16], SaberDance, () => Gauge.Esprit >= 50),
            ([17, 18, 19, 20], SaberDance, () => Gauge.Esprit > 80),
            ([17, 18, 19, 20], StarfallDance, () =>
                HasEffect(Buffs.FlourishingStarfall)),
            ([17, 18, 19, 20], SaberDance  , () => Gauge.Esprit >= 50),
            ([17, 18, 19, 20], LastDance, () => HasEffect(Buffs.LastDanceReady)),
            ([17, 18, 19, 20], Fountainfall, () =>
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
