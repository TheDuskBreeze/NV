using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveAndLoadManager : MonoBehaviour
{
    public TextMeshProUGUI panelTitle;
    public Button[] saveAndLoadButtons;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button backButton;

    private bool isSave;
    private int currentPage = Constants.DEFAULT_START_INDEX;
    private readonly int slotsPerPage = Constants.SLOTS_PER_PAGE;
    private readonly int totalSlots = Constants.TOTAL_SLOTS;

    private bool isLoad => GameManager.Instance.currentSaveLoadMode == GameManager.SaveLoadMode.Load;

    private System.Action<int> currentAction;
    private System.Action menuAction;

    public static SaveAndLoadManager Instance { get; private set; }

    private void Awake()     {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        prevPageButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.PREV_PAGE;
        nextPageButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.NEXT_PAGE;
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.BACK;

        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);
        
        panelTitle.text = isLoad ? Constants.LOAD_GAME : Constants.SAVE_GAME;
        UpdateUI();
    }
    private void UpdateUI()
    {
        for (int i = 0; i < slotsPerPage; i++) {
            int slotIndex = currentPage * slotsPerPage + i;
            if (slotIndex < totalSlots)
            {
                UpdateSaveAndLoadButtons(saveAndLoadButtons[i], slotIndex);
                LoadStorylineAndScreenShots(saveAndLoadButtons[i], slotIndex);
            }
            else
            {
                saveAndLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void OnButtonClick(Button button, int index)
    {
        if (!isLoad)
        {
            GameManager.Instance.Save(index);
            LoadStorylineAndScreenShots(button, index);
        }
        else
        {
            GameManager.Instance.Load(index);
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
    }
    private void UpdateSaveAndLoadButtons(Button button, int index)
    {
        button.gameObject.SetActive(true);
        button.interactable = true;

        var savePath = GameManager.Instance.GenerateDataPath(index);
        var fileExists = File.Exists(savePath);

        if (isLoad && !fileExists)
        {
            button.interactable = false;
        }

        var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
        textComponents[0].text = null;
        textComponents[1].text = (index + 1) + Constants.COLON + Constants.EMPTY_SLOT;
        button.GetComponentInChildren<RawImage>().texture = null;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButtonClick(button, index));
    }
    private void LoadStorylineAndScreenShots(Button button, int index)
    {
        var savePath = GameManager.Instance.GenerateDataPath(index); 
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<GameManager.SaveData>(json);
            if (saveData.savedScreenshotData != null)
            {
                Texture2D screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(saveData.savedScreenshotData);
                button.GetComponentInChildren<RawImage>().texture = screenshot;
            }
            if (saveData.savedHistoryRecords.Last != null)
            {
                var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                textComponents[0].text = saveData.savedHistoryRecords.Last.Value.content;
                textComponents[1].text = File.GetLastWriteTime(savePath).ToString("G");
            }

        }
    }
    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    private void NextPage()
    {
        if ((currentPage + 1) * slotsPerPage < totalSlots)
        {
            currentPage++;
            UpdateUI();
        }
    }

    private void GoBack()
    {
        var sceneName = GameManager.Instance.currentScene;
        if (sceneName == Constants.GAME_SCENE)
        {
            GameManager.Instance.historyRecords.RemoveLast();
        }
        GameManager.Instance.pendingData = null;
        SceneManager.LoadScene(sceneName);
    }
}
