using UnityEngine;
public enum DifficultyLevel
{
    Easy,
    Hard
}
[CreateAssetMenu(fileName = "ConfigLevel", menuName = "Game/ConfigLevel")]
public class ConfigLevel : ScriptableObject
{
    public int levelId;
    public DifficultyLevel difficulty;
}


