using UnityEngine;
[CreateAssetMenu(menuName = "Weapon/Weapon Stats", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
   [SerializeField] private Bullet bullet;

    public Bullet Bullet
    { get { return bullet; } private set { bullet = value; } }

}
