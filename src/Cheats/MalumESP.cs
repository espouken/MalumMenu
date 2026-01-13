using UnityEngine;
using Sentry.Internal.Extensions;

namespace MalumMenu;
public static class MalumESP
{
    public static bool freecamActive;
    public static bool resolutionchangeNeeded;
    public static void sporeCloudVision(Mushroom mushroom)
    {
        if (CheatToggles.fullBright)
        {
            

            mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, -1);
            return;
        }

        
        mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, 5f);
    }

    public static bool fullBrightActive()
    {
        
        

        return CheatToggles.fullBright || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }

    public static void zoomOut(HudManager hudManager)
    {
        if (CheatToggles.zoomOut)
        {
            if (hudManager.Chat.IsOpenOrOpening || PlayerCustomizationMenu.Instance || (Utils.isLobby && (FriendsListUI.Instance.IsOpen ||
                GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active || GameStartManager.Instance.RulesEditPanel))) return;

            resolutionchangeNeeded = true;

            if(Input.GetAxis("Mouse ScrollWheel") < 0f ){ 

                

                Camera.main.orthographicSize++;
                hudManager.UICamera.orthographicSize++;

                
                

                Utils.adjustResolution();

            } else if(Input.GetAxis("Mouse ScrollWheel") > 0f )
            {
                
                if (!(Camera.main.orthographicSize > 3f)) return; 

                Camera.main.orthographicSize--;
                hudManager.UICamera.orthographicSize--;

                Utils.adjustResolution();
            }
        } else {

            
            Camera.main.orthographicSize = 3f;
            hudManager.UICamera.orthographicSize = 3f;

            
            if (resolutionchangeNeeded){
                Utils.adjustResolution();
                resolutionchangeNeeded = false;
            }
        }
    }

    public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            foreach (var playerState in meetingHud.playerStates)
            {
                
                var data = GameData.Instance.GetPlayerById(playerState.TargetPlayerId);

                if (data.IsNull() || data.Disconnected || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                
                playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);

                
                if (CheatToggles.seeRoles && CheatToggles.showPlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    playerState.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.showPlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }
        }catch{}
    }

    public static void PlayerNametags(PlayerPhysics playerPhysics)
    {
        try
        {
            playerPhysics.myPlayer.cosmetics.SetName(Utils.GetNameTag(playerPhysics.myPlayer.Data, playerPhysics.myPlayer.CurrentOutfit.PlayerName));
            
            if (CheatToggles.seeRoles && CheatToggles.showPlayerInfo)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.186f, 0f);
            }
            else if (CheatToggles.seeRoles || CheatToggles.showPlayerInfo)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.093f, 0f);
            }
            else
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }catch{}
    }

    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            
            chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);

            
            chatBubble.NameText.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.2f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);
        }catch{}
    }

    public static void seeGhostsCheat(PlayerPhysics playerPhysics)
    {
        try{

            if(playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead){
                playerPhysics.myPlayer.Visible = CheatToggles.seeGhosts;
            }

        }catch{}
    }

    public static void freecamCheat()
    {
        if(CheatToggles.freecam){
            
            if (!freecamActive){

                Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = false;
                Camera.main.gameObject.GetComponent<FollowerCamera>().Target = null;

                freecamActive = true;

            }

            
            PlayerControl.LocalPlayer.moveable = false;

            
            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);

            
            
            Camera.main.transform.position = Camera.main.transform.position + movement * 10f * Time.deltaTime;

        }else
        {
            
            if (!freecamActive) return;
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            freecamActive = false;
        }
    }
}
