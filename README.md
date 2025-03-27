# Complete In-Game Mod Manager (need XXMI)
[Click here to Download](https://gamebanana.com/mods/582623)

## Overview
The **Complete In-Game Mod Manager** is a tool designed to make managing mods easier. With this tool, you can:

- Select skins/mods directly without needing to reload the game.
- View and edit mod keybindings.
- Fix broken mods after game updates.

This tool supports **mouse**, **keyboard**, and **gamepad (XInput)** inputs for interaction.

---

## 🚀 Features
- **Mod Selection**: Choose and apply mods on the fly.
- **Keybinding Editor**: Modify mod keybindings conveniently.
- **Mod Fixer**: Resolve mod issues caused by game updates.
- **Automatic Game Detection**: Ensures the mod manager runs only when the target game is in focus.

---

## 📌 Installation & Setup

### 1️⃣ How To Use (Detailed)
1. **Download** the zip/rar file.
2. **Extract** the contents and open the folder.
3. **Select the appropriate .bat file** for your game (e.g., *Wuthering Waves*).
4. **Shift + Right Click** on the .bat file and select **Copy as Path**.
5. **Open XXMI Launcher** → Select your game → **Settings → Advanced**.
6. Enable **Run Pre-Launch** and paste the copied path into this field.
   - If another program (e.g., ReShade) is already using this field, separate them using `&` (e.g., `"..\..\ReShade\XXMI-ReShade-Extension.vbs" ww & "D:\Mods_GachaGames\Tool_InGameModManager\WUWA_modmanager.bat"`). 'ww' is reshade special arg(ww, gi, etc).
7. **Launch the game** via XXMI.
8. **Press F5** in-game to open the Mod Manager. You can change this key on **Setting** tab.
9. **Set your Mods folder path** in the **Settings** tab.
10. Navigate to the **Mods** tab, hide the manager with **F5**, and press **F10** in-game to reload.

🎉 Done! You can now organize mods, edit keybindings, and apply fixes.

### 2️⃣ Quick Guide
1. **Copy** the .bat file path.
2. **Paste** it into **XXMI → Settings → Advanced → Run Pre-Launch**.
3. **Launch the game**, press **F5** to open the manager.
4. **Set your Mods folder path** and follow the on-screen instructions.

### 🔄 Update Guide (only if you're from v1.2.0 and below)
If you're updating from **v1.2.0 or earlier**, follow these steps:

1. **Delete** previous tool version (v1.2.0 and below), if any.
2. Place tool under the same path/directory/folder(optional).
3. Make sure you double check **XXMI=>Setting=>Advanced=>Run Pre-Launch**. Make sure it's using the latest version.
4. On your "`Mods`" folder, rename `MANAGED-DO_NOT_EDIT_COPY_MOVE_CUT` to be `DISABLED_OTHERFOLDERNAME`.
Or just move it outside "`Mods`" folder (also need rename to other folder name). Folder name must not contains something like "`managed`".
6. Then just use & run the game as usual with XXMI.
7. You need to redo the adding process, you can just add group, add mod, select mod folder.
8. You can add back mods that still on `DISABLED_OTHERFOLDERNAME`. **USE THE TOOL! NOT MANUAL COPY PASTE. DO NOT DO IT MANUALLY WITH FILE EXPLORER (Copy Paste)**!
9. Done
   In previous version, If you encounter game crash because adding unsupported mod, the guide above will also fix it, and you can play the game again.
   If after following the guide above, game still crash and cannot be opened. Just move everything inside your "`Mods`" folder, place it outside "`Mods`" folder. Add back manually one-by-one to see which mod cause crash.

---
## 📌 IMPORTANT (To avoid Error/Bug on your mods)

### ✅ What you can do
- If you have merged/combined mods (example: merged.ini)
  - Place it under 1 folder, select that 1 unified folder. Do not select one by one.
- If you ever used other mod manager that also modify .ini files
  - **Quick & easy way**: revert it with the tool that they provided. And do not use it anymore.
  - **Advanced way (I personally never try it)**: Place them under 1 folder, make sure you know which one of them. Just add it select that 1 unified folder.
- How to use with ReShade or other? Run Pre-Launch is not empty
  - Just add '**&**' between them. Please read the guide, I already wrote it.
  
### ❌ What you must NOT do
- Do not add mods manually by **move/cut/copy** with **File Explorer**!
  - It will not work & .ini files remain unmodified!
  - And it basically break basic modding rules (1 mod for 1 character at a time). Because your mods all are active at the same time.
  - `I mean, do not add mod with copy paste to "V1_3_x_MANAGED..." folder. You can still add other mod with File Explorer of course, just do not put it in that folder.`
- Do not ever remove added/managed mods manually by move/cut/copy with FIle Explorer
  - Removing it manually with File Explorer will not revert modified mod!
  - `I mean, do not remove mod with move/cut/copy from  "V1_3_x_MANAGED..." folder.`
- In case game crash because adding unsupported mod that I never tested
  - Use "reverter_only.rar" read the GUIDE.txt inside that folder.
  - And you better tell me which mod that cause crash.

---

## ⚠️ Disclaimer & Warnings
- This is a **Windows Overlay App**.
- Simulates **key presses** but does **not** interact directly with the game. Only with WWMI/SRMI/ZZMI/GIMI
- **Use at your own risk.**
- **Not responsible for account issues due to mod usage.**
- By downloading and using this tool, you **accept full responsibility**.

---

## 📸 Media & Screenshots
- **Video Review & Tutorial**
  [![Youtube Video](https://img.youtube.com/vi/-PWS8t3XWS8/0.jpg)](https://www.youtube.com/watch?v=-PWS8t3XWS8)
- **Mod Selection**
  ![Mod Selection](https://files.gamebanana.com/img/ss/mods/67dbb710d0142.jpg)
- **Keybind Viewer & Editor**
  ![Keybind](https://files.gamebanana.com/img/ss/mods/67dbb711c5d28.jpg)
- **Mod Fix System**
  ![Mod Fix system](https://files.gamebanana.com/img/ss/mods/67dbb7165ab88.jpg)

> **Mini Mode Panel Release Incoming! (After there's really no bug)** 🎉
![Mini panel](https://files.gamebanana.com/img/ss/mods/67dbdb5e482ad.jpg)

---

## 🔧 Technical Details
- Built with **Unity 6 (C#)** (I only understand C# Unity).
- Uses **pre-made .bat files** for different game versions.
- Works by reading **target game window names** (e.g., `Wuthering Waves`).
- Supports **multi-instance** (e.g., playing WuWa & Genshin at the same time, but please don't).
- Uses **Rust-based DLLs** for **DDS image conversion** (optimized icon loading) (Rust, becauset ried with unity itself, can't).
- Saves and manages **mod data in JSON format**.
- Ensures **WWMI/GIMI/SRMI/ZZMI receives keypresses in the background**.

### 🔑 Keybindings Simulated
- **Clear Key (VK_CLEAR)** – Base for keypress simulation.
- **F13 - F24** – Used to change groups.
- **Enter / Backspace** – Used to change groups.
- **Tab / Right Ctrl** – Used to change mods.
- **Numbers 1-5 & Z-B** – Used to change mods.

> 🛑 **Note:** If your PC/system uses these keys for other shortcuts, you'll need to **change your system settings** if you can/if you want.

---

## 🛡️ Security Concerns
- Runs in the **background** after launching via XXMI.
- Auto-closes **when the target game exits**.
- If not closed properly, manually **end task** via Task Manager (`Tool_InGameModManager`).
- **VirusTotal Reports**: Some antivirus programs may flag this tool due to its use of external DLLs and keypress simulation.
- **If unsure, check the GitHub source code before use.** Or just don't use it

---

## 🙏 Credits
Special thanks to:
- **SacredSi1ence (Ko-Fi Supporters)**
- **Trixye (Ko-Fi Supporters)**
- **Jason (Ko-Fi Supporters)**
- **HuanJue** – For reporting some bug & give detailed explanation.
- **IverinNova** – Helped with Chinese game version compatibility & testing more on compatibility issues.
- **All bug reporters, testers version v1.2.0 and below, and supporters!** ❤️

---
