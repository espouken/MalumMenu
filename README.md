
<p align="center">
  <img width="1028" height="441" alt="banner" src="./logo.png" />
</p>


<p align="center">

  <a href="https://discord.gg/YYcYf88jAb">
    <img src="https://img.shields.io/badge/Join%20us%20on-Discord-blue?style=flat&logo=discord" alt="Discord">
  </a>

  <a href="https://github.com/astra1dev#%EF%B8%8F-support-me">
    <img src="https://img.shields.io/badge/Support-me-ff5f5f?style=flat&logo=github-sponsors">
  </a>

  <a href="https://github.com/astra1dev/MalumMenu/actions/workflows/main.yml">
    <img src="https://github.com/astra1dev/MalumMenu/actions/workflows/main.yml/badge.svg?event=push&style=plastic">
  </a>

  <a href="../../releases">
    <img src="https://img.shields.io/github/downloads/astra1dev/MalumMenu/total.svg?style=plastic&color=red">
  </a>

  <a href="../../releases/latest">
    <img src="https://img.shields.io/github/downloads/astra1dev/MalumMenu/latest/total?style=plastic">
  </a>

</p>

<p align="center">
<b>An easy-to-use Among Us cheat menu with a simple GUI and lots of useful modules. </b>


# 🎁 Releases

**PLEASE READ**:
This repository is my fork of @astra1dev's Malum Menu fork is my personal version of malum menu i use, for releases check on the release tab, im lazy to type this out tbh.

For older (official) versions, please refer to the [original MalumMenu repository](https://github.com/scp222thj/MalumMenu).

# ⬇️ Installation

The installation process is the same as for the original MalumMenu (see [here](https://github.com/scp222thj/MalumMenu?tab=readme-ov-file#%EF%B8%8F-installation)).

Instead of downloading the latest ZIP or DLL release from the original repository, download it from the table above (or get the CI build artifact from the latest commit for more bleeding-edge features).

If you are using the DLL, make sure you have BepInEx 6.0.0-BE-735 installed. Older ones will not work.

Make sure you are only having one version of MalumMenu installed at a time, as having multiple versions can cause issues.

# 📋 Features

Changes from @astra1dev's fork:

#### Fixes
- Fixed menu position not saving correctly between sessions (menu now remembers its last position)
- Fixed "Auto-Open Doors On Use" disabling mid-games

#### Additions
- Added "Auto-Fix Lights On Use" (Ship category)
- Added "Bind Close Room" keybind to close doors in current/previous room (Ship category, note: doesn't work properly on decontamination)
- Added "Event Logger" to ESP category, which logs: kills, body reports, votes, sabotages, completed tasks, vent entries/exits, and shapeshifts
- Added "Radar" sub-tab to ESP category with replay functionality, featuring:
  - Player rendering for Crewmates, Impostors, Ghosts and Dead bodys
  - Tracking with color/role-based display options (else its role color based)
  - Map background support (Skeld, Polus only rn)
  - Kill, vent in/out, shapeshifts, and task completion indicators
  - Configurable tracers showing player movement paths (between 0-3000ms)
  - Toggle between follow (centered, zoomed) and fixed (full map) view modes
- Added replay system to Radar with round navigation (Prev/Next/Live), timeline slider, and play/pause controls

---


Btw, most icons were gotten from(directly copy pasted):
https://github.com/g0aty/SickoMenu/tree/main/resources 


![](https://github.com/user-attachments/assets/e7342201-aa01-4435-8c9e-d543712842e0)

[Original MalumMenu features](https://github.com/scp222thj/MalumMenu?tab=readme-ov-file#-features)

Changes from the original MalumMenu:
#### Fixes
- v16.0.0 fix where the menu wouldn't load at all
- v17.0.0 fix where PPM and SeeRoles wouldn't work
- Fix SeeRoles nametags overlaying with colorblind text if it is enabled
- Fix killing as impostor kicking you from the lobby
- Fix detecting if the player is the lobby host
- Fix not being able to input russian characters (and possibly others) in chat
- Fix and enable previously implemented but disabled Telekill cheat

#### Additions
- Added option to disable cheats in Passive category ([#164](https://github.com/scp222thj/MalumMenu/pull/164))
- Added a new "Config" category with "RGB Mode" and "Open Config File" options
- Added "Fake Revive" cheat (Player category)
- Added "No Options Limits" cheat (Host-Only category)
- Added new "Animations" category
- Added "Panic (Disable MalumMenu)" button (Passive category)
- Added "Show Player Info" (ESP category)
- Added "Reload plugin config", "Save to Profile" and "Load from Profile" buttons (Config category)
- Added new Viper and Detective roles to "Set Fake Role" cheat (Roles category)
- Added "Spoof Date to April 1st" (Passive category)
- Added "Protect Player" PPM (Host-Only category)
- Added "More Lobby Info" (ESP category)
- Added "Open Sabotage Map" (Ship category)
- Added a new horizontal tab-based UI config option
- Changed "SpeedHack" to be a slider instead of a toggle (Player category)
- Added "Invert Controls" (Player category)
- Added "Meetings" submenu with "Call Meeting", "Skip Meeting", "VoteImmune" and "Eject Player" cheats (Host-Only category)
- Added "Game State" submenu with "Force Start Game" and "No Game End" cheats (Host-Only category, [#49](https://github.com/scp222thj/MalumMenu/pull/49))
- Added "Trigger Spores" (Ship category, [#40](https://github.com/scp222thj/MalumMenu/pull/40))
- Added "Auto-Open Doors On Use" (Ship category)
- Added Doors Menu to close / open individual doors (Ship category)
- Added Tasks Menu to complete individual tasks and see other players' tasks (Roles category)
- Added a keybind system to bind cheats to keyboard keys (defined in MalumProfile.txt)
- Added pasting and cutting text between the chatbox and the device's clipboard
- Changed "ZoomOut" to disable while Chat, Friends List or Game Settings Panel is open
- Added being able to kick players while in-game as host (no 3 votes required to kick)
- Added "Show Task Arrows" (ESP category)

#### Other changes
- Some refactoring and code style changes
- BepInEx version bump and CI updates

Full Changelog [here](https://github.com/scp222thj/MalumMenu/compare/main...astra1dev:MalumMenu:astralum).

<hr>

<details>
  <summary>Known Issues, won't be fixed</summary>

  - Current Room Name doesn't show when NoClip is enabled
  - No "slide-in" animation plays when a PPM is opened
  - If the player opens any PPM while the shapeshift menu is open, the menus will overlay on each other
  - If the player opens any PPM while walking, the player will keep walking until the PPM is closed
  - "Complete all tasks" sometimes doesn't complete all tasks
  - NameTag ESP-related features (e.g. "Show Player Info") don't apply to previous chat messages when toggled
  - Some cheats automatically get turned off when the player leaves a game (e.g. NoClip)
</details>

# ⚠️ Disclaimer

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC.

Usage of this mod can violate the terms of service of Among Us, which may lead to punitive action including temporary or permanent bans from the game. The creator is not responsible for any consequences you may face due to usage. Use at your own risk.
