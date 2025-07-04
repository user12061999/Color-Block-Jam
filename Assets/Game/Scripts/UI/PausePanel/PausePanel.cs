using HAVIGAME.Scenes;
using HAVIGAME.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : UIFrame
{
    [Header("[References]")]
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnSkip;
    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnRetry;
    [SerializeField] private Button btnBack;

    [SerializeField] private GameObject panelReplay;
    [SerializeField] private TextMeshProUGUI textLevel;
    int index = 0; // 0: Home, 1: Retry

    private void Start()
    {
        OnStart();
        btnHome.onClick.AddListener(QuitToHome);
        btnSkip.onClick.AddListener(SkipGame);
        btnPause.onClick.AddListener(BtnPause);
        btnRetry.onClick.AddListener(RestartGame);
        btnBack.onClick.AddListener(ButtonBack);
    }
    void OnStart()
    {
        panelReplay.SetActive(false);
        SetTextLevel();
    }
    void SetTextLevel()
    {
        int level = GameController.Instance.LoadLevelOption.Level;
        textLevel.text = string.Format("Level {0}", level);
    }
    private void QuitToHome()
    {
        // GameController.Instance.DestroyGame();
        // ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Home);
        index = 0;
        panelReplay.SetActive(true);
    }

    private void SkipGame()
    {
        GameController.Instance.SkipGame();
        GameController.Instance.DestroyGame();

        int levelToPlay = GameData.Classic.LevelUnlocked;
        GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(levelToPlay);
        ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
    }
    private void ButtonBack()
    {
        panelReplay.SetActive(false);
    }

    private void RestartGame()
    {
        bool isEnought = GameData.Inventory.IsEnought(new ItemStack(ItemID.Heart, 1));
        if (!isEnought)
        {

            if (UIManager.HasInstance)
            {
                string title;
                string message;
                if (index == 0)
                {
                    //HOME
                    title = "Home!";
                    message = "You don't have enough hearts to continue. Do you want to go home?";
                }
                else
                {
                    //RETRY
                    title = "Retry!";
                    message = "You don't have enough hearts to retry. Do you want to retry?";
                }

                UIManager.Instance.Push<DialogPanel>().Dialog(title, message);
            }
        }
        else
        {
            if (index == 0)
            {
                //HOME
                GameController.Instance.DestroyGame();
                ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Home);
            }
            else
            {
                //RETRY
                GameController.Instance.DestroyGame();
                int levelToPlay = GameData.Classic.LevelUnlocked;
                GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(levelToPlay);
                ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
            }
        }





    }
    private void BtnPause()
    {
        index = 1; // Set index to 1 for Retry
        panelReplay.SetActive(true);
    }
}
