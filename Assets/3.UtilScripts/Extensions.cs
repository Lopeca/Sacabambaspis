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

public static class ColorExtensions
{
    /// <summary>
    /// 기존 색상에서 Alpha 값만 변경된 새로운 Color를 반환합니다.
    /// </summary>
    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color; // 복사본의 a를 수정 후 새 Color 구조체로 반환
    }
}
