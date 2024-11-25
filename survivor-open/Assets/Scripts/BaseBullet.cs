using System.Collections.Generic;
using UnityEngine;

public class BaseBullet : MonoBehaviour, IControllable, IBullet
{
    protected float inheritedDamage;
    protected float inheritedRadius;
    protected int inheritedMaxTargets;
    protected float inheritedRange;

    protected SpatialGroupManager spatialGroupManager;
    protected Weapon parentWeapon;
    protected int spatialGroup;
    protected List<int> surroundingSpatialGroups = new List<int>();
    protected bool isDestroyed;

    public virtual void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        parentWeapon = _injectedElements[1] as Weapon;

        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        spatialGroup = spatialGroupManager.GetSpatialGroupStatic(transform.position.x, transform.position.y);

        inheritedDamage = parentWeapon.CurrentDamage;
        inheritedMaxTargets = parentWeapon.CurrentMaxTargets;
    }

    public virtual void EveryFrameLogic()
    {
        CheckCollisionWithEnemy();
    }

    public virtual void CheckCollisionWithEnemy()
    {
        List<Enemy> surroundingEnemies = spatialGroupManager.GetAllEnemiesInSpatialGroups(surroundingSpatialGroups);
        int targetCounter = 0;
        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null) continue;

            if (Vector2.Distance(transform.position, enemy.transform.position) < inheritedRadius)
            {
                enemy.ChangeHealth(inheritedDamage);
                targetCounter++;
                if (targetCounter >= inheritedMaxTargets)
                {
                    DestroyBullet();

                    break;
                }
            }else
            {
                DestroyBullet();  
            }
        }
    }

    public virtual void DestroyBullet()
    {
        if (isDestroyed) return;

        spatialGroupManager.bulletSpatialGroups[spatialGroup].Remove(this);
        isDestroyed = true;
        Destroy(gameObject);
    }
}