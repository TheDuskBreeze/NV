using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Security;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class NV_Manager : MonoBehaviour {
    #region Variables
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerName;
    public TypeWritterEffect typewritterEffect;
    public ScreenShotter screenShotter;

    public Image avatarImage;
    public Image backgroundImage;
    public Image characterImage1;
    public Image characterImage2;
    public AudioSource vocalAudio;
    public AudioSource backgroundMusic;

    public GameObject choicePanel;
    public Button choiceButton1;
    public Button choiceButton2;

    public GameObject BottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button settingsButton;
    public Button homeButton;
    public Button closeButton;
    public Button historyButton;

    private int defaultStoryStartLine = Constants.DEFAULT_START_LINE;
    private string excelFileExtension = Constants.EXCEL_FILE_EXTENSION;

    private int currentLine;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
    private string currentStoryFileName;
    private List<ExcelReader.ExcelData> storyData;

    private string storyPath = Constants.STORY_PATH;
    private string defaultStoryFileName = Constants.DEFAULT_STORY_FILE;

    private string saveFolderPath;
    private byte[] screenshotData;
    private string currentSpeakingConent;


    #endregion
    #region Lifecycle
    public static NV_Manager Instance { get; private set; }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        GameManager.Instance.currentScene = Constants.GAME_SCENE;
        InitializeSaveFilePath();
        ButtomButtonsAddListener();
        InitializeAndLoadStory(GameManager.Instance.currentStoryFile, GameManager.Instance.currentLineIndex);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogueBox.activeSelf)
            {
                OpenUI();
            }
            else if (!IsHittingBottomButtons())
            {
                DisplayNextLine();
            }  
        }
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (dialogueBox.activeSelf)
            {
                CloseUI();
            }
            else
            {
                OpenUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) ||  Input.GetKeyDown(KeyCode.RightControl))
        {
            CtrlSkip();
        }
    }
    #endregion
    #region Initialization
    void InitializeSaveFilePath()
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH);
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }
    void ButtomButtonsAddListener() {
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
        historyButton.onClick.AddListener(OnHistoryButtonClick);
        homeButton.onClick.AddListener(OnHomeButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
        settingsButton.onClick.AddListener(OnSettingButtonClick);
    }
    void Initialize(int lineNumber)
    {
        currentLine = lineNumber;

        avatarImage.gameObject.SetActive(false);
        vocalAudio.gameObject.SetActive(false);

        backgroundImage.gameObject.SetActive(false);
        backgroundMusic.gameObject.SetActive(false);

        characterImage1.gameObject.SetActive(false);
        characterImage2.gameObject.SetActive(false);

        choicePanel.SetActive(false);
    }
    void InitializeAndLoadStory(string fileName, int lineNumber)
    {
        Initialize(lineNumber);
        LoadStoryFromFile(fileName);
        RecoverLastBackgroundAndCharacter();
        DisplayNextLine();
    }
    public void StartGame() {
        InitializeAndLoadStory(defaultStoryFileName, defaultStoryStartLine);
    }
    void LoadStoryFromFile(string fileName) {
        currentStoryFileName = fileName;
        var filePath = Path.Combine(Application.streamingAssetsPath, Constants.STORY_PATH, fileName + excelFileExtension);
        storyData = ExcelReader.ReadExcel(filePath);
        if (storyData == null || storyData.Count == 0)
        {
            Debug.LogError(Constants.NO_DATA_FOUND);
        }
        GameManager.Instance.currentStoryFile = currentStoryFileName;
        
        if (GameManager.Instance.maxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex = GameManager.Instance.maxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }
    #endregion
    #region Display
    void DisplayNextLine() {
        if (currentLine > maxReachedLineIndex)  //更新最远到达的行数
        {
            maxReachedLineIndex = currentLine;
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if (currentLine >= storyData.Count - 1)    //结尾分支选择
        {
            if (isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF, autoButton);
            }
            if (storyData[currentLine].speaker == Constants.END_OF_STORY)
            {
                GameManager.Instance.hasStarted = false;
                SceneManager.LoadScene(Constants.MENU_SCENE);
            }
            if (storyData[currentLine].speaker == Constants.CHOICE)
            {
                ShowChoices();
            }
            if (storyData[currentLine].speaker == Constants.GOTO)
            {
                InitializeAndLoadStory(storyData[currentLine].content, defaultStoryStartLine);
            }
            return;
        }
        if (typewritterEffect.IsTyping()) 
        {
            typewritterEffect.CompleteTyping();
        }
        else {
            DisplayThisLine();
        }
    }
    void DisplayThisLine() {
        GameManager.Instance.currentLineIndex = currentLine;
        var data = storyData[currentLine];
        string playerName = GameManager.Instance.playerName;
        string speaker = data.speaker.Replace(Constants.NAME_PLACEHOLDER, playerName);
        string content = data.content.Replace(Constants.NAME_PLACEHOLDER, playerName);
        speakerName.text = speaker;
        currentSpeakingConent = content;

        typewritterEffect.StartTyping(currentSpeakingConent, currentTypingSpeed);

        GameManager.Instance.historyRecords.AddLast(data);

        if (NotNullNorEmpty(data.avatarImageFileName)) {
            UpdateAvatarImage(data.avatarImageFileName);
        }
        else {
            avatarImage.gameObject.SetActive(false);
        }
        if (NotNullNorEmpty(data.vocalAudioFileName)) {
            PlayVocalAudio(data.vocalAudioFileName);
        }
        if (NotNullNorEmpty(data.backgroundImageFileName))
        {
            GameManager.Instance.currentBackgroundImg = data.backgroundImageFileName;
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            GameManager.Instance.currentBackgroundMusic = data.backgroundMusicFileName;
            PlayBackgroundMusic(data.backgroundMusicFileName);
        }
        if (NotNullNorEmpty(data.character1Action))
        {
            if (data.character1Action == Constants.DISAPPEAR)
            {
                GameManager.Instance.isCharacter1Display = false;
            }
            else
            {
                GameManager.Instance.isCharacter1Display = true;
                GameManager.Instance.currentCharacter1Img = data.character1ImageFileName;
                GameManager.Instance.currentCharacter1Position = data.coordinateX1;
            }
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName, characterImage1, data.coordinateX1);
        }
        if (NotNullNorEmpty(data.character2Action))
        {
            if (data.character2Action == Constants.DISAPPEAR)
            {
                GameManager.Instance.isCharacter2Display = false;
            }
            else
            {
                GameManager.Instance.isCharacter2Display = true;
                GameManager.Instance.currentCharacter2Img = data.character2ImageFileName;
                GameManager.Instance.currentCharacter2Position = data.coordinateX2;
            }
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName, characterImage2, data.coordinateX2);
        }
        //if (NotNullNorEmpty(data.character1ImageFileName))
        //{
        //    UpdateCharacterImage(data.character1Action, data.character1ImageFileName, characterImage1, data.coordinateX1);
        //}
        //if (NotNullNorEmpty(data.character2ImageFileName))
        //{
        //    UpdateCharacterImage(data.character2Action, data.character2ImageFileName, characterImage2, data.coordinateX2);
        //}
        currentLine++;
    }
    bool NotNullNorEmpty(string str) {
        return !string.IsNullOrEmpty(str);
    }
    void RecoverLastBackgroundAndCharacter()
    {
        var data = storyData[currentLine];
        if (NotNullNorEmpty(GameManager.Instance.currentBackgroundImg))
        {
            UpdateBackgroundImage(GameManager.Instance.currentBackgroundImg);
        }
        if (NotNullNorEmpty(GameManager.Instance.currentBackgroundMusic))
        {
            PlayBackgroundMusic(GameManager.Instance.currentBackgroundMusic);
        }
        if (GameManager.Instance.isCharacter1Display)
        {
            UpdateCharacterImage(Constants.APPEAR_AT, GameManager.Instance.currentCharacter1Img, characterImage1, GameManager.Instance.currentCharacter1Position);
        }
        if (GameManager.Instance.isCharacter2Display)
        {
            UpdateCharacterImage(Constants.APPEAR_AT, GameManager.Instance.currentCharacter2Img, characterImage2, GameManager.Instance.currentCharacter2Position);
        }
    }
    void RecordHistory(string speaker, string content)
    {
        //string historyRecord = speaker + Constants.COLON + content;
        //if (historyRecords.Count >= Constants.MAX_LENGTH)
        //{
        //    historyRecords.RemoveFirst();
        //}
        //historyRecords.AddLast(historyRecord);
    }

    #endregion
    #region Choices
    void ShowChoices()
    {
        var data = storyData[currentLine];
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        choicePanel.SetActive(true);
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.content;
        choiceButton1.onClick.AddListener(() => {
            InitializeAndLoadStory(data.avatarImageFileName, defaultStoryStartLine);
        });
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton2.onClick.AddListener(() => {
            InitializeAndLoadStory(data.backgroundImageFileName, defaultStoryStartLine);
        });
    }
    #endregion
    #region Image
    void UpdateAvatarImage(string imageFileName) {
        string imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath, avatarImage);
    }
    void UpdateBackgroundImage(string imageFileName) {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);
    }
    void UpdateCharacterImage(string action, string imageFileName, Image characterImage, string x)
    {
        if (action.StartsWith(Constants.APPEAR_AT))
        {
            string imagePath = Constants.CHARACTER_PATH + imageFileName;
            if (NotNullNorEmpty(x))
            {
                UpdateImage(imagePath, characterImage);
                var newPosition = new Vector2(float.Parse(x), characterImage.rectTransform.anchoredPosition.y);
                characterImage.rectTransform.anchoredPosition = newPosition;
                characterImage.DOFade(1, (isLoad ? 0 : Constants.DURATION_TIME)).From(0);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }

        }
        else if (action == Constants.DISAPPEAR)
        {
            characterImage.DOFade(0, Constants.DURATION_TIME).OnComplete(() => characterImage.gameObject.SetActive(false));
        }
        else if (action.StartsWith(Constants.MOVE_TO))
        {
            if (NotNullNorEmpty(x))
            {
                characterImage.rectTransform.DOAnchorPosX(float.Parse(x), Constants.DURATION_TIME);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
    }
    void UpdateImage(string imagePath, Image image)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FAILED + imagePath);
        }
    }
    void UpdateButtonImage(string imageFileName, Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath, button.image);
    }
    #endregion
    #region Audio
    void PlayBackgroundMusic(string audioFileName) {
        string audioPath = Constants.MUSIC_PATH + audioFileName;
        PlayAudio(audioPath, backgroundMusic, true);
    }
    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;
        PlayAudio(audioPath, vocalAudio, false);
    }
    void PlayAudio(string audioPath, AudioSource audioSource, bool isLoop)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            audioSource.loop = isLoop;
        }
        else
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + audioPath);
        }
    }
    #endregion
    #region Buttons
    #region Bottom
    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            BottomButtons.GetComponent<RectTransform>(),
            Input.mousePosition,
            Camera.main
        );
    }
    #endregion
    #region Auto
    private bool isAutoPlay = false;
    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoButton);
        if (isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }
    private IEnumerator StartAutoPlay()
    {
        while (isAutoPlay)
        {
            if (!typewritterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITING_SECONDS);
        }
    }
    #endregion
    #region Skip
    private bool isSkip = false;
    private int maxReachedLineIndex = 0;

    void OnSkipButtonClick()
    {
        if (!isSkip && CanSkip())
        {
            StartSkip();
        }
        else if (isSkip)
        {
            StopCoroutine(SkipToMaxReachedLine());
            EndSkip();
        }
    }
    bool CanSkip()
    {
        return currentLine < maxReachedLineIndex;
    }

    void StartSkip()
    {
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }
    
    void EndSkip() {
        isSkip = false;
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
        currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
    }

    void CtrlSkip()
    {
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipWhilePressingCtrl());
    }
    private IEnumerator SkipWhilePressingCtrl()
    {
        while (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            DisplayThisLine();
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS);
        }
    }
    private IEnumerator SkipToMaxReachedLine()
    {
        while (isSkip)
        {
            if (CanSkip())
            {
                DisplayThisLine();   
            }
            else
            {
                EndSkip();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS);
        }
    }
    #endregion
    #region Save
    void OnSaveButtonClick()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Save;
        SceneManager.LoadScene(Constants.SAVE_AND_LOAD_SCENE);
    }
    void SaveGame(int slotIndex)
    {
        var saveData = new SaveData
        {
            savedStoryFileName = currentStoryFileName,
            savedLine = currentLine,
            savedSpeakingContent = currentSpeakingConent,
            savedscreenshotData = screenshotData,
            //savedHistoryRecords = historyRecords,
            savedPlayerName = PlayerData.Instance.playerName
        };
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }
    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public string savedSpeakingContent;
        public byte[] savedscreenshotData;
        public LinkedList<string> savedHistoryRecords;
        public string savedPlayerName;
    }
    #endregion
    #region Load
    private bool isLoad = false;
    void OnLoadButtonClick()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Load;
        SceneManager.LoadScene(Constants.SAVE_AND_LOAD_SCENE);
    }
    void LoadGame(int slotIndex)
    { 
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        if (File.Exists(savePath))
        {
            isLoad = true;
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            //historyRecords = saveData.savedHistoryRecords;
            //historyRecords.RemoveLast();

            PlayerData.Instance.playerName = saveData.savedPlayerName;

            var lineNumber = saveData.savedLine - 1;
            InitializeAndLoadStory(saveData.savedStoryFileName, lineNumber);
        }
    }

    #endregion
    #region History
    void OnHistoryButtonClick()
    {
        SceneManager.LoadScene(Constants.HISTORY_SCENE);
    }
    #endregion
    #region Home
    void OnHomeButtonClick()
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }
    #endregion
    #region Close
    void OnCloseButtonClick()
    {
        CloseUI();
    }
    void OpenUI()
    {
        dialogueBox.SetActive(true);
        BottomButtons.SetActive(true);
    }
    void CloseUI()
    {
        dialogueBox.SetActive(false);
        BottomButtons.SetActive(false);
    }
    #endregion
    #region Setting
    void OnSettingButtonClick()
    {
        SceneManager.LoadScene(Constants.SETTING_SCENE);
    }
    #endregion
    #endregion
}