using UnityEngine;

[System.Serializable]
public class Direction 
{
    public Direction2D direction;

    public Vector2 GetVector()
    {
        return direction switch
        {
            Direction2D.Up => Vector2.up,
            Direction2D.Down => Vector2.down,
            Direction2D.Left => Vector2.left,
            Direction2D.Right => Vector2.right,
            _ => Vector2.zero,
        };
    }

    public Vector3 GetVector3()
    {
        Vector2 vec2 = GetVector();
        return new Vector3(vec2.x, vec2.y, 0f);
    }
    public bool IsHorizontal =>
        direction == Direction2D.Left || direction == Direction2D.Right;

    public bool IsVertical =>
        direction == Direction2D.Up || direction == Direction2D.Down;
}

public enum Direction2D
{
    Up,
    Down,
    Left,
    Right
}