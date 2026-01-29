using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public List<GroupInfo> groups = [];
    private bool isDragging = false;
    private Rect windowRect = new(10, 10, 300, 500);
    private Rect horizontalWindowRect = new(10, 10, 700, 550);
    private bool isGUIActive = false;
    public static bool isPanicked = false;
    public int selectedTab;

    private GUIStyle submenuButtonStyle;
    public GUIStyle tabTitleStyle;
    public GUIStyle tabSubtitleStyle;
    public GUIStyle separatorStyle;
    private float hue;

    private void Start()
    {
        groups.Add(new GroupInfo("Player", false, [
            new ToggleInfo(" NoClip", () => CheatToggles.noClip, x => CheatToggles.noClip = x),
            new ToggleInfo(" Fake Revive", () => CheatToggles.revive, x => CheatToggles.revive = x),
            new ToggleInfo(" Invert Controls", () => CheatToggles.invertControls, x => CheatToggles.invertControls = x)
        ], [
            new SubmenuInfo("Teleport", false, [
                new ToggleInfo(" to Cursor", () => CheatToggles.teleportCursor, x => CheatToggles.teleportCursor = x),
                new ToggleInfo(" to Player", () => CheatToggles.teleportPlayer, x => CheatToggles.teleportPlayer = x)
            ])

        ]));

        groups.Add(new GroupInfo("ESP", false, [
            new ToggleInfo(" Show Player Info", () => CheatToggles.showPlayerInfo, x => CheatToggles.showPlayerInfo = x),
            new ToggleInfo(" See Roles", () => CheatToggles.seeRoles, x => CheatToggles.seeRoles = x),
            new ToggleInfo(" See Ghosts", () => CheatToggles.seeGhosts, x => CheatToggles.seeGhosts = x),
            new ToggleInfo(" No Shadows", () => CheatToggles.fullBright, x => CheatToggles.fullBright = x),
            new ToggleInfo(" Show Task Arrows", () => CheatToggles.showTaskArrows, x => CheatToggles.showTaskArrows = x),
            new ToggleInfo(" Reveal Votes", () => CheatToggles.revealVotes, x => CheatToggles.revealVotes = x),
            new ToggleInfo(" More Lobby Info", () => CheatToggles.moreLobbyInfo, x => CheatToggles.moreLobbyInfo = x),
            new ToggleInfo(" Event Logger", () => CheatToggles.eventLogger, x => { CheatToggles.eventLogger = x; if (MalumMenu.consoleUI != null) MalumMenu.consoleUI.isVisible = x; })
        ], [
            new SubmenuInfo("Event Logger Filter", false, [
                new ToggleInfo(" Log Kills", () => CheatToggles.eventLogKills, x => CheatToggles.eventLogKills = x),
                new ToggleInfo(" Log Vents", () => CheatToggles.eventLogVents, x => CheatToggles.eventLogVents = x),
                new ToggleInfo(" Log Tasks", () => CheatToggles.eventLogTasks, x => CheatToggles.eventLogTasks = x),
                new ToggleInfo(" Log Votes", () => CheatToggles.eventLogVotes, x => CheatToggles.eventLogVotes = x),
                new ToggleInfo(" Log Sabotage", () => CheatToggles.eventLogSabotage, x => CheatToggles.eventLogSabotage = x),
                new ToggleInfo(" Log Body Reports", () => CheatToggles.eventLogBodyReports, x => CheatToggles.eventLogBodyReports = x),
                new ToggleInfo(" Log Disconnects", () => CheatToggles.eventLogDisconnects, x => CheatToggles.eventLogDisconnects = x),
                new ToggleInfo(" Log Shapeshift", () => CheatToggles.eventLogShapeshift, x => CheatToggles.eventLogShapeshift = x),
                new ToggleInfo(" Log Protect", () => CheatToggles.eventLogProtect, x => CheatToggles.eventLogProtect = x),
                new ToggleInfo(" Log Scanner", () => CheatToggles.eventLogScanner, x => CheatToggles.eventLogScanner = x),
                new ToggleInfo(" Log Roles", () => CheatToggles.eventLogRoles, x => CheatToggles.eventLogRoles = x)
            ]),

            new SubmenuInfo("Camera", false, [

                new ToggleInfo(" Zoom Out", () => CheatToggles.zoomOut, x => CheatToggles.zoomOut = x),
                new ToggleInfo(" Spectate", () => CheatToggles.spectate, x => CheatToggles.spectate = x),
                new ToggleInfo(" Freecam", () => CheatToggles.freecam, x => CheatToggles.freecam = x)
            ]),

            new SubmenuInfo("Tracers", false, [
                new ToggleInfo(" Crewmates", () => CheatToggles.tracersCrew, x => CheatToggles.tracersCrew = x),
                new ToggleInfo(" Impostors", () => CheatToggles.tracersImps, x => CheatToggles.tracersImps = x),
                new ToggleInfo(" Ghosts", () => CheatToggles.tracersGhosts, x => CheatToggles.tracersGhosts = x),
                new ToggleInfo(" Dead Bodies", () => CheatToggles.tracersBodies, x => CheatToggles.tracersBodies = x),
                new ToggleInfo(" Color-based", () => CheatToggles.colorBasedTracers,
                    x => CheatToggles.colorBasedTracers = x)

            ]),

            new SubmenuInfo("Minimap", false, [
                new ToggleInfo(" Crewmates", () => CheatToggles.mapCrew, x => CheatToggles.mapCrew = x),
                new ToggleInfo(" Impostors", () => CheatToggles.mapImps, x => CheatToggles.mapImps = x),
                new ToggleInfo(" Ghosts", () => CheatToggles.mapGhosts, x => CheatToggles.mapGhosts = x),
                new ToggleInfo(" Color-based", () => CheatToggles.colorBasedMap, x => CheatToggles.colorBasedMap = x)
            ]),

            new SubmenuInfo("Radar", false, [
                new ToggleInfo(" Enable Radar", () => CheatToggles.radarEnabled, x => CheatToggles.radarEnabled = x),
                new ToggleInfo(" Crewmates", () => CheatToggles.radarCrew, x => CheatToggles.radarCrew = x),
                new ToggleInfo(" Impostors", () => CheatToggles.radarImps, x => CheatToggles.radarImps = x),
                new ToggleInfo(" Ghosts", () => CheatToggles.radarGhosts, x => CheatToggles.radarGhosts = x),
                new ToggleInfo(" Dead Bodies", () => CheatToggles.radarBodies, x => CheatToggles.radarBodies = x),
                new ToggleInfo(" Color-based", () => CheatToggles.radarColorBased, x => CheatToggles.radarColorBased = x),
                new ToggleInfo(" Show Map", () => CheatToggles.radarShowMap, x => CheatToggles.radarShowMap = x),
                new ToggleInfo(" Show Kills", () => CheatToggles.radarShowKills, x => CheatToggles.radarShowKills = x),
                new ToggleInfo(" Show Vents", () => CheatToggles.radarShowVents, x => CheatToggles.radarShowVents = x),
                new ToggleInfo(" Show Tasks", () => CheatToggles.radarShowTasks, x => CheatToggles.radarShowTasks = x),
                new ToggleInfo(" Show Shapeshift", () => CheatToggles.radarShowShapeshift, x => CheatToggles.radarShowShapeshift = x)
            ])

        ]));

        groups.Add(new GroupInfo("Roles", false, [
                new ToggleInfo(" Set Fake Role", () => CheatToggles.changeRole, x => CheatToggles.changeRole = x)
            ],
            [
                new SubmenuInfo("Impostor", false, [
                    new ToggleInfo(" Kill Reach", () => CheatToggles.killReach, x => CheatToggles.killReach = x),
                    new ToggleInfo(" Do Tasks", () => CheatToggles.impostorTasks, x => CheatToggles.impostorTasks = x)
                ]),

                new SubmenuInfo("Shapeshifter", false, [
                    new ToggleInfo(" No Ss Animation", () => CheatToggles.noShapeshiftAnim,
                        x => CheatToggles.noShapeshiftAnim = x),

                    new ToggleInfo(" Endless Ss Duration", () => CheatToggles.endlessSsDuration,
                        x => CheatToggles.endlessSsDuration = x)

                ]),

                new SubmenuInfo("Crewmate", false, [
                    new ToggleInfo(" Show Tasks Menu", () => CheatToggles.showTasksMenu, x => CheatToggles.showTasksMenu = x)
                ]),

                new SubmenuInfo("Tracker", false, [
                    new ToggleInfo(" Endless Tracking", () => CheatToggles.endlessTracking,
                        x => CheatToggles.endlessTracking = x),

                    new ToggleInfo(" No Track Delay", () => CheatToggles.noTrackingDelay,
                        x => CheatToggles.noTrackingDelay = x),

                    new ToggleInfo(" No Track Cooldown", () => CheatToggles.noTrackingCooldown,
                        x => CheatToggles.noTrackingCooldown = x),

                    new ToggleInfo(" Track Reach", () => CheatToggles.trackReach, x => CheatToggles.trackReach = x)

                ]),

                new SubmenuInfo("Engineer", false, [
                    new ToggleInfo(" Endless Vent Time", () => CheatToggles.endlessVentTime,
                        x => CheatToggles.endlessVentTime = x),

                    new ToggleInfo(" No Vent Cooldown", () => CheatToggles.noVentCooldown,
                        x => CheatToggles.noVentCooldown = x)

                ]),

                new SubmenuInfo("Scientist", false, [
                    new ToggleInfo(" Endless Battery", () => CheatToggles.endlessBattery,
                        x => CheatToggles.endlessBattery = x),

                    new ToggleInfo(" No Vitals Cooldown", () => CheatToggles.noVitalsCooldown,
                        x => CheatToggles.noVitalsCooldown = x)

                ]),

                new SubmenuInfo("Detective", false, [
                    new ToggleInfo(" Interrogate Reach", () => CheatToggles.interrogateReach, x => CheatToggles.interrogateReach = x)
                ])

            ]));

        groups.Add(new GroupInfo("Ship", false, [
            new ToggleInfo(" Unfixable Lights", () => CheatToggles.unfixableLights,
                x => CheatToggles.unfixableLights = x),
            new ToggleInfo(" Auto-Fix Lights On Use", () => CheatToggles.autoFixLights, x => CheatToggles.autoFixLights = x),
            new ToggleInfo(" Auto-Fix Comms On Use", () => CheatToggles.autoFixComms, x => CheatToggles.autoFixComms = x),
            new ToggleInfo(" Report Body", () => CheatToggles.reportBody, x => CheatToggles.reportBody = x),
            new ToggleInfo(" Close Meeting", () => CheatToggles.closeMeeting, x => CheatToggles.closeMeeting = x),
            new ToggleInfo(" Auto-Open Doors On Use", () => CheatToggles.autoOpenDoorsOnUse, x => CheatToggles.autoOpenDoorsOnUse = x)
        ], [
            new SubmenuInfo("Sabotage", false, [
                new ToggleInfo(" Reactor", () => CheatToggles.reactorSab, x => CheatToggles.reactorSab = x),
                new ToggleInfo(" Oxygen", () => CheatToggles.oxygenSab, x => CheatToggles.oxygenSab = x),
                new ToggleInfo(" Lights", () => CheatToggles.elecSab, x => CheatToggles.elecSab = x),
                new ToggleInfo(" Comms", () => CheatToggles.commsSab, x => CheatToggles.commsSab = x),
                new ToggleInfo(" Show Doors Menu", () => CheatToggles.showDoorsMenu, x => CheatToggles.showDoorsMenu = x),
                new ToggleInfo(" MushroomMixup", () => CheatToggles.mushSab, x => CheatToggles.mushSab = x),
                new ToggleInfo(" Trigger Spores", () => CheatToggles.mushSpore, x => CheatToggles.mushSpore = x),
                new ToggleInfo(" Open Sabotage Map", () => CheatToggles.sabotageMap, x => CheatToggles.sabotageMap = x)
            ]),

            new SubmenuInfo("Vents", false, [
                new ToggleInfo(" Unlock Vents", () => CheatToggles.useVents, x => CheatToggles.useVents = x),
                new ToggleInfo(" Kick All From Vents", () => CheatToggles.kickVents, x => CheatToggles.kickVents = x),
                new ToggleInfo(" Walk In Vents", () => CheatToggles.walkVent, x => CheatToggles.walkVent = x)
            ])

        ]));

        groups.Add(new GroupInfo("Chat", false, [
            new ToggleInfo(" Enable Chat", () => CheatToggles.alwaysChat, x => CheatToggles.alwaysChat = x),
            new ToggleInfo(" Unlock Textbox", () => CheatToggles.chatJailbreak, x => CheatToggles.chatJailbreak = x)
        ], []));

        groups.Add(new GroupInfo("Host-Only", false,
            [
                new ToggleInfo(" Kill While Vanished", () => CheatToggles.killVanished,
                    x => CheatToggles.killVanished = x),
                new ToggleInfo(" Kill Anyone", () => CheatToggles.killAnyone, x => CheatToggles.killAnyone = x),
                new ToggleInfo(" No Kill Cooldown", () => CheatToggles.zeroKillCd, x => CheatToggles.zeroKillCd = x),
                new ToggleInfo(" Protect Player", () => CheatToggles.protectPlayer, x => CheatToggles.protectPlayer = x),
                new ToggleInfo(" No Options Limits", () => CheatToggles.noOptionsLimits, x => CheatToggles.noOptionsLimits = x)
            ],
            [
                new SubmenuInfo("Murder", false, [
                    new ToggleInfo(" Kill Player", () => CheatToggles.killPlayer, x => CheatToggles.killPlayer = x),
                    new ToggleInfo(" Telekill Player", () => CheatToggles.telekillPlayer, x => CheatToggles.telekillPlayer = x),
                    new ToggleInfo(" Kill All Crewmates", () => CheatToggles.killAllCrew,
                        x => CheatToggles.killAllCrew = x),

                    new ToggleInfo(" Kill All Impostors", () => CheatToggles.killAllImps,
                        x => CheatToggles.killAllImps = x),

                    new ToggleInfo(" Kill Everyone", () => CheatToggles.killAll, x => CheatToggles.killAll = x)

                ]),

                new SubmenuInfo("Game State", false, [
                    new ToggleInfo(" Force Start Game", () => CheatToggles.forceStartGame, x => CheatToggles.forceStartGame = x),
                    new ToggleInfo(" No Game End", () => CheatToggles.noGameEnd, x => CheatToggles.noGameEnd = x)
                ]),

                new SubmenuInfo("Meetings", false, [
                    new ToggleInfo(" Call Meeting", () => CheatToggles.callMeeting, x => CheatToggles.callMeeting = x),
                    new ToggleInfo(" Skip Meeting", () => CheatToggles.skipMeeting, x => CheatToggles.skipMeeting = x),
                    new ToggleInfo(" VoteImmune", () => CheatToggles.voteImmune, x => CheatToggles.voteImmune = x),
                    new ToggleInfo(" Eject Player", () => CheatToggles.ejectPlayer, x => CheatToggles.ejectPlayer = x),
                ])
            ]));

        groups.Add(new GroupInfo("Passive", false, [
            new ToggleInfo(" Free Cosmetics", () => CheatToggles.freeCosmetics, x => CheatToggles.freeCosmetics = x),
            new ToggleInfo(" Avoid Penalties", () => CheatToggles.avoidBans, x => CheatToggles.avoidBans = x),
            new ToggleInfo(" Copy Lobby Code on Disconnect", () => CheatToggles.copyLobbyCodeOnDisconnect, x => CheatToggles.copyLobbyCodeOnDisconnect = x),
            new ToggleInfo(" Unlock Extra Features", () => CheatToggles.unlockFeatures, x => CheatToggles.unlockFeatures = x),
            new ToggleInfo(" Spoof Date to April 1st", () => CheatToggles.spoofAprilFoolsDate, x => CheatToggles.spoofAprilFoolsDate = x),
            new ToggleInfo(" Stealth Mode", () => CheatToggles.stealthMode, x => CheatToggles.stealthMode = x),
            new ToggleInfo(" Panic (Disable MalumMenu)", () => CheatToggles.panic, x => CheatToggles.panic = x)
        ], []));

        groups.Add(new GroupInfo("Animations", false, [
            new ToggleInfo(" Shields", () => CheatToggles.animShields, x => CheatToggles.animShields = x),
            new ToggleInfo(" Asteroids", () => CheatToggles.animAsteroids, x => CheatToggles.animAsteroids = x),
            new ToggleInfo(" Empty Garbage", () => CheatToggles.animEmptyGarbage, x => CheatToggles.animEmptyGarbage = x),
            new ToggleInfo(" Medbay Scan", () => CheatToggles.animScan, x => CheatToggles.animScan = x),
            new ToggleInfo(" Fake Cams In Use", () => CheatToggles.animCamsInUse, x => CheatToggles.animCamsInUse = x),
            new ToggleInfo(" Moonwalk", () => CheatToggles.moonwalk, x => CheatToggles.moonwalk = x)
        ], []));

        groups.Add(new GroupInfo("Config", false, [
            new ToggleInfo(" Open plugin config", () => false, x => Utils.OpenConfigFile()),
            new ToggleInfo(" Reload plugin config", () => CheatToggles.reloadConfig, x => CheatToggles.reloadConfig = x),
            new ToggleInfo(" Save to Profile", () => false, x => CheatToggles.SaveTogglesToProfile()),
            new ToggleInfo(" Load from Profile", () => false, x => CheatToggles.LoadTogglesFromProfile()),
            new ToggleInfo(" RGB Mode", () => CheatToggles.RGBMode, x => CheatToggles.RGBMode = x)
        ], []));
    }

    public void InitStyles()
    {
        if (!MalumMenu.useHorizontalUI.Value && (GUI.skin.toggle.fontSize == 13 || GUI.skin.toggle.fontSize == 0))
        {
            GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = 20;
        }
        else if (MalumMenu.useHorizontalUI.Value && GUI.skin.toggle.fontSize != 13)
        {
            GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 13;
        }

        if (submenuButtonStyle != null) return;

        submenuButtonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { textColor = Color.white, background = Texture2D.grayTexture },
            fontSize = 18
        };
        submenuButtonStyle.normal.background.Apply();

        tabTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
        };

        tabSubtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
        };

        separatorStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture },
            margin = new RectOffset { top = 4, bottom = 4 },
            padding = new RectOffset(),
            border = new RectOffset()
        };
    }

    private void Update()
    {

        if (Input.GetKeyDown(Utils.stringToKeycode(MalumMenu.menuKeybind.Value)))
        {
            isGUIActive = !isGUIActive;
        }

        if (CheatToggles.RGBMode)
        {
            hue += Time.deltaTime * 0.3f;
            if (hue > 1f) hue -= 1f;
        }

        if (CheatToggles.panic)
        {
            Utils.Panic();
            isPanicked = true;
            CheatToggles.panic = false;
        }

        if (CheatToggles.stealthMode && ModManager.Instance.ModStamp && ModManager.Instance.ModStamp.enabled)
        {
            ModManager.Instance.ModStamp.enabled = false;
        }
        else if (!CheatToggles.stealthMode && ModManager.Instance.ModStamp && !ModManager.Instance.ModStamp.enabled)
        {
            ModManager.Instance.ShowModStamp();
        }

        if (!Utils.isPlayer)
        {
            CheatToggles.changeRole = CheatToggles.killAll = CheatToggles.telekillPlayer = CheatToggles.killAllCrew = CheatToggles.killAllImps = CheatToggles.teleportCursor = CheatToggles.teleportPlayer = CheatToggles.spectate = CheatToggles.freecam = CheatToggles.killPlayer = CheatToggles.protectPlayer = false;
        }

        if (!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = CheatToggles.telekillPlayer = CheatToggles.killAllCrew = CheatToggles.killAllImps = CheatToggles.killPlayer = CheatToggles.protectPlayer = CheatToggles.ejectPlayer = CheatToggles.zeroKillCd = CheatToggles.killAnyone = CheatToggles.killVanished = CheatToggles.forceStartGame = CheatToggles.noGameEnd = CheatToggles.skipMeeting = CheatToggles.callMeeting = false;
        }

        if (!Utils.isShip)
        {
            CheatToggles.revive = CheatToggles.sabotageMap = CheatToggles.unfixableLights = CheatToggles.completeMyTasks = CheatToggles.kickVents = CheatToggles.reportBody = CheatToggles.ejectPlayer = CheatToggles.closeMeeting = CheatToggles.skipMeeting = CheatToggles.callMeeting = CheatToggles.reactorSab = CheatToggles.oxygenSab = CheatToggles.commsSab = CheatToggles.elecSab = CheatToggles.mushSab = CheatToggles.closeAllDoors = CheatToggles.openAllDoors = CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = CheatToggles.mushSpore = CheatToggles.animShields = CheatToggles.animAsteroids = CheatToggles.animEmptyGarbage = CheatToggles.animScan = CheatToggles.animCamsInUse = false;
        }
    }

    public void OnGUI()
    {

        if (!isGUIActive || isPanicked) return;

        InitStyles();

        if (!isDragging)
        {
            var windowHeight = CalculateWindowHeight();
            windowRect.height = windowHeight;
        }

        if (CheatToggles.RGBMode)
        {
            GUI.backgroundColor = Color.HSVToRGB(hue, 1f, 1f);
        }
        else
        {
            var configHtmlColor = MalumMenu.menuHtmlColor.Value;

            if (!ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
            {
                if (!configHtmlColor.StartsWith("#"))
                {
                    if (ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
                    {
                        GUI.backgroundColor = uiColor;
                    }
                }
            }
            else
            {
                GUI.backgroundColor = uiColor;
            }
        }

        if (MalumMenu.useHorizontalUI.Value)
        {
            horizontalWindowRect = GUI.Window(0, horizontalWindowRect, (GUI.WindowFunction)HorizontalWindowFunction, "MalumMenu v" + MalumMenu.malumVersion);
        }
        else
        {
            windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)WindowFunction, "MalumMenu v" + MalumMenu.malumVersion);
        }
    }

    public void WindowFunction(int windowID)
    {
        int groupSpacing = 50;
        int toggleSpacing = 40;
        int submenuSpacing = 40;
        int currentYPosition = 20;

        for (int groupId = 0; groupId < groups.Count; groupId++)
        {
            GroupInfo group = groups[groupId];

            if (GUI.Button(new Rect(10, currentYPosition, 280, 40), group.name))
            {
                group.isExpanded = !group.isExpanded;
                groups[groupId] = group;
                CloseAllGroupsExcept(groupId);
            }
            currentYPosition += groupSpacing;

            if (!group.isExpanded) continue;

            foreach (var toggle in group.toggles)
            {
                bool currentState = toggle.getState();
                bool newState = GUI.Toggle(new Rect(20, currentYPosition, 260, 30), currentState, toggle.label);
                if (newState != currentState)
                {
                    toggle.setState(newState);
                }
                currentYPosition += toggleSpacing;
            }

            if (group.name == "Player")
            {
                try
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Utils.CustomSlider(new Rect(20, currentYPosition, 250, 30), PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, 0f, 20f);
                        Utils.snapSpeedToDefault(0.05f, true);
                        
                        GUI.Label(new Rect(20, currentYPosition + 30, 250, 20), $"Current Speed: {PlayerControl.LocalPlayer?.MyPhysics.GhostSpeed} {(Utils.isSpeedDefault(true) ? "(Default)" : "")}");
                        
                        currentYPosition += 60;
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.MyPhysics.Speed = Utils.CustomSlider(new Rect(20, currentYPosition, 250, 30), PlayerControl.LocalPlayer.MyPhysics.Speed, 0f, 20f);
                        Utils.snapSpeedToDefault(0.05f);
                        
                        GUI.Label(new Rect(20, currentYPosition + 30, 250, 20), $"Current Speed: {PlayerControl.LocalPlayer?.MyPhysics.Speed} {(Utils.isSpeedDefault() ? "(Default)" : "")}");
                        
                        currentYPosition += 60;
                    }
                }
                catch (NullReferenceException) { }
            }

            for (int submenuId = 0; submenuId < group.submenus.Count; submenuId++)
            {
                var submenu = group.submenus[submenuId];


                if (GUI.Button(new Rect(20, currentYPosition, 260, 30), submenu.name, submenuButtonStyle))
                {
                    submenu.isExpanded = !submenu.isExpanded;
                    group.submenus[submenuId] = submenu;
                    if (submenu.isExpanded)
                    {
                        CloseAllSubmenusExcept(group, submenuId);
                    }
                }
                currentYPosition += submenuSpacing;

                if (!submenu.isExpanded) continue;

                foreach (var toggle in submenu.toggles)
                {
                    bool currentState = toggle.getState();
                    bool newState = GUI.Toggle(new Rect(30, currentYPosition, 250, 30), currentState, toggle.label);
                    if (newState != currentState)
                    {
                        toggle.setState(newState);
                    }
                    currentYPosition += toggleSpacing;
                }
            }
        }

        isDragging = Event.current.type switch
        {
            EventType.MouseDrag => true,
            EventType.MouseUp => false,
            _ => isDragging
        };

        GUI.DragWindow();
    }

    private int CalculateWindowHeight()
    {
        int totalHeight = 70;
        int groupHeight = 50;
        int toggleHeight = 30;
        int submenuHeight = 40;

        foreach (GroupInfo group in groups)
        {
            totalHeight += groupHeight;

            if (!group.isExpanded) continue;
            totalHeight += group.toggles.Count * toggleHeight;

            foreach (SubmenuInfo submenu in group.submenus)
            {
                totalHeight += submenuHeight;

                if (submenu.isExpanded)
                {
                    totalHeight += submenu.toggles.Count * toggleHeight;
                }
            }
        }

        return totalHeight;
    }

    private void CloseAllGroupsExcept(int indexToKeepOpen)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            if (i == indexToKeepOpen) continue;
            GroupInfo group = groups[i];
            group.isExpanded = false;
            groups[i] = group;
        }
    }

    private void CloseAllSubmenusExcept(GroupInfo group, int submenuIndexToKeepOpen)
    {
        for (int i = 0; i < group.submenus.Count; i++)
        {
            if (i == submenuIndexToKeepOpen) continue;
            var submenu = group.submenus[i];
            submenu.isExpanded = false;
            group.submenus[i] = submenu;
        }
    }

    public void HorizontalWindowFunction(int windowID)
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(horizontalWindowRect.width * 0.15f));
        for (var i = 0; i < groups.Count; i++)
        {
            if (GUILayout.Button(groups[i].name, GUILayout.Height(40)))
                selectedTab = i;
        }
        GUILayout.EndVertical();

        GUILayout.Box("", separatorStyle, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(10f), GUILayout.ExpandHeight(true));

        GUILayout.BeginVertical(GUILayout.Width(horizontalWindowRect.width * 0.85f));

        if (selectedTab >= 0 && selectedTab < groups.Count)
        {
            GUILayout.Label(groups[selectedTab].name, tabTitleStyle);
            HorizontalDrawContent(selectedTab);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private int GetLeftSubmenuCount(int groupId)
    {
        var name = groups[groupId].name;
        return name switch
        {
            "Player" => 1,
            "ESP" => 2,
            "Roles" => 4,
            "Ship" => 1,
            "Chat" => 1,
            "Host-Only" => 2,
            "Passive" => 1,
            "Animations" => 1,
            "Config" => 1,
            _ => 2
        };
    }

    public void HorizontalDrawContent(int groupId)
    {
        var group = groups[groupId];

        var count = group.submenus.Count;
        if (count == 0)
        {
            HorizontalDrawToggles(group.toggles);
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(horizontalWindowRect.width * 0.425f));

        HorizontalDrawToggles(group.toggles);

        if (group.name == "Player")
        {
            try
            {
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Utils.CustomLayoutSlider(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, 0f, 20f, GUILayout.Width(250f));
                    Utils.snapSpeedToDefault(0.05f, true);
                    GUILayout.Label($"Current Speed: {PlayerControl.LocalPlayer?.MyPhysics.GhostSpeed} {(Utils.isSpeedDefault(true) ? "(Default)" : "")}");
                }
                else
                {
                    PlayerControl.LocalPlayer.MyPhysics.Speed = Utils.CustomLayoutSlider(PlayerControl.LocalPlayer.MyPhysics.Speed, 0f, 20f, GUILayout.Width(250f));
                    Utils.snapSpeedToDefault(0.05f);
                    GUILayout.Label($"Current Speed: {PlayerControl.LocalPlayer?.MyPhysics.Speed} {(Utils.isSpeedDefault() ? "(Default)" : "")}");
                }
            }
            catch (NullReferenceException) { }
        }

        var desiredLeft = GetLeftSubmenuCount(groupId);
        var leftCount = Mathf.Clamp(desiredLeft, 0, count);

        var leftSubmenus = group.submenus.GetRange(0, leftCount);
        foreach (var submenu in leftSubmenus)
        {
            GUILayout.Label(submenu.name, tabSubtitleStyle, GUILayout.Height(30));
            HorizontalDrawToggles(submenu.toggles);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (count > leftCount)
        {
            var rightSubmenus = group.submenus.GetRange(leftCount, count - leftCount);
            foreach (var submenu in rightSubmenus)
            {
                GUILayout.Label(submenu.name, tabSubtitleStyle, GUILayout.Height(30));
                HorizontalDrawToggles(submenu.toggles);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public void HorizontalDrawToggles(List<ToggleInfo> toggles)
    {
        foreach (var toggle in toggles)
        {
            var currentState = toggle.getState();
            var newState = GUILayout.Toggle(currentState, toggle.label, GUILayout.Height(20));
            if (newState != currentState)
                toggle.setState(newState);
        }
    }
}
