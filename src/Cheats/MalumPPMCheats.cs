using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace MalumMenu;
public static class MalumPPMCheats
{
    public static bool telekillPlayerActive;
    public static bool killPlayerActive;
    public static bool spectateActive;
    public static bool teleportPlayerActive;
    public static bool protectPlayerActive;
    public static bool reportBodyActive;
    public static bool ejectPlayerActive;
    public static bool changeRoleActive;
    public static RoleTypes? oldRole = null;

    public static void reportBodyPPM(){
        if (CheatToggles.reportBody){

            if (!reportBodyActive){

                
                if (PlayerPickMenu.playerpickMenu != null){
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("reportBody");
                }

                
                PlayerPickMenu.openPlayerPickMenu(Utils.GetAllPlayerData(), (Action) (() =>
                {

                    Utils.reportDeadBody(PlayerPickMenu.targetPlayerData);

                }));

                reportBodyActive = true;
            }

            
            if (PlayerPickMenu.playerpickMenu == null){
                CheatToggles.reportBody = false;
            }

        }else{
            if (reportBodyActive){
                reportBodyActive = false;
            }
        }
    }

    public static void ejectPlayerPPM()
    {
        if (CheatToggles.ejectPlayer)
        {
            if (!ejectPlayerActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("ejectPlayer");
                }
                if (!Utils.isMeeting)
                {
                    CheatToggles.ejectPlayer = false;
                    return;
                }

                List<NetworkedPlayerInfo> val = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        val.Add(player.Data);
                    }
                }
                PlayerPickMenu.openPlayerPickMenu(val, (Action)(() =>
                {
                    NetworkedPlayerInfo playerToEject = PlayerPickMenu.targetPlayerData;
                    MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), playerToEject, false);
                }));

                ejectPlayerActive = true;
            }
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.ejectPlayer = false;
            }
        }
        else if (ejectPlayerActive)
        {
            ejectPlayerActive = false;
        }
    }

    public static void killPlayerPPM()
    {
        if (CheatToggles.killPlayer)
        {
            if (!killPlayerActive)
            {
                
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("killPlayer");
                }

                if (Utils.isLobby){
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.killPlayer = false;
                    return;
                }

                
                PlayerPickMenu.openPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    Utils.murderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                }));

                killPlayerActive = true;
            }

            
            if (PlayerPickMenu.playerpickMenu == null){
                CheatToggles.killPlayer = false;
            }
        }
        else if (killPlayerActive)
        {
            killPlayerActive = false;
        }
    }

    public static void telekillPlayerPPM()
    {
        if (CheatToggles.telekillPlayer)
        {
            if (!telekillPlayerActive)
            {
                
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("telekillPlayer");
                }

                if (Utils.isLobby){
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.telekillPlayer = false;
                    return;
                }

                
                PlayerPickMenu.openPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
                    Utils.murderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                    AmongUsClient.Instance.StartCoroutine(DelayedTeleportBack(oldPos));
                }));

                telekillPlayerActive = true;
            }

            
            if (PlayerPickMenu.playerpickMenu == null){
                CheatToggles.telekillPlayer = false;
            }
        }
        else if (telekillPlayerActive)
        {
            telekillPlayerActive = false;
        }
    }

    
    
    
    
    
    public static System.Collections.IEnumerator DelayedTeleportBack(Vector2 position)
    {
        yield return new WaitForSeconds(0.25f);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
    }

    public static void teleportPlayerPPM()
    {
        if (CheatToggles.teleportPlayer)
        {
            if (!teleportPlayerActive)
            {
                
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("teleportPlayer");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner){
                        playerDataList.Add(player.Data);
                    }
                }

                
                PlayerPickMenu.openPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerPickMenu.targetPlayerData.Object.transform.position);
                }));

                teleportPlayerActive = true;
            }

            
            if (PlayerPickMenu.playerpickMenu == null){
                CheatToggles.teleportPlayer = false;
            }
        }
        else if (teleportPlayerActive)
        {
            teleportPlayerActive = false;
        }
    }

    public static void ProtectPlayerPPM()
    {
        if (CheatToggles.protectPlayer)
        {
            if (!protectPlayerActive && !Utils.isLobby)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("protectPlayer");
                }

                PlayerPickMenu.openPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    var targetPlayer = PlayerPickMenu.targetPlayerData.Object;
                    if (targetPlayer != null)
                    {
                        int protectColorId = PlayerControl.LocalPlayer.cosmetics.ColorId;
                        PlayerControl.LocalPlayer.RpcProtectPlayer(targetPlayer, protectColorId);
                    }
                }));

                protectPlayerActive = true;
            }

            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.protectPlayer = false;
            }
        }
        else if (protectPlayerActive)
        {
            protectPlayerActive = false;
        }
    }

    public static void changeRolePPM()
    {
        if (CheatToggles.changeRole){

            if (!changeRoleActive){

                
                if (PlayerPickMenu.playerpickMenu != null){
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("changeRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                
                
                if (oldRole == RoleTypes.Shapeshifter || Utils.isFreePlay){

                    playerDataList.Add(PlayerPickMenu.customPPMChoice("Shapeshifter", Outfits.shapeshifter, Utils.getBehaviourByRoleType(RoleTypes.Shapeshifter)));

                }

                
                
                if (oldRole == RoleTypes.Phantom || Utils.isFreePlay){

                    playerDataList.Add(PlayerPickMenu.customPPMChoice("Phantom", Outfits.phantom, Utils.getBehaviourByRoleType(RoleTypes.Phantom)));

                }

                
                
                if (oldRole == RoleTypes.Viper || Utils.isFreePlay){

                    playerDataList.Add(PlayerPickMenu.customPPMChoice("Viper", Outfits.viper, Utils.getBehaviourByRoleType(RoleTypes.Viper)));

                }

                
                
                if (oldRole == RoleTypes.Impostor || Utils.isFreePlay || Utils.isHost){

                    playerDataList.Add(PlayerPickMenu.customPPMChoice("Impostor", Outfits.impostor, Utils.getBehaviourByRoleType(RoleTypes.Impostor)));

                }

                playerDataList.Add(PlayerPickMenu.customPPMChoice("Tracker", Outfits.tracker, Utils.getBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.customPPMChoice("Noisemaker", Outfits.noisemaker, Utils.getBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.customPPMChoice("Engineer", Outfits.engineer, Utils.getBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.customPPMChoice("Scientist", Outfits.scientist, Utils.getBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.customPPMChoice("Detective", Outfits.detective, Utils.getBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.customPPMChoice("Crewmate", Outfits.crewmate, Utils.getBehaviourByRoleType(RoleTypes.Crewmate)));

                
                PlayerPickMenu.openPlayerPickMenu(playerDataList, (Action) (() =>
                {

                    
                    if (!Utils.isLobby && !Utils.isFreePlay && oldRole == null){
                        oldRole = PlayerControl.LocalPlayer.Data.RoleType;
                    }

                    if (PlayerControl.LocalPlayer.Data.IsDead){ 
                        if (PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor){
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.ImpostorGhost);
                        }else{
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                        }
                    }else{

                        













                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                    }



                }));

                changeRoleActive = true;
            }

            
            if (PlayerPickMenu.playerpickMenu == null){
                CheatToggles.changeRole = false;
            }

        }else{
            if (changeRoleActive){
                changeRoleActive = false;
            }
        }
    }

    public static void spectatePPM()
    {
        if (CheatToggles.spectate){

            if (!spectateActive){

                
                if (PlayerPickMenu.playerpickMenu != null){
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("spectate");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                
                foreach (var player in PlayerControl.AllPlayerControls){
                    if (!player.AmOwner){
                        playerDataList.Add(player.Data);
                    }
                }

                
                PlayerPickMenu.openPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerPickMenu.targetPlayerData.Object);
                }));

                spectateActive = true;

                PlayerControl.LocalPlayer.moveable = false; 

                CheatToggles.freecam = false; 

            }

            
            if (PlayerPickMenu.playerpickMenu == null && Camera.main.gameObject.GetComponent<FollowerCamera>().Target == PlayerControl.LocalPlayer){
                CheatToggles.spectate = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }else{
            
            if (spectateActive){
                spectateActive = false;
                PlayerControl.LocalPlayer.moveable = true;
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }
}
