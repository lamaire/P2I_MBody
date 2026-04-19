using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

// Tests utilitaires pour la logique pure de VTITask :
// construction des listes de distances, catch trials,
// conversion des unités et sérialisation des données.
// Ces tests ne dépendent pas du runtime Unity (pas de MonoBehaviour).

[TestFixture]
public class VTITaskLogicTests
{
    // Reproduction de la logique de VTITask 

    private static List<float> BaseDistances => new List<float> { 0.15f, 0.30f, 0.45f, 0.60f, 0.75f, 0.90f };

    // Méthode qui reproduit la logique de construction de la liste (sans randomisation)

    private List<float> BuildDistanceList(int numberOfTrials, float catchMarker = -1f)
    {
        int numberOfCatchTrials = Mathf.RoundToInt(numberOfTrials * 0.15f);
        var list = new List<float>(BaseDistances);
        int index = 0;

        for (int i = list.Count; i < numberOfTrials - numberOfCatchTrials; i++)
        {
            if (index >= BaseDistances.Count) index = 0;
            list.Add(BaseDistances[index]);
            index++;
        }

        for (int i = 0; i < numberOfCatchTrials; i++)
            list.Add(catchMarker);

        return list;
    }

    //  Nombre de trials 

    [Test]
    public void BuildDistanceList_TotalCount_MatchesNumberOfTrials()
    {
        int n = 7;
        var list = BuildDistanceList(n);
        Assert.AreEqual(n, list.Count);
    }

    [TestCase(7)]
    [TestCase(12)]
    [TestCase(20)]
    public void BuildDistanceList_TotalCount_VariousTrialCounts(int n)
    {
        var list = BuildDistanceList(n);
        Assert.AreEqual(n, list.Count);
    }

    //  Catch trials 

    [Test]
    public void BuildDistanceList_CatchTrials_Are15Percent()
    {
        int n = 20;
        var list = BuildDistanceList(n);
        int catchCount = list.Count(d => d == -1f);
        int expected = Mathf.RoundToInt(n * 0.15f);
        Assert.AreEqual(expected, catchCount);
    }

    [Test]
    public void BuildDistanceList_WithDefault7Trials_HasOneCatchTrial()
    {
        // 7 * 0.15 = 1.05 → arrondi à 1
        var list = BuildDistanceList(7);
        int catchCount = list.Count(d => d == -1f);
        Assert.AreEqual(1, catchCount);
    }

    [Test]
    public void BuildDistanceList_NonCatchTrials_AreAllFromBaseDistances()
    {
        var list = BuildDistanceList(7);
        var validDistances = BaseDistances;

        foreach (float d in list.Where(d => d != -1f))
        {
            Assert.IsTrue(validDistances.Contains(d),
                $"Distance {d} n'est pas dans la liste de base.");
        }
    }

    //  distancesTrial (sans catch) 

    [Test]
    public void DistancesTrial_ExcludesCatchTrials()
    {
        var shuffled = BuildDistanceList(7);
        var distancesTrial = shuffled.Where(d => d != -1f).ToList();

        Assert.IsFalse(distancesTrial.Contains(-1f));
    }

    [Test]
    public void DistancesTrial_Count_IsTrialsMinusCatchTrials()
    {
        int n = 7;
        int catchCount = Mathf.RoundToInt(n * 0.15f);
        var shuffled = BuildDistanceList(n);
        var distancesTrial = shuffled.Where(d => d != -1f).ToList();

        Assert.AreEqual(n - catchCount, distancesTrial.Count);
    }

    //  Conversion des unités 

    [Test]
    public void DistanceConversion_MetersToIntCm_IsCorrect()
    {
        // Dans SaveVTIData : distance = Mathf.RoundToInt(distancesTrial[i] * 100)
        Assert.AreEqual(15, Mathf.RoundToInt(0.15f * 100));
        Assert.AreEqual(30, Mathf.RoundToInt(0.30f * 100));
        Assert.AreEqual(45, Mathf.RoundToInt(0.45f * 100));
        Assert.AreEqual(60, Mathf.RoundToInt(0.60f * 100));
        Assert.AreEqual(75, Mathf.RoundToInt(0.75f * 100));
        Assert.AreEqual(90, Mathf.RoundToInt(0.90f * 100));
    }

    [Test]
    public void ReactionTimeConversion_SecondsToMs_IsCorrect()
    {
        // Dans SaveVTIData : reactionTime = reactionTimes[i] * 1000
        float rtSeconds = 0.312f;
        float rtMs = rtSeconds * 1000f;
        Assert.AreEqual(312f, rtMs, 0.001f);
    }

    //  Sérialisation VTIData 

    [Test]
    public void SaveVTIData_Json_ContainsExpectedFields()
    {
        var data = new VTIData();
        data.trials.Add(new VTITrialData { distance = 45, reactionTime = 312f });

        string json = JsonUtility.ToJson(data, true);

        StringAssert.Contains("VTI", json);
        StringAssert.Contains("distance", json);
        StringAssert.Contains("reactionTime", json);
        StringAssert.Contains("45", json);
    }

    [Test]
    public void SaveVTIData_Json_MinTrialsCount_UsesMin()
    {
        // Si distances.Count != reactionTimes.Count, on prend le min
        var distances = new List<float> { 0.15f, 0.30f, 0.45f };
        var reactionTimes = new List<float> { 0.2f, 0.35f }; // une de moins

        int count = Mathf.Min(distances.Count, reactionTimes.Count);
        Assert.AreEqual(2, count);

        var data = new VTIData();
        for (int i = 0; i < count; i++)
        {
            data.trials.Add(new VTITrialData
            {
                distance = Mathf.RoundToInt(distances[i] * 100),
                reactionTime = reactionTimes[i] * 1000
            });
        }

        Assert.AreEqual(2, data.trials.Count);
    }

    [Test]
    public void SaveVTIData_Json_RoundtripPreservesData()
    {
        var data = new VTIData();
        data.trials.Add(new VTITrialData { distance = 60, reactionTime = 450.5f });

        string json = JsonUtility.ToJson(data, true);
        VTIData restored = JsonUtility.FromJson<VTIData>(json);

        Assert.AreEqual(1, restored.trials.Count);
        Assert.AreEqual(60, restored.trials[0].distance);
        Assert.AreEqual(450.5f, restored.trials[0].reactionTime, 0.01f);
    }
}
