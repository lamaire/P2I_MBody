using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class TBTask : IP2ITask
{

    enum TBSteps { Init, TrialRunning, WaitingBeepDelay, WaitingResponse, TrialEnd, Finished }
    TBSteps step = TBSteps.Init;
    string taskStep = "Init";

    InputAction grasp;
    private readonly HeadLookController headLook;
    public static int numberOfTrials = 3;
    public int trialIndex;
    public bool forceNextTrial = false;
    public int targetDuration;
    private readonly Stopwatch trialSw = new Stopwatch();
    private List<int> durationsList = new List<int> { 300, 500, 700 }; // in ms
    public List<int> estimatedDurations = new List<int>();
    private List<int> shuffledNewDurationsList = new List<int>();

    public int TrialIndex => trialIndex;
    public int NumberOfTrials => numberOfTrials;
    public bool ForceNextTrial => forceNextTrial;
    public string TaskStep => taskStep;

    public TBTask(InputAction graspAction, InputAction lookAction)
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
        UnityEngine.Debug.Log("TB Task running");
        headLook.EnterHeadLook();
        trialIndex = 0;
        estimatedDurations.Clear();

        var index = 0;
        var newDurationsList = new List<int>(durationsList);

        for (int i = newDurationsList.Count; i < numberOfTrials; i++)
        {
            if (index >= durationsList.Count)
                index = 0;

            newDurationsList.Add(durationsList[index]);
            index++;
        }

        shuffledNewDurationsList = newDurationsList.OrderBy(x => Random.value).ToList();

        LaunchNewTrial();
    }

    public void UpdateTask()
    {
        if (step == TBSteps.Finished) return;

        headLook.UpdateHeadLook();

        switch (step)
        {

            case TBSteps.TrialRunning:

                taskStep = "TrialRunning";
                UnityEngine.Debug.Log("Please press the keyboard (W, X or Z) to start.");

                if (grasp.WasPressedThisFrame())
                {
                    UnityEngine.Debug.Log("Keyboard pressed !");
                    trialSw.Restart();
                    step = TBSteps.WaitingBeepDelay;
                }
                break;

            case TBSteps.WaitingBeepDelay:

                taskStep = "WaintingBeepDelay";
                if (trialSw.ElapsedMilliseconds >= targetDuration)
                {
                    trialSw.Reset();
                    AudioService.PlayBeep();
                    UnityEngine.Debug.Log("Beep played !");
                    step = TBSteps.WaitingResponse;
                }
                break;

            case TBSteps.WaitingResponse:

                taskStep = "WaitingResponse";
                // Slider managed by P2IManager.cs
                UnityEngine.Debug.Log("Please estimate the time duration with the slider, and press 'Enter' to continue.");
                break;

            case TBSteps.TrialEnd:

                taskStep = "TrialEnd";
                trialIndex++;
                if (trialIndex >= numberOfTrials)
                {
                    UnityEngine.Debug.Log("END EXPERIMENT");
                    UnityEngine.Debug.Log("=== ESTIMATED DURATIONS ===");
                    UnityEngine.Debug.Log(string.Join(", ", estimatedDurations.ConvertAll(rt => rt.ToString())));
                    UnityEngine.Debug.Log("=== DURATIONS ===");
                    UnityEngine.Debug.Log(string.Join(", ", shuffledNewDurationsList.ConvertAll(d => d.ToString())));

                    ExitTask();
                }
                else
                {
                    LaunchNewTrial();
                }
                break;

            case TBSteps.Finished:

                // End of experiment
                taskStep = "Finished";
                break;
        }
    }

    public void ExitTask()
    {
        step = TBSteps.Finished;
        headLook.ExitHeadLook();
    }

    public void LaunchNewTrial()
    {
        step = TBSteps.TrialRunning;
        targetDuration = shuffledNewDurationsList[trialIndex];
    }

    public void RegisterEstimate(float estimate)
    {
        UnityEngine.Debug.Log($"Estimated duration : {estimate} , target duration : {targetDuration}");
        estimatedDurations.Add(Mathf.RoundToInt(estimate));
        step = TBSteps.TrialEnd;
    }
}
