
public static class ConstantVar
{
    public const string LINK_VALIDKEYS = "https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes";
    public const string LINK_GAMEBANANA = "https://gamebanana.com/members/3948540";
    public const string LINK_GETSUPPORTLINK = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/supportlink.txt";
    public const string LINK_GETSUPPORTICON = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/My_Sprite/UI/IconDonate.png";

    public static readonly string[] LINK_PATH_MODFIXES = new string[4]{
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/Wuwa",
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/Genshin",
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/HSR",
        "https://api.github.com/repos/Aglglg/WW_Mod_InGameModManager/contents/ModFixDatas/ZZZ"
    };
    public const string FILE_FIX_LOG = "LOG.json";
    public const string PATH_CACHED_FIXES = "cache_mod_fix";
    public const string FIX_BACKUP_SUFFIX = "ini_managed_fix_backup";
    public const string TAG_MODFIXMANAGER = "ModFixManager";
    public const string TAG_INPUTSYSTEM = "InputSystem";


    public const string MODFOLDER_VALIDSUFFIX = "mi\\mods";
    public const string MODDATA_JSON_FILE = "moddata.json";
    public const string MODDATA_ICON_FILE = "icon.dds";
    public const string MANAGED_PATH = "MANAGED_DO-NOT-EDIT";
    public const string REMOVED_PATH = "DISABLED_MANAGED_REMOVED";
    public const string MANAGER_BACKUP_SUFFIX = "ini_managed_backup";

    //suffix only, the usage will be ConstantVar.PREFIX_PLAYERPERFKEY_MODPATH + Initialization.gameName (Mod Path Wuthering Waves)
    public const string PREFIX_PLAYERPERFKEY_MODPATH = "Mod Path "; 
    public const string PLAYERPERFKEY_OPACITY = "Opacity";
    public const float DEFAULT_OPACITY = 0.5f;
    public const string PLAYERPERFKEY_SCALE = "Scale";
    public const float DEFAULT_SCALE = 0.85f;
    public const string PLAYERPERFKEY_WIDTH = "Width";
    public const float DEFAULT_WIDTH = 820f;
    public const string PLAYERPERFKEY_HEIGHT = "Height";
    public const float DEFAULT_HEIGHT = 970f;
}
