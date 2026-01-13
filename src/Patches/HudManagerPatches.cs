using HarmonyLib;
using System;

namespace MalumMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManager_Start
{
	
	
	
	
	public static void Postfix(HudManager __instance)
	{
		__instance.MapButton.OnClick.RemoveAllListeners(); 

		
		
		__instance.MapButton.OnClick.AddListener((Action) (() =>
        {
			__instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.Normal
			});

		}));
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
	public static void Postfix(HudManager __instance)
    {
		__instance.ShadowQuad.gameObject.SetActive(!MalumESP.fullBrightActive()); 

		if (Utils.chatUiActive()){ 
			__instance.Chat.gameObject.SetActive(true);
		} else {
			Utils.closeChat();
			__instance.Chat.gameObject.SetActive(false);
		}

		MalumCheats.useVentCheat(__instance);
		MalumESP.zoomOut(__instance);
		MalumESP.freecamCheat();

		
		if (PlayerPickMenu.playerpickMenu != null && CheatToggles.shouldPPMClose()){
            PlayerPickMenu.playerpickMenu.Close();
        }
    }
}
