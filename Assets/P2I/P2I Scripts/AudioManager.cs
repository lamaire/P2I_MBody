using UnityEngine;

public static class AudioManager
{
    static AudioSource source;
    static AudioClip beepClip;

    static void Init()
    {
        if (source != null) return;

        GameObject go = new GameObject("AudioService");
        Object.DontDestroyOnLoad(go);

        source = go.AddComponent<AudioSource>();

        beepClip = Resources.Load<AudioClip>("beep");

        if (beepClip == null)
            Debug.LogError("Beep audio not found in Resources");
    }

    public static void PlayBeep()
    {
        Init();

        if (beepClip != null)
            source.PlayOneShot(beepClip);
    }
}
