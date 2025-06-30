using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int duration;

    public int Duration
    {
        get => duration;
        set => duration = value;
    }
}
    
