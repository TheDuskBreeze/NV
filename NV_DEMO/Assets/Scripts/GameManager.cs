using System.Collections;
using System.Collections.Generic;
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

    public enum SaveLoadMode { Save, Load }
    public SaveLoadMode currentSaveLoadMode;

    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public byte[] savedScreenshotData;
        public LinkedList<ExcelReader.ExcelData> savedHistoryRecords;
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
}
