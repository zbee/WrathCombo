using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using WrathCombo.Combos.JobHelpers.Enums;
using WrathCombo.Data;
using WrathCombo.Services;

namespace WrathCombo.CustomComboNS
{
    public abstract class WrathOpener : IDisposable
    {
        private OpenerState currentState = OpenerState.OpenerNotReady;
        private int openerStep;

        protected WrathOpener()
        {
            Svc.Framework.Update += UpdateOpener;
        }

        private void UpdateOpener(Dalamud.Plugin.Services.IFramework framework)
        {
            if (!Service.IconReplacer.getIconHook.IsEnabled)
            {
                uint _ = 0;
                FullOpener(ref _);
            }
        }

        public void ProgressOpener(uint actionId)
        {
            if (actionId == CurrentOpenerAction)
            {
                OpenerStep++;
                if (OpenerStep > OpenerActions.Count)
                    CurrentState = OpenerState.OpenerFinished;
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

                    if (value == OpenerState.OpenerReady)
                        if (Service.Configuration.OutputOpenerLogs)
                            DuoLog.Information("Opener Now Ready");
                        else
                            Svc.Log.Debug($"Opener Now Ready");

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

        public abstract List<uint> OpenerActions { get; protected set; }

        public uint CurrentOpenerAction { get; set; }

        public abstract int OpenerLevel { get; }

        public bool LevelChecked => Player.Level >= OpenerLevel;

        public abstract bool HasCooldowns();

        public bool FullOpener(ref uint actionID)
        {
            if (!LevelChecked)
                return false;

            CurrentOpener = this;

            if (CurrentState == OpenerState.OpenerNotReady)
            {
                if (HasCooldowns())
                {
                    CurrentState = OpenerState.OpenerReady;
                    OpenerStep = 1;
                }
            }

            if (CurrentState == OpenerState.OpenerReady)
            {
                if (!HasCooldowns() && OpenerStep == 1)
                {
                    ResetOpener();
                    return false;
                }

                if (OpenerStep > 1 && ActionWatching.TimeSinceLastAction.TotalSeconds >= 4)
                {
                    CurrentState = OpenerState.FailedOpener;
                    return false;
                }

                actionID = CurrentOpenerAction = OpenerActions[OpenerStep - 1];
                return true;
            }

            return false;
        }

        public void ResetOpener()
        {
            Svc.Log.Debug($"Opener Reset");
            OpenerStep = 0;
            CurrentOpenerAction = 0;
            CurrentState = OpenerState.OpenerNotReady;
        }

        public void Dispose()
        {
            Svc.Framework.Update -= UpdateOpener;
        }

        public static WrathOpener? CurrentOpener { get; set; }
    }
}
