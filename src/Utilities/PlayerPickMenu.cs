using UnityEngine;
using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Sentry.Internal.Extensions;

namespace MalumMenu;

public static class PlayerPickMenu
{
    public static ShapeshifterMinigame playerpickMenu;
    public static bool IsActive;
    public static NetworkedPlayerInfo targetPlayerData;
    public static Il2CppSystem.Action customAction;
    public static List<NetworkedPlayerInfo> customPlayerList;

    
    public static ShapeshifterMinigame getShapeshifterMenu()
    {
        var rolePrefab = Utils.getBehaviourByRoleType(RoleTypes.Shapeshifter);
        if (rolePrefab == null) return null;

        return rolePrefab.Cast<ShapeshifterRole>().ShapeshifterMenu;
    }

    public static void openPlayerPickMenu(List<NetworkedPlayerInfo> playerList, Il2CppSystem.Action action)
    {
        IsActive = true;
        customPlayerList = playerList;
        customAction = action;

        var menuPrefab = getShapeshifterMenu();
        if (menuPrefab == null)
        {
            IsActive = false;
            return;
        }

        
        playerpickMenu = UnityEngine.Object.Instantiate(menuPrefab);

        
        
        if (DestroyableSingleton<HudManager>.InstanceExists)
        {
            playerpickMenu.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform, false);
        }
        else
        {
            playerpickMenu.transform.SetParent(Camera.main.transform, false);
        }

        
        playerpickMenu.transform.localPosition = Vector3.zero;
        playerpickMenu.transform.localScale = Vector3.one;

        playerpickMenu.Begin(null);
    }

    public static NetworkedPlayerInfo customPPMChoice(string name, NetworkedPlayerInfo.PlayerOutfit outfit, RoleBehaviour role = null)
    {
        
        NetworkedPlayerInfo customChoice = UnityEngine.Object.Instantiate<NetworkedPlayerInfo>(GameData.Instance.PlayerInfoPrefab);

        outfit.PlayerName = name;

        customChoice.Outfits[PlayerOutfitType.Default] = outfit;

        if (!role.IsNull())
        {
            customChoice.Role = role;
        }

        return customChoice;
    }
}
