using System;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public class ConsoleUI : MonoBehaviour
{
    public bool isVisible = false;
    private Vector2 scrollPosition = Vector2.zero;
    private static System.Collections.Generic.List<string> logEntries = new();
    private const int MaxLogEntries = 100;
    private Rect windowRect = new Rect(320, 10, 500, 350);
    private GUIStyle logStyle;
    private GUIStyle windowStyle;
    private GUIStyle buttonStyle;


    private System.Collections.Generic.Dictionary<byte, (string entryVentName, int currentVentId, string currentVentName)> currentVents = new();

    public void RegisterEnterVent(byte playerId, string playerName, int ventId, string ventName)
    {
        if (currentVents.ContainsKey(playerId))
        {
            var tracker = currentVents[playerId];
            if (tracker.currentVentId != ventId)
            {
                Log($"{playerName} moved to {ventName}");
            }

            currentVents[playerId] = (tracker.entryVentName, ventId, ventName);
        }
        else
        {
            currentVents[playerId] = (ventName, ventId, ventName);
            Log($"<color=#A9A9A9>💨</color> {playerName} entered {ventName}");
        }
    }

    public void RegisterExitVent(byte playerId, int ventId, string ventName)
    {
        string pName = playerNameFromId(playerId);

        if (currentVents.ContainsKey(playerId))
        {
            var tracker = currentVents[playerId];
            string realExitName = ventName;

            if (tracker.entryVentName != realExitName)
            {
                Log($"<color=#A9A9A9>💨</color> {pName} exited {realExitName} (entered at {tracker.entryVentName})");
            }
            else
            {
                Log($"<color=#A9A9A9>💨</color> {pName} exited {realExitName}");
            }
            currentVents.Remove(playerId);
        }
        else
        {
            Log($"<color=#A9A9A9>💨</color> {pName} exited {ventName}");
        }
    }

    private string playerNameFromId(byte playerId)
    {
        var player = GameData.Instance?.GetPlayerById(playerId);
        return (player != null) ? GetColoredName(player) : "Unknown";
    }

    public void Log(string message)
    {
        if (logEntries.Count >= MaxLogEntries)
        {
            logEntries.RemoveAt(0);
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        logEntries.Add($"<color=#808080>[{timestamp}]</color> {message}");

        scrollPosition.y = float.MaxValue;
    }

    public static string GetColoredName(NetworkedPlayerInfo data)
    {
        if (data == null) return "Unknown";

        string name = data.PlayerName;
        if (string.IsNullOrEmpty(name)) name = "Unknown";

        if (CheatToggles.seeRoles && data.Role != null)
        {
            string roleColor = ColorUtility.ToHtmlStringRGB(data.Role.TeamColor);
            return $"<color=#{roleColor}>{name}</color>";
        }

        try
        {
            int colorId = data.DefaultOutfit.ColorId;
            if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
            {
                Color32 c = Palette.PlayerColors[colorId];
                return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{name}</color>";
            }
        }
        catch { }

        return name;
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        if (logStyle == null)
        {
            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = true,
                wordWrap = true,
                padding = new RectOffset() { left = 5, right = 5, top = 2, bottom = 2 }
            };

            windowStyle = new GUIStyle(GUI.skin.window);
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.05f, 0.9f));
            bgTex.Apply();
            windowStyle.normal.background = bgTex;
            windowStyle.onNormal.background = bgTex;
            windowStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
        }

        if (ColorUtility.TryParseHtmlString(MalumMenu.menuHtmlColor.Value, out var configUIColor))
        {
            GUI.backgroundColor = configUIColor;
        }
        else
        {
            GUI.backgroundColor = Color.white;
        }

        var styleToUse = (MalumMenu.menuHtmlColor.Value == "") ? windowStyle : GUI.skin.window;

        windowRect = GUI.Window(1, windowRect, (GUI.WindowFunction)ConsoleWindow, "Action Logger", styleToUse);
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical();

        GUI.skin.box.normal.background = Texture2D.blackTexture;
        GUILayout.BeginVertical(GUI.skin.box);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Height(windowRect.height - 65));

        foreach (var log in logEntries)
        {
            GUILayout.Label(log, logStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Clear Logs", buttonStyle, GUILayout.Width(100), GUILayout.Height(25)))
        {
            logEntries.Clear();
            currentVents.Clear();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}
