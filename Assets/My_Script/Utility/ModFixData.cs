using System.Collections.Generic;

public class ModFixData
{
    public string fixName;
    public string note;
    public string submitter;
    public string dateTime;
    public ModFixType modFixType;
    public ModFixGame modFixGame;
    public Dictionary<string, string> hashpair;
}

public enum ModFixType
{
    HashReplacement,
    HashAddition,
}

public enum ModFixGame
{
    HSR,
    GI,
    WUWA,
    ZZZ
}