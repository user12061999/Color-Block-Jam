using DG.Tweening;
using HAVIGAME;
using HAVIGAME.Scenes;
using HAVIGAME.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicLevelController : LevelController
{

    [SerializeField] protected LevelTimer timer;
    [SerializeField] protected LevelGenerator generator;
    [SerializeField] protected GridManager gridManager;

    protected ParticleSystem highlightVFX;
    protected GamePanel gamePanel;
    private int boosterUsed;
    protected bool isUsingBooster;
    protected bool isResolving;
    protected float startedTime;
    protected int totalMove;
    protected bool isWon = false;
    protected Dictionary<int, int> dictBooster;
    public LevelTimer Timer => timer;
    public bool IsResolving => isResolving;
    public bool IsUsingBooster => isUsingBooster;
    public bool IsCountdownPaused => timer.IsPaused;
    public int BoosterUsed => boosterUsed;

    public int TotalMove
    {
        get { return totalMove; }
        set { totalMove = value; }
    }

    public void OnBlockShapeChange(GameEvent.DestroyBlockShape e)
    {
        Debug.Log("OnBlockShapeChange");
        CheckWinLose();
    }

    private void Start()
    {
        EventDispatcher.AddListener<GameEvent.DestroyBlockShape>(OnBlockShapeChange);
    }

    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<GameEvent.DestroyBlockShape>(OnBlockShapeChange);

    }

    #region OVERRIDE

    public override void Initilaze(LoadLevelOption option, LevelController parent)
    {
        base.Initilaze(option, parent);

        this.isResolving = false;

        dictBooster = new Dictionary<int, int>();
        
        //CameraController.Instance.UpdateCamera(generator.Bounds);
        CheckHideTutorial();
    }

    public override void StartLevel()
    {
        base.StartLevel();
    }

    protected override void OnStartLevel()
    {
        base.OnStartLevel();
        startedTime = Time.time;
        TotalMove = 0;


        gamePanel = UIManager.Instance.Push<GamePanel>();
        //gamePanel.SetCountdownTime(generator.Duration);
        gamePanel.Interactable = true;

        int seconds = generator.Duration;
        

        StartCountdown(seconds);
        PauseCountdown(false);
    }

    /*public void SetInputSource(PlayerInputHandler playerInputHandler)
    {
        playerInputHandler.SetListener(this);
        playerInputHandler.active = true;
    }*/

    [ContextMenu("Pause Countdown")]

    private void HandleLevelStart()
    {
        
    }

    public override void WinLevel(bool isWinBySkip = false)
    {
        base.WinLevel();
    }

    protected override void OnWinLevel(bool isWinBySkip = false)
    {
        base.OnWinLevel(isWinBySkip);
        
        isWon = true;
        timer.Stop();
        gamePanel.Interactable = false;
        Time.timeScale = 1;

        DOVirtual.DelayedCall(1.5f, () =>
        {
            Debug.Log("OnWinLevel");
            gamePanel.Interactable = true;
            GameData.Classic.OnLevelCompleted(GameController.Instance.LoadLevelOption.Level);
            WinPanel winPanel = UIManager.Instance.Push<WinPanel>();
            
            /*if (GameData.Classic.LevelUnlocked == 2)
            {
                GameSceneController.pendingLoadLevelOption = LoadLevelOption.Create(GameData.Classic.LevelUnlocked);
                ScenesManager.Instance.LoadSceneAsyn(GameScene.ByIndex.Game);
            }
            else
            {
                WinPanel winPanel = UIManager.Instance.Push<WinPanel>();
                //winPanel.SetStarRewards(new ItemStack(ItemID.Star, totalStarEarned));
                //winPanel.SetRewards(new ItemStack[] { /*new ItemStack(ItemID.BuildTicket, 1),#1# new ItemStack(ItemID.Credit, 200) });
            }*/

        });
    }

    public override void LoseLevel()
    {
        base.LoseLevel();
    }

    protected override void OnLoseLevel()
    {
        base.OnLoseLevel();

        timer.Pause();

        gamePanel.Interactable = false;
        CheckHideTutorial();
        StopShowInterstitialAd();
        Time.timeScale = 1;


    }

    public int GetBoosterUsed(int boosterId)
    {
        if (!dictBooster.ContainsKey(boosterId))
            return 0;
        return dictBooster[boosterId];
    }

    public override void DestroyLevel()
    {
        base.DestroyLevel();

    }

    protected override void OnDestroyLevel()
    {
        base.OnDestroyLevel();

        timer.Stop();

        if (highlightVFX != null)
        {
            highlightVFX.Recycle();
            highlightVFX = null;
        }

        CheckHideTutorial();
        StopShowInterstitialAd();

        Time.timeScale = 1;

    }

    #endregion

    #region TUTORIAL CHECK

    private void CheckShowTutorial(float delay = 1)
    {

    }

    private void CheckHideTutorial()
    {

    }

    #endregion

    #region INPUT

    public void OnPointerDown(Vector3 screenPoint)
    {
        MouseDown(screenPoint, InputSource.Player);
    }

    public void OnPointerMove(Vector3 screenPoint)
    {
        MouseMove(screenPoint, InputSource.Player);
    }

    public void OnPointerUp(Vector3 screenPoint)
    {
        MouseUp(screenPoint, InputSource.Player);
    }

    public void OnPointerClick(Vector3 screenPoint)
    {

    }

    public void MouseDown(Vector3 screenPoint, InputSource inputSource)
    {
        switch (inputSource)
        {
            case InputSource.Player:
                CheckHideTutorial();
                break;
            case InputSource.Tutorial:
                break;
            default:
                break;
        }


        if (IsUsingBooster) return;


    }

    public void MouseMove(Vector3 screenPoint, InputSource inputSource)
    {

    }

    public void MouseUp(Vector3 screenPoint, InputSource inputSource)
    {

    }

    #endregion

    #region CHEAT

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            WinLevel();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            LoseLevel();
        }
    }
#endif

    #endregion

    private void StartCountdown(int seconds)
    {

        timer.Countdown(seconds, LoseLevel);
    }



    protected virtual void ResloveElements(Cell cell, Action onCompleteAnimReslove = null)
    {

    }



    protected virtual void CheckWinLose()
    {
        int remainingBlocks = gridManager.BlockParent.childCount; // Assuming blocks are children of GridManager
        Debug.Log("check win lose: " + remainingBlocks );
        if (remainingBlocks == 0)
        {
            WinLevel();
            return;
        }

        // Check Lose Condition
        if (TotalMove > 100 ) // Example thresholds
        {
            LoseLevel();
        }
    }

    protected bool IsAllCellFull()
    {


        return true;
    }

    protected bool IsAllCellEmpty()
    {


        return true;
    }

    public void OnFocus(bool isFocus)
    {
        /*if (!isFocus && draggingSlot != null) {

            draggingSlot.EndMove(draggingSlot.DragTarget.position);

            if (highlightVFX != null) {
                highlightVFX.Recycle();
                highlightVFX = null;
            }

            highlightSlot = null;
            draggingSlot = null;

            CheckHideTutorial();
        } else {
            CheckShowTutorial();
        }

        if (isFocus) {
            ResumeCountdown(false);
        } else {
            PauseCountdown(false);
        }*/
    }

    public void FillElements()
    {

    }


    protected void DestroyElements()
    {

    }




    [ContextMenu("Pause Countdown")]
    public virtual void PauseCountdown(bool nofity = true)
    {

        timer.Pause();

        if (nofity)
        {
            EventDispatcher.Dispatch(new GameEvent.LevelCountdownChanged(true));
        }
    }


    [ContextMenu("Resume Countdown")]
    public virtual void ResumeCountdown(bool nofity = true)
    {
        timer.Resume();

        if (nofity)
        {
            EventDispatcher.Dispatch(new GameEvent.LevelCountdownChanged(false));
        }
    }


    [ContextMenu("Add 60s")]
    public virtual void Add60Seconds()
    {
        timer.Add(60);
    }

    public virtual void AddMoreSeconds(int amount)
    {
        timer.Add(amount);
    }


    [ContextMenu("Double Stars")]
    public virtual void DoubleStars()
    {

    }

    [System.Serializable]
    public class LevelInfomation
    {
        private float startTime;
        private float finishTime;
        private int reslovedElements;
        private int currentStar;
        private int currentCombo;
        private int highestCombo;
        private float lastTimeResloved;
        private bool doubleStarEnabled;
        private bool doubleAllEnabled;

        public float StartTime => startTime;
        public float FinishTime => finishTime;
        public int CurrentStar => currentStar;
        public int CurrentCombo => currentCombo;
        public int HighestCombo => highestCombo;
        public bool IsCombing => Time.unscaledTime < ComboTime;
        public int ReslovedElements => reslovedElements;
        public float LastTimeResloved => lastTimeResloved;
        public float ComboTime => lastTimeResloved + GetCurrentComboTime(currentCombo);
        public bool DoubleStarEnabled => doubleStarEnabled;
        public bool DoubleAllEnabled => doubleAllEnabled;

        public LevelInfomation()
        {
            startTime = 0;
            finishTime = 0;
            reslovedElements = 0;
            currentStar = 0;
            currentCombo = 0;
            highestCombo = 0;
            lastTimeResloved = 0;
            doubleStarEnabled = false;
        }

        public LevelInfomation(bool hasDoubleAllBooster) : this()
        {
            this.doubleAllEnabled = hasDoubleAllBooster;
        }

        public void OnDoubleStar()
        {
            doubleStarEnabled = true;

        }

        public void OnStarted()
        {
            startTime = Time.unscaledTime;
        }

        public void OnFinished()
        {
            finishTime = Time.unscaledTime;
        }

        public void OnResolved()
        {
            if (IsCombing)
            {
                currentCombo++;
            }
            else
            {
                currentCombo = 1;
            }

            if (currentCombo > highestCombo)
            {
                highestCombo = currentCombo;
            }

            int starBonus = GetBonus(currentCombo);

            if (DoubleStarEnabled) starBonus *= 2;

            currentStar += starBonus;

            lastTimeResloved = Time.unscaledTime;

        }

        public void ResetCombo()
        {
            currentCombo = 0;
            lastTimeResloved = 0;
        }

        private int GetCurrentComboTime(int combo)
        {
            switch (combo)
            {
                case 1:
                    return 25;
                case 2:
                    return 20;
                case 3:
                    return 15;
                case 4:
                case 5:
                case 6:
                    return 10;
                case 7:
                case 8:
                case 9:
                    return 7;
                case 10:
                case 11:
                case 12:
                    return 5;
                case 13:
                case 14:
                case 15:
                    return 3;
                case 16:
                case 17:
                    return 2;
                default:
                    return 1;
            }
        }

        private int GetBonus(int combo)
        {
            return 1 + combo / 3;
        }
    }

    public enum InputSource
    {
        Player,
        Tutorial,
    }

    private Coroutine intersitialAdCoroutine;

    #region FUNC RELATE SHOW ADS


    private void StopShowInterstitialAd()
    {
        if (intersitialAdCoroutine != null) StopCoroutine(intersitialAdCoroutine);
        intersitialAdCoroutine = null;
    }

    private IEnumerator IEShowIntersitialAd(float cappingTime)
    {
        WaitForSeconds waitFoCappingTime = new WaitForSeconds(cappingTime);

        while (true)
        {
            yield return waitFoCappingTime;

            GameAdvertising.TryShowInterstitialAd();
        }
    }

    #endregion
}

#region EVENT ANALYTICS
    /*public void LogWinLevelEvent()
    {
        GameAnalytics.LogEvent(CreateWinlevelEvent());
    }

    public void LogLoseLevelEvent(string reason)
    {
        GameAnalytics.LogEvent(CreateLoselevelEvent(reason));
    }

    private GameAnalytics.GameEvent CreateWinlevelEvent()
    {
        return GameAnalytics.GameEvent.Create("level_end")
            .Add("level", Option.Level.ToString())
            .Add("result", "win")
            .Add("total_move", TotalMove.ToString())
            .Add("play_time", Mathf.CeilToInt(Time.time - startedTime))
            .Add("use_time_freeze", GetBoosterUsed(ItemID.TimeFreezeBooster))
            .Add("use_reroll", GetBoosterUsed(ItemID.RerollBooster))
            .Add("use_more_time", GetBoosterUsed(ItemID.MoreTimeBooster))
            .Add("use_magic_wand", GetBoosterUsed(ItemID.MagicWandBooster))
            .Add("use_double_star", GetBoosterUsed(ItemID.DoubleStarBooster))
            .Add("use_big_hammer", GetBoosterUsed(ItemID.BigHammerBooster))
            .Add("use_small_hammer", GetBoosterUsed(ItemID.SmallHammerBooster))
            .Add("use_used", boosterUsed.ToString());
    }

    private GameAnalytics.GameEvent CreateLoselevelEvent(string reason)
    {
        return GameAnalytics.GameEvent.Create("level_end")
            .Add("level", Option.Level.ToString())
            .Add("result", "fail")
            .Add("reason", reason)
            .Add("play_time", Mathf.CeilToInt(Time.time - startedTime).ToString())
            .Add("use_time_freeze", GetBoosterUsed(ItemID.TimeFreezeBooster))
            .Add("use_reroll", GetBoosterUsed(ItemID.RerollBooster))
            .Add("use_more_time", GetBoosterUsed(ItemID.MoreTimeBooster))
            .Add("use_magic_wand", GetBoosterUsed(ItemID.MagicWandBooster))
            .Add("use_double_star", GetBoosterUsed(ItemID.DoubleStarBooster))
            .Add("use_big_hammer", GetBoosterUsed(ItemID.BigHammerBooster))
            .Add("use_small_hammer", GetBoosterUsed(ItemID.SmallHammerBooster))
            .Add("use_used", boosterUsed.ToString());
    }*/
    #endregion
