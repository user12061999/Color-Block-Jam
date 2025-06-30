using HAVIGAME.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HAVIGAME.Scenes;

public class HomePanel : UITab {
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

    private int currentLevel;

    private void Start() {
        btnPlay.onClick.AddListener(PlayGame);
        btnPreviousLevel.onClick.AddListener(PreviousLevel);
        btnNextLevel.onClick.AddListener(NextLevel);

        btnProfile.onClick.AddListener(OpenProfile);
        btnSetting.onClick.AddListener(OpenSettings);

        btnDailyReward.onClick.AddListener(OpenDailyReward);
        btnLuckySpin.onClick.AddListener(OpenLuckySpin);

        btnAdvertising.onClick.AddListener(OpenAdvertising);
    }

    protected override void OnShow(bool instant = false) {
        base.OnShow(instant);

        int levelUnlocked = GameData.Classic.LevelUnlocked;
        ShowLevel(levelUnlocked);

    }

    protected override void OnBack() {
        OpenSettings();
    }

    private void ShowLevel(int level) {
        currentLevel = level;
        txtLevel.text = string.Format("LEVEL {0}", level);

        btnPreviousLevel.gameObject.SetActive(currentLevel > 1);
        btnNextLevel.gameObject.SetActive(currentLevel < GameData.Classic.LevelUnlocked);
    }

    private void PreviousLevel() {
        ShowLevel(currentLevel - 1);
    }

    private void NextLevel() {
        ShowLevel(currentLevel + 1);
    }

    private void PlayGame() {
        int levelToPlay = currentLevel;
        Debug.Log(levelToPlay);
        GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(levelToPlay);
        ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
    }

    private void OpenSettings() {
        UIManager.Instance.Push<SettingsPanel>();
    }

    private void OpenProfile() {
        UIManager.Instance.Push<ProfilePanel>();
    }

    private void OpenDailyReward() {
        UIManager.Instance.Push<DailyRewardPanel>();
    }

    private void OpenLuckySpin() {
        UIManager.Instance.Push<LuckySpinPanel>();
    }

    private void OpenAdvertising() {
        UIManager.Instance.Push<TestPanel>();
    }
}
