using System.IO;
using UnityEngine;

public static class JsonSaver
{
    public static void SaveJson(string json, string fileName)
    {
        // string path = Path.Combine(Application.persistentDataPath, fileName);
        string folderPath = "C:/Users/leava/OneDrive - univ-lyon2.fr/Bureau/ENSC/STAGE 2A/P2I/P2I Code/P2I_Analysis/data";
        string path = Path.Combine(folderPath, fileName);

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.Write(json);
        }

        Debug.Log("Données sauvegardées : " + path);
    }
}