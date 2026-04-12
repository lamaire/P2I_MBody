using System.IO;
using UnityEngine;

public static class JsonSaver
{
    public static void SaveJson(string json, string fileName)
    {
        string folderPath = "../P2I Analysis";
        string path = Path.Combine(folderPath, fileName);

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.Write(json);
        }

        Debug.Log("Données sauvegardées : " + path);
    }
}