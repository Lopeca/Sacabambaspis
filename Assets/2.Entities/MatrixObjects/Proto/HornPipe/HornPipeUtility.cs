using UnityEngine;

public static class HornPipeUtility
{
    public static PipeDirection GetDirectionBit(Vector2Int dir)
    {
        if (dir == Vector2Int.up)    return PipeDirection.Up; 
        if (dir == Vector2Int.right) return PipeDirection.Right;  
        if (dir == Vector2Int.down)  return PipeDirection.Down; 
        if (dir == Vector2Int.left)  return PipeDirection.Left; 
        return PipeDirection.None;
    }
}