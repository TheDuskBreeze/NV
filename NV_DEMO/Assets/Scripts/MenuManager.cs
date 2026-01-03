using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManger : MonoBehaviour
{
    public GameObject menuPanel;
    public Button startButton;
    public Button continueButton;
    public Button loadButton;
    public Button settingsButton;
    public Button quitButton;

    private bool hasStarted = false;

    public static MenuManger Instance { get; private set; }

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
        MenuButtonsAddListener();
    }

    void MenuButtonsAddListener()
    {
        startButton.onClick.AddListener(StartNewGame);
        startButton.onClick.AddListener(ShowInputPanel);
        continueButton.onClick.AddListener(ContinueGame);
        loadButton.onClick.AddListener(LoadGame);
        settingsButton.onClick.AddListener(ShowSettingPanel);
    }

    public void StartNewGame()
    {
        hasStarted = true;
        NV_Manager.Instance.StartGame();
        ShowGamePanel();
    }

    private void ContinueGame()
    {
        if (hasStarted)
        {
           ShowGamePanel();
        }
    }

    private void LoadGame()
    {
        NV_Manager.Instance.ShowLoadPanel(ShowGamePanel);
    }

    private void ShowGamePanel()
    {
        menuPanel.SetActive(false);
        NV_Manager.Instance.gamePanel.SetActive(true);
    }

    private void ShowSettingPanel()
    {
        SettingManager.Instance.ShowSettingPanel();
    }
    private void ShowInputPanel()
    {
        InputManager.Instance.ShowInputPanel();
    }
}
