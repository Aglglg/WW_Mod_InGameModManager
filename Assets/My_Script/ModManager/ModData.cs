using System;
using System.Collections.Generic;

[Serializable]
public class ModData
{
    public int toolVersion;
    public List<GroupData> groupDatas;
}

[Serializable]
public class GroupData
{
    public string groupName;
    public string groupPath;
    public List<string> modNames;
    public List<string> modFolders;
    public int selectedModIndex = 0;
}