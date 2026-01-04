using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManger : MonoBehaviour
{
    public Image backgroundImage;

    public Button startButton;
    public Button continueButton;
    public Button loadButton;
    public Button settingsButton;
    public Button quitButton;

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
        GameManager.Instance.currentScene = Constants.MENU_SCENE ;
        MenuButtonsAddListener();
    }

    void MenuButtonsAddListener()
    {
        startButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        loadButton.onClick.AddListener(LoadGame);
        settingsButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.SETTING_SCENE));
        quitButton.onClick.AddListener(QuitGame);
    }

    public void StartNewGame()
    {
        GameManager.Instance.currentStoryFile = Constants.DEFAULT_STORY_FILE;
        GameManager.Instance.currentLineIndex = Constants.DEFAULT_START_LINE;
        GameManager.Instance.currentBackgroundImg = string.Empty;
        GameManager.Instance.currentBackgroundMusic = string.Empty;
        GameManager.Instance.isCharacter1Display = false;
        GameManager.Instance.isCharacter2Display = false;
        GameManager.Instance.historyRecords = new LinkedList<ExcelReader.ExcelData>();
        SceneManager.LoadScene(Constants.INPUT_SCENE);
    }

    private void ContinueGame()
    {
        if (GameManager.Instance.hasStarted)
        {
           GameManager.Instance.historyRecords.RemoveLast();
           SceneManager.LoadScene(Constants.GAME_SCENE);
        }
    }

    private void LoadGame()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Load;
        SceneManager.LoadScene(Constants.SAVE_AND_LOAD_SCENE);
    }
    private void QuitGame()
    {
        Application.Quit();
    }
}
