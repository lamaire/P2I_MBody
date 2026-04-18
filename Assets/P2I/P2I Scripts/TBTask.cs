using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.UI;
using TMPro;

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

    // ADDED : Slider
    private CanvasGroup sliderCanvasGroup;
    private Slider slider;
    private TMP_Text sliderValueText;
    private TMP_Text sliderMinText;
    private TMP_Text sliderMaxText;
    private TMP_Text instructionText;
    private CanvasGroup instructionCanvasGroup;
    private bool dataSaved = false;
    private InputAction sliderAdjust;

    public int TrialIndex => trialIndex;
    public int NumberOfTrials => numberOfTrials;
    public bool ForceNextTrial => forceNextTrial;
    public string TaskStep => taskStep;

    public TBTask(InputAction graspAction, InputAction lookAction, InputAction sliderAdjustAction)
    {
        grasp = graspAction;
        headLook = new HeadLookController(lookAction);
        sliderAdjust = sliderAdjustAction;
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

        // ADDED : Slider & Instructions
        GameObject wall = GameObject.Find("CanvaWallFront");

        Transform infoTf = wall.transform.Find("TextInfoForSubject");
        Transform sliderTf = wall.transform.Find("SliderFeedback");

        sliderValueText = sliderTf.Find("ValueText").GetComponent<TMP_Text>();
        sliderMinText = sliderTf.Find("MinText").GetComponent<TMP_Text>();
        sliderMaxText = sliderTf.Find("MaxText").GetComponent<TMP_Text>();

        instructionText = infoTf.GetComponent<TMP_Text>();
        instructionCanvasGroup = infoTf.GetComponent<CanvasGroup>();

        slider = sliderTf.GetComponent<Slider>();
        sliderCanvasGroup = sliderTf.GetComponent<CanvasGroup>();

        HideInstructions();
        HideSlider();

        // Durations
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
                ShowInstructions("Press the keyboard (W, X or Z) to start.", TextAlignmentOptions.Midline); // ADDED

                if (grasp.WasPressedThisFrame())
                {
                    UnityEngine.Debug.Log("Keyboard pressed !");
                    UnityEngine.Debug.Log($"Target duration : {targetDuration} ms");

                    trialSw.Restart();
                    step = TBSteps.WaitingBeepDelay;
                }
                break;

            case TBSteps.WaitingBeepDelay:

                taskStep = "WaintingBeepDelay";
                HideInstructions(); // ADDED
                if (trialSw.ElapsedMilliseconds >= targetDuration)
                {
                    trialSw.Reset();
                    AudioManager.PlayBeep();
                    UnityEngine.Debug.Log("Beep played !");
                    step = TBSteps.WaitingResponse;
                }
                break;

            case TBSteps.WaitingResponse:

                taskStep = "WaitingResponse";
                // ADDED
                ShowInstructions("Use the slider to estimate the duration.", TextAlignmentOptions.Top);
                ShowSlider();

                UpdateSliderWithInput();
                UpdateSliderLabel();

                if (Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    int estimation = Mathf.RoundToInt(slider.value);
                    RegisterEstimation(estimation);
                }
                break;

            case TBSteps.TrialEnd:

                taskStep = "TrialEnd";
                HideSlider(); // ADDED

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
        //ADDED
        HideSlider();
        HideInstructions();

        if (dataSaved == false)
        {
            // SaveTBData("P001");
            SaveTBData();
            dataSaved = true;
        }
    }

    public void LaunchNewTrial()
    {
        trialIndex++;
        step = TBSteps.TrialRunning;
        targetDuration = shuffledNewDurationsList[trialIndex - 1];
        ResetSlider(); //ADDED
    }

    public void RegisterEstimation(float estimate)
    {
        UnityEngine.Debug.Log($"Estimated duration : {estimate} , target duration : {targetDuration}");
        estimatedDurations.Add(Mathf.RoundToInt(estimate));
        step = TBSteps.TrialEnd;
    }

    //ADDED
    private void ShowSlider()
    {
        sliderCanvasGroup.alpha = 1f;
        sliderCanvasGroup.interactable = true;
        sliderCanvasGroup.blocksRaycasts = true;
    }

    private void HideSlider()
    {
        sliderCanvasGroup.alpha = 0f;
        sliderCanvasGroup.interactable = false;
        sliderCanvasGroup.blocksRaycasts = false;
    }

    private void UpdateSliderLabel()
    {
        sliderValueText.text = $"{slider.value:0} ms";
    }

    private void UpdateSliderWithInput()
    {
        if (slider == null) return;

        float input = sliderAdjust.ReadValue<float>();

        if (Mathf.Abs(input) > 0.01f)
        {
            float stepSize = 10f; // 10 ms par cran / impulsion
            slider.value += Mathf.Sign(input) * stepSize;
            slider.value = Mathf.Clamp(slider.value, slider.minValue, slider.maxValue);
        }
    }

    private void ResetSlider()
    {
        slider.value = 0f;
        sliderMinText.text = $"{slider.minValue:0} ms";
        sliderMaxText.text = $"{slider.maxValue:0} ms";
        sliderValueText.text = $"{slider.value:0} ms";
    }

    private void ShowInstructions(string message, TextAlignmentOptions alignmentOption)
    {
        instructionText.text = message;
        instructionText.alignment = alignmentOption;

        instructionCanvasGroup.alpha = 1f;
        instructionCanvasGroup.interactable = false;
        instructionCanvasGroup.blocksRaycasts = false;
    }

    private void HideInstructions()
    {
        instructionCanvasGroup.alpha = 0f;
        instructionCanvasGroup.interactable = false;
        instructionCanvasGroup.blocksRaycasts = false;

    }

    public void SaveTBData()
    {
        TBData data = new TBData();

        int count = Mathf.Min(shuffledNewDurationsList.Count, estimatedDurations.Count);

        for (int i = 0; i < count; i++)
        {
            TBTrialData trial = new TBTrialData();
            trial.trueDelay = shuffledNewDurationsList[i];
            trial.estimatedDelay = estimatedDurations[i];

            data.trials.Add(trial);
        }

        string json = JsonUtility.ToJson(data, true);

        string fileName = $"TB_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        JsonSaver.SaveJson(json, fileName);
    }

}
