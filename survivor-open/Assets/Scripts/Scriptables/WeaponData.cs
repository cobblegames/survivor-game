using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Weapon Stats", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponName;
    [SerializeField] private float damage;
    [SerializeField] private float attackCooldown;
    [SerializeField] private int maxTargets;
    [SerializeField] private int baseBulletCount;
    [SerializeField] private float range;
    [SerializeField] private Sprite uiIcon;

    public string WeaponName
    { get { return weaponName; } }

    public float Damage
    { get { return damage; } }

    public float AttackCooldown
    { get { return attackCooldown; } }

    public int MaxTargets
    { get { return maxTargets; } } // For AoE or chaining effects

    public int BaseBulletCount
    { get { return baseBulletCount; } }

    public float Range
    { get { return range; } }

    public Sprite UiIcon
    { get { return uiIcon; } }
}