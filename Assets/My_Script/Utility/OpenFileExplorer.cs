using SFB;
using UnityEngine;

public static class OpenFileExplorer
{
    public static string[] OpenFile(string title, ExtensionFilter[] extensions)
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(title, "", extensions, false);
        return paths;
    }

    public static string[] OpenFolder(string title)
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel(title, "", false);
        return paths;
    }
}
