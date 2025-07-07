using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FireStartCell
{
    public int gridIndex;
    public int nodeIndex;
}

[Serializable]
public class FireSimulation
{
    public List<FireStartCell> startCells = new List<FireStartCell>();
}

[Serializable]
public class FireConfig
{
    public FireSimulation fireSimulation = new FireSimulation();
}
