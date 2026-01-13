namespace MalumMenu;

public static class MalumSabotageSystem
{
    public static bool reactorSab;
    public static bool oxygenSab;
    public static bool commsSab;
    public static bool elecSab;
    public static bool unfixableLights;

    public static void HandleReactor(ShipStatus shipStatus, byte mapId)
    {
        switch (mapId)
        {
            case 2:
            {
                

                var labSys = shipStatus.Systems[SystemTypes.Laboratory].Cast<ReactorSystemType>();

                if (CheatToggles.reactorSab != reactorSab)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Laboratory, reactorSab ? (byte)16 : (byte)128);
                    reactorSab = CheatToggles.reactorSab;
                }

                CheatToggles.reactorSab = reactorSab = labSys.IsActive;
                break;
            }
            case 4:
            {
                

                var heliSys = shipStatus.Systems[SystemTypes.HeliSabotage].Cast<HeliSabotageSystem>();

                if (CheatToggles.reactorSab != reactorSab){

                    if (reactorSab){
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0); 
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
                    }else{
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 128); 
                    }

                    reactorSab = CheatToggles.reactorSab;
                }

                CheatToggles.reactorSab = reactorSab = heliSys.IsActive;
                break;
            }
            default:
            {
                

                var reactorSys = shipStatus.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();

                if (CheatToggles.reactorSab != reactorSab)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Reactor, reactorSab ? (byte)16 : (byte)128);
                    reactorSab = CheatToggles.reactorSab;
                }

                CheatToggles.reactorSab = reactorSab = reactorSys.IsActive;
                break;
            }
        }
    }

    public static void HandleOxygen(ShipStatus shipStatus, byte mapId)
    {
        if (mapId != 4 && mapId != 2 && mapId != 5) { 

            var oxygenSys = shipStatus.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();

            if (CheatToggles.oxygenSab != oxygenSab)
            {
                shipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, oxygenSab ? (byte)16 : (byte)128);
                oxygenSab = CheatToggles.oxygenSab;
            }

            CheatToggles.oxygenSab = oxygenSab = oxygenSys.IsActive;

            return;

        }

        
        if (!CheatToggles.oxygenSab) return;
        HudManager.Instance.Notifier.AddDisconnectMessage("Oxygen system not present on this map");
        CheatToggles.oxygenSab = false;
    }

    public static void HandleComms(ShipStatus shipStatus, byte mapId)
    {
        if (mapId is 1 or 5) { 

            var hqCommsSys = shipStatus.Systems[SystemTypes.Comms].Cast<HqHudSystemType>();

            if (CheatToggles.commsSab != commsSab){

                if (commsSab){
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0); 
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                }else{
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, 128); 
                }

                commsSab = CheatToggles.commsSab;

            }

            CheatToggles.commsSab = commsSab = hqCommsSys.IsActive;

        }else{ 

            var commsSys = shipStatus.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();

            if (CheatToggles.commsSab != commsSab)
            {
                shipStatus.RpcUpdateSystem(SystemTypes.Comms, commsSab ? (byte)16 : (byte)128);
                commsSab = CheatToggles.commsSab;
            }

            CheatToggles.commsSab = commsSab = commsSys.IsActive;

        }
    }

    public static void HandleElectrical(ShipStatus shipStatus, byte mapId)
    {
        if (mapId != 5) { 

            var elecSys = shipStatus.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();

            
            HandleUnfixLights(shipStatus);

            if (CheatToggles.elecSab != elecSab){
                if (elecSab){

                    

                    for (var i = 0; i < 5; i++)
                    {
                        var switchMask = 1 << (i & 0x1F);

                        if ((elecSys.ActualSwitches & switchMask) != (elecSys.ExpectedSwitches & switchMask))
                        {
                            shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
                        }
                    }

                }else{

                    

                    CheatToggles.unfixableLights = false; 

                    byte b = 4;
                    for (var i = 0; i < 5; i++)
                    {
                        if (BoolRange.Next(0.5f))
                        {
                            b |= (byte)(1 << i);
                        }
                    }

                    shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)(b | 128));

                }

                elecSab = CheatToggles.elecSab;
            }

            CheatToggles.elecSab = elecSab = elecSys.IsActive && !unfixableLights;

            return;

        }

        
        if (!CheatToggles.elecSab && !CheatToggles.unfixableLights) return;
        HudManager.Instance.Notifier.AddDisconnectMessage("Electrical system not present on this map");
        CheatToggles.elecSab = CheatToggles.unfixableLights = false;
    }

    public static void HandleUnfixLights(ShipStatus shipStatus)
    {
        if (CheatToggles.unfixableLights == unfixableLights) return;
        
        
        

        if (!unfixableLights){
            CheatToggles.elecSab = false;
        }

        shipStatus.RpcUpdateSystem(SystemTypes.Electrical, 69); 

        unfixableLights = CheatToggles.unfixableLights;
    }

    public static void HandleMushMix(ShipStatus shipStatus, byte mapId)
    {
        if (!CheatToggles.mushSab) return;
        if (mapId == 5){ 

            shipStatus.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 1); 

        } else {

            

            HudManager.Instance.Notifier.AddDisconnectMessage("Mushrooms not present on this map");
        }

        
        
        

        CheatToggles.mushSab = false; 
    }

    public static void HandleSpores(FungleShipStatus shipStatus, byte mapId)
    {
        if (!CheatToggles.mushSpore) return;
        if (mapId == 5)
        {
            foreach (var mushroom in shipStatus.sporeMushrooms.Values)
            {
                PlayerControl.LocalPlayer.CmdCheckSporeTrigger(mushroom);
            }
        }
        else
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Mushrooms not present on this map");
        }

        CheatToggles.mushSpore = false;
    }

    public static void HandleDoors(ShipStatus shipStatus)
    {
        if (CheatToggles.closeAllDoors)
        {
            DoorsHandler.CloseAllDoors();
            CheatToggles.closeAllDoors = false;
        }
        if (CheatToggles.openAllDoors)
        {
            DoorsHandler.OpenAllDoors();
            CheatToggles.openAllDoors = false;
        }

        if (CheatToggles.spamCloseAllDoors)
        {
            DoorsHandler.CloseAllDoors();
        }
        if (CheatToggles.spamOpenAllDoors)
        {
            DoorsHandler.OpenAllDoors();
        }
    }

    public static void OpenSabotageMap()
    {
        if (!CheatToggles.sabotageMap) return;
        DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.Sabotage
        });
        CheatToggles.sabotageMap = false;
    }
}
