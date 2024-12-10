using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private float health = 10;
    [SerializeField] private float damage = 5;
    [SerializeField] private float movementSpeed = 1f;

    public float Health
    { get { return health; } }

    public float Damage
    { get { return damage; } }

    public float MovementSpeed
    { get { return movementSpeed; } }
}