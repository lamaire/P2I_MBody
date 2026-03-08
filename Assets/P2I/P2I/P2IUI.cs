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
    public Slider timeSlider;
    public TMP_Text sliderValueText;
    public TMP_Text sliderMinText;
    public TMP_Text sliderMaxText;


    void Awake()
    {
        screensList = new List<GameObject>(){
            screenMainMenu,
            screenTask
        };
    }

    private void Update()
    {
        if (timeSlider != null && timeSlider.gameObject.activeSelf)
        {
            sliderValueText.text = $"{timeSlider.value:0} ms";
        }
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
        HideTimeSlider();
        buttonTrial.gameObject.SetActive(false);
    }

    public void UpdateTrialCounter(int current, int total)
    {
        if (trialCounterText != null)
            trialCounterText.text = $"Trial {current} / {total}";
    }

    public void ShowTimeSlider()
    {
        sliderMinText.text = $"{timeSlider.minValue:0} ms";
        sliderMaxText.text = $"{timeSlider.maxValue:0} ms";
        timeSlider.gameObject.SetActive(true);
    }

    public void HideTimeSlider()
    {
        timeSlider.gameObject.SetActive(false);
    }

    public void ResetTimeSlider(float startValue = 0f)
    {
        timeSlider.value = startValue;
    }

    public void UpdateSliderValue(float value)
    {
        sliderValueText.text = $"{value:0} ms";
        UnityEngine.Debug.Log($"Slider value changed: {value}");
    }

    public float GetSliderValue()
    {
        return timeSlider.value;
    }

}
