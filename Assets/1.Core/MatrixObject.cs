using UnityEngine;

public class MatrixObject : MonoBehaviour
{
    [SerializeField] private string tileKey;
    public string TileKey => tileKey;

    public int posX;
    public int posY;

}
