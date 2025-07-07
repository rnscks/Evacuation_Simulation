using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EscapeAgent
{
    public int planeIndex;
}

[Serializable]
public class EscapeAgentConfig
{
    public List<EscapeAgent> agents = new List<EscapeAgent>();
}
