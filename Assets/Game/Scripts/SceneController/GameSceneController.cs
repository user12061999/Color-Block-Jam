using System.Collections.Generic;

public class GameSceneController : SceneController {
    public static LoadLevelOption pendingLoadLevelOption;

    protected override void OnSceneStart() {
        pendingLoadLevelOption= LoadLevelOption.Create(GameData.Classic.LevelUnlocked);
        GameController.Instance.StartGame(pendingLoadLevelOption);
    }
}
