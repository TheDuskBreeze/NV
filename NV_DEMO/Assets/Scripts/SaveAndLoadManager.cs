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
    public SaveSlot[] slots;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button backButton;

    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;
    public Button confirmButton;
    public Button cancelButton;

    private int currentPage = Constants.DEFAULT_START_INDEX;
    private readonly int slotsPerPage = Constants.SLOTS_PER_PAGE;
    private readonly int totalSlots = Constants.TOTAL_SLOTS;

    private bool isLoad => GameManager.Instance.currentSaveLoadMode == GameManager.SaveLoadMode.Load;

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
        panelTitle.text = isLoad ? Constants.LOAD_GAME : Constants.SAVE_GAME;

        prevPageButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.PREV_PAGE;
        nextPageButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.NEXT_PAGE;
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = Constants.BACK;

        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);

        confirmPanel.SetActive(false);

        RefreshPage();
    }

    private void RefreshPage()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int slotIndex = currentPage * slotsPerPage + i;
            if (slotIndex >= totalSlots)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            slots[i].gameObject.SetActive(true);
            slots[i].Init(this, slotIndex);
            slots[i].Refresh();
        }
    }
    public void HandleEmptySlot(int slotIndex, SaveSlot slot)
    {
        SaveToSlot(slotIndex, slot);
    }
    public void HandleExistingSlot(int slotIndex, SaveSlot slot)
    {
        if (isLoad)
        {
            GameManager.Instance.Load(slotIndex);
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
        else
        {
            ShowConfirm(Constants.CONFIRM_COVER_SAVE_FILE, () => { SaveToSlot(slotIndex, slot); });
        }
    }
    public void RequestDelete(int slotIndex, SaveSlot slot)
    {
        ShowConfirm(Constants.CONFIRM_DELETE_SAVE_FILE, () => { DeleteSlot(slotIndex, slot); });
    }
    private void ShowConfirm(string msg, System.Action onYes)
    {
        confirmText.text = msg;
        confirmPanel.SetActive(true);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirmPanel.SetActive(false);
            onYes?.Invoke();
        });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => confirmPanel.SetActive(false)); 
    }
    private void SaveToSlot(int slotIndex, SaveSlot slot)
    {
        GameManager.Instance.Save(slotIndex);
        slot.Refresh();
    }
    private void DeleteSlot(int slotIndex, SaveSlot slot)
    {
        File.Delete(GameManager.Instance.GenerateDataPath(slotIndex));
        slot.Refresh();
    }
    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }
    private void NextPage()
    {
        if ((currentPage + 1) * slotsPerPage < totalSlots)
        {
            currentPage++;
            RefreshPage();
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
