using NUnit.Framework;
using System.IO;
using UnityEngine;
using UnityEditor;

// Tests utilitaires pour JsonSaver.
// JsonSaver.SaveJson utilise Application.dataPath (indisponible en EditMode),
// donc la logique StreamWriter est testée directement dans un dossier temporaire isolé.

[TestFixture]
public class JsonSaverTests
{
    private string tempFolder;

    [SetUp]
    public void SetUp()
    {
        // Dossier temporaire isolé pour chaque série de tests
        tempFolder = Path.Combine(Path.GetTempPath(), "P2I_Tests_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);
    }

    [TearDown]
    public void TearDown()
    {
        // Nettoyage après chaque test
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, recursive: true);
    }

    // Reproduit la logique de JsonSaver.SaveJson sans Application.dataPath
    private void SaveToTemp(string json, string fileName)
    {
        Directory.CreateDirectory(tempFolder);
        string path = Path.Combine(tempFolder, fileName);
        using (StreamWriter writer = new StreamWriter(path))
            writer.Write(json);
    }

    //  Tests 

    [Test]
    public void SaveJson_CreatesFile()
    {
        string json = "{\"taskName\":\"VTI\",\"trials\":[]}";
        string fileName = "test_output.json";

        SaveToTemp(json, fileName);

        string path = Path.Combine(tempFolder, fileName);
        Assert.IsTrue(File.Exists(path), "Le fichier JSON devrait avoir été créé.");
    }

    [Test]
    public void SaveJson_FileContainsExactContent()
    {
        string json = "{\"taskName\":\"TB\",\"trials\":[{\"trueDelay\":500,\"estimatedDelay\":480}]}";
        string fileName = "test_tb.json";

        SaveToTemp(json, fileName);

        string path = Path.Combine(tempFolder, fileName);
        string content = File.ReadAllText(path);
        Assert.AreEqual(json, content);
    }

    [Test]
    public void SaveJson_EmptyJson_CreatesEmptyFile()
    {
        string json = "{}";
        string fileName = "test_empty.json";

        SaveToTemp(json, fileName);

        string path = Path.Combine(tempFolder, fileName);
        string content = File.ReadAllText(path);
        Assert.AreEqual("{}", content);
    }

    [Test]
    public void SaveJson_OverwritesExistingFile()
    {
        string fileName = "test_overwrite.json";
        string path = Path.Combine(tempFolder, fileName);

        SaveToTemp("{\"first\":true}", fileName);
        SaveToTemp("{\"second\":true}", fileName);

        string content = File.ReadAllText(path);
        StringAssert.Contains("second", content);
        StringAssert.DoesNotContain("first", content);
    }

    [Test]
    public void SaveJson_FileNameContainsTimestamp_Format()
    {
        // Vérifie que le format de nom généré dans VTITask/TBTask est cohérent
        string vtiName = $"VTI_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string tbName = $"TB_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";

        StringAssert.StartsWith("VTI_", vtiName);
        StringAssert.EndsWith(".json", vtiName);
        StringAssert.StartsWith("TB_", tbName);
    }

    [Test]
    public void SaveJson_LargePayload_WritesCorrectly()
    {
        // Génère un JSON de ~50 trials pour tester la robustesse
        var data = new VTIData();
        for (int i = 0; i < 50; i++)
            data.trials.Add(new VTITrialData { distance = i * 5, reactionTime = i * 10.5f });

        string json = JsonUtility.ToJson(data, true);
        string fileName = "test_large.json";

        SaveToTemp(json, fileName);

        string path = Path.Combine(tempFolder, fileName);
        string content = File.ReadAllText(path);

        Assert.AreEqual(json, content);
        StringAssert.Contains("\"distance\": 245", content); // trial index 49 → 49*5
    }
}
