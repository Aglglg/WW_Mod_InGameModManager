
public static class ConstantVar
{
    public const string LINK_VALIDKEYS = "https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes";
    public const string LINK_GAMEBANANA = "https://gamebanana.com/members/3948540";
    public const string LINK_GETSUPPORTLINK = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/supportlink.txt";
    public const string LINK_GETSUPPORTICON = "https://raw.githubusercontent.com/Aglglg/WW_Mod_InGameModManager/refs/heads/main/Assets/My_Sprite/UI/IconDonate.png";
    public const string MANAGER_BACKUP_SUFFIX = "ini_managed_backup";

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

    public const string MANAGED_PATH = "MANAGED-DO_NOT_EDIT";
    public const string MANAGED_REMOVED_PATH = "DISABLED_RemovedFromModManager";
    public const string PATH_MODJSONDATA = "managedModData.json";
    public const string SUFFIX_PLAYERPREFKEY_MODPATH = "Mod Path "; //suffix only, the usage will be ConstantVar.SUFFIX_PLAYERPREFKEY_MODPATH + Initialization.gameName (Mod Path Wuthering Waves) 
    public const string PLAYERPREFKEY_OPACITY = "Opacity";
    public const float DEFAULT_OPACITY = 0.5f;
    public const string PLAYERPREFKEY_SCALE = "Scale";
    public const float DEFAULT_SCALE = 0.85f;
    public const string PLAYERPREFKEY_WIDTH = "Width";
    public const float DEFAULT_WIDTH = 820f;
    public const string PLAYERPREFKEY_HEIGHT = "Height";
    public const float DEFAULT_HEIGHT = 1080f;

    public const string MANAGED_SLOT_CONDITION = @"$managed_slot_id == $\agulag\managed_group\{groupName}\active_slot";
    public const string MANAGED_SLOT_CONDITION_TOBE_REPLACED = "{groupName}";
    public const string MANAGED_CONSTANTS_SECTION = "[Constants]\nglobal $managed_slot_id = {slotId}\n";
    public const string MANAGED_CONSTANTS_SECTION_TOBE_REPLACED = "{slotId}";
    public const string MANAGED_ICON_FILE = "icon.png";

}
