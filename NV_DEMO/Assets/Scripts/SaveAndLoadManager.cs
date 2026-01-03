using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class SaveAndLoadManager : MonoBehaviour
{
    public GameObject saveAndLoadPanel;
    public TextMeshProUGUI panelTitle;
    public Button[] saveAndLoadButtons;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button backButton;

    private bool isSave;
    private int currentPage = Constants.DEFAULT_START_INDEX;
    private readonly int slotsPerPage = Constants.SLOTS_PER_PAGE;
    private readonly int totalSlots = Constants.TOTAL_SLOTS;
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
        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);
        saveAndLoadPanel.SetActive(false);
    }

    //public void ShowSaveAndLoadUI(bool save)
    //{
    //    isSave = save;
    //    panelTitle.text = isSave ? Constants.SAVE_GAME : Constants.LOAD_GAME;
    //    UpdateSaveAndLoadUI();
    //    saveAndLoadPanel.SetActive(true);
    //    LoadStorylineAndScreenShots();
    //}

    //private void LoadStorylineAndScreenShots()
    //{

    //}

    public void ShowSavePanel(System.Action<int> action)
    {
        isSave = true;
        panelTitle.text = Constants.SAVE_GAME;
        currentAction = action;
        UpdateUI();
        saveAndLoadPanel.SetActive(true);
    }

    public void ShowLoadPanel(System.Action<int> action, System.Action menuAction)
    {
        isSave = false;
        panelTitle.text = Constants.LOAD_GAME;
        currentAction = action;
        this.menuAction = menuAction;
        UpdateUI();
        saveAndLoadPanel.SetActive(true);
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

    private void UpdateSaveAndLoadButtons(Button button, int index)
    {
        button.gameObject.SetActive(true);
        button.interactable = true;

        var savePath = GenerateDataPath(index);
        var fileExists = File.Exists(savePath);

        if (!isSave && !fileExists)
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

    private void OnButtonClick(Button button, int index)
    {
        menuAction?.Invoke();
        currentAction?.Invoke(index);
        if (isSave)
        {
            LoadStorylineAndScreenShots(button, index);
        }
        else
        {
            GoBack();
        }
    }
    private void LoadStorylineAndScreenShots(Button button, int index)
    {
        var savePath = GenerateDataPath(index); 
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<NV_Manager.SaveData>(json);
            if (saveData.savedscreenshotData != null)
            {
                Texture2D screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(saveData.savedscreenshotData);
                button.GetComponentInChildren<RawImage>().texture = screenshot;
            }
            if (saveData.savedSpeakingContent != null)
            {
                var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                textComponents[0].text = saveData.savedSpeakingContent;
                textComponents[1].text = File.GetLastWriteTime(savePath).ToString("G");
            }

        }
    }

    private void UpdateSaveAndLoadUI()
    {
        for (int i = 0; i < slotsPerPage; i++)
        {
            int slotIndex = currentPage * slotsPerPage + i;
            if (slotIndex < totalSlots)
            {
                saveAndLoadButtons[i].gameObject.SetActive(true);
                saveAndLoadButtons[i].interactable = true;
                var slotText = (slotIndex + 1) + Constants.COLON + Constants.EMPTY_SLOT;
                var textComponents = saveAndLoadButtons[i].GetComponentsInChildren<TextMeshProUGUI>();
                textComponents[0].text = null;
                textComponents[1].text = slotText;
                saveAndLoadButtons[i].GetComponentInChildren<RawImage>().texture = null;
            }
            else
            {
                saveAndLoadButtons[i].gameObject.SetActive(false);
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
        saveAndLoadPanel.SetActive(false);
    }

    private string GenerateDataPath(int index) {
        return Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH, index + Constants.SAVE_FILE_EXTENSION);
    }
}
