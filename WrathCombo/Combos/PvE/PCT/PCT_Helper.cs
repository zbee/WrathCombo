using Dalamud.Game.ClientState.JobGauge.Types;
using ECommons.DalamudServices;
using System.Collections.Generic;
using WrathCombo.CustomComboNS;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;


namespace WrathCombo.Combos.PvE;

internal partial class PCT
{
    public static PCTopener1 PCTOpener = new();

    private static PCTGauge Gauge = GetJobGauge<PCTGauge>();

    public static bool HasMotifs()
    {

        if (!Gauge.CanvasFlags.HasFlag(Dalamud.Game.ClientState.JobGauge.Enums.CanvasFlags.Pom))
            return false;
      
        if (!Gauge.CanvasFlags.HasFlag(Dalamud.Game.ClientState.JobGauge.Enums.CanvasFlags.Weapon))
            return false;

        if (!Gauge.CanvasFlags.HasFlag(Dalamud.Game.ClientState.JobGauge.Enums.CanvasFlags.Landscape))
            return false;

        return true;
    }

    internal class PCTopener1 : WrathOpener
    {
        public override int OpenerLevel => 100;

        public override List<uint> OpenerActions { get; protected set; } =
        [
            RainbowDrip,
            PomMuse,
            StrikingMuse,
            WingMotif,
            StarryMuse,
            HammerStamp,
            SubtractivePalette,
            BlizzardinCyan,
            StoneinYellow,
            ThunderinMagenta,
            CometinBlack,
            WingedMuse,
            MogoftheAges,
            StarPrism,
            HammerBrush,
            PolishingHammer,
            RainbowDrip,
            All.Swiftcast,
            ClawMotif,
            ClawedMuse,
        ];

        public override bool HasCooldowns()
        {
            if (!ActionReady(StarryMuse))
                return false;

            if (GetRemainingCharges(LivingMuse) < 3)
                return false;

            if (GetRemainingCharges(SteelMuse) < 2)
                return false;

            if (!HasMotifs())
                return false;

            if (HasEffect(Buffs.SubtractivePalette))
                return false;

            if (IsOnCooldown(All.Swiftcast))
                return false;

            return true;
        }

    }
}