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

public class NV_Manager : MonoBehaviour {
    #region Variables
    public GameObject gamePanel;
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

    private int currentLine;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
    private string currentStoryFileName;

    private int defaultStoryStartLine = Constants.DEFAULT_START_LINE;
    private string storyPath = Constants.STORY_PATH;
    private string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private string excelFileExtension = Constants.EXCEL_FILE_EXTENSION;

    private string saveFolderPath;
    private byte[] screenshotData;
    private string currentSpeakingConent;

    private List<ExcelReader.ExcelData> storyData;
    private Dictionary<string, int> globalMaxReachedLineIndices = new Dictionary<string, int>();    //全局存储每个文件的最远行索引
    private LinkedList<string> historyRecords = new LinkedList<string>();    //保存历史记录
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
        InitializeSaveFilePath();
        ButtomButtonsAddListener();
    }
    void Update()
    {
        if (!MenuManger.Instance.menuPanel.activeSelf && 
            !SaveAndLoadManager.Instance.saveAndLoadPanel.activeSelf && 
            !HistoryManager.Instance.historyScrollView.activeSelf &&
            !SettingManager.Instance.settingPanel.activeSelf &&
            gamePanel.activeSelf)
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
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) 
            {
                Debug.Log("按下Ctrl键");
                CtrlSkip();
            }
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
        if (isLoad)
        {
            RecoverLastBackgroundAndCharacter();
            isLoad = false;
        }
        DisplayNextLine();
    }
    public void StartGame() {
        InitializeAndLoadStory(defaultStoryFileName, defaultStoryStartLine);
    }

    void LoadStoryFromFile(string fileName) {
        var path = storyPath + fileName + excelFileExtension;
        storyData = ExcelReader.ReadExcel(path);
        if (storyData == null || storyData.Count == 0)
        {
            Debug.LogError(Constants.NO_DATA_FOUND);
        }
        currentStoryFileName = fileName;
        if (globalMaxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex = globalMaxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }
    #endregion
    #region Display
    void DisplayNextLine() {
        if (currentLine > maxReachedLineIndex)  //更新最远到达的行数
        {
            maxReachedLineIndex = currentLine;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
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
                Debug.Log(Constants.END_OF_STORY);
                return;
            }
            if (storyData[currentLine].speaker == Constants.CHOICE)
            {
                ShowChoices();
                return;
            }
            if (storyData[currentLine].speaker == Constants.GOTO)
            {
                InitializeAndLoadStory(storyData[currentLine].content, defaultStoryStartLine);
            }
        }
        if (typewritterEffect.IsTyping()) {
            typewritterEffect.CompleteTyping();
        }
        else {
            DisplayThisLine();
        }
    }

    void DisplayThisLine() {
        var data = storyData[currentLine];
        string playerName = PlayerData.Instance.playerName;
        string speaker = data.speaker.Replace(Constants.NAME_PLACEHOLDER, playerName);
        string content = data.content.Replace(Constants.NAME_PLACEHOLDER, playerName);
        speakerName.text = speaker;
        currentSpeakingConent = content;

        //speakerName.text = data.speaker;
        //currentSpeakingConent = data.content;
        typewritterEffect.StartTyping(currentSpeakingConent, currentTypingSpeed);

        RecordHistory(speakerName.text, currentSpeakingConent);

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
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            PlayBackgroundMusic(data.backgroundMusicFileName);
        }
        if (NotNullNorEmpty(data.character1ImageFileName))
        {
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName, characterImage1, data.coordinateX1);
        }
        if (NotNullNorEmpty(data.character2ImageFileName))
        {
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName, characterImage2, data.coordinateX2);
        }
        currentLine++;
    }
    bool NotNullNorEmpty(string str) {
        return !string.IsNullOrEmpty(str);
    }

    void RecoverLastBackgroundAndCharacter()
    {
        var data = storyData[currentLine];
        if (NotNullNorEmpty(data.lastBackgroundImage))
        {
            UpdateBackgroundImage(data.lastBackgroundImage);
        }
        if (NotNullNorEmpty(data.lastBackgroundMusic))
        {
            PlayBackgroundMusic(data.lastBackgroundMusic);
        }
        if (data.character1Action != Constants.APPEAR_AT && NotNullNorEmpty(data.character1ImageFileName))
        {
            UpdateCharacterImage(Constants.APPEAR_AT, data.character1ImageFileName, characterImage1, data.lastCoordinateX1);
        }
        if (data.character2Action != Constants.APPEAR_AT && NotNullNorEmpty(data.character2ImageFileName))
        {
            UpdateCharacterImage(Constants.APPEAR_AT, data.character2ImageFileName, characterImage2, data.lastCoordinateX2);
        }
    }

    void RecordHistory(string speaker, string content)
    {
        string historyRecord = speaker + Constants.COLON + content;
        if (historyRecords.Count >= Constants.MAX_LENGTH)
        {
            historyRecords.RemoveFirst();
        }
        historyRecords.AddLast(historyRecord);
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
        CloseUI();
        Texture2D texture2D = screenShotter.CaptureScreenshot();
        screenshotData = texture2D.EncodeToPNG();
        SaveAndLoadManager.Instance.ShowSavePanel(SaveGame);
        OpenUI();
    }

    void SaveGame(int slotIndex)
    {
        var saveData = new SaveData
        {
            savedStoryFileName = currentStoryFileName,
            savedLine = currentLine,
            savedSpeakingContent = currentSpeakingConent,
            savedscreenshotData = screenshotData,
            savedHistoryRecords = historyRecords,
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
        ShowLoadPanel(null);
    }
    public void ShowLoadPanel(Action action)
    {
        SaveAndLoadManager.Instance.ShowLoadPanel(LoadGame, action);
    }
    void LoadGame(int slotIndex)
    { 
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        if (File.Exists(savePath))
        {
            isLoad = true;
            string json = File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            historyRecords = saveData.savedHistoryRecords;
            historyRecords.RemoveLast();

            PlayerData.Instance.playerName = saveData.savedPlayerName;

            var lineNumber = saveData.savedLine - 1;
            InitializeAndLoadStory(saveData.savedStoryFileName, lineNumber);
        }
    }

    #endregion
    #region History
    void OnHistoryButtonClick()
    {
        HistoryManager.Instance.ShowHistory(historyRecords);
    }
    #endregion
    #region Home
    void OnHomeButtonClick()
    {
        gamePanel.SetActive(false);
        MenuManger.Instance.menuPanel.SetActive(true);
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
       SettingManager.Instance.ShowSettingPanel();
    }
    #endregion
    #endregion
}