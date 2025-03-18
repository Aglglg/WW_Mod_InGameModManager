using System;
using System.Collections.Generic;

[Serializable]
public class ModData
{
    public List<GroupData> groupDatas;
}

[Serializable]
public class GroupData
{
    public string groupName;
    public string groupPath;
    public string[] modNames;
    public string[] modFolders;
    public int selectedModIndex = 0;
}