using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MalumMenu;

public class DoorsUI : MonoBehaviour
{
    private Rect _windowRect = new(320, 10, 530, 320);
    private GUIStyle _separatorStyle;
    private GUIStyle _normalButtonStyle;
    private GUIStyle _normalToggleStyle;
    private List<SystemTypes> doorsToSpamOpen = new();
    private List<SystemTypes> doorsToSpamClose = new();

    private bool _waitingForBind = false;
    private float _bindReadyTime = 0f;

    private void OnGUI()
    {
        if (!CheatToggles.showDoorsMenu) return;

        _separatorStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture },
            margin = new RectOffset { top = 4, bottom = 4 },
            padding = new RectOffset(),
            border = new RectOffset()
        };
        _normalButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 13
        };
        _normalToggleStyle ??= new GUIStyle(GUI.skin.toggle)
        {
            fontSize = 13
        };

        if (ColorUtility.TryParseHtmlString(MalumMenu.menuHtmlColor.Value, out var configUIColor))
        {
            GUI.backgroundColor = configUIColor;
        }

        _windowRect = GUI.Window(2, _windowRect, (GUI.WindowFunction)DoorsWindow, "Doors");
    }

    private void DoorsWindow(int windowID)
    {
        if (_waitingForBind)
        {
            Event e = Event.current;
            if (e.isKey || e.isMouse)
            {
                KeyCode key = KeyCode.None;
                if (e.isKey && e.keyCode != KeyCode.None)
                {
                    key = e.keyCode;
                }
                else if (e.isMouse)
                {
                    key = (KeyCode)((int)KeyCode.Mouse0 + e.button);
                }

                if (key != KeyCode.None)
                {
                    CheatToggles.closeRoomBind = key;
                    _waitingForBind = false;
                    _bindReadyTime = Time.realtimeSinceStartup + 0.5f;
                    e.Use();
                }
            }
        }

        if (!Utils.isShip)
        {
            GUI.DragWindow();
            return;
        }

        var map = (MapNames)Utils.getCurrentMapID();

        if (map is MapNames.MiraHQ)
        {
            GUI.DragWindow();
            return;
        }

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.BeginVertical();
        GUILayout.Label("Keybinds (Any Key/Mouse):");

        GUILayout.BeginHorizontal();
        string bindText = _waitingForBind ? "Press any key..." : $"Bind Close Room: {CheatToggles.closeRoomBind}";
        if (GUILayout.Button(bindText, _normalButtonStyle))
        {
            _waitingForBind = !_waitingForBind;
        }
        if (GUILayout.Button("X", _normalButtonStyle, GUILayout.Width(25)))
        {
            CheatToggles.closeRoomBind = KeyCode.None;
            _waitingForBind = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        foreach (var doorRoom in DoorsHandler.GetDoorRooms())
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{doorRoom.ToString()}", GUILayout.Width(120f));

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Status: {DoorsHandler.GetStatusOfDoorsInRoom(doorRoom, true)}");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", _normalButtonStyle, GUILayout.Width(50f)))
            {
                DoorsHandler.CloseDoorsOfRoom(doorRoom);
            }

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                if (GUILayout.Button("Open", _normalButtonStyle, GUILayout.Width(50f)))
                {
                    DoorsHandler.OpenDoorsOfRoom(doorRoom);
                }
            }

            if (Utils.isHost)
            {
                var spamClose = doorsToSpamClose.Contains(doorRoom);
                spamClose = GUILayout.Toggle(spamClose, "Spam Close", _normalToggleStyle);
                if (spamClose && !doorsToSpamClose.Contains(doorRoom))
                    doorsToSpamClose.Add(doorRoom);
                else if (!spamClose && doorsToSpamClose.Contains(doorRoom))
                    doorsToSpamClose.Remove(doorRoom);

                if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
                {
                    var spamOpen = doorsToSpamOpen.Contains(doorRoom);
                    spamOpen = GUILayout.Toggle(spamOpen, "Spam Open", _normalToggleStyle);
                    if (spamOpen && !doorsToSpamOpen.Contains(doorRoom))
                        doorsToSpamOpen.Add(doorRoom);
                    else if (!spamOpen && doorsToSpamOpen.Contains(doorRoom))
                        doorsToSpamOpen.Remove(doorRoom);
                }
            }
            else
            {

                if (doorsToSpamClose.Count != 0 || doorsToSpamOpen.Count != 0)
                {
                    doorsToSpamClose.Clear();
                    doorsToSpamOpen.Clear();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        GUILayout.FlexibleSpace();
        GUILayout.Box("", _separatorStyle, GUILayout.Height(1f), GUILayout.ExpandWidth(true));
        GUILayout.Box("", GUIStyle.none, GUILayout.Height(1f), GUILayout.ExpandWidth(true));

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Close All", _normalButtonStyle))
        {
            CheatToggles.closeAllDoors = true;
        }

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            if (GUILayout.Button("Open All", _normalButtonStyle))
            {
                CheatToggles.openAllDoors = true;
            }
        }

        GUILayout.FlexibleSpace();

        if (Utils.isHost)
        {
            CheatToggles.spamCloseAllDoors = GUILayout.Toggle(CheatToggles.spamCloseAllDoors, "Spam Close All", _normalToggleStyle);

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                CheatToggles.spamOpenAllDoors = GUILayout.Toggle(CheatToggles.spamOpenAllDoors, "Spam Open All", _normalToggleStyle);
            }
        }
        else
        {
            CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = false;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void Update()
    {
        if (!Utils.isShip) return;

        
        
        
        bool isChatOpen = false;
        if (HudManager.Instance && HudManager.Instance.Chat)
        {
            isChatOpen = HudManager.Instance.Chat.IsOpenOrOpening;
        }

        if (!isChatOpen && !_waitingForBind && Time.realtimeSinceStartup > _bindReadyTime)
        {
            KeyCode bind = CheatToggles.closeRoomBind;
            if (bind != KeyCode.None && (Input.GetKeyDown(bind) || (bind >= KeyCode.Mouse0 && bind <= KeyCode.Mouse6 && Input.GetKeyDown(bind))))
            {
                var currentRoom = Utils.getCurrentRoom();
                DoorsHandler.CloseDoorsOfRoom(currentRoom);

                try
                {
                    var doorsInRoom = DoorsHandler.GetDoorsInRoom(currentRoom);
                    foreach (var door in doorsInRoom)
                    {
                        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)door.Id);
                    }
                }
                catch { }
            }
        }

        foreach (var doorRoom in doorsToSpamClose)
        {
            DoorsHandler.CloseDoorsOfRoom(doorRoom);
        }

        var map = (MapNames)Utils.getCurrentMapID();

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            foreach (var doorRoom in doorsToSpamOpen)
            {
                DoorsHandler.OpenDoorsOfRoom(doorRoom);
            }
        }
    }
}
