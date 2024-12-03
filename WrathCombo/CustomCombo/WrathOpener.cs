using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using System;
using WrathCombo.Combos.JobHelpers.Enums;
using WrathCombo.Data;
using WrathCombo.Services;

namespace WrathCombo.CustomComboNS
{
    public abstract class WrathOpener
    {
        private OpenerState currentState = OpenerState.OpenerNotReady;
        private int openerStep;
        private int prePullStep;

        public void ProgressOpener(uint actionId)
        {
            if (actionId == CurrentOpenerAction)
            {
                if (CurrentState == OpenerState.PrePull)
                {
                    PrePullStep++;
                    if (PrePullStep > PrePullStepCount)
                    {
                        CurrentState = OpenerState.InOpener;
                        OpenerStep = 1;
                    }

                    return;
                }
                if (CurrentState == OpenerState.InOpener)
                {
                    OpenerStep++;
                    if (OpenerStep > OpenerStepCount)
                        CurrentState = OpenerState.OpenerFinished;
                }
            }
        }

        public virtual OpenerState CurrentState
        {
            get => currentState;
            set
            {
                if (value != currentState)
                {
                    currentState = value;

                    if (value == OpenerState.OpenerNotReady)
                        Svc.Log.Debug($"Opener Not Ready");

                    if (value == OpenerState.PrePull)
                        if (Service.Configuration.OutputOpenerLogs)
                            DuoLog.Information("Entered Pre-Pull Opener");
                        else
                            Svc.Log.Debug($"Entered Pre-Pull Opener");

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

        public uint CurrentOpenerAction { get; set; }

        public abstract int OpenerLevel { get; }

        public bool LevelChecked => Player.Level >= OpenerLevel;

        public abstract bool HasCooldowns();

        public abstract bool PrePullSteps();

        public abstract bool Opener();

        public abstract bool OpenerFailStates();

        public abstract bool PrePullFailStates();

        public bool FullOpener(ref uint actionID)
        {
            if (!LevelChecked)
                return false;

            CurrentOpener = this;

            if (CurrentState == OpenerState.OpenerNotReady)
            {
                if (HasCooldowns())
                {
                    CurrentState = OpenerState.PrePull;
                    PrePullStep = 1;
                }
            }

            if (CurrentState == OpenerState.PrePull)
            {
                if (!HasCooldowns() && PrePullStep == 1)
                {
                    CurrentState = OpenerState.OpenerNotReady;
                    return false;
                }

                if (PrePullStep == 0)
                    PrePullStep = 1;

                if (PrePullSteps())
                {
                    actionID = CurrentOpenerAction;
                    return true;
                }

                if (PrePullFailStates())
                {
                    CurrentState = OpenerState.FailedOpener;
                    return false;
                }
            }

            if (CurrentState == OpenerState.InOpener)
            {
                if (Opener())
                {
                    actionID = CurrentOpenerAction;
                    return true;
                }

                if (OpenerFailStates())
                {
                    CurrentState = OpenerState.FailedOpener;
                    return false;
                }
            }

            return false;
        }

        public void ResetOpener()
        {
            Svc.Log.Debug($"Opener Reset");
            OpenerStep = 0;
            PrePullStep = 0;
            CurrentOpenerAction = 0;
            CurrentState = OpenerState.OpenerNotReady;
        }

        public static WrathOpener? CurrentOpener { get; set; }
    }
}
