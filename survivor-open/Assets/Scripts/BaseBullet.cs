using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBullet : MonoBehaviour, IControllable, IBullet
{
    [SerializeField] protected float destroyTime;
    protected float inheritedDamage;
    protected float inheritedRadius;
    protected int inheritedMaxTargets;
    protected float inheritedRange;

    protected SpatialGroupManager spatialGroupManager;
    protected Weapon parentWeapon;
    protected int spatialGroup;
    protected List<int> surroundingSpatialGroups = new List<int>();
    protected bool isDestroyed;

    protected WaitForEndOfFrame endOfFrameInterval = new WaitForEndOfFrame();

    public virtual void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        parentWeapon = _injectedElements[1] as Weapon;

        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        spatialGroup = spatialGroupManager.GetSpatialGroupStatic(transform.position.x, transform.position.y);

        inheritedDamage = parentWeapon.CurrentDamage;
        inheritedMaxTargets = parentWeapon.CurrentMaxTargets;
        inheritedRange = parentWeapon.CurrentRange;

        StartCoroutine(MainBulletLoop());
    }

    private IEnumerator MainBulletLoop()
    {
        while (!isDestroyed)
        {
            EveryFrameLogic();

            yield return endOfFrameInterval;
        }
    }

    public virtual void EveryFrameLogic()
    {
        CheckCollisionWithEnemy();
    }

    public virtual void CheckCollisionWithEnemy()
    {
        surroundingSpatialGroups = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, transform.forward);
        List<Enemy> surroundingEnemies = spatialGroupManager.GetAllItemsInSpatialGroups<Enemy>(surroundingSpatialGroups, spatialGroupManager.enemySpatialGroups);

        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null)
            {
                Debug.Log("No enemy");
                continue;
            }

            if (CheckHitBox(enemy))
            {
                DoAttack(enemy);
            }
            else
            {
                Debug.Log("No collision");
            }
        }

        DestroyBullet();
    }

    protected virtual bool CheckHitBox(Enemy _enemy)
    {
        return false;
    }

    protected virtual void DoAttack(Enemy _enemy)
    {
        _enemy.ChangeHealth(inheritedDamage);
    }

    public virtual void DestroyBullet()
    {
        if (isDestroyed) return;
        spatialGroupManager.bulletSpatialGroups[spatialGroup].Remove(this);
        isDestroyed = true;
        Destroy(gameObject, destroyTime);
    }
}