using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HistoryManager : MonoBehaviour
{
    public Transform historyContent;
    public GameObject historyItemPrefab;
    public GameObject historyScrollView;
    public Button closeButton;

    private LinkedList<ExcelReader.ExcelData> historyRecords;
    public static HistoryManager Instance { get; private set; } 

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
        closeButton.onClick.AddListener(CloseHistory);
        ShowHistory(GameManager.Instance.historyRecords);
    }

    public void CloseHistory() {
        GameManager.Instance.historyRecords.RemoveLast();
        SceneManager.LoadScene(Constants.GAME_SCENE);
    }

    public void ShowHistory(LinkedList<ExcelReader.ExcelData> records)
    {
        foreach (Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }
        historyRecords = records;
        LinkedListNode<ExcelReader.ExcelData> currentNode = historyRecords.Last;
        while (currentNode != null)
        {
            var name = currentNode.Value.speaker;
            var content = currentNode.Value.content;
            AddHistoryItem(name + Constants.COLON + content);
            currentNode = currentNode.Previous;
        }

        historyContent.GetComponent<RectTransform>().localPosition = Vector3.zero;
        historyScrollView.SetActive(true);
    }

    private void AddHistoryItem(string text)
    {
        GameObject historyItem = Instantiate(historyItemPrefab, historyContent);
        historyItem.GetComponentInChildren<TextMeshProUGUI>().text = text;
        historyItem.transform.SetAsFirstSibling();
    }
}
