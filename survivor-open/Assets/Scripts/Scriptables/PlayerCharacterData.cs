using UnityEngine;

[CreateAssetMenu(menuName = "Players/Player Data", fileName = "PlayerData")]
public class PlayerCharacterData : ScriptableObject
{
    [SerializeField] private string characterName;
    [SerializeField] private float health = 100;
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float hitBoxRadius = 0.4f;
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Weapon[] defaultWeapons;

    public string CharacterName
    { get { return characterName; } }

    public Sprite PlayerIcon
    { get { return playerIcon; } }

    public Weapon[] DefaultWeapons
    { get { return defaultWeapons; } }

    public float Health
    { get { return health; } }

    public float MovementSpeed
    { get { return movementSpeed; } }

    public float HitBoxRadius
    { get { return hitBoxRadius; } }
}