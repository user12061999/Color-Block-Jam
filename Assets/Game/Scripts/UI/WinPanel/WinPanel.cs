using DG.Tweening;
using HAVIGAME.Scenes;
using HAVIGAME.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : UIFrame
{
    [Header("[References]")] [SerializeField]
    private Button btnClaim,btnClaimAds, btnHome;

    [SerializeField] private ItemView rewardView;

    [SerializeField] private ItemView rewardWithAdsView;

    [SerializeField] private ItemView rewardViewPrefab;
    [SerializeField] private Transform rewardViewContainer;

    private Sequence sequence;

    private ItemStack starReward;
    private Tween audioTween;
    private CollectionView<ItemView, ItemStack> views;

    private void Awake()
    {
        views = new CollectionView<ItemView, ItemStack>(rewardViewPrefab, rewardViewContainer);
    }

    private void Start()
    {
        btnClaim.onClick.AddListener(OnButtonClaimClicked);
        btnClaimAds.onClick.AddListener(OnButtonClaimAdsClicked);
        btnHome.onClick.AddListener(OnClickButtonHome);
    }

    protected override void OnShow(bool instant = false)
    {
        base.OnShow(instant);

        audioTween?.Kill();
        sequence?.Kill();
        sequence = null;
    }

    protected override void OnHide(bool instant = false)
    {
        base.OnHide(instant);

        sequence?.Kill();
        sequence = null;
        audioTween?.Kill();
    }

    protected override void OnBack()
    {
    }

    public void SetRewards(ItemStack[] rewards)
    {
        views.SetModels(rewards).Show();
    }

    private void OnButtonClaimClicked()
    {
        GameData.Inventory.Add(new ItemStack(ItemID.Coin,40));
        GameAdvertising.TryShowInterstitialAd();
        if (GameController.Instance.DestroyGame())
        {
            ClassicLevelController levelController =
                GameController.Instance.LevelController as ClassicLevelController;
            GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(GameData.Classic.LevelUnlocked);

            ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
        }
    }
    private void OnButtonClaimAdsClicked()
    {
        GameAdvertising.TryShowRewardedAd(() =>
        {
            GameData.Inventory.Add(new ItemStack(ItemID.Coin,80));
            ClassicLevelController levelController =
                GameController.Instance.LevelController as ClassicLevelController;
            GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(GameData.Classic.LevelUnlocked);

            ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
        }, () =>
        {
           
        });
    }
    public void OnClickButtonHome()
    {
        GameController.Instance.DestroyGame();
        ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Home);
    }
}