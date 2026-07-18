using System;
using System.Collections.Generic;
using UnityEngine;

public class MatrixObject : MonoBehaviour
{
    [SerializeField] private string tileKey;
    public string TileKey => tileKey;
    
    public CollectibleObject CollectibleObject { get; private set; }
    public IGridInteractable GridInteractable { get; private set; }
    public bool isRounded;

    public int posX;
    public int posY;
    
    List<IGridComponent> gridComponents;

    private void Awake()
    {
        gridComponents = new List<IGridComponent>();
        
        CollectibleObject = GetComponent<CollectibleObject>();
        GridInteractable = GetComponent<IGridInteractable>();
    }

    public Vector2Int GetPos()
    {
        return new Vector2Int(posX, posY);
    }
    public void AppendGridComponent(IGridComponent gridComponent)
    {
        gridComponents.Add(gridComponent);
    }
    public void GridUpdate()
    {
        foreach (var gridComponent in gridComponents)
        {
            gridComponent.GridUpdate();
        }
    }
}
