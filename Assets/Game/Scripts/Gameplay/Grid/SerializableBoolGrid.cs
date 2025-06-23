using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableBoolGrid
{
    public int width = 6;
    public int height = 6;
    [SerializeField] public List<bool> data = new List<bool>();

    public void Resize(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;
        data = new List<bool>(newWidth * newHeight);
        for (int i = 0; i < width * height; i++)
            data.Add(false);
    }

    public bool Get(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        return data[x + y * width];
    }

    public void Set(int x, int y, bool value)
    {
        if (!IsInBounds(x, y)) return;
        data[x + y * width] = value;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public List<Vector2Int> ToVector2IntList()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (Get(x, y)) result.Add(new Vector2Int(x, y));
        return result;
    }

    public void EnsureInitialized()
    {
        if (data == null || data.Count != width * height)
        {
            Resize(width, height);
        }
    }
}