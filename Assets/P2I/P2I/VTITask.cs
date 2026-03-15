using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class VTITask : IP2ITask
{
    enum VTISteps { Init, TrialRunning, WaitingResponse, TrialEnd, InterTrial }
    VTISteps step = VTISteps.Init;
    string taskStep = "Init";
    GameObject stimulus;
    GameObject hand;
    InputAction grasp;
    private readonly HeadLookController headLook;
    Vector3 startPos;
    Vector3 endPos;
    Vector3 handPos;
    float moveStartTime;
    float onsetTime;
    bool beeped;

    // Trials
    private List<float> distancesList = new List<float> { 0.15f, 0.30f, 0.45f, 0.60f, 0.75f, 0.90f };
    public static int numberOfTrials = 7;
    private int numberOfCatchTrials = Mathf.RoundToInt(numberOfTrials * 0.15f);
    private List<float> shuffledNewDistancesList = new List<float>();
    public int trialIndex;
    private bool isControlTrial = false;
    public List<float> reactionTimes = new List<float>();
    public List<float> distancesTrial = new List<float>();
    private float targetDistance;
    private readonly Stopwatch interTrialSw = new Stopwatch();
    private const int interTrialMs = 10000; //800 ms
    public bool forceNextTrial = false;

    public int TrialIndex => trialIndex;
    public int NumberOfTrials => numberOfTrials;
    public bool ForceNextTrial => forceNextTrial;
    public string TaskStep => taskStep;

    public VTITask(InputAction graspAction, InputAction lookAction)
    {
        grasp = graspAction;
        headLook = new HeadLookController(lookAction);
    }

    public void RequestNextTrial()
    {
        forceNextTrial = true;
    }

    public void EnterTask()
    {
        SpawnCube();
        headLook.EnterHeadLook();
        trialIndex = 0;
        reactionTimes.Clear();
        distancesTrial.Clear();
        shuffledNewDistancesList.Clear();

        var index = 0;
        var newDistancesList = new List<float>(distancesList);

        for (int i = newDistancesList.Count; i < numberOfTrials - numberOfCatchTrials; i++)
        {
            if (index >= distancesList.Count)
                index = 0;

            newDistancesList.Add(distancesList[index]);
            index++;
        }

        for (int i = 0; i < numberOfCatchTrials; i++)
        {
            newDistancesList.Add(-1f);
        }

        // Randomization
        shuffledNewDistancesList = newDistancesList.OrderBy(x => Random.value).ToList();

        // List of the distances of the experiment, without the catch trial(s)
        for (int i = 0; i < shuffledNewDistancesList.Count; i++)
        {
            if (shuffledNewDistancesList[i] != -1f)
                distancesTrial.Add(shuffledNewDistancesList[i]);
        }

        LaunchNewTrial();
    }

    public void UpdateTask()
    {
        headLook.UpdateHeadLook();

        if (stimulus == null)
            return;

        if (step == VTISteps.TrialRunning || step == VTISteps.WaitingResponse)
        {
            RunMovement();
        }

        switch (step)
        {
            case VTISteps.TrialRunning:
                taskStep = "TrialRunning";
                UnityEngine.Debug.Log("Please press the keyboard (W, X or Z) when you hear the beep.");
                break;

            case VTISteps.WaitingResponse:

                taskStep = "WaitingResponse";
                if (grasp.WasPressedThisFrame())
                {
                    float rt = Time.time - onsetTime;
                    UnityEngine.Debug.Log($"RT={rt:0.000}s, target distance={targetDistance}, trial n°{trialIndex + 1}");
                    reactionTimes.Add(rt);
                    step = VTISteps.TrialEnd;
                }
                break;

            case VTISteps.TrialEnd:

                taskStep = "TrialEnd";
                trialIndex++;
                if (trialIndex >= numberOfTrials)
                {
                    UnityEngine.Debug.Log("END EXPERIMENT");
                    UnityEngine.Debug.Log("=== REACTION TIMES ===");
                    UnityEngine.Debug.Log(string.Join(", ", reactionTimes.ConvertAll(rt => rt.ToString("0.000"))));
                    UnityEngine.Debug.Log("=== DISTANCES ===");
                    UnityEngine.Debug.Log(string.Join(", ", distancesTrial.ConvertAll(d => d.ToString("0.00"))));

                    ExitTask();
                }
                else
                {
                    interTrialSw.Restart();
                    step = VTISteps.InterTrial;
                }
                break;

            case VTISteps.InterTrial:

                taskStep = "InterTrial";
                bool timeOK = interTrialSw.ElapsedMilliseconds >= interTrialMs;
                if (timeOK || forceNextTrial)
                {
                    interTrialSw.Reset();
                    LaunchNewTrial();
                }
                break;
        }
    }

    private void RunMovement()
    {
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / 0.66f;
        float progress = Mathf.Clamp01((Time.time - moveStartTime) / duration);
        stimulus.transform.position = Vector3.Lerp(startPos, endPos, progress);

        float currentDistance = Vector3.Distance(stimulus.transform.position, endPos);

        if (!isControlTrial && !beeped && currentDistance <= targetDistance)
        {
            AudioService.PlayBeep();
            UnityEngine.Debug.Log("STIMULUS ONSET");
            beeped = true;
            step = VTISteps.WaitingResponse;
            onsetTime = Time.time;
        }

        // End of trial when the sphere arrive to destination (targetDistance)
        if (progress >= 1f)
        {
            if (isControlTrial)
            {
                UnityEngine.Debug.Log($"CATCH TRIAL (no onset), trial n°{trialIndex + 1}");
                step = VTISteps.TrialEnd;
            }
        }
    }

    public void ExitTask()
    {
        headLook.ExitHeadLook();

        if (stimulus != null) GameObject.Destroy(stimulus);
        stimulus = null;

        if (hand != null) GameObject.Destroy(hand);
        hand = null;
    }

    private GameObject Spawn(string resourcePath, Vector3 pos)
    {
        var prefab = Resources.Load<GameObject>(resourcePath);
        return GameObject.Instantiate(prefab, pos, Quaternion.identity);
    }

    private void SpawnSphere()
    {
        endPos = new Vector3(handPos.x - 0.1f, handPos.y, handPos.z + 0.5f);
        startPos = new Vector3(handPos.x - 0.5f, handPos.y - 0.1f, handPos.z + 2f);
        stimulus = Spawn("Sphere", startPos);
        stimulus.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        var renderer = stimulus.GetComponent<Renderer>();
        renderer.material.color = Color.red;
    }

    private void SpawnCube()
    {
        var cam = Camera.main.transform;
        handPos = cam.position + cam.forward * 0.35f + cam.up * -0.15f;
        hand = Spawn("Cube", handPos);
        hand.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        hand.transform.rotation = Quaternion.LookRotation(cam.forward, Vector3.up);
    }

    private void LaunchNewTrial()
    {
        // Reset
        beeped = false;
        step = VTISteps.TrialRunning;
        moveStartTime = Time.time;
        isControlTrial = false;
        forceNextTrial = false;

        if (stimulus != null)
            GameObject.Destroy(stimulus);

        SpawnSphere();

        targetDistance = shuffledNewDistancesList[trialIndex];

        if (targetDistance == -1f)
            isControlTrial = true;

    }
}



