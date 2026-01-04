using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public Button slotButton;
    public Button deleteButton;
    public RawImage thumbnail;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;

    private int slotIndex;
    private SaveAndLoadManager owner;
    private bool hasFile;

    //public void Init(SaveAndLoadManager mgr, int index)
    //{
    //    owner = mgr; 
    //    slotIndex = index;

    //    slotButton.onClick.RemoveAllListeners();
    //    slotButton.onClick.AddListener(OnSlotClick);

    //    deleteButton.onClick.RemoveAllListeners();
    //    deleteButton.onClick.AddListener(OnDeleteClick);
    //}
    //public void Refresh()
    //{
    //    string path = GameManager.Instance.GenerateDataPath(slotIndex);
    //    hasFile = File.Exists(path);
    //    bool isLoad = GameManager.Instance.currentSaveLoadMode == GameManager.SaveLoadMode.Load;

    //    deleteButton.gameObject.SetActive(hasFile);

    //    slotButton.interactable = hasFile || !isLoad;

    //    thumbnail.texture = null;

    //    if (!hasFile)
    //    {
    //        topText.text = "";
    //        bottomText.text = (slotIndex + 1) + " " + Constants.EMPTY_SLOT;
    //        return;
    //    }

    //    string json = File.ReadAllText(path);
    //    var data = JsonConvert.DeserializeObject(GameManager.SaveData)(json);

    //    if (data.savedScreenshotData != null)
    //    {
    //        Texture2D tex = new Texture2D(2, 2);
    //        tex.LoadImage(data.savedScreenshotData);
    //        thumbnail.texture = tex;
    //    }
    //    if (data.savedHistoryRecords?.Last != null)
    //    {
    //        topText.text = data.savedHistoryRecords.Last.Value;
    //    }
    //    bottomText.text = File.GetLastWriteTime(path).ToString("G");
    //}
    //private void OnSlotClick()
    //{
    //    if (hasFile)
    //    {
    //        owner.HandleExistngSlot(slotIndex, this);
    //    }
    //    else
    //    {
    //        owner.HandleEmptySlot(slotIndex, this);
    //    }
    //}
    //private void OnDeleteClick()
    //{
    //    owner.RequestDelete(slotIndex, this);
    //}
}
