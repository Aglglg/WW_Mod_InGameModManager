using System;
using System.Collections.Generic;

[Serializable]
public class ManagedModDatas
{
    public List<ManagedGroupData> groupDatas;
}

[Serializable]
public class ManagedGroupData
{
    public string groupPath;
    public List<string> modsPath;
}