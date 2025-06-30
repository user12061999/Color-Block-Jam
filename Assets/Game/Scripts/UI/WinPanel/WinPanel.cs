using DG.Tweening;
using HAVIGAME.Scenes;
using HAVIGAME.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : UIFrame
{

    [Header("[References]")]
    [SerializeField] private Button btnClaim;
    [SerializeField] private ItemView rewardView;

    [SerializeField] private ItemView rewardWithAdsView;
    [SerializeField] private ItemView starView;
    [SerializeField] private ItemView starView2;
    
    [SerializeField] private GameObject normalGO;
    [SerializeField] private GameObject leaderboardGO;

    [SerializeField] private ItemView rewardViewPrefab;
    [SerializeField] private Transform rewardViewContainer;
    [SerializeField] private ProgressBarInt winChestProgressBar;
    [SerializeField] private Transform winChest;
    [SerializeField] private ProgressBarInt winChestProgressBar2;
    [SerializeField] private Transform winChest2;

    private Sequence sequence;

    private ItemStack starReward;
    private Tween audioTween;
    private CollectionView<ItemView, ItemStack> views;

    private void Awake()
    {
        //views = new CollectionView<ItemView, ItemStack>(rewardViewPrefab, rewardViewContainer);
    }

    private void Start()
    {
        btnClaim.onClick.AddListener(OnButtonClaimClicked);
      
    }

    protected override void OnShow(bool instant = false)
    {
        base.OnShow(instant);

       
        audioTween?.Kill();
      
      


     

        sequence?.Kill();
        sequence = null;

     
    }

    protected override void OnHide(bool instant = false) {
        base.OnHide(instant);

        sequence?.Kill();
        sequence = null;
        audioTween?.Kill();
    }

    protected override void OnBack()
    {

    }

    public void SetStarRewards(ItemStack reward)
    {
        this.starReward = reward;

        rewardView.SetModel(reward).Show();

        starView.SetModel(reward).Show();
        starView2.SetModel(reward).Show();
        
    }

    public void SetRewards(ItemStack[] rewards)
    {
        views.SetModels(rewards).Show();
    }

    private void OnButtonClaimClicked()
    {
        GameAdvertising.TryShowInterstitialAd();
        

        if (GameController.Instance.DestroyGame())
        {
            if (/*GameData.Classic.LevelUnlocked == 2 ||*/ GameData.Classic.LevelUnlocked == 3) {
                ClassicLevelController levelController = GameController.Instance.LevelController as ClassicLevelController;
                GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(GameData.Classic.LevelUnlocked);
            
                ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
            } else {
                ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Home, () =>
                {
                    UIManager.Instance.Clear(true);

                });
            }
          
        }
    }
}
