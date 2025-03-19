using System;
using System.Collections.Generic;

[Serializable]
public class ModFixList
{
    public List<string> fixesLink;
}

[Serializable]
public class ModFixDataWrapper
{
    public string title;
    public string note;
    public ModFixType modFixType;
    public ModFixGame modFixGame;
    public List<Hashpair> hashpair; // Use a list of key-value pairs
    public string modifiedDate; // Store as string for manual parsing
}

[Serializable]
public class Hashpair
{
    public string key;
    public string value;
}

[Serializable]
public class ModFixData
{
    public string title;
    public string note;
    public ModFixType modFixType;
    public ModFixGame modFixGame;
    public Dictionary<string, string> hashpair;
    public DateTime modifiedDate;
}

[Serializable]
public enum ModFixType
{
    HashReplacement,
    HashAddition,
}

[Serializable]
public enum ModFixGame
{
    Wuwa,
    Genshin,
    HSR,
    ZZZ
}