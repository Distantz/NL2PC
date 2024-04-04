using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class settingsMenu : MonoBehaviour
{

    public bool qualityToggle;
    public bool toggleUi;

    public bool settingsUi;

    public bool currentlyHolding;

    public Canvas uiContainer;
    public Canvas settingsContainer;

    public Slider timerDur;
    public Slider qualitySetting;
    public Button postPro;
    public Button resume;
    public Button quit;

    public int timerNum;
    public int qualityNum;
    public int isUsingPostPro;

    private CSV manager;


    public PostProcessVolume PP;

    private string[] levels;

    // Start is called before the first frame update
    void Start()
    {

        manager = gameObject.GetComponent<CSV>();

        toggleUi = false;
        currentlyHolding = false;

        timerDur.onValueChanged.AddListener(delegate { changeTimerDir(); });
        qualitySetting.onValueChanged.AddListener(delegate { changeQuality(); });
        resume.onClick.AddListener(toggleSettingsUI);
        quit.onClick.AddListener(doQuit);
        postPro.onClick.AddListener(changePostPro);

    }

    void applySettings()
    {
        PlayerPrefs.Save();

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("qualityIndex") - 1);

        manager.timerDuration = (float)PlayerPrefs.GetInt("timerDuration");

        if (PlayerPrefs.GetInt("postProcessing") == 1)
        {
            PP.enabled = true;
        }
        else
        {
            PP.enabled = false;
        }
    }

    void doQuit()
    {
        Application.Quit();
    }

    void changeQuality()
    {
        PlayerPrefs.SetInt("qualityIndex", (int)qualitySetting.value);

        qualitySetting.GetComponentInChildren<TMP_Text>().text = string.Format("{0}", (int)qualitySetting.value);

    }

    void changeTimerDir()
    {
        PlayerPrefs.SetInt("timerDuration", (int)timerDur.value);

        timerDur.GetComponentInChildren<TMP_Text>().text = string.Format("{0}", (int)timerDur.value);
    }
    
    void changePostPro()
    {

        if (PlayerPrefs.GetInt("postProcessing") == 1)
        {
            postPro.GetComponentInChildren<TMP_Text>().text = "post processing\ndisabled";
            PlayerPrefs.SetInt("postProcessing", 0);
        }
        else
        {
            postPro.GetComponentInChildren<TMP_Text>().text = "post processing\nenabled";
            PlayerPrefs.SetInt("postProcessing", 1);

        }
    }
    
    void toggleSettingsUI()
    {
        settingsUi = !settingsUi;

        if (settingsUi)
        {

            manager.doTimer = false;
            manager.currentTimerVal = 0;
            manager.timerImage.fillAmount = 0.5f;

            uiContainer.enabled = false;
            settingsContainer.enabled = true;

            timerNum = PlayerPrefs.GetInt("timerDuration", 8);
            qualityNum = PlayerPrefs.GetInt("qualityIndex", 6);
            isUsingPostPro = PlayerPrefs.GetInt("postProcessing", 1);

            timerDur.value = timerNum;
            qualitySetting.value = qualityNum;

            timerDur.GetComponentInChildren<TMP_Text>().text = string.Format("{0}", (int)timerDur.value);
            qualitySetting.GetComponentInChildren<TMP_Text>().text = string.Format("{0}", (int)qualitySetting.value);

            if (isUsingPostPro == 1)
            {
                postPro.GetComponentInChildren<TMP_Text>().text = "post processing\nenabled";
                PP.enabled = true;
            }
            else
            {
                postPro.GetComponentInChildren<TMP_Text>().text = "post processing\ndisabled";
                PP.enabled = false;
            }
            

        }
        else
        {

            manager.doTimer = false;
            manager.currentTimerVal = 0;
            manager.timerImage.fillAmount = 0.5f;

            toggleUi = false;
            uiContainer.enabled = true;
            settingsContainer.enabled = false;

            applySettings();
        }
    }

    void uiToggle()
    {
        toggleUi = !toggleUi;

        if (toggleUi)
        {
            uiContainer.enabled = false;
        }
        else if (!settingsUi)
        {
            uiContainer.enabled = true;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.G) && currentlyHolding == false)
        {
            currentlyHolding = true;
            uiToggle();
        }
        else if (!Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.G) && currentlyHolding == true)
        {
            currentlyHolding = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleSettingsUI();
        }
        
    }
}
