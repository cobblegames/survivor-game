using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private int health = 10;
    [SerializeField] private int damage = 5;
    [SerializeField] private float movementSpeed = 1f;

    public int Health
    { get { return health; } private set { health = value; } }

    public int Damage
    { get { return damage; } private set { damage = value; } }

    public float MovementSpeed
    { get { return movementSpeed; } private set { movementSpeed = value; } }

}
