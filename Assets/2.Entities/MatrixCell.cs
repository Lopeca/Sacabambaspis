using UnityEngine;

public class MatrixCell : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private GameObject go; // 나중에 MatrixObject로 변경할 수도 있음

    public void SetObject(GameObject go)
    {
        this.go = go;
    }

    public GameObject GetObject()
    {
        return go;
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
