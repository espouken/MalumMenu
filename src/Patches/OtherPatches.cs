using HarmonyLib;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;
using InnerNet;

namespace MalumMenu;

[HarmonyPatch(typeof(PlatformSpecificData), nameof(PlatformSpecificData.Serialize))]
public static class PlatformSpecificData_Serialize
{
    public static void Prefix(PlatformSpecificData __instance)
    {
        MalumSpoof.spoofPlatform(__instance);
    }
}

[HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
public static class FreeChatInputField_UpdateCharCount
{
    public static void Postfix(FreeChatInputField __instance)
    {
        if (!CheatToggles.chatJailbreak)
        {
            return;
        }

        int length = __instance.textArea.text.Length;
        __instance.charCountText.SetText($"{length}/{__instance.textArea.characterLimit}");

        if (length < 90)
        {
            __instance.charCountText.color = Color.black;
        }
        else if (length < 119)
        {
            __instance.charCountText.color = new Color(1f, 1f, 0f, 1f);
        }
        else
        {
            __instance.charCountText.color = Color.red;
        }
    }
}

[HarmonyPatch(typeof(SystemInfo), nameof(SystemInfo.deviceUniqueIdentifier), MethodType.Getter)]
public static class SystemInfo_deviceUniqueIdentifier_Getter
{
    public static void Postfix(ref string __result)
    {
        if (!MalumMenu.spoofDeviceId.Value) return;
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        __result = BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    public static void Postfix()
    {
        MalumSpoof.spoofLevel();

        if (!EOSManager.Instance.loginFlowFinished || !MalumMenu.guestMode.Value) return;
        DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;

        if (!string.IsNullOrWhiteSpace(EOSManager.Instance.FriendCode)) return;
        var friendCode = MalumSpoof.spoofFriendCode();
        var editUsername = EOSManager.Instance.editAccountUsername;
        editUsername.UsernameText.SetText(friendCode);
        editUsername.SaveUsername();
        EOSManager.Instance.FriendCode = friendCode;
    }
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShower_Start
{
    public static void Postfix(VersionShower __instance)
    {
        if (MalumMenu.supportedAU.Contains(Application.version))
        {
            __instance.text.text = $"MalumMenu v{MalumMenu.malumVersion} (v{Application.version})";
        }
        else
        {
            __instance.text.text = $"MalumMenu v{MalumMenu.malumVersion} (<color=red>v{Application.version}</color>)";
        }
    }
}

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    public static void Postfix(PingTracker __instance)
    {
        __instance.text.alignment = TMPro.TextAlignmentOptions.Center;

        if (AmongUsClient.Instance.IsGameStarted)
        {
            __instance.aspectPosition.DistanceFromEdge = new Vector3(-0.21f, 0.50f, 0f);
            __instance.text.text = $"MalumMenu by scp222thj & Astral ~ {Utils.getColoredPingText(AmongUsClient.Instance.Ping)}";
            return;
        }

        __instance.text.text = $"MalumMenu by scp222thj & Astral\n{Utils.getColoredPingText(AmongUsClient.Instance.Ping)}";
    }
}

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class HatManager_Initialize
{
    public static void Postfix(HatManager __instance)
    {
        CosmeticsUnlocker.unlockCosmetics(__instance);
    }
}

[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
public static class PlayerBanData_BanMinutesLeft_Getter
{
    public static void Postfix(PlayerBanData __instance, ref int __result)
    {
        if (!CheatToggles.avoidBans) return;
        __instance.BanPoints = 0f;
        __result = 0;
    }
}

[HarmonyPatch(typeof(FullAccount), nameof(FullAccount.CanSetCustomName))]
public static class FullAccount_CanSetCustomName
{
    public static void Prefix(ref bool canSetName)
    {
        if (CheatToggles.unlockFeatures)
        {
            canSetName = true;
        }
    }
}

[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
public static class AccountManager_CanPlayOnline
{
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.unlockFeatures)
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
public static class InnerNetClient_JoinGame
{
    public static void Prefix()
    {
        if (CheatToggles.unlockFeatures)
        {
            DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
        }
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
public static class GameManager_CheckTaskCompletion
{
    public static bool Prefix(ref bool __result)
    {
        if (!CheatToggles.noGameEnd) return true;
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
public static class Mushroom_FixedUpdate
{
    public static void Postfix(Mushroom __instance)
    {
        MalumESP.sporeCloudVision(__instance);
    }
}


[HarmonyPatch(typeof(DoorBreakerGame), nameof(DoorBreakerGame.Start))]
public static class DoorBreakerGame_Start
{
    public static bool Prefix(DoorBreakerGame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        DoorsHandler.OpenDoor(__instance.MyDoor);
        __instance.MyDoor.SetDoorway(true);
        __instance.Close();
        return false;
    }
}

[HarmonyPatch(typeof(DoorCardSwipeGame), nameof(DoorCardSwipeGame.Begin))]
public static class DoorCardSwipeGame_Begin
{
    public static bool Prefix(DoorCardSwipeGame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        DoorsHandler.OpenDoor(__instance.MyDoor);
        __instance.MyDoor.SetDoorway(true);
        __instance.Close();
        return false;
    }
}

[HarmonyPatch(typeof(MushroomDoorSabotageMinigame), nameof(MushroomDoorSabotageMinigame.Begin))]
public static class MushroomDoorSabotageMinigame_Begin
{
    public static bool Prefix(MushroomDoorSabotageMinigame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        __instance.FixDoorAndCloseMinigame();
        return false;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class Vent_CanUse
{
    public static void Postfix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
        if (PlayerControl.LocalPlayer.Data.Role.CanVent || PlayerControl.LocalPlayer.Data.IsDead) return;
        if (!CheatToggles.useVents) return;
        var @object = pc.Object;

        var center = @object.Collider.bounds.center;
        var position = __instance.transform.position;
        var num = Vector2.Distance(center, position);


        canUse = num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        couldUse = true;
        __result = num;
    }
}

[HarmonyPatch(typeof(AmongUsDateTime), nameof(AmongUsDateTime.UtcNow), MethodType.Getter)]
public static class AmongUsDateTime_UtcNow
{
    public static bool Prefix(ref Il2CppSystem.DateTime __result)
    {
        if (!CheatToggles.spoofAprilFoolsDate) return true;

        var managedDate = new DateTime(DateTime.UtcNow.Year, 4, 2, 7, 1, 0, DateTimeKind.Utc);
        __result = new Il2CppSystem.DateTime(managedDate.Ticks);
        return false;
    }
}


[HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
public static class GameContainer_SetupGameInfo
{
    public static void Postfix(GameContainer __instance)
    {
        if (!CheatToggles.moreLobbyInfo) return;

        var trueHostName = __instance.gameListing.TrueHostName;

        const string separator = "<#0000>000000000000000</color>";

        var age = __instance.gameListing.Age;
        var lobbyTime = $"Age: {age / 60}:{(age % 60 < 10 ? "0" : "")}{age % 60}";

        var platformId = __instance.gameListing.Platform switch
        {
            Platforms.StandaloneEpicPC => "Epic",
            Platforms.StandaloneSteamPC => "Steam",
            Platforms.StandaloneMac => "Mac",
            Platforms.StandaloneWin10 => "Microsoft Store",
            Platforms.StandaloneItch => "Itch.io",
            Platforms.IPhone => "iPhone / iPad",
            Platforms.Android => "Android",
            Platforms.Switch => "Nintendo Switch",
            Platforms.Xbox => "Xbox",
            Platforms.Playstation => "PlayStation",
            _ => "Unknown"
        };

        __instance.capacity.text = $"<size=40%>{separator}\n{trueHostName}\n{__instance.capacity.text}\n" +
                                   $"<#fb0>{GameCode.IntToGameName(__instance.gameListing.GameId)}</color>\n" +
                                   $"<#b0f>{platformId}</color>\n{lobbyTime}\n{separator}</size>";
    }
}

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
public static class VoteBanSystem_AddVote
{
    public static bool Prefix(VoteBanSystem __instance, int srcClient, int clientId)
    {
        if (!Utils.isHost) return true;

        if (AmongUsClient.Instance.ClientId == srcClient)
        {
            AmongUsClient.Instance.KickPlayer(clientId, false);
        }
        return false;
    }
}

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
public static class VoteBanSystem_CmdAddVote
{
    public static bool Prefix(int clientIdToVoteBan)
    {
        return !Utils.isHost;
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
public static class BanMenu_SetVisible
{
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!Utils.isHost) return true;

        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;
        __instance.BanButton.gameObject.SetActive(true);
        __instance.KickButton.gameObject.SetActive(true);
        __instance.MenuButton.gameObject.SetActive(show);
        return false;
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
public static class IGameOptionsExtensions_GetAdjustedNumImpostors
{
    public static bool Prefix(IGameOptions __instance, int playerCount, ref int __result)
    {
        if (!CheatToggles.noOptionsLimits) return true;
        __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        return false;
    }
}

[HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
public static class SwitchMinigame_Begin
{
    public static bool Prefix(SwitchMinigame __instance)
    {
        if (!CheatToggles.autoFixLights) return true;

        var electricalSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();

        int mismatch = electricalSystem.ActualSwitches ^ electricalSystem.ExpectedSwitches;

        for (int i = 0; i < 5; i++)
        {
            if ((mismatch & (1 << i)) != 0)
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
            }
        }

        __instance.Close();

        return false;
    }
}


[HarmonyPatch(typeof(TuneRadioMinigame), nameof(TuneRadioMinigame.Begin))]
public static class TuneRadioMinigame_Begin
{
    public static bool Prefix(TuneRadioMinigame __instance)
    {
        if (!CheatToggles.autoFixComms) return true;

        // one of them works, idk which :sob:

        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 128);

        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 255);

        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 0);
        __instance.Close();
        return false;
    }
}
