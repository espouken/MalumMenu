using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUs.GameOptions;
using UnityEngine;

namespace MalumMenu;

public struct CheatToggles
{
    public static bool noClip;
    public static bool speedBoost;
    public static bool teleportPlayer;
    public static bool teleportCursor;
    public static bool reportBody;
    public static bool ejectPlayer;
    public static bool killPlayer;
    public static bool telekillPlayer;
    public static bool killAll;
    public static bool killAllCrew;
    public static bool killAllImps;
    public static bool revive;
    public static bool protectPlayer;
    public static bool invertControls;
    public static bool moonwalk;

    public static bool changeRole;
    public static bool zeroKillCd;
    public static bool showTasksMenu;
    public static bool completeMyTasks;
    public static bool killReach;
    public static bool killAnyone;
    public static bool endlessSsDuration;
    public static bool endlessBattery;
    public static bool endlessTracking;
    public static bool noTrackingCooldown;
    public static bool noTrackingDelay;
    public static bool noVitalsCooldown;
    public static bool noVentCooldown;
    public static bool endlessVentTime;
    public static bool endlessVanish;
    public static bool killVanished;
    public static bool noVanishAnim;
    public static bool noShapeshiftAnim;

    public static bool fullBright;
    public static bool seeGhosts;
    public static bool seeRoles;
    public static bool showPlayerInfo;
    public static bool seeDisguises;
    public static bool showTaskArrows;
    public static bool revealVotes;
    public static bool moreLobbyInfo;
    public static bool eventLogger;

    public static bool spectate;
    public static bool zoomOut;
    public static bool freecam;

    public static bool mapCrew;
    public static bool mapImps;
    public static bool mapGhosts;
    public static bool colorBasedMap;

    public static bool tracersImps;
    public static bool tracersCrew;
    public static bool tracersGhosts;
    public static bool tracersBodies;
    public static bool colorBasedTracers;
    public static bool distanceBasedTracers;

    public static bool radarEnabled;
    public static bool radarCrew = true;
    public static bool radarImps = true;
    public static bool radarGhosts;
    public static bool radarBodies;
    public static bool radarColorBased;
    public static bool radarShowMap = true;
    public static bool radarShowKills = true;
    public static bool radarShowVents = true;
    public static bool radarShowTasks = true;
    public static bool radarShowShapeshift = true;

    public static bool alwaysChat;
    public static bool chatJailbreak;

    public static bool closeMeeting;
    public static bool sabotageMap;
    public static bool openAllDoors;
    public static bool closeAllDoors;
    public static bool spamOpenAllDoors;
    public static bool spamCloseAllDoors;
    public static bool autoOpenDoorsOnUse;
    public static bool autoFixLights;
    public static bool unfixableLights;
    public static bool commsSab;
    public static bool elecSab;
    public static bool reactorSab;
    public static bool oxygenSab;
    public static bool mushSab;
    public static bool mushSpore;
    public static bool showDoorsMenu;
    public static bool closeCurrentRoomDoors;

    public static KeyCode closeRoomBind = KeyCode.None;

    public static bool useVents;
    public static bool walkVent;
    public static bool kickVents;

    public static bool voteImmune;
    public static bool skipMeeting;
    public static bool callMeeting;
    public static bool forceStartGame;
    public static bool noGameEnd;
    public static bool noOptionsLimits;

    public static bool unlockFeatures;
    public static bool freeCosmetics;
    public static bool avoidBans;
    public static bool spoofAprilFoolsDate;
    public static bool panic;

    public static bool animShields;
    public static bool animAsteroids;
    public static bool animEmptyGarbage;
    public static bool animScan;
    public static bool animCamsInUse;

    public static bool reloadConfig;
    public static bool RGBMode;

    public static readonly Dictionary<string, KeyCode> Keybinds = new();

    private static readonly Dictionary<string, FieldInfo> ToggleFields = new();

    public static readonly string ProfilePath = Path.Combine(BepInEx.Paths.ConfigPath, "MalumProfile.txt");

    static CheatToggles()
    {
        var fields = typeof(CheatToggles).GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType != typeof(bool)) continue;
            ToggleFields[field.Name] = field;
            Keybinds[field.Name] = KeyCode.None;
        }
    }

    public static void DisablePPMCheats(string variableToKeep)
    {
        ejectPlayer = variableToKeep == "ejectPlayer" && ejectPlayer;
        reportBody = variableToKeep == "reportBody" && reportBody;
        killPlayer = variableToKeep == "killPlayer" && killPlayer;
        telekillPlayer = variableToKeep == "telekillPlayer" && telekillPlayer;
        spectate = variableToKeep == "spectate" && spectate;
        changeRole = variableToKeep == "changeRole" && changeRole;
        teleportPlayer = variableToKeep == "teleportPlayer" && teleportPlayer;
        protectPlayer = variableToKeep == "protectPlayer" && protectPlayer;
    }

    public static bool shouldPPMClose()
    {
        return !changeRole && !ejectPlayer && !reportBody && !telekillPlayer && !killPlayer && !spectate && !teleportPlayer && !protectPlayer;
    }

    public static void DisableAll()
    {
        foreach (var field in ToggleFields.Values)
        {
            field.SetValue(null, false);
        }
    }

    public static void SaveTogglesToProfile()
    {
        using var writer = new StreamWriter(ProfilePath);

        writer.WriteLine("# MalumProfile");
        writer.WriteLine("# Format: ToggleName = True/False = KeyCode.Foo");
        writer.WriteLine("# - List of keycodes: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");
        writer.WriteLine("# - KeyCode part is optional; use KeyCode.None for no key");
        writer.WriteLine("# - Multiple toggles may have the same key, but multiple keys per toggle are NOT supported");
        writer.WriteLine("# - Keybinds are only applied after loading this profile by pressing 'Load from Profile' (Config category)");
        writer.WriteLine();

        foreach (var field in ToggleFields.Values)
        {
            Keybinds.TryGetValue(field.Name, out var key);
            writer.WriteLine($"{field.Name} = {field.GetValue(null)} = KeyCode.{key}");
        }

        
        writer.WriteLine($"closeRoomBind = False = KeyCode.{closeRoomBind}");
    }

    public static void LoadTogglesFromProfile()
    {
        if (!File.Exists(ProfilePath)) return;

        using var reader = new StreamReader(ProfilePath);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            line = line.Trim();
            if (line.StartsWith("#")) continue;

            var parts = line.Split('=', 3);
            if (parts.Length < 2) continue;

            var name = parts[0].Trim();

            
            if (name == "closeRoomBind")
            {
                if (parts.Length >= 3)
                {
                    var keyPart = parts[2].Trim();
                    if (keyPart.StartsWith("KeyCode."))
                    {
                        keyPart = keyPart["KeyCode.".Length..];
                    }

                    if (!string.IsNullOrEmpty(keyPart) && System.Enum.TryParse<KeyCode>(keyPart, true, out var parsed))
                    {
                        closeRoomBind = parsed;
                    }
                }
                continue;
            }

            if (!ToggleFields.TryGetValue(name, out var field)) continue;

            if (bool.TryParse(parts[1].Trim(), out var boolVal))
            {
                field.SetValue(null, boolVal);
            }

            KeyCode key = KeyCode.None;
            if (parts.Length >= 3)
            {
                var keyPart = parts[2].Trim();
                if (keyPart.StartsWith("KeyCode."))
                {
                    keyPart = keyPart["KeyCode.".Length..];
                }

                if (!string.IsNullOrEmpty(keyPart) && System.Enum.TryParse<KeyCode>(keyPart, true, out var parsed))
                {
                    key = parsed;
                }
            }

            Keybinds[name] = key;
        }
    }

    public class KeybindListener : MonoBehaviour
    {
        public MalumMenu Plugin { get; internal set; }

        public void Update()
        {
            if (MenuUI.isPanicked) return;
            if (!HudManager.InstanceExists || !HudManager.Instance.Chat || HudManager.Instance.Chat.IsOpenOrOpening) return;

            if (reloadConfig)
            {
                Plugin.Config.Reload();
                Plugin.Log.LogInfo("Plugin config reloaded.");
                reloadConfig = false;
            }

            foreach (var (name, key) in Keybinds)
            {
                if (key == KeyCode.None) continue;
                if (!Input.GetKeyDown(key)) continue;

                if (!ToggleFields.TryGetValue(name, out var field)) continue;
                var current = (bool)field.GetValue(null);
                field.SetValue(null, !current);
            }
        }
    }
}
