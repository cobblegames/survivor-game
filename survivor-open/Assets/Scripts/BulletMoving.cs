using UnityEngine;

public class BulletMoving : BaseBullet, IMovable
{
    protected float currentBulletSpeed;
    protected Vector2 movementDirection = Vector2.zero;
    protected Transform modelTransform;

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);
    }

    public virtual void UpdateSpatialGroup()
    {
        int newSpatialGroup = spatialGroupManager.GetSpatialGroupStatic(transform.position.x, transform.position.y);
        if (newSpatialGroup != spatialGroup)
        {
            spatialGroupManager.bulletSpatialGroups[spatialGroup].Remove(this);

            spatialGroup = newSpatialGroup;
            spatialGroupManager.bulletSpatialGroups[spatialGroup].Add(this);
            surroundingSpatialGroups = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, movementDirection);
        }
    }

    public override void EveryFrameLogic()
    {
        base.EveryFrameLogic();

        transform.position += (Vector3)(movementDirection * currentBulletSpeed * Time.deltaTime);
        UpdateSpatialGroup();
    }

    public void IntervalLogic()
    {
        if (!isDestroyed && spatialGroupManager.IsOutOfBounds((Vector2)transform.position))
        {
            DestroyBullet();
        }
    }
}