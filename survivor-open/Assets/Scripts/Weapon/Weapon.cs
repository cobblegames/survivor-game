using UnityEngine;
using System.Collections;
public class Weapon : MonoBehaviour, IControllable
{
    [SerializeField] protected WeaponData weaponData;


    // Core weapon properties
    protected string weaponName;
    protected float currentDamage;
    protected float currentAttackCoolDown;
    protected int currentMaxTargets;
    protected float currentRange;
    protected float currentCooldownTimer;

    protected SpatialGroupManager spatialGroupManager;
 

    public virtual void Initialize(IControllable[] _injectedElements)
    {
        weaponName = weaponData.WeaponName;
        currentDamage = weaponData.Damage;
        currentAttackCoolDown = weaponData.AttackCooldown;
        currentMaxTargets = weaponData.MaxTargets;
        currentRange = weaponData.Range;

        spatialGroupManager = _injectedElements[0] as SpatialGroupManager; // we might need it later for collision witrh enemy checks
    }

    public virtual void Attack(Vector3 origin, Vector3 direction)
    {
       
            PerformAttack(origin, direction);
            currentCooldownTimer = currentAttackCoolDown;
        
    }

    public virtual bool CanAttack() // all weapons will be processed in one loop
    {
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.deltaTime;
            return false;
        }else
        {
            return true;
        }
            
    }


    // method for specific attack logic
    protected virtual void PerformAttack(Vector3 origin, Vector3 direction) { }
    

  
}