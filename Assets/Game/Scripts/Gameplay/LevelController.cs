using HAVIGAME.UI;
using UnityEngine;

public class LevelController : MonoBehaviour {

    public virtual void Initilaze() {

    }

    public virtual void StartLevel() {
        UIManager.Instance.Push<GamePanel>();
    }

    public virtual void WinLevel(bool isWinBySkip = false) {
        GameData.Classic.OnLevelCompleted(GameController.Instance.LoadLevelOption.Level);
    }

    public virtual void LoseLevel() {

    }

    public virtual void DestroyLevel() {

    }
}