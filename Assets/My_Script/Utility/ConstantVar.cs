
public static class ConstantVar
{
    public const string Link_ValidKeys = "https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes";
    public const string Link_GameBanana = "https://gamebanana.com/members/3948540";
    public const string Link_GetSupportLink = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/supportlink.txt";
    public const string Link_GetSupportIcon = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/My_Sprite/UI/IconDonate.png";

    public static readonly string[] Link_ListModFixes = new string[4]{
        "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/ModFixDatas/Wuwa/!Wuwa_ModFixes_List.json",
        "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/ModFixDatas/Genshin/!Genshin_ModFixes_List.json",
        "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/ModFixDatas/HSR/!Hsr_ModFixes_List.json",
        "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/ModFixDatas/ZZZ/!Zzz_ModFixes_List.json"
    };

    public const string File_Fixes_Log = "LOG.txt";
    public const string Path_Cached_Fixes = "cache_mod_fix";
    public const string Fix_Backup_Extension = "ini_managed_fix_backup";
    public const string Tag_ModFixManager = "ModFixManager";

    public const int Total_MaxGroup = 49; //Actually 48, but 0 index is reserved for Add group button
    public const int Total_MaxModPerGroup = 41; //Actually 40
    public const string ModsFolder_ValidSuffix = "\\mods";
    public const string ModData_Json_File = "moddata.json";
    public const string ModData_Icon_File = "icon.dds";
    public const string Managed_Folder = "V1_3_x_MANAGED-DO_NOT_EDIT_COPY_MOVE_CUT";
    public const string Removed_Folder = "DISABLED_MANAGED_REMOVED";
    public const string Managed_Backup_Extension = "ini_managed_backup";
    public const string IniFile_GroupManager = "manager_group.ini";
    public const string IniFile_BackgroundKeypress = "background_keypress.ini";
    public const string IniFile_Group = "{group_x}.ini";

    //suffix only, the usage will be ConstantVar.PREFIX_PLAYERPERFKEY_MODPATH + Initialization.gameName (Mod Path Wuthering Waves)
    public const string Prefix_PlayerPrefKey_ModPath = "Mod Path "; 
    public const string PlayerPrefKey_Opacity = "Opacity";
    public const float Default_Opacity = 0.5f;
    public const string PlayerPrefKey_Scale = "Scale";
    public const float Default_Scale = 0.85f;
    public const string PlayerPerfKey_Width = "Width";
    public const float Default_Width = 820f;
    public const string PlayerPrefKey_Height = "Height";
    public const float Default_Height = 1015f;

    //used with Initialization.gameName
    public const string Prefix_PlayerPrefKey_KeyboardToggle = "KeyboardToggleWindow";
    public const string Prefix_PlayerPrefKey_GamepadXToggle = "GamepadXToggleWindow";


    public const int Width_ModIcon = 216;
    public const int Height_ModIcon = 312;
    public const int WidthHeight_GroupIcon = 160;
}
