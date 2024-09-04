using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Internal.Types.Manifest;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Linq;
using XIVSlothCombo.Combos;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Extensions;
using XIVSlothCombo.Services;
using XIVSlothCombo.Window.Functions;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace XIVSlothCombo.AutoRotation
{
    internal unsafe static class AutoRotationController
    {
        internal static void Run()
        {
            if (!Service.Configuration.RotationConfig.Enabled || !Player.Available || (Service.Configuration.RotationConfig.InCombatOnly && !CustomComboFunctions.InCombat()))
                return;

            if (!EzThrottler.Throttle("AutoRotController", 750))
                return;

            var c = 0;
            foreach (var preset in Service.Configuration.AutoActions.OrderByDescending(x => Presets.Attributes[x.Key].AutoAction.IsHeal)
                                                                    .ThenByDescending(x => Presets.Attributes[x.Key].AutoAction.IsAoE))
            {
                if (!CustomComboFunctions.IsEnabled(preset.Key) || !preset.Value) continue;

                var attributes = Presets.Attributes[preset.Key];
                var action = attributes.AutoAction;
                var gameAct = attributes.ReplaceSkill.ActionIDs.First();
                var sheetAct = Svc.Data.GetExcelSheet<Action>().GetRow(gameAct);
                if ((byte)Player.Job != attributes.CustomComboInfo.JobID)
                    continue;
                
                var outAct = AutoRotationHelper.InvokeCombo(preset.Key, attributes);
                var healTarget = AutoRotationHelper.GetSingleTarget(Service.Configuration.RotationConfig.HealerRotationMode);
                var aoeHeal = HealerTargeting.GetPartyMax(outAct, out int count) <= Service.Configuration.RotationConfig.HealerSettings.AoETargetHPP && count >= 2;
                if (action.IsHeal)
                {
                    AutomateHealing(preset.Key, attributes, gameAct);
                    continue;
                }

                //if (Player.Object.GetRole() is CombatRole.Tank)
                //{
                //    AutomateTanking(preset.Key, attributes, gameAct);
                //    continue;
                //}

                if (healTarget == null && !aoeHeal)
                    AutomateDPS(preset.Key, attributes, gameAct);
            }


        }

        private static bool AutomateDPS(CustomComboPreset preset, Presets.PresetAttributes attributes, uint gameAct)
        {
            var mode = Service.Configuration.RotationConfig.DPSRotationMode;
            if (attributes.AutoAction.IsAoE)
            {
                return AutoRotationHelper.ExecuteAoE(preset, attributes, gameAct);
            }
            else
            {
                return AutoRotationHelper.ExecuteST(mode, preset, attributes, gameAct);
            }
        }

        private static void AutomateTanking(CustomComboPreset key, Presets.PresetAttributes attributes, uint gameAct)
        {
            if (attributes.AutoAction.IsAoE)
            {

            }
            else
            {

            }
        }

        private static bool AutomateHealing(CustomComboPreset preset, Presets.PresetAttributes attributes, uint gameAct)
        {
            if (attributes.AutoAction.IsAoE)
            {
                return AutoRotationHelper.ExecuteAoE(preset, attributes, gameAct);
            }
            else
            {
                var mode = Service.Configuration.RotationConfig.HealerRotationMode;
                return AutoRotationHelper.ExecuteST(mode, preset, attributes, gameAct);
            }
        }

        public static class AutoRotationHelper
        {
            public static IGameObject? GetSingleTarget(System.Enum rotationMode)
            {
                if (rotationMode is DPSRotationMode dpsmode)
                {
                    IGameObject? target = dpsmode switch
                    {
                        DPSRotationMode.Manual => Svc.Targets.Target,
                        DPSRotationMode.Highest_Max => DPSTargeting.GetHighestMaxTarget(),
                        DPSRotationMode.Lowest_Max => DPSTargeting.GetLowestMaxTarget(),
                        DPSRotationMode.Highest_Current => DPSTargeting.GetHighestCurrentTarget(),
                        DPSRotationMode.Lowest_Current => DPSTargeting.GetLowestCurrentTarget(),
                        DPSRotationMode.Tank_Target => DPSTargeting.GetTankTarget(),
                    };
                    return target;
                }
                if (rotationMode is HealerRotationMode healermode)
                {
                    if (Player.Object.GetRole() != CombatRole.Healer) return null;
                    IGameObject? target = healermode switch
                    {
                        HealerRotationMode.Manual => Svc.Targets.Target,
                        HealerRotationMode.Highest_Current => HealerTargeting.GetHighestCurrent(),
                        HealerRotationMode.Lowest_Current => HealerTargeting.GetLowestCurrent(),
                    };

                    return target;
                }
                if (rotationMode is TankRotationMode tankmode)
                {

                }

                return null;
            }

            public static bool ExecuteAoE(CustomComboPreset preset, Presets.PresetAttributes attributes, uint gameAct)
            {
                if (attributes.AutoAction.IsHeal)
                {
                    uint outAct = InvokeCombo(preset, attributes, Player.Object);

                    if (HealerTargeting.GetPartyMax(outAct, out int count) <= Service.Configuration.RotationConfig.HealerSettings.AoETargetHPP && count >= 2)
                    {
                        var castTime = ActionManager.GetAdjustedCastTime(ActionType.Action, outAct);
                        if (CustomComboFunctions.IsMoving && castTime > 0)
                            return false;

                        ActionManager.Instance()->UseAction(ActionType.Action, outAct);
                        return true;
                    }
                }
                else
                {
                    uint outAct = InvokeCombo(preset, attributes, Player.Object);
                    var target = GetSingleTarget(Service.Configuration.RotationConfig.DPSRotationMode);
                    var mustTarget = Svc.Data.GetExcelSheet<Action>().GetRow(outAct).CanTargetHostile;

                    if (CustomComboFunctions.NumberOfEnemiesInRange(outAct, target) >= Service.Configuration.RotationConfig.DPSAoETargets)
                    {
                        var castTime = ActionManager.GetAdjustedCastTime(ActionType.Action, outAct);
                        if (CustomComboFunctions.IsMoving && castTime > 0)
                            return false;

                        if (mustTarget)
                            Svc.Targets.Target = target;

                        ActionManager.Instance()->UseAction(ActionType.Action, outAct);
                        return true;
                    }
                }
                return false;
            }

            public static bool ExecuteST(Enum mode, CustomComboPreset preset, Presets.PresetAttributes attributes, uint gameAct)
            {
                var target = GetSingleTarget(mode);
                if (target is null)
                    return false;

                var outAct = InvokeCombo(preset, attributes, target);

                var castTime = ActionManager.GetAdjustedCastTime(ActionType.Action, outAct);
                if (CustomComboFunctions.IsMoving && castTime > 0)
                    return false;

                var inRange = ActionManager.GetActionInRangeOrLoS(outAct, Player.GameObject, target.Struct()) != 562;
                var canUseTarget = ActionManager.CanUseActionOnTarget(outAct, target.Struct());
                var canUseSelf = ActionManager.CanUseActionOnTarget(outAct, Player.GameObject);
                var canUse = canUseSelf || canUseTarget;
                if (canUse)
                {
                    Svc.Targets.Target = target;
                    ActionManager.Instance()->UseAction(ActionType.Action, outAct, canUseTarget ? target.GameObjectId : Player.Object.GameObjectId);
                    return true;
                }

                return false;
            }

            public static uint InvokeCombo(CustomComboPreset preset, Presets.PresetAttributes attributes, IGameObject? optionalTarget = null)
            {
                var outAct = attributes.ReplaceSkill.ActionIDs.FirstOrDefault();
                foreach (var actToCheck in attributes.ReplaceSkill.ActionIDs)
                {
                    var customCombo = Service.IconReplacer.CustomCombos.FirstOrDefault(x => x.Preset == preset);
                    if (customCombo != null)
                    {
                        if (customCombo.TryInvoke(actToCheck, (byte)Player.Level, ActionManager.Instance()->Combo.Action, ActionManager.Instance()->Combo.Timer, out var changedAct, optionalTarget))
                        {
                            outAct = changedAct;
                            break;
                        }
                    }
                }

                return outAct;
            }
        }

        public static class DPSTargeting
        {
            public static IGameObject? GetTankTarget()
            {
                var tank = CustomComboFunctions.GetPartyMembers().Where(x => x.GetRole() == CombatRole.Tank).FirstOrDefault();
                if (tank == null)
                    return null;

                return tank.TargetObject;
            }

            public static IGameObject? GetLowestCurrentTarget()
            {
                return Svc.Objects.Where(x => x is IBattleChara && x.IsHostile() && CustomComboFunctions.IsInRange(x) && !x.IsDead && x.IsTargetable).OrderBy(x => (x as IBattleChara).CurrentHp).FirstOrDefault();
            }

            public static IGameObject? GetHighestCurrentTarget()
            {
                return Svc.Objects.Where(x => x is IBattleChara && x.IsHostile() && CustomComboFunctions.IsInRange(x) && !x.IsDead && x.IsTargetable).OrderByDescending(x => (x as IBattleChara).CurrentHp).FirstOrDefault();
            }

            public static IGameObject? GetLowestMaxTarget()
            {
                return Svc.Objects.Where(x => x is IBattleChara && x.IsHostile() && CustomComboFunctions.IsInRange(x) && !x.IsDead && x.IsTargetable).OrderBy(x => (x as IBattleChara).MaxHp).ThenBy(x => CustomComboFunctions.GetTargetHPPercent(x)).FirstOrDefault();
            }

            public static IGameObject? GetHighestMaxTarget()
            {
                return Svc.Objects.Where(x => x is IBattleChara && x.IsHostile() && CustomComboFunctions.IsInRange(x) && !x.IsDead && x.IsTargetable).OrderByDescending(x => (x as IBattleChara).MaxHp).ThenBy(x => CustomComboFunctions.GetTargetHPPercent(x)).FirstOrDefault();
            }
        }

        public static class HealerTargeting
        {
            internal static IGameObject? GetHighestCurrent()
            {
                if (CustomComboFunctions.GetPartyMembers().Count == 0) return Player.Object;
                var target = CustomComboFunctions.GetPartyMembers().Where(x => CustomComboFunctions.GetTargetHPPercent(x) <= Service.Configuration.RotationConfig.HealerSettings.SingleTargetHPP).OrderByDescending(x => CustomComboFunctions.GetTargetHPPercent(x)).FirstOrDefault();
                return target;
            }

            internal static IGameObject? GetLowestCurrent()
            {
                if (CustomComboFunctions.GetPartyMembers().Count == 0) return Player.Object;
                var target = CustomComboFunctions.GetPartyMembers().Where(x => CustomComboFunctions.GetTargetHPPercent(x) <= Service.Configuration.RotationConfig.HealerSettings.SingleTargetHPP).OrderBy(x => CustomComboFunctions.GetTargetHPPercent(x)).FirstOrDefault();
                return target;
            }

            internal static float GetPartyMax(uint outAct, out int count)
            {
                var members = CustomComboFunctions.GetPartyMembers().Where(x => CustomComboFunctions.InActionRange(outAct, x));
                count = members.Count();
                if (count == 0) return 0;
                var avg = members.Average(x => CustomComboFunctions.GetTargetHPPercent(x));
                return avg;
            }
        }
    }
}
