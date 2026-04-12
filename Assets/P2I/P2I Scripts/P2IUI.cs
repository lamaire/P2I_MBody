using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class P2IUI : MonoBehaviour
{
    private List<GameObject> screensList;
    public GameObject screenMainMenu;
    public Button buttonVTI;
    public Button buttonTB;
    public Button buttonMenu;
    public Button buttonTrial;
    public GameObject screenTask;
    public TextMeshProUGUI trialCounterText;


    void Awake()
    {
        screensList = new List<GameObject>(){
            screenMainMenu,
            screenTask
        };
    }

    public void SwitchToScreen(GameObject screenToShow)
    {
        HideInterface();
        screenToShow.SetActive(true);
    }

    public void HideInterface()
    {
        foreach (GameObject screen in screensList)
        {
            if (screen != null)
                screen.SetActive(false);
        }
        buttonTrial.gameObject.SetActive(false);
    }

    public void UpdateTrialCounter(int current, int total)
    {
        if (trialCounterText != null)
            trialCounterText.text = $"Trial {current} / {total}";
    }

}
