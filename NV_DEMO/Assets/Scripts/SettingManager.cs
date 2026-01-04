using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Text toggleLabel;
    public TMP_Dropdown resolutionDropDown;
    public Button defaultButton;
    public Button closeButton;

    private Resolution[] avaliableResolutions;
    private Resolution defaultResolution;
    public static SettingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeResolutions();
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        UpdateToggleLabel(fullscreenToggle.isOn);

        fullscreenToggle.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropDown.onValueChanged.AddListener(SetResolution);
        closeButton.onClick.AddListener(CloseSetting);
        defaultButton.onClick.AddListener(ResetSetting);
    }
    void InitializeResolutions()
    {
        avaliableResolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();

        var resolutionMap = new Dictionary<string, Resolution>();
        int currentResolutionIndex = 0; 

        foreach(var res in avaliableResolutions)
        {
            const float aspectRatio = 16f / 9f;
            const float epsilon = 0.01f;

            if (Mathf.Abs((float)res.width / res.height - aspectRatio) > epsilon)
            {
                continue;
            }

            string option = res.width + "x" + res.height;
            if (!resolutionMap.ContainsKey(option))
            {
                resolutionMap[option] = res;
                resolutionDropDown.options.Add(new TMP_Dropdown.OptionData(option));
                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = resolutionDropDown.options.Count - 1;
                    defaultResolution = res;
                }
            }
        }
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }

    void UpdateToggleLabel(bool isFullscreen) 
    {
        toggleLabel.text = isFullscreen ? "Fullscreen" : "Windowed";
    }

    void SetDisplayMode(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        UpdateToggleLabel(isFullscreen);

    }

    void SetResolution(int index)
    {
        string[] dimensions = resolutionDropDown.options[index].text.Split('x');
        int width = int.Parse(dimensions[0].Trim());
        int height = int.Parse(dimensions[1].Trim());   
        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }
    public void CloseSetting()
    {
        var sceneName = GameManager.Instance.currentScene;
        if (sceneName == Constants.GAME_SCENE)
        {
            GameManager.Instance.historyRecords.RemoveLast();
        }
        SceneManager.LoadScene(sceneName);
    }

    void ResetSetting()
    {
        resolutionDropDown.value = resolutionDropDown.options.FindIndex(option => option.text == $"{defaultResolution.width + "x" + defaultResolution.height}");
        fullscreenToggle.isOn = true;
    }
}
