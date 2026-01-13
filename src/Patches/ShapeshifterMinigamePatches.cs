using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
public static class ShapeshifterMinigame_Begin
{
    
    
    
    
    
    public static bool Prefix(ShapeshifterMinigame __instance)
    {
        if (!PlayerPickMenu.IsActive) return true; 
        

        
        List<NetworkedPlayerInfo> list = PlayerPickMenu.customPlayerList;

        __instance.potentialVictims = new List<ShapeshifterPanel>();
        List<UiElement> list2 = new List<UiElement>();

        for (int i = 0; i < list.Count; i++)
        {
            NetworkedPlayerInfo playerData = list[i];
            int num = i % 3;
            int num2 = i / 3;
            ShapeshifterPanel shapeshifterPanel = Object.Instantiate(__instance.PanelPrefab, __instance.transform);
            shapeshifterPanel.transform.localPosition = new Vector3(__instance.XStart + num * __instance.XOffset, __instance.YStart + num2 * __instance.YOffset, -1f);

            shapeshifterPanel.SetPlayer(i, playerData, (Il2CppSystem.Action) (() =>
            {
                PlayerPickMenu.targetPlayerData = playerData; 

                PlayerPickMenu.customAction.Invoke(); 

                __instance.Close();
            }));

            if (playerData.Object != null){
                shapeshifterPanel.NameText.text = Utils.GetNameTag(playerData, playerData.DefaultOutfit.PlayerName);

                
                if (CheatToggles.seeRoles && CheatToggles.showPlayerInfo)
                {
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.showPlayerInfo)
                {
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }
            __instance.potentialVictims.Add(shapeshifterPanel);
            list2.Add(shapeshifterPanel.Button);
        }

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, list2, false);

        PlayerPickMenu.IsActive = false;

        return false; 

    }
}

[HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
public static class ShapeshifterPanel_SetPlayer
{
    
    
    
    
    
    
    
    
    public static bool Prefix(ShapeshifterPanel __instance, int index, NetworkedPlayerInfo playerInfo, Il2CppSystem.Action onShift)
    {
        if (!PlayerPickMenu.IsActive) return true; 
        

        __instance.shapeshift = onShift;
        __instance.PlayerIcon.SetFlipX(false);
        __instance.PlayerIcon.ToggleName(false);
        SpriteRenderer[] componentsInChildren = __instance.GetComponentsInChildren<SpriteRenderer>();
        foreach (var t in componentsInChildren)
        {
            t.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
        }
        __instance.PlayerIcon.SetMaskLayer(index + 2);
        __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);
        __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);

        

        __instance.NameText.text = playerInfo.PlayerName;
        DataManager.Settings.Accessibility.OnColorBlindModeChanged += (Il2CppSystem.Action)__instance.SetColorblindText;
        __instance.SetColorblindText();

        return false; 

    }
}
