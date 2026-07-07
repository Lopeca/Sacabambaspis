using UnityEngine;

public static class GameObjectExtensions
{
    public static void ClearChildren(this GameObject go)
    {
        var transform = go.transform;
        // 역순으로 순회해야 삭제 시 인덱스가 꼬이지 않습니다.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}
