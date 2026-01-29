using System;
using UnityEngine;
using InnerNet;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System.IO;
using Hazel;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Sentry.Internal.Extensions;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace MalumMenu;

public static class Utils
{

    public static ReferenceDataManager referenceDataManager = DestroyableSingleton<ReferenceDataManager>.Instance;
    public static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
    public static bool isShip => ShipStatus.Instance;
    public static bool isLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;
    public static bool isOnlineGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool isLocalGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool isFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool isPlayer => PlayerControl.LocalPlayer;
    public static bool isHost => AmongUsClient.Instance && AmongUsClient.Instance.AmHost;
    public static bool isInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && isPlayer;
    public static bool isMeeting => MeetingHud.Instance;
    public static bool isMeetingVoting => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool isMeetingProceeding => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Proceeding;
    public static bool isExiling => ExileController.Instance && !(AirshipIsActive && SpawnInMinigame.Instance.isActiveAndEnabled);
    public static bool isAnySabotageActive => ShipStatus.Instance && SabotageSystem.AnyActive;
    public static bool isNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
    public static bool SkeldIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Skeld;
    public static bool MiraHQIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.MiraHQ;
    public static bool PolusIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Polus;
    public static bool DleksIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Dleks;
    public static bool AirshipIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Airship;
    public static bool FungleIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Fungle;

    public const float DefaultSpeed = 2.5f;
    public const float DefaultGhostSpeed = 3f;






    public static bool isSpeedDefault(bool forGhost = false)
    {
        return forGhost ? Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, DefaultGhostSpeed) :
            Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.Speed, DefaultSpeed);
    }






    public static void snapSpeedToDefault(float snapRange, bool forGhost = false)
    {
        if (forGhost)
        {
            PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed - DefaultGhostSpeed)
                                                             < snapRange ? DefaultGhostSpeed : PlayerControl.LocalPlayer.MyPhysics.GhostSpeed;
        }
        else
        {
            PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed - DefaultSpeed)
                                                        < snapRange ? DefaultSpeed : PlayerControl.LocalPlayer.MyPhysics.Speed;
        }
    }


    public static ClientData getClientByPlayer(PlayerControl player)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
            return client;
        }
        catch
        {
            return null;
        }
    }


    public static int getClientIdByPlayer(PlayerControl player)
    {
        if (player == null) return -1;
        var client = getClientByPlayer(player);
        return client == null ? -1 : client.Id;
    }


    public static bool isVanished(NetworkedPlayerInfo playerInfo)
    {
        PhantomRole phantomRole = playerInfo.Role as PhantomRole;

        if (phantomRole != null)
        {
            return phantomRole.fading || phantomRole.isInvisible;
        }

        return false;
    }


    public static bool isValidTarget(NetworkedPlayerInfo target)
    {
        var killAnyoneRequirements = target && !target.Disconnected && target.Object.Visible && target.PlayerId != PlayerControl.LocalPlayer.PlayerId && target.Role && target.Object;

        var fullRequirements = killAnyoneRequirements && !target.IsDead && !target.Object.inVent && !target.Object.inMovingPlat && target.Role.CanBeKilled;

        return CheatToggles.killAnyone ? killAnyoneRequirements : fullRequirements;
    }

    public static List<NetworkedPlayerInfo> GetAllPlayerData()
    {
        var playerDataList = new List<NetworkedPlayerInfo>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null)
            {
                playerDataList.Add(player.Data);
            }
        }

        return playerDataList;
    }



    public static void adjustResolution()
    {
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }


    public static RoleBehaviour getBehaviourByRoleType(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == roleType);
    }


    public static void murderPlayer(PlayerControl target, MurderResultFlags result)
    {
        if (isFreePlay)
        {

            PlayerControl.LocalPlayer.MurderPlayer(target, MurderResultFlags.Succeeded);
            return;

        }

        foreach (var item in PlayerControl.AllPlayerControls)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            writer.WriteNetObject(target);
            writer.Write((int)result);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }


    public static void reportDeadBody(NetworkedPlayerInfo playerData)
    {

        if (isFreePlay)
        {

            PlayerControl.LocalPlayer.CmdReportDeadBody(playerData);
            return;

        }

        var HostData = AmongUsClient.Instance.GetHost();
        if (HostData == null || HostData.Character.Data.Disconnected) return;
        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.ReportDeadBody, SendOption.None, HostData.Id);
        writer.Write(playerData.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }



    public static void completeMyTasks()
    {

        if (isFreePlay)
        {

            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
            }
            return;

        }

        var hostData = AmongUsClient.Instance.GetHost();
        if (hostData == null || hostData.Character.Data.Disconnected) return;
        {
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.IsComplete) continue;
                foreach (var item in PlayerControl.AllPlayerControls)
                {
                    var messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
                    messageWriter.WritePacked(task.Id);
                    AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                }
            }
        }
    }

    public static void completeTask(PlayerTask task)
    {
        if (isFreePlay)
        {
            PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
            return;
        }

        var hostData = AmongUsClient.Instance.GetHost();
        if (hostData == null || hostData.Character.Data.Disconnected) return;

        if (task.IsComplete) return;
        foreach (var item in PlayerControl.AllPlayerControls)
        {
            var messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            messageWriter.WritePacked(task.Id);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }


    public static void openChat()
    {
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }
        }

    }

    public static void drawTracer(GameObject sourceObject, GameObject targetObject, Color color)
    {
        var lineRenderer = sourceObject.GetComponent<LineRenderer>();

        if (!lineRenderer)
        {
            lineRenderer = sourceObject.AddComponent<LineRenderer>();
        }

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetWidth(0.02F, 0.02F);


        Material material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;

        lineRenderer.material = material;
        lineRenderer.SetColors(color, color);

        lineRenderer.SetPosition(0, sourceObject.transform.position);
        lineRenderer.SetPosition(1, targetObject.transform.position);
    }


    public static bool chatUiActive()
    {
        try
        {
            return CheatToggles.alwaysChat || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead;
        }
        catch
        {
            return false;
        }
    }


    public static void closeChat()
    {
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
        }
    }


    public static float getDistanceFrom(PlayerControl target, PlayerControl source = null)
    {

        if (source.IsNull())
        {
            source = PlayerControl.LocalPlayer;
        }

        Vector2 vector = target.GetTruePosition() - source.GetTruePosition();
        float magnitude = vector.magnitude;

        return magnitude;

    }


    public static System.Collections.Generic.List<PlayerControl> getPlayersSortedByDistance(PlayerControl source = null)
    {

        if (source.IsNull())
        {
            source = PlayerControl.LocalPlayer;
        }

        System.Collections.Generic.List<PlayerControl> outputList = new System.Collections.Generic.List<PlayerControl>();

        outputList.Clear();

        var allPlayers = GameData.Instance.AllPlayers;
        foreach (var t in allPlayers)
        {
            var player = t.Object;
            if (player)
            {
                outputList.Add(player);
            }
        }

        outputList = outputList.OrderBy(target => getDistanceFrom(target, source)).ToList();

        return outputList.Count <= 0 ? null : outputList;
    }


    public static byte getCurrentMapID()
    {

        if (isFreePlay)
        {
            return (byte)AmongUsClient.Instance.TutorialMapId;
        }


        if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return 0;
        return GameOptionsManager.Instance.currentGameOptions.MapId;
    }


    public static SystemTypes getCurrentRoom()
    {
        return HudManager.Instance.roomTracker.LastRoom.RoomId;
    }


    public static string getColoredPingText(int ping)
    {
        return ping switch
        {
            <= 100 => $"<color=#00ff00ff>PING: {ping} ms</color>",
            < 400 => $"<color=#ffff00ff>PING: {ping} ms</color>",
            _ => $"<color=#ff0000ff>PING: {ping} ms</color>"
        };
    }


    public static KeyCode stringToKeycode(string keyCodeStr)
    {

        if (!string.IsNullOrEmpty(keyCodeStr))
        {
            try
            {

                KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyCodeStr, true);

                return keyCode;
            }
            catch { }
        }

        return KeyCode.Delete;
    }


    public static bool stringToPlatformType(string platformStr, out Platforms? platform)
    {
        if (!string.IsNullOrEmpty(platformStr))
        {
            try
            {

                platform = (Platforms)Enum.Parse(typeof(Platforms), platformStr, true);

                return true;
            }
            catch { }
        }

        platform = null;
        return false;
    }

    public static string PlatformTypeToString(Platforms platform)
    {
        return platform switch
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
    }



    public static string getRoleName(NetworkedPlayerInfo playerData)
    {
        var translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(playerData.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        if (translatedRole != "STRMISS") return translatedRole;
        if (playerData.RoleWhenAlive.HasValue)
        {
            translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(getBehaviourByRoleType(playerData.RoleWhenAlive.Value).StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        }
        else
        {
            translatedRole = "Ghost";
        }

        return translatedRole;
    }


    public static string GetNameTag(NetworkedPlayerInfo playerInfo, string playerName, bool isChat = false)
    {
        var nameTag = playerName;

        if (playerInfo.Role.IsNull() || playerInfo.IsNull() || playerInfo.Disconnected ||
            playerInfo.Object.CurrentOutfit.IsNull()) return nameTag;

        var player = AmongUsClient.Instance.GetClientFromPlayerInfo(playerInfo);
        var host = AmongUsClient.Instance.GetHost();
        var level = playerInfo.PlayerLevel + 1;
        var platform = "Unknown";
        try { platform = PlatformTypeToString(player.PlatformData.Platform); } catch { }


        var roleColor = ColorUtility.ToHtmlStringRGB(playerInfo.Role.TeamColor);

        var hostString = player == host ? "Host - " : "";

        if (CheatToggles.seeRoles)
        {

            if (CheatToggles.showPlayerInfo)
            {
                if (isChat)
                {
                    nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{getRoleName(playerInfo)}</size></color> <size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>";
                    return nameTag;
                }

                nameTag =
                    $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n<color=#{roleColor}><size=70%>{getRoleName(playerInfo)}</size>\r\n{nameTag}</color>";
            }
            else
            {
                if (isChat)
                {
                    nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{getRoleName(playerInfo)}</size></color>";
                    return nameTag;
                }

                nameTag = $"<color=#{roleColor}><size=70%>{getRoleName(playerInfo)}</size>\r\n{nameTag}</color>";
            }
        }
        else
        {
            if (CheatToggles.showPlayerInfo)
            {
                if (PlayerControl.LocalPlayer.Data.Role.NameColor == playerInfo.Role.NameColor)
                {
                    if (isChat)
                    {
                        nameTag =
                            $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}</color> <size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>";
                        return nameTag;
                    }

                    nameTag =
                        $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}";
                }
                else
                {
                    if (isChat)
                    {
                        nameTag = $"{nameTag} <size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>";
                        return nameTag;
                    }

                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n{nameTag}";
                }
            }
            else
            {
                if (PlayerControl.LocalPlayer.Data.Role.NameColor != playerInfo.Role.NameColor || isChat)
                    return nameTag;

                nameTag = $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}</color>";
            }
        }

        return nameTag;
    }

    public static string GetRandomName()
    {

        var length = UnityEngine.Random.Range(1, 13);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
    }



    public static void showPopup(string text)
    {
        var popup = Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main!.transform);

        var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var size = background.size;
        size.x *= 2.5f;
        background.size = size;

        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(text);
    }

    public static void ShowNewPopup(string text)
    {
        DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(text);
    }



    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;

            Texture2D texture = LoadTextureFromResources(path);
            if (texture == null) return null;

            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            Debug.LogError($"Failed to load Sprite: {path}");
        }
        return null;
    }

    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();


            var stream = assembly.GetManifestResourceStream(path);


            if (stream == null)
            {
                var resourceNames = assembly.GetManifestResourceNames();
                var foundResource = resourceNames.FirstOrDefault(r => r.EndsWith(path) || r.Contains($".{path}"));

                if (!string.IsNullOrEmpty(foundResource))
                {
                    stream = assembly.GetManifestResourceStream(foundResource);
                    Debug.Log($"[MalumMenu] Found resource: {foundResource} (requested: {path})");
                }
                else
                {
                    Debug.LogWarning($"[MalumMenu] Resource not found: {path}. Available resources: {string.Join(", ", resourceNames)}");
                    return null;
                }
            }

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();

            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MalumMenu] Failed to read Texture: {path}. Error: {e.Message}");
        }
        return null;
    }

    public static void OpenConfigFile()
    {

        var configFilePath = Path.Combine(Paths.ConfigPath, "MalumMenu.cfg");

        if (File.Exists(configFilePath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = configFilePath,
                    UseShellExecute = true,
                    Verb = "edit"
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open config file: {ex.Message}. If you are on Linux, this is expected.");
            }
        }
        else
        {
            Debug.LogError("Config file does not exist.");
        }
    }

    public class PanicCleaner : MonoBehaviour
    {
        public static void Create()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PanicCleaner>();
            var go = new GameObject("MalumMenu_PanicCleaner");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PanicCleaner>();
        }

        private void LateUpdate()
        {
            try { Harmony.UnpatchID(MalumMenu.Id); }
            catch { }
            Destroy(gameObject);
        }
    }

    public static void Panic()
    {
        CheatToggles.DisableAll();
        ModManager.Instance.ModStamp.enabled = false;



        PanicCleaner.Create();
    }

    public static float CustomSlider(Rect position, float value, float min, float max)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event current = Event.current;

        
        
        Rect hitRect = new Rect(position.x, position.y - 5f, position.width, position.height + 15f);

        switch (current.type)
        {
            case EventType.MouseDown:
                if (hitRect.Contains(current.mousePosition) && current.button == 0)
                {
                    GUIUtility.hotControl = controlID;
                    value = Mathf.Clamp(min + (current.mousePosition.x - position.x) / position.width * (max - min), min, max);
                    GUI.changed = true;
                    current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    value = Mathf.Clamp(min + (current.mousePosition.x - position.x) / position.width * (max - min), min, max);
                    GUI.changed = true;
                    current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                    current.Use();
                }
                break;

            case EventType.Repaint:
                
                float trackHeight = 8f;
                Rect trackRect = new Rect(position.x, position.y + (position.height - trackHeight) / 2f, position.width, trackHeight);
                GUI.skin.horizontalSlider.Draw(trackRect, false, false, false, false);

                
                GUIStyle thumbStyle = GUI.skin.horizontalSliderThumb;

                float thumbWidth = 12f;
                float thumbHeight = 16f;

                float normalizedValue = (value - min) / (max - min);
                float availableWidth = position.width - thumbWidth;
                float thumbX = position.x + (normalizedValue * availableWidth);

                
                float thumbY = (position.y + (position.height - thumbHeight) / 2f) + 4.0f;

                Rect thumbRect = new Rect(thumbX, thumbY, thumbWidth, thumbHeight);
                thumbStyle.Draw(thumbRect, false, false, GUIUtility.hotControl == controlID, false);
                break;
        }

        return value;
    }

    public static float CustomLayoutSlider(float value, float min, float max, params GUILayoutOption[] options)
    {
        Rect position = GUILayoutUtility.GetRect(100f, 20f, GUI.skin.horizontalSlider, options);
        return CustomSlider(position, value, min, max);
    }
}
