
public static class ConstantVar
{
    public const string Link_ValidKeys = "https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes";
    public const string Link_GameBanana = "https://gamebanana.com/members/3948540";
    public const string Link_GetSupportLink = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/supportlink.txt";
    public const string Link_GetSupportIcon = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/My_Sprite/UI/IconDonate.png";

    public static readonly string[] Link_PathModFixes = new string[4]{
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/Wuwa",
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/Genshin",
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/HSR",
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/ZZZ"
    };
    public const string File_Fixes_Log = "LOG.json";
    public const string Path_Cached_Fixes = "cache_mod_fix";
    public const string Fix_Backup_Extension = "ini_managed_fix_backup";
    public const string Tag_ModFixManager = "ModFixManager";
    public const string Tag_InputSystem = "InputSystem";

    public const int Total_MaxModPerGroup = 20;
    public const int Total_MaxGroup = 100;
    public const string ModsFolder_ValidSuffix = "mi\\mods";
    public const string ModData_Json_File = "moddata.json";
    public const string ModData_Icon_File = "icon.dds";
    public const string Managed_Path = "MANAGED_DO-NOT-EDIT";
    public const string Removed_Path = "DISABLED_MANAGED_REMOVED";
    public const string Managed_Backup_Suffix = "ini_managed_backup";

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

    public const int Width_ModIcon = 216;
    public const int Height_ModIcon = 312;
    public const int WidthHeight_GroupIcon = 160;
}
