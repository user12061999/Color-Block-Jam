using DG.Tweening;
using HAVIGAME;
using HAVIGAME.Scenes;
using HAVIGAME.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LosePanel : GameUIFrame
{
    [Header("[References]")]
    [SerializeField] private Button btnClose;
    [SerializeField] private ItemView rewardViewPrefab;
    [SerializeField] private Transform rewardViewContainer;
    [SerializeField] private ItemView priceView;
    [SerializeField] private Button btnReviveAds, btnReviveGold;

    private int itemId;
    private Action<bool> callback;
    private Tween audioTween;
    private CollectionView<ItemView, ItemStack> views;

    private void Awake()
    {
        views = new CollectionView<ItemView, ItemStack>(rewardViewPrefab, rewardViewContainer);
    }

    private void Start()
    {
        btnReviveAds.onClick.AddListener(OnClickButtonAds);
        btnReviveGold.onClick.AddListener(OnClickButtonGold);
        btnClose.onClick.AddListener(OnClickButtonHome);
    }

    protected override void OnShow(bool instant = false)
    {
        base.OnShow(instant);

        //GameAnalytics.LogEvent(GameAnalytics.GameEvent.Create("iap_show").Add("position", "ingame"));
    }
    protected override void OnShowCompleted()
    {
        base.OnShowCompleted();
        ShowPack();
    }

    private void ShowPack()
    {

    }

    protected override void OnHide(bool instant = false)
    {
        base.OnHide(instant);

        audioTween?.Kill();
    }

    protected override void OnBack()
    {

    }

    public void SetLostRewards(ItemStack[] rewards)
    {
        views.SetModels(rewards).Show();
    }

    public void SetItem(int itemId, Action<bool> callback)
    {
        this.itemId = itemId;
        this.callback = callback;

        ItemData data = ItemDatabase.Instance.GetDataById(itemId);

        priceView.SetModel(data.Price).Show();
    }

    private void OnPurchase()
    {
        /*ItemData data = ItemDatabase.Instance.GetDataById(itemId);

        bool isEnought = GameData.Inventory.IsEnought(data.Price);

        if (isEnought) {
            GameData.Inventory.Remove(data.Price, "trade");
            GameData.Inventory.Add(new ItemStack(itemId, 1), "trade");
            Hide();
            callback?.Invoke(true);
        } else {
            ItemData priceData = ItemDatabase.Instance.GetDataById(data.Price.Id);
            NotifyPopupManager.Instance.PushNotify(Utility.Text.Format("You don't have enough {0}.", priceData.Name));
        }*/
    }

    public void OnRevive(int time = 20)
    {
        ClassicLevelController levelController = GameController.Instance.LevelController as ClassicLevelController;
        if (levelController != null) levelController.AddMoreSeconds(time);
    }
    private void OnAdsPurchase()
    {
        //GameData.Inventory.Add(new ItemStack(itemId, 1), "rewarded_ad");
        Hide();
        callback?.Invoke(true);
    }

    public void OnClickButtonGold()
    {
        ItemData data = ItemDatabase.Instance.GetDataById(itemId);
        bool isEnought = GameData.Inventory.IsEnought(data.Price);
        if (isEnought)
        {
            GameData.Inventory.Remove(data.Price);
            OnRevive();
            Hide();
            callback?.Invoke(true);
        }
    }
    public void OnClickButtonAds()
    {
        GameAdvertising.TryShowRewardedAd(() =>
        {
            OnRevive();
            Hide();
            callback?.Invoke(true);
        });
    }
    public void OnClickButtonHome()
    {
        GameController.Instance.DestroyGame();
        ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Home);
    }

    private void OnCloseButtonClicked()
    {
        Hide();
        callback?.Invoke(false);
    }
}
