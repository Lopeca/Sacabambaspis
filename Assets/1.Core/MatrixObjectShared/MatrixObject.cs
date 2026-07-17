using System;
using UnityEngine;

public class MatrixObject : MonoBehaviour
{
    [SerializeField] private string tileKey;
    public string TileKey => tileKey;

    public IActiveGridElement ActiveElement { get; private set; }
    public CollectibleObject CollectibleObject { get; private set; }
    public IGridObstacle GridObstacle { get; private set; }
    public int posX;
    public int posY;

    private void Awake()
    {
        ActiveElement = GetComponent<IActiveGridElement>();
        CollectibleObject = GetComponent<CollectibleObject>();
        GridObstacle = GetComponent<IGridObstacle>();
    }

    public Vector2Int GetPos()
    {
        return new Vector2Int(posX, posY);
    }
}
