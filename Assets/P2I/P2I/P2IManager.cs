using UnityEngine;
using UnityEngine.InputSystem;

public class P2IManager : MonoBehaviour
{
    public enum ExpeSteps { Menu, Launching, RunningTask, Ending }
    public enum TaskTypes { VTI, TB } // Visuo-Tactile Integration & Temporal Binding
    P2IUI myUI;
    ExpeSteps step = ExpeSteps.Menu;
    IP2ITask currentTask;
    InputAction graspAction; // Keyboard : W, Z or X
    InputAction lookAction; // Keyboard : arrows
    InputAction sliderAdjustAction;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        graspAction = InputSystem.actions.FindAction("Grasp");
        lookAction = InputSystem.actions.FindAction("HeadLook");
        sliderAdjustAction = InputSystem.actions.FindAction("SliderAdjust");

        SetUpUI();
        GoToMenu();
    }
    void SetUpUI()
    {
        myUI = GameObject.Find("InterfaceP2I").GetComponent<P2IUI>();
        myUI.buttonVTI.onClick.AddListener(() => OnClickLaunch(TaskTypes.VTI));
        myUI.buttonTB.onClick.AddListener(() => OnClickLaunch(TaskTypes.TB));
        myUI.buttonMenu.onClick.AddListener(() => GoToMenu());
        myUI.buttonTrial.onClick.AddListener(() => currentTask?.RequestNextTrial()); // CHANGED : Je m'étais trompée ici c'est buttonTrial et non buttonMenu !
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
        }
    }

    void GoToMenu()
    {
        currentTask?.ExitTask();
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
                return new TBTask(graspAction, lookAction, sliderAdjustAction);
        }
    }
}
