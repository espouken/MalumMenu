using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MalumMenu;

public class RadarUI : MonoBehaviour
{
    public static RadarUI Instance;


    private const float BaseRadarWidth = 450f;
    private const float SkeldRadarWidth = 620f;
    private float currentRadarWidth = 450f;

    private const float RadarHeight = 350f;
    private const float PlayerDotSize = 20f;
    private const float DeadBodyDotSize = 18f;

    private Texture2D ventInTex, ventOutTex, killTex, tickTex, playTex, pauseTex;
    private Texture2D shiftIconWhiteTex, shiftIconRedTex;

    private List<RadarRound> allRounds = new();
    private RadarRound currentRound;
    private int viewingRoundIndex = -1;

    private float recordingStartTime;
    private float currentReplayTime = 0f;
    private float lastSliderValue = -1f;
    private bool isReplayPaused = false;
    private float replaySpeed = 1.0f;
    private bool isDraggingSlider = false;

    private float tracerDuration = 0f;



    private Dictionary<byte, List<Vector2>> cachedTracerPaths = new();
    private Dictionary<byte, Color> cachedTracerColors = new();
    private int lastFoundFrameIndex = -1;


    private float lastTracerTime = -1f;
    private float lastTracerDuration = -1f;
    private int lastTracerRoundIndex = -99;
    private int lastTracerFrameCount = -1;


    private static readonly List<Vector2> tempScreenPoints = new(256);


    private const int MaxTracerPoints = 150;
    private const float MinPointDistanceSq = 0.04f;

    public enum RadarEventType { Kill, VentIn, VentOut, Task, Shapeshift }
    public struct RadarEventData
    {
        public RadarEventType Type;
        public Vector2 Position;
        public float Time;
        public Color Color;
        public byte PlayerId;
    }

    public struct PlayerFrameData
    {
        public Vector2 Position;
        public bool IsDead;
        public bool IsImpostor;
        public byte PlayerId;
    }

    public struct BodyFrameData
    {
        public Vector2 Position;
        public byte ParentId;
    }

    public struct RadarFrame
    {
        public float Time;
        public List<PlayerFrameData> Players;
        public List<BodyFrameData> Bodies;
    }

    public class RadarRound
    {
        public List<RadarFrame> Frames = new();
        public List<RadarEventData> Events = new();
        public float StartTime;
        public float Duration => Frames.Count > 0 ? Frames[Frames.Count - 1].Time : 0f;
    }

    private float radarZoom = 15f;
    private bool fixedMapMode = true;

    private Rect radarWindowRect;
    private bool initialized;
    private bool texturesCreated;

    private Texture2D circleTex;
    private Texture2D circleOutlineTex;
    private Texture2D squareTex;
    private Texture2D lineTex; 
    private Texture2D crossTex;

    private Dictionary<byte, Texture2D> mapTextures = new();
    private Dictionary<byte, MapConfig> mapConfigs = new();

    private Texture2D playerIconTex;
    private Texture2D playerVisorTex;
    private Texture2D deadBodyIconTex;

    private Dictionary<Texture2D, GUIStyle> textureStyles = new();


    private GUIStyle radarButtonStyle;
    private GUIStyle radarLabelStyle;

    private int lastMapId = -1;

    private class MapConfig
    {
        public float WorldScale;
        public Vector2 Offset;
    }

    private void Start()
    {
        Instance = this;
        recordingStartTime = Time.time;
        currentRound = new RadarRound { StartTime = 0f };


        radarWindowRect = new Rect(Screen.width - BaseRadarWidth - 20, 20, BaseRadarWidth, RadarHeight + 110);
        InitializeMapConfigs();
        initialized = true;
    }

    private void InitializeMapConfigs()
    {

        mapConfigs[0] = new MapConfig { WorldScale = 22.5f, Offset = new Vector2(-2.0f, -6.0f) };
        mapConfigs[1] = new MapConfig { WorldScale = 11.0f, Offset = new Vector2(1, 2) };
        mapConfigs[2] = new MapConfig { WorldScale = 20.2f, Offset = new Vector2(20.5f, -12.9f) };
        mapConfigs[4] = new MapConfig { WorldScale = 18.0f, Offset = new Vector2(0, 5) };
        mapConfigs[5] = new MapConfig { WorldScale = 12.0f, Offset = new Vector2(0, 0) };
    }

    private void Update()
    {
        if (!Utils.isShip)
        {
            ResetRadarData();
            return;
        }

        int currentMapId = Utils.getCurrentMapID();
        if (lastMapId != -1 && lastMapId != currentMapId)
        {
            ResetRadarData();
        }
        lastMapId = currentMapId;

        if (Utils.isMeeting)
        {
            recordingStartTime += Time.deltaTime;
        }
        else
        {
            float relativeNow = Time.time - recordingStartTime;
            
            
            if (Time.frameCount % 10 == 0)
            {
                
                if (Time.frameCount % 60 == 0)
                {
                    var bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                    cachedBodies.Clear();
                    foreach (var body in bodies) cachedBodies.Add(body);
                }

                RecordFrame(relativeNow - currentRound.StartTime);
            }
        }

        float calculatedRelativeNow = Time.time - recordingStartTime;

        RadarRound activeRound = viewingRoundIndex >= 0 && viewingRoundIndex < allRounds.Count
            ? allRounds[viewingRoundIndex]
            : currentRound;

        float maxTime = (activeRound == currentRound) ? (calculatedRelativeNow - activeRound.StartTime) : activeRound.Duration;

        bool isLive = (activeRound == currentRound) && (currentReplayTime >= maxTime - 0.05f);

        if (!isLive && !isReplayPaused)
        {
            currentReplayTime += Time.deltaTime * replaySpeed;

            if (currentReplayTime > maxTime)
            {
                currentReplayTime = maxTime;
                if (activeRound != currentRound) isReplayPaused = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            fixedMapMode = !fixedMapMode;
        }

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            byte mapId = (byte)currentMapId;

            float speedMult = 1.0f;
            if (Input.GetKey(KeyCode.LeftShift)) speedMult = 5.0f;
            if (Input.GetKey(KeyCode.LeftControl)) speedMult = 0.05f;

            if (!fixedMapMode)
            {
                if (Input.GetKey(KeyCode.PageUp)) radarZoom -= 10f * Time.deltaTime * speedMult;
                if (Input.GetKey(KeyCode.PageDown)) radarZoom += 10f * Time.deltaTime * speedMult;
                radarZoom = Mathf.Clamp(radarZoom, 5f, 100f);
            }

            if (mapConfigs.ContainsKey(mapId))
            {
                MapConfig config = mapConfigs[mapId];

                float moveStep = 10.0f * Time.deltaTime * speedMult;
                if (Input.GetKey(KeyCode.UpArrow)) config.Offset.y += moveStep;
                if (Input.GetKey(KeyCode.DownArrow)) config.Offset.y -= moveStep;
                if (Input.GetKey(KeyCode.RightArrow)) config.Offset.x += moveStep;
                if (Input.GetKey(KeyCode.LeftArrow)) config.Offset.x -= moveStep;

                float scaleStep = 5.0f * Time.deltaTime * speedMult;
                if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus)) config.WorldScale += scaleStep;
                if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus)) config.WorldScale -= scaleStep;

                if (config.WorldScale < 0.1f) config.WorldScale = 0.1f;
            }
        }
    }

    private void ResetRadarData()
    {
        allRounds.Clear();
        currentRound = new RadarRound { StartTime = 0f };
        recordingStartTime = Time.time;
        viewingRoundIndex = -1;
        currentReplayTime = 0f;
        isReplayPaused = false;
        isReplayPaused = false;
        InvalidateTracerCache();
        cachedBodies.Clear(); 
    }

    private void InvalidateTracerCache()
    {
        lastTracerTime = -1f;
        lastTracerDuration = -1f;
        lastTracerRoundIndex = -99;
        lastTracerFrameCount = -1;
        cachedTracerPaths.Clear();
        cachedTracerColors.Clear();
    }

    
    private static readonly HashSet<DeadBody> cachedBodies = new();

    public static void AddBody(DeadBody body)
    {
        if (body != null) cachedBodies.Add(body);
    }

    public static void RemoveBody(DeadBody body)
    {
        if (body != null) cachedBodies.Remove(body);
    }
    

    public void OnMeetingEnd()
    {
        if (currentRound.Frames.Count > 0)
        {
            allRounds.Add(currentRound);
        }
        float newStartTime = Time.time - recordingStartTime;
        currentRound = new RadarRound { StartTime = newStartTime };
        viewingRoundIndex = -1;
        currentReplayTime = 0f;
        InvalidateTracerCache();
    }

    private void RecordFrame(float time)
    {
        RadarFrame frame = new RadarFrame
        {
            Time = time,
            Players = new List<PlayerFrameData>(),
            Bodies = new List<BodyFrameData>()
        };

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null) continue;
            frame.Players.Add(new PlayerFrameData
            {
                Position = p.GetTruePosition(),
                IsDead = p.Data.IsDead,
                IsImpostor = p.Data.Role != null && p.Data.Role.IsImpostor,
                PlayerId = p.PlayerId
            });
        }

        
        
        cachedBodies.RemoveWhere(b => b == null); 

        foreach (var deadBody in cachedBodies)
        {
            if (deadBody == null) continue;
            
            frame.Bodies.Add(new BodyFrameData
            {
                Position = deadBody.TruePosition,
                ParentId = deadBody.ParentId
            });
        }

        currentRound.Frames.Add(frame);
    }

    public void RegisterEvent(RadarEventType type, Vector2 pos, Color col, byte playerId = 255)
    {
        float time = Time.time - recordingStartTime - currentRound.StartTime;
        currentRound.Events.Add(new RadarEventData
        {
            Type = type,
            Position = pos,
            Time = time,
            Color = col,
            PlayerId = playerId
        });
    }

    private void CreateTextures()
    {

        if (texturesCreated && playerIconTex != null) return;

        LoadMapAsset(0, "skeld.png");
        LoadMapAsset(1, "mira_hq.png");
        LoadMapAsset(2, "polus.png");
        LoadMapAsset(4, "airship.png");
        LoadMapAsset(5, "fungle.png");

        playerIconTex = Utils.LoadTextureFromResources("player.png");
        playerVisorTex = Utils.LoadTextureFromResources("player_visor.png");
        deadBodyIconTex = Utils.LoadTextureFromResources("dead_body.png");

        shiftIconWhiteTex = Utils.LoadTextureFromResources("Shift_icon.png");
        shiftIconRedTex = Utils.LoadTextureFromResources("Shift_icon_1.png");

        circleTex = CreateCircleTexture(64, Color.white, true);
        circleOutlineTex = CreateCircleTexture(64, Color.white, false);

        
        squareTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        squareTex.SetPixel(0, 0, Color.white);
        squareTex.Apply();

        
        lineTex = CreateLineTexture(16); 

        crossTex = Utils.LoadTextureFromResources("cross.png");
        if (crossTex == null) crossTex = CreateCrossTexture(32);

        if (!textureStyles.ContainsKey(squareTex))
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = squareTex;
            textureStyles[squareTex] = style;
        }

        ventInTex = Utils.LoadTextureFromResources("vent_in.png");
        ventOutTex = Utils.LoadTextureFromResources("vent_out.png");
        killTex = Utils.LoadTextureFromResources("kill.png");
        tickTex = Utils.LoadTextureFromResources("tick.png");
        playTex = Utils.LoadTextureFromResources("play.png");
        pauseTex = Utils.LoadTextureFromResources("pause.png");

        texturesCreated = true;
    }

    private void LoadMapAsset(byte mapId, string fileName)
    {
        var tex = Utils.LoadTextureFromResources(fileName);
        if (tex != null)
        {
            mapTextures[mapId] = tex;
            GUIStyle style = new GUIStyle();
            style.normal.background = tex;
            textureStyles[tex] = style;
        }
    }

    private Texture2D CreateCircleTexture(int size, Color color, bool filled)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size / 2f;
        float center = size / 2f;
        Color transparent = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (filled)
                {
                    if (dist < radius - 2) tex.SetPixel(x, y, color);
                    else if (dist < radius) tex.SetPixel(x, y, new Color(color.r, color.g, color.b, (radius - dist) / 2f));
                    else tex.SetPixel(x, y, transparent);
                }
                else
                {
                    float ringWidth = 3f;
                    float innerRadius = radius - ringWidth;
                    if (dist >= innerRadius && dist < radius) tex.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.8f));
                    else tex.SetPixel(x, y, transparent);
                }
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    private Texture2D CreateCrossTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        Color red = new Color(1f, 0.2f, 0.2f, 0.9f);
        float center = size / 2f;
        float thickness = size / 6f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center);
                float dy = Mathf.Abs(y - center);
                if (Mathf.Abs(dx - dy) < thickness || Mathf.Abs(dx - (size - y)) < thickness)
                    tex.SetPixel(x, y, red);
                else
                    tex.SetPixel(x, y, transparent);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    public void OnGUI()
    {
        if (!CheatToggles.radarEnabled || MenuUI.isPanicked) return;
        if (!Utils.isPlayer || !Utils.isShip) return;


        byte mapId = Utils.getCurrentMapID();
        if (mapId == 0)
        {
            currentRadarWidth = SkeldRadarWidth;
        }
        else
        {
            currentRadarWidth = BaseRadarWidth;
        }

        if (!initialized)
        {
            radarWindowRect = new Rect(Screen.width - currentRadarWidth - 20, 20, currentRadarWidth, RadarHeight + 110);
            initialized = true;
        }
        else
        {

            radarWindowRect.width = currentRadarWidth;
            
            radarWindowRect.height = RadarHeight + 110;

            if (radarWindowRect.xMax > Screen.width)
            {
                radarWindowRect.x = Screen.width - currentRadarWidth - 20;
            }
        }

        CreateTextures();


        if (radarButtonStyle == null)
        {
            radarButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset() { left = 2, right = 2, top = 0, bottom = 0 },
                contentOffset = Vector2.zero,
                clipping = TextClipping.Overflow,
                wordWrap = false
            };
            radarButtonStyle.normal.textColor = Color.white;
        }

        if (radarLabelStyle == null)
        {
            radarLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Overflow,
                wordWrap = false
            };
            radarLabelStyle.normal.textColor = Color.white;
        }

        Color prevBgColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.92f);

        radarWindowRect = GUI.Window(999, radarWindowRect, (GUI.WindowFunction)RadarWindowFunction, "Radar");

        GUI.backgroundColor = prevBgColor;
    }

    private void RadarWindowFunction(int windowID)
    {
        try
        {
            float contentWidth = currentRadarWidth - 10f;
            float contentHeight = RadarHeight - 10f;
            float centerX = currentRadarWidth / 2f;
            float centerY = 20f + (contentHeight / 2f);

            float renderScale;
            byte mapId = Utils.getCurrentMapID();
            Vector2 cameraCenterPos;

            if (fixedMapMode)
            {
                if (mapConfigs.ContainsKey(mapId))
                {
                    cameraCenterPos = mapConfigs[mapId].Offset;
                    renderScale = 14f;
                    if (mapId == 2 || mapId == 4) renderScale = 9f;
                }
                else
                {
                    cameraCenterPos = Vector2.zero;
                    renderScale = 10f;
                }
            }
            else
            {
                cameraCenterPos = PlayerControl.LocalPlayer.GetTruePosition();
                renderScale = (Mathf.Min(contentWidth, contentHeight) / 2f) / radarZoom;
            }

            if (CheatToggles.radarShowMap)
            {
                DrawMapBackground(centerX, centerY, renderScale, cameraCenterPos);
            }

            if (!fixedMapMode) DrawCenterMarker(centerX, centerY);


            RadarRound activeRound = viewingRoundIndex >= 0 && viewingRoundIndex < allRounds.Count
                ? allRounds[viewingRoundIndex]
                : currentRound;

            float maxTime = activeRound.Duration;
            float relativeNow = Time.time - recordingStartTime - activeRound.StartTime;

            bool isLive = viewingRoundIndex < 0 && (currentReplayTime >= maxTime - 0.1f || maxTime < 0.1f);
            float displayTime = isLive ? relativeNow : currentReplayTime;

            Rect bounds = new Rect(0, 20, currentRadarWidth, RadarHeight);

            if (tracerDuration > 0f)
            {
                DrawTracers(centerX, centerY, renderScale, cameraCenterPos, displayTime, activeRound, tracerDuration, bounds);
            }

            if (isLive && activeRound == currentRound && currentRound.Frames.Count > 0)
            {
                DrawReplayFrame(centerX, centerY, renderScale, cameraCenterPos, displayTime, activeRound, true);
                DrawLocalPlayer(centerX, centerY, renderScale, cameraCenterPos);
            }
            else
            {
                DrawReplayFrame(centerX, centerY, renderScale, cameraCenterPos, displayTime, activeRound, false);
            }

            DrawEvents(centerX, centerY, renderScale, cameraCenterPos, displayTime, activeRound);

            DrawReplayControls(RadarHeight, activeRound, maxTime, isLive);



            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (mapConfigs.ContainsKey(mapId))
                {
                    var cfg = mapConfigs[mapId];
                    string info = $"SCALE: {cfg.WorldScale:F1} | OFF: {cfg.Offset.x:F1}, {cfg.Offset.y:F1}";
                    GUI.Label(new Rect(5, 20, currentRadarWidth, 20), info, radarLabelStyle);
                }
            }
        }
        catch { }

        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    private void DrawTextureSafe(Rect rect, Texture2D texture, Color color)
    {
        if (texture == null) return;
        Color prev = GUI.color;
        GUI.color = color;
        if (!textureStyles.TryGetValue(texture, out GUIStyle style) || style.normal.background == null)
        {
            style = new GUIStyle();
            style.normal.background = texture;
            textureStyles[texture] = style;
        }
        GUI.Box(rect, GUIContent.none, style);
        GUI.color = prev;
    }


    private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 d = end - start;
        float a = Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x);
        if (a < 0) a += 360;

        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(a, start);

        float overlapLength = d.magnitude + (width * 0.5f);

        DrawTextureSafe(new Rect(start.x, start.y - width / 2f, overlapLength, width), squareTex, color);
        GUI.matrix = matrixBackup;
    }

    private void DrawMapBackground(float centerX, float centerY, float renderScale, Vector2 cameraCenterPos)
    {
        byte mapId = Utils.getCurrentMapID();
        if (!mapTextures.ContainsKey(mapId) || mapTextures[mapId] == null) return;

        Texture2D mapTex = mapTextures[mapId];

        MapConfig config = mapConfigs.ContainsKey(mapId) ? mapConfigs[mapId] : new MapConfig { WorldScale = 12.0f, Offset = Vector2.zero };

        float widthOnScreen = (mapTex.width / config.WorldScale) * renderScale;
        float heightOnScreen = (mapTex.height / config.WorldScale) * renderScale;

        Vector2 relativePos = config.Offset - cameraCenterPos;

        float screenX = centerX + (relativePos.x * renderScale);
        float screenY = centerY - (relativePos.y * renderScale);

        Rect mapRect = new Rect(
            screenX - (widthOnScreen / 2f),
            screenY - (heightOnScreen / 2f),
            widthOnScreen,
            heightOnScreen
        );

        if (mapRect.xMax < 0 || mapRect.x > currentRadarWidth || mapRect.yMax < 0 || mapRect.y > RadarHeight) return;

        DrawTextureSafe(mapRect, mapTex, new Color(0.9f, 0.9f, 0.9f, 0.75f));
    }

    private void DrawCenterMarker(float centerX, float centerY)
    {
        Color markerColor = new Color(0.4f, 0.4f, 0.5f, 0.3f);
        DrawTextureSafe(new Rect(centerX - 1, centerY - 8, 2, 16), squareTex, markerColor);
        DrawTextureSafe(new Rect(centerX - 8, centerY - 1, 16, 2), squareTex, markerColor);
    }

    private void DrawLocalPlayer(float centerX, float centerY, float scale, Vector2 cameraCenterPos)
    {
        if (PlayerControl.LocalPlayer?.Data == null) return;

        Vector2 playerPos = PlayerControl.LocalPlayer.GetTruePosition();
        Vector2 relative = playerPos - cameraCenterPos;
        float x = centerX + relative.x * scale;
        float y = centerY - relative.y * scale;

        Rect bounds = new Rect(0, 20, currentRadarWidth, RadarHeight);
        if (!bounds.Contains(new Vector2(x, y))) return;

        Color bodyColor = GetPlayerDisplayColor(PlayerControl.LocalPlayer);

        DrawPlayerIcon(new Vector2(x, y), PlayerDotSize, bodyColor, false);
    }

    private void DrawPlayerIcon(Vector2 centerPos, float size, Color color, bool isDead)
    {
        Rect rect = new Rect(centerPos.x - size / 2, centerPos.y - size / 2, size, size);
        Texture2D icon = (playerIconTex != null) ? playerIconTex : circleTex;

        DrawTextureSafe(rect, icon, color);

        if (playerVisorTex != null)
        {
            Color visorCol = Color.white;
            if (isDead) visorCol.a = 0.6f;
            DrawTextureSafe(rect, playerVisorTex, visorCol);
        }

        if (playerIconTex == null)
        {
            DrawTextureSafe(new Rect(centerPos.x - size / 2 - 1, centerPos.y - size / 2 - 1, size + 2, size + 2), circleOutlineTex, new Color(color.r, color.g, color.b, 0.8f));
        }
    }

    private void DrawPlayerDot(Vector2 pos, float size, Color color, bool isDead)
    {
        DrawPlayerIcon(pos, size, color, isDead);

        if (isDead)
        {
            DrawTextureSafe(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), crossTex, new Color(1f, 0.2f, 0.2f, 0.9f));
        }
    }

    private void DrawDeadBodyDot(Vector2 pos, float size, Color color)
    {
        Rect rect = new Rect(pos.x - size / 2, pos.y - size / 2, size, size);
        Texture2D icon = (deadBodyIconTex != null) ? deadBodyIconTex : circleTex;
        DrawTextureSafe(rect, icon, color);

        if (deadBodyIconTex == null)
        {
            DrawTextureSafe(rect, crossTex, new Color(1f, 0.1f, 0.1f, 0.9f));
        }
    }

    private bool ShouldShowPlayer(PlayerControl player)
    {
        if (player.Data == null || player.Data.Role == null) return false;
        if (player.Data.Disconnected) return false;

        bool isDead = player.Data.IsDead;
        bool isImpostor = player.Data.Role.IsImpostor;

        if (isDead) return CheatToggles.radarGhosts;
        if (isImpostor) return CheatToggles.radarImps;
        return CheatToggles.radarCrew;
    }

    private Color GetPlayerDisplayColor(PlayerControl player)
    {
        if (player?.Data == null) return Color.white;

        if (CheatToggles.radarColorBased)
        {
            return GetColorById(player.PlayerId);
        }
        else
        {
            if (player.Data.IsDead) return new Color(0.6f, 0.6f, 0.6f, 1f);
            if (player.Data.Role != null && player.Data.Role.IsImpostor) return new Color(1f, 0.2f, 0.2f, 1f);
            return new Color(0.2f, 0.6f, 1f, 1f);
        }
    }

    private Color GetPlayerDisplayColorFromFrame(PlayerFrameData p)
    {
        if (CheatToggles.radarColorBased)
        {
            return GetColorById(p.PlayerId);
        }
        else
        {
            if (p.IsDead) return new Color(0.6f, 0.6f, 0.6f, 1f);
            if (p.IsImpostor) return new Color(1f, 0.2f, 0.2f, 1f);
            return new Color(0.2f, 0.6f, 1f, 1f);
        }
    }

    private Color GetColorById(byte playerId)
    {
        NetworkedPlayerInfo data = GameData.Instance?.GetPlayerById(playerId);
        if (data == null) return Color.gray;

        int colorId = data.DefaultOutfit.ColorId;
        if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
        {
            Color32 c = Palette.PlayerColors[colorId];
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f, 1f);
        }
        return Color.gray;
    }

    private void DrawReplayFrame(float centerX, float centerY, float scale, Vector2 camPos, float time, RadarRound round, bool skipLocalPlayer)
    {
        int frameIndex = FindFrameIndex(round, time);
        if (frameIndex < 0 || frameIndex >= round.Frames.Count) return;

        lastFoundFrameIndex = frameIndex;
        RadarFrame bestFrame = round.Frames[frameIndex];

        if (bestFrame.Players == null) return;

        Rect bounds = new Rect(0, 20, currentRadarWidth, RadarHeight);

        if (CheatToggles.radarBodies && bestFrame.Bodies != null)
        {
            foreach (var b in bestFrame.Bodies)
            {
                Vector2 relative = b.Position - camPos;
                float x = centerX + relative.x * scale;
                float y = centerY - relative.y * scale;
                if (!bounds.Contains(new Vector2(x, y))) continue;

                Color col = GetColorById(b.ParentId);
                DrawDeadBodyDot(new Vector2(x, y), DeadBodyDotSize, col);
            }
        }

        foreach (var p in bestFrame.Players)
        {
            if (skipLocalPlayer && PlayerControl.LocalPlayer && p.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
            if (p.IsDead && !CheatToggles.radarGhosts) continue;
            if (!p.IsDead && p.IsImpostor && !CheatToggles.radarImps) continue;
            if (!p.IsDead && !p.IsImpostor && !CheatToggles.radarCrew) continue;

            Vector2 relative = p.Position - camPos;
            float x = centerX + relative.x * scale;
            float y = centerY - relative.y * scale;
            if (!bounds.Contains(new Vector2(x, y))) continue;

            Color col = GetPlayerDisplayColorFromFrame(p);
            if (p.IsDead) col.a = 0.5f;

            DrawPlayerDot(new Vector2(x, y), PlayerDotSize, col, p.IsDead);
        }
    }


    private int FindFrameIndex(RadarRound round, float targetTime)
    {
        if (round.Frames.Count == 0) return -1;
        if (round.Frames.Count == 1) return 0;

        int left = 0;
        int right = round.Frames.Count - 1;


        while (left < right)
        {
            int mid = (left + right + 1) / 2;
            if (round.Frames[mid].Time <= targetTime)
                left = mid;
            else
                right = mid - 1;
        }


        if (left < round.Frames.Count - 1)
        {
            float diffLeft = Mathf.Abs(round.Frames[left].Time - targetTime);
            float diffRight = Mathf.Abs(round.Frames[left + 1].Time - targetTime);
            if (diffRight < diffLeft) return left + 1;
        }

        return left;
    }

    private void DrawTracers(float centerX, float centerY, float scale, Vector2 camPos, float time, RadarRound round, float duration, Rect bounds)
    {
        if (round.Frames.Count < 2) return;

        float startTime = time - duration;
        int roundIndex = viewingRoundIndex;
        int frameCount = round.Frames.Count;


        bool needsRebuild = Mathf.Abs(time - lastTracerTime) > 0.05f ||
                           Mathf.Abs(duration - lastTracerDuration) > 0.01f ||
                           roundIndex != lastTracerRoundIndex ||
                           frameCount != lastTracerFrameCount;

        if (needsRebuild)
        {
            lastTracerTime = time;
            lastTracerDuration = duration;
            lastTracerRoundIndex = roundIndex;
            lastTracerFrameCount = frameCount;


            foreach (var kvp in cachedTracerPaths)
            {
                kvp.Value.Clear();
            }


            int endFrameIdx = FindFrameIndex(round, time);
            int startFrameIdx = FindFrameIndex(round, startTime);

            if (endFrameIdx < 0) endFrameIdx = 0;
            if (startFrameIdx < 0) startFrameIdx = 0;


            for (int i = endFrameIdx; i >= startFrameIdx; i--)
            {
                RadarFrame frame = round.Frames[i];
                if (frame.Time > time) continue;
                if (frame.Time < startTime) break;

                foreach (var p in frame.Players)
                {
                    if (p.IsDead && !CheatToggles.radarGhosts) continue;
                    if (!p.IsDead && p.IsImpostor && !CheatToggles.radarImps) continue;
                    if (!p.IsDead && !p.IsImpostor && !CheatToggles.radarCrew) continue;

                    if (!cachedTracerPaths.TryGetValue(p.PlayerId, out List<Vector2> path))
                    {
                        path = new List<Vector2>(MaxTracerPoints);
                        cachedTracerPaths[p.PlayerId] = path;
                        cachedTracerColors[p.PlayerId] = GetPlayerDisplayColorFromFrame(p);
                    }



                    if (path.Count == 0)
                    {
                        path.Add(p.Position);
                    }
                    else
                    {
                        Vector2 lastPos = path[path.Count - 1];
                        float dx = p.Position.x - lastPos.x;
                        float dy = p.Position.y - lastPos.y;


                        float distSq = dx * dx + dy * dy;
                        if (distSq > MinPointDistanceSq)
                        {

                            if (path.Count >= MaxTracerPoints)
                            {
                                path.RemoveAt(0);
                            }
                            path.Add(p.Position);
                        }
                    }
                }
            }
        }


        foreach (var kvp in cachedTracerPaths)
        {
            List<Vector2> points = kvp.Value;
            if (points.Count < 2) continue;

            Color c = cachedTracerColors[kvp.Key];
            c.a = 0.6f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 p1 = points[i];
                Vector2 p2 = points[i + 1];

                float dx = p1.x - p2.x;
                float dy = p1.y - p2.y;
                float sqDist = dx * dx + dy * dy;

                if (sqDist > 16f) continue;
                if (sqDist < 0.0001f) continue;
                if (dx * dx + dy * dy > 16f) continue;

                float sx1 = centerX + (p1.x - camPos.x) * scale;
                float sy1 = centerY - (p1.y - camPos.y) * scale;
                float sx2 = centerX + (p2.x - camPos.x) * scale;
                float sy2 = centerY - (p2.y - camPos.y) * scale;


                if ((sx1 < 0 && sx2 < 0) || (sx1 > currentRadarWidth && sx2 > currentRadarWidth)) continue;
                if ((sy1 < 20 && sy2 < 20) || (sy1 > RadarHeight + 20 && sy2 > RadarHeight + 20)) continue;

                DrawLineFast(sx1, sy1, sx2, sy2, c, 3.5f);
            }
        }
    }

    
    private Texture2D CreateLineTexture(int height)
    {
        Texture2D tex = new Texture2D(4, height, TextureFormat.RGBA32, false);
        float center = (height - 1) / 2f;
        
        for (int y = 0; y < height; y++)
        {
            float distFromCenter = Mathf.Abs(y - center);
            float alpha = 1f - (distFromCenter / center);
            alpha = Mathf.Pow(alpha, 0.5f); 
            Color c = new Color(1f, 1f, 1f, alpha);
            for (int x = 0; x < 4; x++)
            {
                tex.SetPixel(x, y, c);
            }
        }
        
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear; 
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private void DrawLineFast(float x1, float y1, float x2, float y2, Color color, float width)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        float length = Mathf.Sqrt(dx * dx + dy * dy);

        if (length < 0.5f) return;

        
        
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, new Vector2(x1, y1));
        
        
        float extend = width * 0.25f;
        
        
        Texture2D tex = (lineTex != null) ? lineTex : squareTex;
        
        
        DrawTextureSafe(new Rect(x1 - extend, y1 - width * 0.5f, length + extend * 2, width), tex, color);
        
        GUI.matrix = matrixBackup;
    }

    private void DrawEvents(float centerX, float centerY, float scale, Vector2 camPos, float currentTime, RadarRound round)
    {
        Rect bounds = new Rect(0, 20, currentRadarWidth, RadarHeight);

        float killDuration = 1.0f;
        float ventDuration = 1.5f;
        float taskDuration = 1.5f;

        foreach (var evt in round.Events)
        {
            float duration = evt.Type == RadarEventType.Kill ? killDuration :
                             (evt.Type == RadarEventType.Task ? taskDuration : ventDuration);

            if (evt.Time <= currentTime && evt.Time > currentTime - duration)
            {
                if (evt.Type == RadarEventType.Kill && !CheatToggles.radarShowKills) continue;
                if ((evt.Type == RadarEventType.VentIn || evt.Type == RadarEventType.VentOut) && !CheatToggles.radarShowVents) continue;
                if (evt.Type == RadarEventType.Task && !CheatToggles.radarShowTasks) continue;
                if (evt.Type == RadarEventType.Shapeshift && !CheatToggles.radarShowShapeshift) continue;

                Vector2 eventPos = evt.Position;
                if (evt.Type == RadarEventType.Task && evt.PlayerId != 255)
                {
                    eventPos = GetPlayerPositionAtTime(round, evt.Time, evt.PlayerId, evt.Position);
                }

                Vector2 relative = eventPos - camPos;
                float x = centerX + relative.x * scale;
                float y = centerY - relative.y * scale;

                if (!bounds.Contains(new Vector2(x, y))) continue;

                float age = currentTime - evt.Time;
                float alpha = 1.0f - (age / duration);
                Color c = evt.Color;
                c.a = alpha;

                Texture2D icon = null;
                switch (evt.Type)
                {
                    case RadarEventType.Kill: icon = crossTex; break;
                    case RadarEventType.VentIn: icon = (ventInTex != null) ? ventInTex : circleTex; break;
                    case RadarEventType.VentOut: icon = (ventOutTex != null) ? ventOutTex : circleTex; break;
                    case RadarEventType.Task: icon = (tickTex != null) ? tickTex : squareTex; break;
                    case RadarEventType.Shapeshift:

                        if (shiftIconWhiteTex != null && shiftIconRedTex != null)
                        {
                            DrawTextureSafe(new Rect(x - 10, y - 10, 20, 20), shiftIconWhiteTex, c);



                            Color overlayColor = Color.white;
                            overlayColor.a = c.a;
                            DrawTextureSafe(new Rect(x - 10, y - 10, 20, 20), shiftIconRedTex, overlayColor);
                            icon = null;
                        }
                        else
                        {
                            icon = squareTex;
                        }
                        break;
                }

                if (icon != null)
                {
                    DrawTextureSafe(new Rect(x - 10, y - 10, 20, 20), icon, c);
                }
            }
        }
    }

    private Vector2 GetPlayerPositionAtTime(RadarRound round, float time, byte playerId, Vector2 fallback)
    {
        int frameIndex = FindFrameIndex(round, time);
        if (frameIndex < 0 || frameIndex >= round.Frames.Count) return fallback;

        RadarFrame bestFrame = round.Frames[frameIndex];
        if (bestFrame.Players == null) return fallback;

        foreach (var p in bestFrame.Players)
        {
            if (p.PlayerId == playerId)
            {
                return p.Position;
            }
        }

        return fallback;
    }

    private void DrawReplayControls(float height, RadarRound activeRound, float maxTime, bool isLiveNow)
    {
        float width = currentRadarWidth;
        float controlsStartY = height + 5;

        float sliderMaxTime = viewingRoundIndex < 0 ? (Time.time - recordingStartTime - currentRound.StartTime) : activeRound.Duration;
        if (sliderMaxTime < 0.1f) sliderMaxTime = 0.1f;

        float row1Y = controlsStartY;
        float row2Y = controlsStartY + 25;
        float row3Y = controlsStartY + 50;

        if (allRounds.Count > 0)
        {
            string roundText = viewingRoundIndex < 0 ? $"R{allRounds.Count + 1}" : $"R{viewingRoundIndex + 1}";

            if (GUI.Button(new Rect(5, row1Y, 50, 18), "< Prev", radarButtonStyle) && (viewingRoundIndex > 0 || viewingRoundIndex < 0))
            {
                viewingRoundIndex = viewingRoundIndex < 0 ? allRounds.Count - 1 : viewingRoundIndex - 1;
                currentReplayTime = 0f;
            }
            GUI.Label(new Rect(60, row1Y, 35, 18), roundText, radarLabelStyle);
            if (GUI.Button(new Rect(95, row1Y, 50, 18), "Next >", radarButtonStyle) && viewingRoundIndex < allRounds.Count)
            {
                viewingRoundIndex++;
                if (viewingRoundIndex >= allRounds.Count) viewingRoundIndex = -1;
                currentReplayTime = 0f;
            }
            if (viewingRoundIndex >= 0)
            {
                if (GUI.Button(new Rect(150, row1Y, 45, 18), "LIVE", radarButtonStyle))
                {
                    viewingRoundIndex = -1;
                    currentReplayTime = sliderMaxTime;
                    isReplayPaused = false;
                }
            }
            else
            {
               
               GUIUtility.GetControlID(FocusType.Passive);
            }
        }
        else
        {
            GUI.Label(new Rect(5, row1Y, 150, 18), "Waiting for round...", radarLabelStyle);
        }

        if (GUI.Button(new Rect(width - 55, row1Y, 50, 18), "CLR", radarButtonStyle))
        {
            ResetRadarData();
        }

        GUI.Label(new Rect(5, row2Y, 100, 18), $"Tracers: {(int)(tracerDuration * 1000)}ms", radarLabelStyle);
        tracerDuration = Utils.CustomSlider(new Rect(110, row2Y + 5, width - 120, 12), tracerDuration, 0f, 3f);

        string timeText;
        if (isLiveNow)
        {
            timeText = "LIVE";
        }
        else
        {
            int mins = (int)(currentReplayTime / 60f);
            int secs = (int)(currentReplayTime % 60f);
            timeText = $"{mins:D2}:{secs:D2}";
        }

        Color oldBg = GUI.backgroundColor;
        GUI.backgroundColor = isLiveNow ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.7f, 0.2f);

        if (GUI.Button(new Rect(5, row3Y, 45, 18), timeText, radarButtonStyle))
        {
            viewingRoundIndex = -1;
            currentReplayTime = sliderMaxTime;
            isReplayPaused = false;
        }

        GUI.backgroundColor = oldBg;

        if (!isLiveNow)
        {
            bool togglePause = false;
            if (isReplayPaused)
            {
                if (playTex != null)
                    togglePause = GUI.Button(new Rect(55, row3Y, 25, 18), playTex, radarButtonStyle);
                else
                    togglePause = GUI.Button(new Rect(55, row3Y, 25, 18), ">", radarButtonStyle);
            }
            else
            {
                if (pauseTex != null)
                    togglePause = GUI.Button(new Rect(55, row3Y, 25, 18), pauseTex, radarButtonStyle);
                else
                    togglePause = GUI.Button(new Rect(55, row3Y, 25, 18), "||", radarButtonStyle);
            }

            if (togglePause)
            {
                isReplayPaused = !isReplayPaused;
            }
        }
        else
        {
             
             GUIUtility.GetControlID(FocusType.Passive);
        }

        float sliderX = isLiveNow ? 55 : 85;
        float sliderWidth = width - sliderX - 70;

        float newVal = Utils.CustomSlider(new Rect(sliderX, row3Y + 5, sliderWidth, 12), currentReplayTime, 0f, sliderMaxTime);

        if (Mathf.Abs(newVal - currentReplayTime) > 0.0001f)
        {
            
            isReplayPaused = true;
            isDraggingSlider = true;
            currentReplayTime = newVal;
        }
        else if (isLiveNow)
        {
            currentReplayTime = sliderMaxTime;
        }

        if (isDraggingSlider && Input.GetMouseButtonUp(0))
        {
            isDraggingSlider = false;
            isReplayPaused = false;
        }

        lastSliderValue = newVal;

        string viewMode = fixedMapMode ? "FIXED" : "FOLLOW";
        if (GUI.Button(new Rect(width - 65, row3Y, 60, 18), viewMode, radarButtonStyle))
        {
            fixedMapMode = !fixedMapMode;
        }

        
        float row4Y = controlsStartY + 75;
        GUI.Label(new Rect(5, row4Y, 40, 18), "Speed:", radarLabelStyle);

        float btnW = 30f;
        float startX = 50f;
        
        float[] speeds = { 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };
        foreach (float s in speeds)
        {
            Color originalColor = GUI.backgroundColor;
            if (Mathf.Abs(replaySpeed - s) < 0.01f) GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f); 

            if (GUI.Button(new Rect(startX, row4Y, btnW, 18), s.ToString("0.##").Replace(',','.'), radarButtonStyle))
            {
                replaySpeed = s;
            }
            GUI.backgroundColor = originalColor;
            startX += btnW + 2;
        }
    }
}
