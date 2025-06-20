using HAVIGAME.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePanel : UIFrame {
    [Header("[References]")]
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Button btnPause;

    private void Start() {
        btnPause.onClick.AddListener(PauseGame);
    }

    protected override void OnShow(bool instant = false) {
        base.OnShow(instant);

        txtLevel.text = string.Format("LEVEL - {0}", GameController.Instance.LoadLevelOption.Level);
    }

    protected override void OnBack() {
        PauseGame();
    }

    private void PauseGame() {
        UIManager.Instance.Push<PausePanel>();
    }
}
