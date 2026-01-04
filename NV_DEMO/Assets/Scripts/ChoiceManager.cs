using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    public GameObject choicePanel;
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;

    public static ChoiceManager Instance { get; private set; }
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
        choicePanel.SetActive(false);
    }

    public void ShowChoices(List<string> options, List<string> actions, Action<string> onChoiceSelected)
    {
        foreach (Transform child in choiceButtonContainer) { 
            Destroy(child.gameObject);
        }
        for (int i = 0; i < options.Count; i++)
        {
            Button choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            int index = i;
            choiceButton.onClick.RemoveAllListeners();
            choiceButton.onClick.AddListener(() =>
            {
                onChoiceSelected?.Invoke(actions[index]);
                choicePanel.SetActive(false);
            });
        }
        choicePanel.SetActive(true);
    }
}
