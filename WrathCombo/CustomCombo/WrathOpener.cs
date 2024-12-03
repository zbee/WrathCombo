using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using System;
using WrathCombo.Combos.JobHelpers.Enums;
using WrathCombo.Data;
using WrathCombo.Services;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.CustomComboNS
{
    public abstract class WrathOpener : IDisposable
    {
        private OpenerState currentState = OpenerState.OpenerNotReady;
        private int openerStep;
        private int prePullStep;

        protected WrathOpener()
        {
            ActionWatching.OnLastActionChange += OnLastActionChange;
        }

        private void OnLastActionChange()
        {
            if (CurrentState == OpenerState.PrePull) PrePullStep++;
            if (CurrentState == OpenerState.InOpener) OpenerStep++;
        }

        public virtual OpenerState CurrentState
        {
            get => currentState;
            set
            {
                if (value != currentState)
                {
                    if (value == OpenerState.PrePull)
                        if (Service.Configuration.OutputOpenerLogs)
                            DuoLog.Information("Entered Pre-Pull Opener");
                        else
                            Svc.Log.Debug($"Entered Pre-Pull Opener");

                    if (value == OpenerState.InOpener) OpenerStep = 1;

                    if (value == OpenerState.OpenerFinished || value == OpenerState.FailedOpener)
                    {
                        if (value == OpenerState.FailedOpener)
                            if (Service.Configuration.OutputOpenerLogs)
                                DuoLog.Error($"Opener Failed at step {OpenerStep}");
                            else
                                Svc.Log.Information($"Opener Failed at step {OpenerStep}");

                        ResetOpener();
                    }
                    if (value == OpenerState.OpenerFinished)
                        if (Service.Configuration.OutputOpenerLogs)
                            DuoLog.Information("Opener Finished");
                        else
                            Svc.Log.Debug($"Opener Finished");

                    currentState = value;
                }
            }
        }
        public virtual int PrePullStep
        {
            get => prePullStep;
            set
            {
                if (prePullStep != value)
                {
                    Svc.Log.Debug($"Prepull Step {value}");
                    prePullStep = value;
                }
            }
        }

        public virtual int OpenerStep
        {
            get => openerStep;
            set
            {
                if (value != openerStep)
                {
                    Svc.Log.Debug($"Opener Step {value}");
                    openerStep = value;
                }
            }
        }

        public abstract int OpenerStepCount { get; }

        public abstract int PrePullStepCount { get; }

        public abstract int OpenerLevel { get; }

        public bool LevelChecked => Player.Level >= OpenerLevel;

        public abstract bool HasCooldowns();

        public abstract bool PrePullSteps(ref uint actionId);

        public abstract bool Opener(ref uint actionId);

        public abstract bool OpenerFailStates(ref uint actionId);

        public abstract bool PrePullFailStates(ref uint actionId);

        public bool FullOpener(ref uint actionID)
        {
            if (!LevelChecked)
                return false;

            if (CurrentState == OpenerState.OpenerNotReady)
                if (HasCooldowns())
                    CurrentState = OpenerState.PrePull;

            if (CurrentState == OpenerState.PrePull)
            {
                if (!HasCooldowns() && PrePullStep == 1)
                {
                    CurrentState = OpenerState.OpenerNotReady;
                    return false;
                }

                if (PrePullStep == 0)
                    PrePullStep = 1;

                if (PrePullSteps(ref actionID))
                    return true;

                if (PrePullFailStates(ref actionID))
                {
                    CurrentState = OpenerState.FailedOpener;
                    return false;
                }
            }

            if (CurrentState == OpenerState.InOpener)
            {
                if (Opener(ref actionID))
                    return true;

                if (OpenerFailStates(ref actionID))
                {
                    CurrentState = OpenerState.FailedOpener;
                    return false;
                }
            }

            if (!InCombat())
            {
                ResetOpener();
                CurrentState = OpenerState.OpenerNotReady;
            }

            return false;
        }

        public void ResetOpener()
        {
            OpenerStep = 0;
            PrePullStep = 0;
        }

        public void Dispose()
        {
            ActionWatching.OnLastActionChange -= OnLastActionChange;
        }
    }
}
