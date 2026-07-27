using Enemies;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Wave Spawner/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [SerializeField]
    Enemy prefab;

    [SerializeField, Min(0.1f)]
    float moveSpeed;

    public Enemy Prefab => prefab;

    public float MoveSpeed => moveSpeed;
}
