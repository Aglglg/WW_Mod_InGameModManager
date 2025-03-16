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
    public string groupPath;
    public string[] modPaths;
}