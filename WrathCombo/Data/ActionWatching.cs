﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WrathCombo.Combos.PvE;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Services;

namespace WrathCombo.Data
{
    public static class ActionWatching
    {
        internal static Dictionary<uint, Lumina.Excel.Sheets.Action> ActionSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()!
            .Where(i => i.RowId is not 7)
            .ToDictionary(i => i.RowId, i => i);

        internal static Dictionary<uint, Lumina.Excel.Sheets.Status> StatusSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>()!
            .ToDictionary(i => i.RowId, i => i);

        internal static Dictionary<uint, Trait> TraitSheet = Svc.Data.GetExcelSheet<Trait>()!
            .Where(i => i.ClassJobCategory.IsValid) //All player traits are assigned to a category. Chocobo and other garbage lacks this, thus excluded.
            .ToDictionary(i => i.RowId, i => i);

        private static readonly Dictionary<string, List<uint>> statusCache = [];

        internal static readonly Dictionary<uint, long> ChargeTimestamps = [];
        internal static readonly Dictionary<uint, long> ActionTimestamps = [];
        internal static readonly Dictionary<uint, long> LastSuccessfulUseTime = [];

        internal readonly static List<uint> CombatActions = [];

        private delegate void ReceiveActionEffectDelegate(ulong sourceObjectId, IntPtr sourceActor, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
        private readonly static Hook<ReceiveActionEffectDelegate>? ReceiveActionEffectHook;
        private static void ReceiveActionEffectDetour(ulong sourceObjectId, IntPtr sourceActor, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
        {
            if (!CustomComboFunctions.InCombat()) CombatActions.Clear();
            ReceiveActionEffectHook!.Original(sourceObjectId, sourceActor, position, effectHeader, effectArray, effectTrail);
            ActionEffectHeader header = Marshal.PtrToStructure<ActionEffectHeader>(effectHeader);
            
            if (ActionType is 13 or 2) return;
            if (header.ActionId != 7 &&
                header.ActionId != 8 &&
                sourceObjectId == Svc.ClientState.LocalPlayer.GameObjectId)
            {
                TimeLastActionUsed = DateTime.Now;
                LastActionUseCount++;
                if (header.ActionId != LastAction)
                {
                    LastActionUseCount = 1;
                }

                LastAction = header.ActionId;
                LastSuccessfulUseTime[LastAction] = Environment.TickCount64;

                if (ActionSheet.TryGetValue(header.ActionId, out var sheet))
                { 
                    switch (sheet.ActionCategory.Value.RowId)
                    {
                        case 2: //Spell
                            LastSpell = header.ActionId;
                            break;
                        case 3: //Weaponskill
                            LastWeaponskill = header.ActionId;
                            break;
                        case 4: //Ability
                            LastAbility = header.ActionId;
                            break;
                    }
                }

                CombatActions.Add(header.ActionId);

                if (Service.Configuration.EnabledOutputLog)
                    OutputLog();
            }
        }

        private delegate void SendActionDelegate(ulong targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9);
        private static readonly Hook<SendActionDelegate>? SendActionHook;
        private unsafe static void SendActionDetour(ulong targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9)
        {
            try
            {
                if (actionType == 1 && CustomComboFunctions.GetMaxCharges(actionId) > 0)
                    ChargeTimestamps[actionId] = Environment.TickCount64;

                if (actionType == 1)
                    ActionTimestamps[actionId] = Environment.TickCount64;

                CheckForChangedTarget(actionId, ref targetObjectId);
                SendActionHook!.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
                TimeLastActionUsed = DateTime.Now;
                ActionType = actionType;

                UpdateHelpers(actionId);

                Svc.Log.Verbose($"{actionId} {sequence} {a5} {a6} {a7} {a8} {a9}");
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "SendActionDetour");
                SendActionHook!.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
            }
        }

        private static void UpdateHelpers(uint actionId)
        {
            if (actionId is NIN.Ten or NIN.Chi or NIN.Jin or NIN.TenCombo or NIN.ChiCombo or NIN.JinCombo)
                NIN.NINHelper.InMudra = true;
            else
                NIN.NINHelper.InMudra = false;
        }

        private unsafe static void CheckForChangedTarget(uint actionId, ref ulong targetObjectId)
        {
            if (actionId is AST.Balance or AST.Spear &&
                AST.QuickTargetCards.SelectedRandomMember is not null &&
                !OutOfRange(actionId, Svc.ClientState.LocalPlayer!, AST.QuickTargetCards.SelectedRandomMember))
            {
                int targetOptions = AST.Config.AST_QuickTarget_Override;

                switch (targetOptions)
                {
                    case 0:
                        Svc.Log.Debug($"Switched to {AST.QuickTargetCards.SelectedRandomMember.Name}");
                        targetObjectId = AST.QuickTargetCards.SelectedRandomMember.GameObjectId;
                        break;
                    case 1:
                        if (CustomComboFunctions.HasFriendlyTarget())
                            targetObjectId = Svc.Targets.Target.GameObjectId;
                        else
                            targetObjectId = AST.QuickTargetCards.SelectedRandomMember.GameObjectId;
                        break;
                    case 2:
                        if (CustomComboFunctions.GetHealTarget(true, true) is not null)
                            targetObjectId = CustomComboFunctions.GetHealTarget(true, true).GameObjectId;
                        else
                            targetObjectId = AST.QuickTargetCards.SelectedRandomMember.GameObjectId;
                        break;
                }
            }
        }

        public static unsafe bool OutOfRange(uint actionId, IGameObject source, IGameObject target)
        {
            return ActionManager.GetActionInRangeOrLoS(actionId, source.Struct(), target.Struct()) is 566;
        }

        /// <summary>
        /// Returns the amount of time since an action was last used.
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns>Time in milliseconds if found, else -1.</returns>
        public static float TimeSinceActionUsed(uint actionId)
        {
            if (ActionTimestamps.ContainsKey(actionId))
                return Environment.TickCount64 - ActionTimestamps[actionId];

            return -1f;
        }

        public static float TimeSinceLastSuccessfulCast(uint actionId)
        {
            if (LastSuccessfulUseTime.ContainsKey(actionId))
                return Environment.TickCount64 - LastSuccessfulUseTime[actionId];

            return -1f;
        }

        public static uint WhichOfTheseActionsWasLast(params uint[] actions)
        {
            if (CombatActions.Count == 0) return 0;

            int currentLastIndex = 0;
            foreach (var action in actions)
            {
                if (CombatActions.Any(x => x == action))
                {
                    int index = CombatActions.LastIndexOf(action);

                    if (index > currentLastIndex) currentLastIndex = index;
                }
            }

            return CombatActions[currentLastIndex];
        }

        public static int HowManyTimesUsedAfterAnotherAction(uint lastUsedIDToCheck, uint idToCheckAgainst)
        {
            if (CombatActions.Count < 2) return 0;
            if (WhichOfTheseActionsWasLast(lastUsedIDToCheck, idToCheckAgainst) != lastUsedIDToCheck) return 0;

            int startingIndex = CombatActions.LastIndexOf(idToCheckAgainst);
            if (startingIndex == -1) return 0;

            int count = 0;
            for (int i = startingIndex + 1; i < CombatActions.Count; i++)
            {
                if (CombatActions[i] == lastUsedIDToCheck) count++;
            }

            return count;
        }

        public static bool HasDoubleWeaved()
        {
            if (CombatActions.Count < 2) return false;
            var lastAction = CombatActions.Last();
            var secondLastAction = CombatActions[^2];

            return (GetAttackType(lastAction) == GetAttackType(secondLastAction) && GetAttackType(lastAction) == ActionAttackType.Ability);
        }

        public static bool HasWeaved()
        {
            if (CombatActions.Count < 1) return false;
            var lastAction = CombatActions.Last();

            return GetAttackType(lastAction) == ActionAttackType.Ability;
        }

        public static int NumberOfGcdsUsed => CombatActions.Count(x => GetAttackType(x) == ActionAttackType.Weaponskill || GetAttackType(x) == ActionAttackType.Spell);
        public static uint LastAction { get; set; } = 0;
        public static int LastActionUseCount { get; set; } = 0;
        public static uint ActionType { get; set; } = 0;
        public static uint LastWeaponskill { get; set; } = 0;
        public static uint LastAbility { get; set; } = 0;
        public static uint LastSpell { get; set; } = 0;

        public static TimeSpan TimeSinceLastAction => DateTime.Now - TimeLastActionUsed;

        private static DateTime TimeLastActionUsed { get; set; } = DateTime.Now;

        public static void OutputLog()
        {
            Svc.Chat.Print($"You just used: {GetActionName(LastAction)} x{LastActionUseCount}");
        }

        public static void Dispose()
        {
            ReceiveActionEffectHook?.Dispose();
            SendActionHook?.Dispose();
        }

        static unsafe ActionWatching()
        {
            ReceiveActionEffectHook ??= Svc.Hook.HookFromSignature<ReceiveActionEffectDelegate>("40 55 56 57 41 54 41 55 41 56 48 8D AC 24", ReceiveActionEffectDetour);
            SendActionHook ??= Svc.Hook.HookFromSignature<SendActionDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B E9 41 0F B7 D9", SendActionDetour);
        }


        public static void Enable()
        {
            ReceiveActionEffectHook?.Enable();
            SendActionHook?.Enable();
            Svc.Condition.ConditionChange += ResetActions;
        }

        private static void ResetActions(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.InCombat && !value)
            {
                CombatActions.Clear();
                ActionTimestamps.Clear();
                LastAbility = 0;
                LastAction = 0;
                LastWeaponskill = 0;
                LastSpell = 0;
            }
        }

        public static void Disable()
        {
            ReceiveActionEffectHook.Disable();
            SendActionHook?.Disable();
            Svc.Condition.ConditionChange -= ResetActions;
        }

        public static int GetLevel(uint id) => ActionSheet.TryGetValue(id, out var action) && action.ClassJobCategory.IsValid ? action.ClassJobLevel : 255;
        public static float GetActionCastTime(uint id) => ActionSheet.TryGetValue(id, out var action) ? action.Cast100ms / (float)10 : 0;
        public unsafe static int GetActionRange(uint id) => (int)ActionManager.GetActionRange(id);
        public static int GetActionEffectRange(uint id) => ActionSheet.TryGetValue(id, out var action) ? action.EffectRange : -1;
        public static int GetTraitLevel(uint id) => TraitSheet.TryGetValue(id, out var trait) ? trait.Level : 255;
        public static string GetActionName(uint id) => ActionSheet.TryGetValue(id, out var action) ? action.Name.ToString() : "UNKNOWN ABILITY";

        public static string GetBLUIndex(uint id)
        {
            var aozKey = Svc.Data.GetExcelSheet<AozAction>()!.First(x => x.Action.RowId == id).RowId;
            var index = Svc.Data.GetExcelSheet<AozActionTransient>().GetRow(aozKey).Number;

            return $"#{index} ";
        }
        public static string GetStatusName(uint id) => StatusSheet.TryGetValue(id, out var status) ? status.Name.ToString() : "Unknown Status";

        public static List<uint>? GetStatusesByName(string status)
        {
            if (statusCache.TryGetValue(status, out List<uint>? list))
                return list;

            return statusCache.TryAdd(status, StatusSheet.Where(x => x.Value.Name.ToString().Equals(status, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Key).ToList())
                ? statusCache[status]
                : null;

        }

        public static ActionAttackType GetAttackType(uint id)
        {
            if (!ActionSheet.TryGetValue(id, out var action)) return ActionAttackType.Unknown;

            return action.ActionCategory.RowId switch
            {
                2 => ActionAttackType.Spell,
                3 => ActionAttackType.Weaponskill,
                4 => ActionAttackType.Ability,
                _ => ActionAttackType.Unknown
            };
        }

        public enum ActionAttackType
        {
            Ability,
            Spell,
            Weaponskill,
            Unknown
        }
    }

    internal unsafe static class ActionManagerHelper
    {
        private static readonly IntPtr actionMgrPtr;
        internal static IntPtr FpUseAction => (IntPtr)ActionManager.Addresses.UseAction.Value;
        internal static IntPtr FpUseActionLocation => (IntPtr)ActionManager.Addresses.UseActionLocation.Value;
        internal static IntPtr CheckActionResources => (IntPtr)ActionManager.Addresses.CheckActionResources.Value;
        public static ushort CurrentSeq => actionMgrPtr != IntPtr.Zero ? (ushort)Marshal.ReadInt16(actionMgrPtr + 0x110) : (ushort)0;
        public static ushort LastRecievedSeq => actionMgrPtr != IntPtr.Zero ? (ushort)Marshal.ReadInt16(actionMgrPtr + 0x112) : (ushort)0;
        public static bool IsCasting => actionMgrPtr != IntPtr.Zero && Marshal.ReadByte(actionMgrPtr + 0x28) != 0;
        public static uint CastingActionId => actionMgrPtr != IntPtr.Zero ? (uint)Marshal.ReadInt32(actionMgrPtr + 0x24) : 0u;
        public static uint CastTargetObjectId => actionMgrPtr != IntPtr.Zero ? (uint)Marshal.ReadInt32(actionMgrPtr + 0x38) : 0u;
        static ActionManagerHelper() => actionMgrPtr = (IntPtr)ActionManager.Instance();
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ActionEffectHeader
    {
        [FieldOffset(0x0)] public long TargetObjectId;
        [FieldOffset(0x8)] public uint ActionId;
        [FieldOffset(0x14)] public uint UnkObjectId;
        [FieldOffset(0x18)] public ushort Sequence;
        [FieldOffset(0x1A)] public ushort Unk_1A;
    }
}
