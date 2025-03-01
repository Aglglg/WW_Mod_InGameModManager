using System.Collections.Generic;

public class ModFixData
{
    public string note;
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
    Wuwa,
    Genshin,
    HSR,
    ZZZ
}