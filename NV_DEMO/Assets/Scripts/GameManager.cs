using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string playerName;
    public string currentScene;
    public string currentStoryFile;
    public int currentLineIndex;
    public string currentBackgroundImg;
    public string currentBackgroundMusic;
    public bool isCharacter1Display;
    public bool isCharacter2Display;
    public string currentCharacter1Img;
    public string currentCharacter2Img;
    public string currentCharacter1Position;
    public string currentCharacter2Position;

    public bool hasStarted;
    public Dictionary<string, int> maxReachedLineIndices = new Dictionary<string, int>();
    public LinkedList<ExcelReader.ExcelData> historyRecords;

    public enum SaveLoadMode { None, Save, Load }
    public SaveLoadMode currentSaveLoadMode { get; set; } = SaveLoadMode.None;
    public SaveData pendingData;

    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public byte[] savedScreenshotData;
        public LinkedList<ExcelReader.ExcelData> savedHistoryRecords;
        public string savedBackgroundImg;
        public string savedBackgroundMusic;
        public string savedCharacter1Img;
        public string savedCharacter2Img;
        public string savedCharacter1Position;
        public string savedCharacter2Position;
        public bool savedCharacter1Display;
        public bool savedCharacter2Display;
        public string savedPlayerName;
    }

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Save(int slotIndex)
    {
        string path = GenerateDataPath(slotIndex);
        File.WriteAllText(path, JsonConvert.SerializeObject(pendingData, Formatting.Indented));
    }
    public void Load(int slotIndex)
    {
        string path = GenerateDataPath(slotIndex);
        pendingData = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path));
    }
    public string GenerateDataPath(int index)
    {
        return Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH, index + Constants.SAVE_FILE_EXTENSION);
    }
}
