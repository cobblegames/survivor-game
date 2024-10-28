using UnityEngine;
[CreateAssetMenu(menuName = "Players/Player Data", fileName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private int health = 100;
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float hitBoxRadius = 0.4f;

    public int Health
    { get { return health; } private set { health = value; } }

    public float MovementSpeed
    { get { return movementSpeed; } private set { movementSpeed = value; } }

    public float HitBoxRadius
    { get { return hitBoxRadius; } private set { hitBoxRadius = value; } }


}
