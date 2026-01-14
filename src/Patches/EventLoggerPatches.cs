using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
public static class PlayerPhysics_RpcEnterVent_EventLogger
{
    public static void Postfix(PlayerPhysics __instance, int ventId)
    {
        var pc = __instance.myPlayer;
        if (!pc || pc.Data == null) return;
        if (!Utils.isShip) return;

        byte playerId = pc.PlayerId;
        string ventName = GetCleanVentName(ventId);
        string playerName = ConsoleUI.GetColoredName(pc.Data);

        if (CheatToggles.eventLogVents)
        {
            MalumMenu.consoleUI?.RegisterEnterVent(playerId, playerName, ventId, ventName);
        }

        RadarUI.Instance?.RegisterEvent(RadarUI.RadarEventType.VentIn, pc.GetTruePosition(), Color.gray, playerId);
    }

    public static string GetCleanVentName(int ventId)
    {
        if (!ShipStatus.Instance) return $"Vent {ventId}";

        foreach (var v in ShipStatus.Instance.AllVents)
        {
            if (v.Id == ventId)
            {
                string name = v.name;
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Replace("Vent", "", StringComparison.OrdinalIgnoreCase)
                               .Replace("_", " ")
                               .Trim();

                    while (name.Length > 0 && char.IsDigit(name[name.Length - 1]))
                        name = name.Substring(0, name.Length - 1).Trim();

                    if (!string.IsNullOrEmpty(name))
                        return $"Vent ({name})";
                }
            }
        }
        return $"Vent {ventId}";
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
public static class PlayerPhysics_RpcExitVent_EventLogger
{
    public static void Postfix(PlayerPhysics __instance, int ventId)
    {
        var pc = __instance.myPlayer;
        if (!pc || pc.Data == null) return;
        if (!Utils.isShip) return;

        byte playerId = pc.PlayerId;

        string ventName = PlayerPhysics_RpcEnterVent_EventLogger.GetCleanVentName(ventId);

        if (CheatToggles.eventLogVents)
        {
            MalumMenu.consoleUI?.RegisterExitVent(playerId, ventId, ventName);
        }

        RadarUI.Instance?.RegisterEvent(RadarUI.RadarEventType.VentOut, pc.GetTruePosition(), Color.gray, playerId);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_EventLogger
{
    public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        if (__instance == null || target == null) return;
        if (__instance.Data == null || target.Data == null) return;

        if ((resultFlags & MurderResultFlags.Succeeded) == 0) return;
        if (CheatToggles.eventLogKills)
        {
            string killerName = ConsoleUI.GetColoredName(__instance.Data);
            string victimName = ConsoleUI.GetColoredName(target.Data);

            MalumMenu.consoleUI?.Log($"<color=#FF0000>☠</color> {killerName} killed {victimName}");
        }

        RadarUI.Instance?.RegisterEvent(RadarUI.RadarEventType.Kill, target.GetTruePosition(), Color.red, target.PlayerId);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class PlayerControl_ReportDeadBody_EventLogger
{
    public static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        if (__instance == null || __instance.Data == null) return;

        string reporterName = ConsoleUI.GetColoredName(__instance.Data);

        if (target == null)
        {
            if (CheatToggles.eventLogBodyReports) MalumMenu.consoleUI?.Log($"<color=#FF4500>🚨</color> {reporterName} called an emergency meeting");
        }
        else
        {
            if (CheatToggles.eventLogBodyReports)
            {
                string victimName = ConsoleUI.GetColoredName(target);
                MalumMenu.consoleUI?.Log($"<color=#FF4500>🚨</color> {reporterName} reported {victimName}'s body");
            }
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class MeetingHud_BloopAVoteIcon_EventLogger
{
    private static readonly HashSet<(byte voter, byte target)> loggedVotes = new();

    public static void Postfix(MeetingHud __instance, NetworkedPlayerInfo voterPlayer, int index, UnityEngine.Transform parent)
    {
        if (voterPlayer == null) return;

        byte voterId = voterPlayer.PlayerId;
        byte targetId = 253;

        if (__instance.SkippedVoting != null && parent == __instance.SkippedVoting.transform)
        {
            targetId = 253;
        }
        else
        {
            foreach (var pva in __instance.playerStates)
            {
                if (pva == null) continue;
                if (pva.transform == parent)
                {
                    targetId = pva.TargetPlayerId;
                    break;
                }
            }
        }

        var voteKey = (voterId, targetId);
        if (loggedVotes.Contains(voteKey)) return;
        loggedVotes.Add(voteKey);

        if (!CheatToggles.eventLogVotes) return;
        
        string voterName = ConsoleUI.GetColoredName(voterPlayer);

        if (targetId == 253)
        {
            MalumMenu.consoleUI?.Log($"<color=#40E0D0>🗳</color> {voterName} skipped vote");
        }
        else
        {
            var targetData = GameData.Instance?.GetPlayerById(targetId);
            if (targetData != null)
            {
                string targetName = ConsoleUI.GetColoredName(targetData);
                MalumMenu.consoleUI?.Log($"<color=#40E0D0>🗳</color> {voterName} voted for {targetName}");
            }
        }
    }

    public static void ClearLoggedVotes() => loggedVotes.Clear();
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHud_Close_EventLogger
{
    public static void Postfix()
    {
        MeetingHud_BloopAVoteIcon_EventLogger.ClearLoggedVotes();
        if (CheatToggles.eventLogger)
            MalumMenu.consoleUI?.Log("<color=#808080>--- Meeting Ended ---</color>");

        RadarUI.Instance?.OnMeetingEnd();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(byte))]
public static class ShipStatus_RpcUpdateSystem_EventLogger
{


    private static bool IsSystemActive(ISystemType system)
    {
        if (system == null) return false;

        var switchSys = system.TryCast<SwitchSystem>();
        if (switchSys != null) return switchSys.IsActive;

        var reactorSys = system.TryCast<ReactorSystemType>();
        if (reactorSys != null) return reactorSys.IsActive;

        var lifeSys = system.TryCast<LifeSuppSystemType>();
        if (lifeSys != null) return lifeSys.IsActive;

        var hudSys = system.TryCast<HudOverrideSystemType>();
        if (hudSys != null) return hudSys.IsActive;

        var hqSys = system.TryCast<HqHudSystemType>();
        if (hqSys != null) return hqSys.IsActive;

        var heliSys = system.TryCast<HeliSabotageSystem>();
        if (heliSys != null) return heliSys.IsActive;

        return false;
    }

    public static void Prefix(ShipStatus __instance, SystemTypes systemType, out bool __state)
    {
        __state = false;
        if (!CheatToggles.eventLogger) return;

        if (__instance.Systems.ContainsKey(systemType))
        {
            var system = __instance.Systems[systemType];
            __state = IsSystemActive(system);
        }
    }

    public static void Postfix(ShipStatus __instance, SystemTypes systemType, byte amount, bool __state)
    {
        if (!CheatToggles.eventLogger) return;
        if (!CheatToggles.eventLogSabotage) return;

        bool wasActive = __state;
        bool isActive = false;
        string sysName = systemType.ToString();

        if (__instance.Systems.ContainsKey(systemType))
        {
            var system = __instance.Systems[systemType];
            isActive = IsSystemActive(system);
        }

        if (wasActive == isActive) return;

        string icon = "<color=#FFA500>🔧</color>";

        if (!wasActive && isActive)
        {
            MalumMenu.consoleUI?.Log($"{icon} Sabotage started: {sysName}");
        }
        else if (wasActive && !isActive)
        {
            MalumMenu.consoleUI?.Log($"{icon} Sabotage fixed: {sysName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class PlayerControl_CompleteTask_EventLogger
{
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (__instance == null || __instance.Data == null) return;
        if (CheatToggles.eventLogTasks)
        {
            string playerName = ConsoleUI.GetColoredName(__instance.Data);
            string taskName = "Task";

            foreach (var task in __instance.myTasks)
            {
                if (task.Id == idx)
                {
                    taskName = task.TaskType.ToString();
                    break;
                }
            }

            MalumMenu.consoleUI?.Log($"<color=#00FF00>✅</color> {playerName} completed {taskName}");
        }

        RadarUI.Instance?.RegisterEvent(RadarUI.RadarEventType.Task, __instance.GetTruePosition(), Color.green, __instance.PlayerId);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class AmongUsClient_OnPlayerLeft_EventLogger
{
    public static void Prefix(ClientData data, DisconnectReasons reason)
    {
        if (data == null || data.Character == null || data.Character.Data == null) return;
        if (!CheatToggles.eventLogDisconnects) return;

        string playerName = ConsoleUI.GetColoredName(data.Character.Data);
        MalumMenu.consoleUI?.Log($"<color=#808080>🚪</color> {playerName} disconnected ({reason})");
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift_EventLogger
{
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        try
        {
            if (__instance == null || __instance.Data == null) return;
            if (CheatToggles.eventLogShapeshift)
            {
                string shifterName = ConsoleUI.GetColoredName(__instance.Data);

                
                if (targetPlayer != null && targetPlayer.Data != null && targetPlayer.PlayerId != __instance.PlayerId)
                {
                    string targetName = ConsoleUI.GetColoredName(targetPlayer.Data);
                    MalumMenu.consoleUI?.Log($"<color=#FF8C00>🎭</color> {shifterName} shapeshifted into {targetName}");
                }
                else
                {
                    MalumMenu.consoleUI?.Log($"<color=#FF8C00>🎭</color> {shifterName} reverted to original form");
                }
            }

            if (targetPlayer != null && targetPlayer.Data != null && targetPlayer.PlayerId != __instance.PlayerId)
            {
                
                Color targetColor = Color.gray;
                int colorId = targetPlayer.Data.DefaultOutfit.ColorId;
                if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
                {
                    Color32 c = Palette.PlayerColors[colorId];
                    targetColor = new Color(c.r / 255f, c.g / 255f, c.b / 255f, 1f);
                }
                RadarUI.Instance?.RegisterEvent(RadarUI.RadarEventType.Shapeshift, __instance.GetTruePosition(), targetColor, __instance.PlayerId);
            }
            else
            {
                
                Color originalColor = Color.gray;
                int colorId = __instance.Data.DefaultOutfit.ColorId;
                if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
                {
                    Color32 c = Palette.PlayerColors[colorId];
                    originalColor = new Color(c.r / 255f, c.g / 255f, c.b / 255f, 1f);
                }
                RadarUI.Instance?.RegisterEvent(RadarUI.RadarEventType.Shapeshift, __instance.GetTruePosition(), originalColor, __instance.PlayerId);
            }
        }
        catch (Exception)
        {
            
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcProtectPlayer))]
public static class PlayerControl_RpcProtectPlayer_EventLogger
{
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.eventLogger) return;
        if (!CheatToggles.eventLogProtect) return;
        if (__instance == null || __instance.Data == null || target == null || target.Data == null) return;

        string angelName = ConsoleUI.GetColoredName(__instance.Data);
        string targetName = ConsoleUI.GetColoredName(target.Data);

        MalumMenu.consoleUI?.Log($"<color=#00FFFF>🛡</color> {angelName} protected {targetName}");
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetScanner))]
public static class PlayerControl_RpcSetScanner_EventLogger
{
    public static void Postfix(PlayerControl __instance, bool value)
    {
        if (!CheatToggles.eventLogger) return;
        if (!CheatToggles.eventLogScanner) return;
        if (__instance == null || __instance.Data == null) return;

        if (value)
        {
            string playerName = ConsoleUI.GetColoredName(__instance.Data);
            MalumMenu.consoleUI?.Log($"<color=#00FF00>🏥</color> {playerName} started MedBay Scan");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
public static class PlayerControl_RpcSetRole_EventLogger
{
    public static void Postfix(PlayerControl __instance, RoleTypes roleType)
    {
        if (!CheatToggles.eventLogger) return;
        if (!CheatToggles.eventLogRoles) return;
        if (__instance == null || __instance.Data == null) return;

        string playerName = ConsoleUI.GetColoredName(__instance.Data);
        MalumMenu.consoleUI?.Log($"<color=#DA70D6>🎲</color> {playerName}'s role changed to {roleType}");
    }
}
