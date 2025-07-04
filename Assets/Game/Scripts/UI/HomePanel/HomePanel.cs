using HAVIGAME.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HAVIGAME.Scenes;

public class HomePanel : UITab
{
    [Header("[References]")]
    [SerializeField] private Button btnProfile;
    [SerializeField] private Button btnSetting;
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnDailyReward;
    [SerializeField] private Button btnLuckySpin;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Button btnPreviousLevel;
    [SerializeField] private Button btnNextLevel;

    [SerializeField] private Button btnAdvertising;

    //------------------------------------------
    private int currentLevel;
    [SerializeField] private GameObject[] arrayButtons;
    [SerializeField] private Sprite[] arraySpriteMap;
    [SerializeField] private Sprite[] arraySpriteButton;

    private void Start()
    {
        btnPlay.onClick.AddListener(PlayGame);
        btnPreviousLevel.onClick.AddListener(PreviousLevel);
        btnNextLevel.onClick.AddListener(NextLevel);

        btnProfile.onClick.AddListener(OpenProfile);
        btnSetting.onClick.AddListener(OpenSettings);

        btnDailyReward.onClick.AddListener(OpenDailyReward);
        btnLuckySpin.onClick.AddListener(OpenLuckySpin);

        btnAdvertising.onClick.AddListener(OpenAdvertising);


    }
    void OnEnable()
    {
        SetForButtonUI();
    }
    public void SetForButtonUI()
    {
        SetTextLevel();
        CheckDifficulty();
    }
    private void SetTextLevel()
    {
        currentLevel = GameData.Classic.LevelUnlocked;
        for (int i = 0; i < arrayButtons.Length; i++)
        {
            arrayButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0}", currentLevel + i);
        }
        btnPlay.GetComponentInChildren<TextMeshProUGUI>().text = "LEVEL " + currentLevel;
    }
    private void CheckDifficulty()
    {
        //BUTTONPLAY
        string paths = $"LevelSO/Level_{currentLevel}";
        ConfigLevel difficultyDatas = Resources.Load<ConfigLevel>(paths);
        btnPlay.gameObject.GetComponent<Image>().sprite = arraySpriteButton[1];

        if (difficultyDatas != null)
        {
            if (difficultyDatas.difficulty == DifficultyLevel.Hard)
            {
                btnPlay.GetComponent<Image>().sprite = arraySpriteButton[1];
            }
            else
            {
                btnPlay.GetComponent<Image>().sprite = arraySpriteButton[0];
            }
        }

        //BUTTON UI
        for (int i = 0; i < arrayButtons.Length; i++)
        {
            string path = $"LevelSO/Level_{currentLevel + i}";
            ConfigLevel difficultyData = Resources.Load<ConfigLevel>(path);
            if (difficultyData != null)
            {
                GameObject hardIndicator = arrayButtons[i].transform.GetChild(1).gameObject;
                if (hardIndicator == null)
                {
                    Debug.LogError("Hard Indicator not found in button at index: " + i);
                    continue; // Skip if hardIndicator is not found
                }


                if (difficultyData.difficulty == DifficultyLevel.Hard)
                {
                    //print(currentLevel + "HARD");
                    hardIndicator.SetActive(true);

                    if (i == 0)
                    {
                        arrayButtons[i].GetComponent<Image>().sprite = arraySpriteMap[1];
                    }
                }
                else
                {
                    // print(currentLevel + "EASY");
                    hardIndicator.SetActive(false);
                    if (i == 0)
                    {
                        arrayButtons[i].GetComponent<Image>().sprite = arraySpriteMap[0];
                    }
                }
            }

        }
    }

    protected override void OnShow(bool instant = false)
    {
        base.OnShow(instant);

        int levelUnlocked = GameData.Classic.LevelUnlocked;
        ShowLevel(levelUnlocked);

    }

    protected override void OnBack()
    {
        OpenSettings();
    }

    private void ShowLevel(int level)
    {
        currentLevel = level;
        txtLevel.text = string.Format("LEVEL {0}", level);

        btnPreviousLevel.gameObject.SetActive(currentLevel > 1);
        btnNextLevel.gameObject.SetActive(currentLevel < GameData.Classic.LevelUnlocked);
    }

    private void PreviousLevel()
    {
        ShowLevel(currentLevel - 1);
    }

    private void NextLevel()
    {
        ShowLevel(currentLevel + 1);
    }

    private void PlayGame()
    {
        bool isEnought = GameData.Inventory.IsEnought(new ItemStack(ItemID.Heart, 1));
        if (!isEnought)
        {
            if (UIManager.HasInstance)
            {
                string title = "Hearts!";
                string message = "You don't have enough hearts to play. Do you want to go to the shop?";
                UIManager.Instance.Push<DialogPanel>().Dialog(title, message);
            }
        }
        else
        {
            int levelToPlay = currentLevel;
            Debug.Log(levelToPlay);
            GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(levelToPlay);
            ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
        }


    }

    private void OpenSettings()
    {
        UIManager.Instance.Push<SettingsPanel>();
    }

    private void OpenProfile()
    {
        UIManager.Instance.Push<ProfilePanel>();
    }

    private void OpenDailyReward()
    {
        UIManager.Instance.Push<DailyRewardPanel>();
    }

    private void OpenLuckySpin()
    {
        UIManager.Instance.Push<LuckySpinPanel>();
    }

    private void OpenAdvertising()
    {
        UIManager.Instance.Push<TestPanel>();
    }
}
