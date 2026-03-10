using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;
#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
#endif

public enum ExpeSteps { Menu, Launching, RunningTask, Ending }
public enum TaskTypes { VTI, TB } // Visuo-Tactile Integration & Temporal Binding

public class P2IManager : MonoBehaviour
{
    P2IUI myUI;
    ExpeSteps step = ExpeSteps.Menu;
    IP2ITask currentTask;
    InputAction graspAction; // Keyboard : W, Z or X
    InputAction lookAction; // Keyboard : arrows
    private bool timeSliderVisible = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myUI = GameObject.Find("InterfaceP2I").GetComponent<P2IUI>();

        BindInput();
        BindUI();
        GoToMenu();

    }

    // Update is called once per frame
    void Update()
    {
        if (step == ExpeSteps.RunningTask && currentTask != null)
        {
            currentTask.UpdateTask();
            myUI.UpdateTrialCounter(currentTask.TrialIndex, currentTask.NumberOfTrials);

            if (currentTask is VTITask)
            {
                myUI.buttonTrial.gameObject.SetActive(true); // Button to force Next Trial to start

                if (currentTask.TaskStep == "InterTrial")
                    myUI.buttonTrial.interactable = true;
                else
                    myUI.buttonTrial.interactable = false;
            }

            if (currentTask is TBTask && currentTask.TaskStep == "WaitingResponse")
            {
                // Slider management
                if (!timeSliderVisible)
                {
                    myUI.ShowTimeSlider();
                    myUI.ResetTimeSlider(0f);
                    timeSliderVisible = true;
                }

                if (Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    float estimate = myUI.GetSliderValue();

                    if (currentTask is TBTask tbTask)
                    {
                        tbTask.RegisterEstimate(estimate);
                    }

                    myUI.HideTimeSlider();
                    timeSliderVisible = false;
                }
            }
            else
            {
                if (timeSliderVisible)
                {
                    myUI.HideTimeSlider();
                    timeSliderVisible = false;
                }
            }
        }
    }

    void BindInput()
    {
        graspAction = InputSystem.actions.FindAction("Grasp");
        lookAction = InputSystem.actions.FindAction("HeadLook");
    }

    void BindUI()
    {
        myUI.buttonVTI.onClick.AddListener(() => OnClickLaunch(TaskTypes.VTI));
        myUI.buttonTB.onClick.AddListener(() => OnClickLaunch(TaskTypes.TB));
        myUI.buttonMenu.onClick.AddListener(() => GoToMenu());
        myUI.buttonMenu.onClick.AddListener(() => currentTask?.RequestNextTrial());
    }

    void GoToMenu()
    {
        currentTask?.ExitTask();
#if UNITY_EDITOR
        ClearConsole();
#endif
        step = ExpeSteps.Menu;
        myUI.SwitchToScreen(myUI.screenMainMenu);
    }

    void OnClickLaunch(TaskTypes type)
    {
        step = ExpeSteps.Launching;
        myUI.SwitchToScreen(myUI.screenTask);

        currentTask?.ExitTask();

        currentTask = CreateTask(type);
        currentTask.EnterTask();

        step = ExpeSteps.RunningTask;
    }

    IP2ITask CreateTask(TaskTypes type)
    {
        switch (type)
        {
            case TaskTypes.VTI:
                return new VTITask(graspAction, lookAction);
            default:
                return new TBTask(graspAction, lookAction);
        }
    }

#if UNITY_EDITOR
    private void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var logEntriesType = assembly.GetType("UnityEditor.LogEntries");
        var clearMethod = logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
#endif
}
