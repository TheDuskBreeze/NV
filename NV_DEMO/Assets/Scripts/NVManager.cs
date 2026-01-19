using DG.Tweening;
using ExcelDataReader.Log;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ExcelReader;

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

    public GameObject BottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button settingsButton;
    public Button homeButton;
    public Button closeButton;
    public Button historyButton;

    private string excelFileExtension = Constants.EXCEL_FILE_EXTENSION;

    private int currentLine;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
    private string currentStoryFileName;
    private List<ExcelReader.ExcelData> storyData;


    private string saveFolderPath;
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
        var gm = GameManager.Instance;
        gm.hasStarted = true;
        gm.currentScene = Constants.GAME_SCENE;
        if (gm.pendingData != null)
        {
            var savedData = gm.pendingData;
            gm.pendingData = null;

            gm.currentStoryFile = savedData.savedStoryFileName;
            savedData.savedLine--;
            gm.currentLineIndex = savedData.savedLine;

            savedData.savedHistoryRecords.RemoveLast();
            gm.historyRecords = savedData.savedHistoryRecords;
            gm.playerName = savedData.savedPlayerName;

            gm.currentBackgroundImg = savedData.savedBackgroundImg;
            gm.currentBackgroundMusic = savedData.savedBackgroundMusic;

            gm.currentCharacter1Img = savedData.savedCharacter1Img;
            gm.currentCharacter2Img = savedData.savedCharacter2Img;
            gm.currentCharacter1Position = savedData.savedCharacter1Position;
            gm.currentCharacter2Position = savedData.savedCharacter2Position;
            gm.isCharacter1Display = savedData.savedCharacter1Display;
            gm.isCharacter2Display = savedData.savedCharacter2Display;
        }
        currentLine = gm.currentLineIndex;
        ButtomButtonsAddListener();
        InitializeImage();
        LoadStory(GameManager.Instance.currentStoryFile, currentLine);
        DisplayNextLine();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogueBox.activeSelf)
            {
                OpenUI();
            }
            else if (!IsHittingBottomButtons() && !ChoiceManager.Instance.choicePanel.activeSelf)
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
    void LoadStory(string fileName, int startLine = 1)
    {
        LoadStoryFromFile(fileName);
        currentLine = startLine;
        RecoverLastBackgroundAndCharacter();
    }
    void InitializeImage()
    {
        backgroundImage.gameObject.SetActive(false);
        avatarImage.gameObject.SetActive(false);
        characterImage1.gameObject .SetActive(false);
        characterImage2.gameObject .SetActive(false);
    }
    void LoadStoryFromFile(string fileName) {
        currentStoryFileName = fileName;
        var filePath = Path.Combine(Application.streamingAssetsPath, Constants.STORY_PATH, fileName + excelFileExtension);
        storyData = ExcelReader.ReadExcel(filePath);
        Debug.Log($"文件名: {fileName}, 读取到的行数: {storyData.Count}");
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
            if (storyData[currentLine].speaker.Trim() == Constants.CHOICE)
            {
                ShowChoices();
                return;
            }
            if (storyData[currentLine].speaker.Trim() == Constants.GOTO)
            {
                LoadStory(storyData[currentLine].content, 1);
                currentLine = Constants.DEFAULT_START_LINE;
                DisplayNextLine();
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
        if (NotNullNorEmpty(data.character1ImageFileName))
        {
            GameManager.Instance.isCharacter1Display = true;
            GameManager.Instance.currentCharacter1Img = data.character1ImageFileName;

            string targetX = NotNullNorEmpty(data.coordinateX1) ? data.coordinateX1 : GameManager.Instance.currentCharacter1Position;
            GameManager.Instance.currentCharacter1Position = targetX;

            if (NotNullNorEmpty(data.character1Action))
            {
                if (data.character1Action == Constants.DISAPPEAR)
                {
                    GameManager.Instance.isCharacter1Display = false;
                    characterImage1.gameObject.SetActive(false);
                }
                else
                {
                    UpdateCharacterImage(data.character1Action, data.character1ImageFileName, characterImage1, targetX);
                }
            }
            else
            {
                UpdateCharacterImage(Constants.APPEAR_AT, data.character1ImageFileName, characterImage1, targetX);
            }
        }
        else
        {

            characterImage1.gameObject.SetActive(false);
            GameManager.Instance.isCharacter1Display = false;
        }
        if (NotNullNorEmpty(data.character2ImageFileName))
        {
            GameManager.Instance.isCharacter2Display = true;
            GameManager.Instance.currentCharacter2Img = data.character2ImageFileName;

            string targetX = NotNullNorEmpty(data.coordinateX2) ? data.coordinateX2 : GameManager.Instance.currentCharacter2Position;
            GameManager.Instance.currentCharacter2Position = targetX;

            if (NotNullNorEmpty(data.character2Action))
            {
                if (data.character2Action == Constants.DISAPPEAR)
                {
                    GameManager.Instance.isCharacter2Display = false;
                    characterImage2.gameObject.SetActive(false);
                }
                else
                {
                    UpdateCharacterImage(data.character2Action, data.character2ImageFileName, characterImage2, targetX);
                }
            }
            else
            {
                UpdateCharacterImage(Constants.APPEAR_AT, data.character2ImageFileName, characterImage2, targetX);
            }
        }
        else
        {
            characterImage2.gameObject.SetActive(false);
            GameManager.Instance.isCharacter2Display = false;
        }
        currentLine++;
    }
    bool NotNullNorEmpty(string str) {
        return !string.IsNullOrEmpty(str);
    }
    void RecoverLastBackgroundAndCharacter()
    {
        if (currentLine >= 0 && currentLine < storyData.Count)
        {
            var data = storyData[currentLine];
            // 执行显示逻辑
        }
        else
        {
            Debug.LogWarning($"对话已结束或索引越界！当前行：{currentLine}, 总行数：{storyData.Count}");
            // 这里可以处理：跳转到下一关、回到主菜单或关闭对话框
        }
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
    #endregion
    #region Choices
    void ShowChoices()
    {
        var data = storyData[currentLine];
        var choices = data.content
                      .Split(Constants.ChoiceDelimiter)
                      .Select(s => s.Trim())
                      .ToList();
        var actions = data.avatarImageFileName
                      .Split(Constants.ChoiceDelimiter)
                      .Select(s => s.Trim())
                      .ToList();
        ChoiceManager.Instance.ShowChoices(choices, actions, HandleChoice);
    }
    void HandleChoice(string selectedChoice)
    {
        currentLine = Constants.DEFAULT_START_LINE;
        LoadStory(selectedChoice, 1);
        DisplayNextLine();
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
            DisplayNextLine();
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
        SaveData();
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Save;
        SceneManager.LoadScene(Constants.SAVE_AND_LOAD_SCENE);
    }
    void SaveData()
    {
        CloseUI();
        Texture2D screenshot = screenShotter.CaptureScreenshot();
        OpenUI();

        var gm = GameManager.Instance;
        gm.pendingData = new GameManager.SaveData
        {
            savedStoryFileName = currentStoryFileName,
            savedLine = currentLine,
            savedScreenshotData = screenshot.EncodeToPNG(),
            savedHistoryRecords = gm.historyRecords,
            savedPlayerName = gm.playerName,
            savedBackgroundImg = gm.currentBackgroundImg,
            savedBackgroundMusic = gm.currentBackgroundMusic,
            savedCharacter1Img = gm.currentCharacter1Img,
            savedCharacter2Img = gm.currentCharacter2Img,
            savedCharacter1Position = gm.currentCharacter1Position,
            savedCharacter2Position = gm.currentCharacter2Position,
            savedCharacter1Display = gm.isCharacter1Display,
            savedCharacter2Display = gm.isCharacter2Display
        };

    }
   
    #endregion
    #region Load
    private bool isLoad = false;
    void OnLoadButtonClick()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Load;
        SceneManager.LoadScene(Constants.SAVE_AND_LOAD_SCENE);
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