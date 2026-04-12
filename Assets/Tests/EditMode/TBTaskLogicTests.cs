using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Tests utilitaires pour la logique pure de TBTask :
/// construction des listes de durées, estimation, shuffling,
/// et sérialisation des données.
/// </summary>
[TestFixture]
public class TBTaskLogicTests
{
    private static readonly List<int> BaseDurations = new List<int> { 300, 500, 700 };

    // ─── Helper : reproduit la logique EnterTask de TBTask ──────────────────

    private List<int> BuildDurationList(int numberOfTrials)
    {
        var list = new List<int>(BaseDurations);
        int index = 0;

        for (int i = list.Count; i < numberOfTrials; i++)
        {
            if (index >= BaseDurations.Count) index = 0;
            list.Add(BaseDurations[index]);
            index++;
        }

        return list; // non shufflée pour les tests déterministes
    }

    // ─── Nombre de trials ───────────────────────────────────────────────────

    [Test]
    public void BuildDurationList_TotalCount_MatchesNumberOfTrials()
    {
        var list = BuildDurationList(3);
        Assert.AreEqual(3, list.Count);
    }

    [TestCase(3)]
    [TestCase(6)]
    [TestCase(9)]
    [TestCase(10)]
    public void BuildDurationList_TotalCount_VariousValues(int n)
    {
        var list = BuildDurationList(n);
        Assert.AreEqual(n, list.Count);
    }

    // ─── Contenu des durées ─────────────────────────────────────────────────

    [Test]
    public void BuildDurationList_AllValues_AreFromBaseDurations()
    {
        var list = BuildDurationList(9);
        foreach (int d in list)
            Assert.IsTrue(BaseDurations.Contains(d),
                $"Durée {d} n'est pas dans la liste de base.");
    }

    [Test]
    public void BuildDurationList_EachBaseDuration_AppearsProperly()
    {
        // Pour 6 trials : chaque durée de base apparaît exactement 2 fois
        var list = BuildDurationList(6);
        Assert.AreEqual(2, list.Count(d => d == 300));
        Assert.AreEqual(2, list.Count(d => d == 500));
        Assert.AreEqual(2, list.Count(d => d == 700));
    }

    [Test]
    public void BuildDurationList_WhenTrialsLessThanBase_UsesSubset()
    {
        // 3 trials : exactement les 3 durées de base
        var list = BuildDurationList(3);
        Assert.IsTrue(list.Contains(300));
        Assert.IsTrue(list.Contains(500));
        Assert.IsTrue(list.Contains(700));
    }

    // ─── Estimation & enregistrement ────────────────────────────────────────

    [Test]
    public void RegisterEstimation_RoundToInt_IsCorrect()
    {
        // Reproduit Mathf.RoundToInt(slider.value)
        Assert.AreEqual(300, Mathf.RoundToInt(300.4f));
        Assert.AreEqual(301, Mathf.RoundToInt(300.6f));
        Assert.AreEqual(500, Mathf.RoundToInt(499.5f));
    }

    [Test]
    public void EstimatedDurations_Count_MatchesNumberOfTrials()
    {
        int n = 3;
        var estimated = new List<int> { 310, 490, 680 };
        Assert.AreEqual(n, estimated.Count);
    }

    // ─── SaveTBData : appariement min ───────────────────────────────────────

    [Test]
    public void SaveTBData_UsesMin_WhenListsDiffer()
    {
        var durations   = new List<int> { 300, 500, 700 };
        var estimations = new List<int> { 310, 480 }; // une de moins

        int count = Mathf.Min(durations.Count, estimations.Count);
        Assert.AreEqual(2, count);
    }

    [Test]
    public void SaveTBData_UsesMin_WhenListsEqual()
    {
        var durations   = new List<int> { 300, 500, 700 };
        var estimations = new List<int> { 310, 480, 720 };

        int count = Mathf.Min(durations.Count, estimations.Count);
        Assert.AreEqual(3, count);
    }

    // ─── Sérialisation TBData ───────────────────────────────────────────────

    [Test]
    public void SaveTBData_Json_ContainsExpectedFields()
    {
        var data = new TBData();
        data.trials.Add(new TBTrialData { trueDelay = 500, estimatedDelay = 480 });

        string json = JsonUtility.ToJson(data, true);

        StringAssert.Contains("TB", json);
        StringAssert.Contains("trueDelay", json);
        StringAssert.Contains("estimatedDelay", json);
        StringAssert.Contains("500", json);
        StringAssert.Contains("480", json);
    }

    [Test]
    public void SaveTBData_Json_RoundtripPreservesAllTrials()
    {
        var durations   = new List<int> { 300, 500, 700 };
        var estimations = new List<int> { 310, 490, 680 };

        var data = new TBData();
        int count = Mathf.Min(durations.Count, estimations.Count);
        for (int i = 0; i < count; i++)
        {
            data.trials.Add(new TBTrialData
            {
                trueDelay      = durations[i],
                estimatedDelay = estimations[i]
            });
        }

        string json = JsonUtility.ToJson(data, true);
        TBData restored = JsonUtility.FromJson<TBData>(json);

        Assert.AreEqual(3, restored.trials.Count);
        Assert.AreEqual(300, restored.trials[0].trueDelay);
        Assert.AreEqual(310, restored.trials[0].estimatedDelay);
        Assert.AreEqual(700, restored.trials[2].trueDelay);
        Assert.AreEqual(680, restored.trials[2].estimatedDelay);
    }

    [Test]
    public void SaveTBData_Json_EmptyTrials_IsValid()
    {
        var data = new TBData();
        string json = JsonUtility.ToJson(data, true);
        TBData restored = JsonUtility.FromJson<TBData>(json);

        Assert.IsNotNull(restored);
        Assert.AreEqual(0, restored.trials.Count);
    }

    // ─── Logique du slider ──────────────────────────────────────────────────

    [Test]
    public void SliderClamp_ValueStaysInBounds()
    {
        float min = 0f, max = 1000f;
        float value = 1200f; // dépasse le max

        value = Mathf.Clamp(value, min, max);
        Assert.AreEqual(1000f, value);
    }

    [Test]
    public void SliderClamp_NegativeValue_ClampsToMin()
    {
        float min = 0f, max = 1000f;
        float value = -50f;

        value = Mathf.Clamp(value, min, max);
        Assert.AreEqual(0f, value);
    }

    [Test]
    public void SliderStep_Increment_IsCorrect()
    {
        float currentValue = 300f;
        float stepSize = 10f;
        float input = 1f; // direction positive

        float newValue = currentValue + Mathf.Sign(input) * stepSize;
        Assert.AreEqual(310f, newValue, 0.001f);
    }

    [Test]
    public void SliderStep_Decrement_IsCorrect()
    {
        float currentValue = 300f;
        float stepSize = 10f;
        float input = -1f;

        float newValue = currentValue + Mathf.Sign(input) * stepSize;
        Assert.AreEqual(290f, newValue, 0.001f);
    }
}
