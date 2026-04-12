using System.IO;
using UnityEngine;

public static class JsonSaver
{
    public static void SaveJson(string json, string fileName)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string folderPath = Path.Combine(projectRoot, "P2I Analysis");

        Directory.CreateDirectory(folderPath);

        string path = Path.Combine(folderPath, fileName);

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.Write(json);
        }

        Debug.Log("Données sauvegardées : " + path);
    }
}