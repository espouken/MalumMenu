using UnityEngine;

namespace MalumMenu;
public static class TracersHandler
{
    public static void drawPlayerTracer(PlayerPhysics playerPhysics){
        try{

            Color color = Color.clear; 

            if (!playerPhysics.myPlayer.Data.IsDead){
                if (CheatToggles.tracersCrew && !playerPhysics.myPlayer.Data.Role.IsImpostor){
                    if (CheatToggles.colorBasedTracers){
                        color = playerPhysics.myPlayer.Data.Color; 
                    }else{
                        color = playerPhysics.myPlayer.Data.Role.TeamColor; 
                    }
                }else if (CheatToggles.tracersImps && playerPhysics.myPlayer.Data.Role.IsImpostor){
                    if (CheatToggles.colorBasedTracers){
                        color = playerPhysics.myPlayer.Data.Color; 
                    }else{
                        color = playerPhysics.myPlayer.Data.Role.TeamColor; 
                    }
                }
            }else{
                if (CheatToggles.tracersGhosts){
                    if (CheatToggles.colorBasedTracers){
                        color = playerPhysics.myPlayer.Data.Color; 
                    }else{
                        color = Palette.White; 
                    }
                }
            }

            
            Utils.drawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);

        }catch{}
    }

    public static void drawBodyTracer(DeadBody deadBody){
        Color color = Color.clear; 

        if (CheatToggles.tracersBodies){
            if (CheatToggles.colorBasedTracers){

                
                NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(deadBody.ParentId);

                color = playerById.Color; 

            }else{

                color = Color.yellow; 

            }
        }

        
        Utils.drawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
    }
}
