using HAVIGAME.Scenes;
using HAVIGAME.UI;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : UIFrame {
    [Header("[References]")]
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnSkip;

    private void Start() {
        btnHome.onClick.AddListener(QuitToHome);
        btnSkip.onClick.AddListener(SkipGame);
    }

    private void QuitToHome() {
        GameController.Instance.DestroyGame();
        ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Home);
    }

    private void SkipGame() {
        GameController.Instance.SkipGame();
        GameController.Instance.DestroyGame();

        int levelToPlay = GameData.Classic.LevelUnlocked;
        GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(levelToPlay);
        ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
    }
}
