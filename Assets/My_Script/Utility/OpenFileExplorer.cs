using SFB;
using UnityEngine;

public static class OpenFileExplorer
{
    public static string[] OpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", "*", false);
        return paths;
    }

    public static string[] OpenFolder(string windowTitle)
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel(windowTitle, "", false);
        return paths;
    }
}
