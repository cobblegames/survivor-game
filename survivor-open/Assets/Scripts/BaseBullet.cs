using System.Collections.Generic;
using UnityEngine;

public class BaseBullet : MonoBehaviour, IControllable
{
    protected float inheritedDamage;
    protected float currentBulletRadius;
    protected SpatialGroupManager spatialGroupManager;
    protected Weapon parentWeapon;
    protected int spatialGroup;
    protected List<int> surroundingSpatialGroups = new List<int>();


    public virtual void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        parentWeapon = _injectedElements[1] as Weapon;

        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        spatialGroup = spatialGroupManager.GetSpatialGroupStatic(transform.position.x, transform.position.y);

    }
}
