using System;
using System.Collections.Generic;

[Serializable]
public class VTITrialData
{
    public int distance;
    public float reactionTime;
}

[Serializable]
public class VTIData
{
    public string taskName = "VTI";
    public List<VTITrialData> trials = new List<VTITrialData>();
}

[Serializable]
public class TBTrialData
{
    public int trueDelay;
    public int estimatedDelay;
}

[Serializable]
public class TBData
{
    public string taskName = "TB";
    public List<TBTrialData> trials = new List<TBTrialData>();
}