using UnityEngine;

[CreateAssetMenu(fileName = "ObjectPhysicsConfig", menuName = "Scriptable Objects/ObjectPhysicsConfig")]
public class ObjectPhysicsConfigSO : ScriptableObject
{
    // 한 칸 이동 소요시간
    public float moveDuration;
}
