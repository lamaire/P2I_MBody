using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Tests utilitaires pour les modèles de données VTI et TB.
// Vérifie la sérialisation JSON et l'intégrité des structures.

[TestFixture]
public class DataModelsTests
{
    //  VTITrialData 

    [Test]
    public void VTITrialData_DefaultValues_AreZero()
    {
        var trial = new VTITrialData();
        Assert.AreEqual(0, trial.distance);
        Assert.AreEqual(0f, trial.reactionTime);
    }

    [Test]
    public void VTITrialData_AssignedValues_ArePreserved()
    {
        var trial = new VTITrialData { distance = 45, reactionTime = 312.5f };
        Assert.AreEqual(45, trial.distance);
        Assert.AreEqual(312.5f, trial.reactionTime, 0.001f);
    }

    //  VTIData

    [Test]
    public void VTIData_TaskName_IsVTI()
    {
        var data = new VTIData();
        Assert.AreEqual("VTI", data.taskName);
    }

    [Test]
    public void VTIData_Trials_InitialisedEmpty()
    {
        var data = new VTIData();
        Assert.IsNotNull(data.trials);
        Assert.AreEqual(0, data.trials.Count);
    }

    [Test]
    public void VTIData_AddTrial_CountIncreases()
    {
        var data = new VTIData();
        data.trials.Add(new VTITrialData { distance = 30, reactionTime = 200f });
        data.trials.Add(new VTITrialData { distance = 60, reactionTime = 350f });
        Assert.AreEqual(2, data.trials.Count);
    }

    [Test]
    public void VTIData_JsonRoundtrip_PreservesAllTrials()
    {
        var data = new VTIData();
        data.trials.Add(new VTITrialData { distance = 15, reactionTime = 180.5f });
        data.trials.Add(new VTITrialData { distance = 90, reactionTime = 420.0f });

        string json = JsonUtility.ToJson(data, true);
        VTIData restored = JsonUtility.FromJson<VTIData>(json);

        Assert.AreEqual("VTI", restored.taskName);
        Assert.AreEqual(2, restored.trials.Count);
        Assert.AreEqual(15, restored.trials[0].distance);
        Assert.AreEqual(180.5f, restored.trials[0].reactionTime, 0.001f);
        Assert.AreEqual(90, restored.trials[1].distance);
        Assert.AreEqual(420.0f, restored.trials[1].reactionTime, 0.001f);
    }

    [Test]
    public void VTIData_EmptyJson_RoundtripGivesEmptyTrials()
    {
        var data = new VTIData();
        string json = JsonUtility.ToJson(data, true);
        VTIData restored = JsonUtility.FromJson<VTIData>(json);

        Assert.AreEqual(0, restored.trials.Count);
    }

    //  TBTrialData

    [Test]
    public void TBTrialData_DefaultValues_AreZero()
    {
        var trial = new TBTrialData();
        Assert.AreEqual(0, trial.trueDelay);
        Assert.AreEqual(0, trial.estimatedDelay);
    }

    [Test]
    public void TBTrialData_AssignedValues_ArePreserved()
    {
        var trial = new TBTrialData { trueDelay = 500, estimatedDelay = 480 };
        Assert.AreEqual(500, trial.trueDelay);
        Assert.AreEqual(480, trial.estimatedDelay);
    }

    // TBData

    [Test]
    public void TBData_TaskName_IsTB()
    {
        var data = new TBData();
        Assert.AreEqual("TB", data.taskName);
    }

    [Test]
    public void TBData_Trials_InitialisedEmpty()
    {
        var data = new TBData();
        Assert.IsNotNull(data.trials);
        Assert.AreEqual(0, data.trials.Count);
    }

    [Test]
    public void TBData_JsonRoundtrip_PreservesAllTrials()
    {
        var data = new TBData();
        data.trials.Add(new TBTrialData { trueDelay = 300, estimatedDelay = 320 });
        data.trials.Add(new TBTrialData { trueDelay = 700, estimatedDelay = 650 });

        string json = JsonUtility.ToJson(data, true);
        TBData restored = JsonUtility.FromJson<TBData>(json);

        Assert.AreEqual("TB", restored.taskName);
        Assert.AreEqual(2, restored.trials.Count);
        Assert.AreEqual(300, restored.trials[0].trueDelay);
        Assert.AreEqual(320, restored.trials[0].estimatedDelay);
        Assert.AreEqual(700, restored.trials[1].trueDelay);
        Assert.AreEqual(650, restored.trials[1].estimatedDelay);
    }

    [Test]
    public void TBData_JsonContainsTaskName()
    {
        var data = new TBData();
        string json = JsonUtility.ToJson(data);
        StringAssert.Contains("TB", json);
    }
}
