using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int duration;
    void Awake()
    {
        SetTime();
    }
    void SetTime()
    {
        int currentLevel = GameData.Classic.LevelUnlocked;
        string paths = $"LevelSO/Level_{currentLevel}";
        ConfigLevel configLevel = Resources.Load<ConfigLevel>(paths);
        duration = configLevel.time;
    }

    public int Duration
    {
        get => duration;
        set => duration = value;
    }
}

