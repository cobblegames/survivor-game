using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IMovable, IControllable
{
    private SpatialGroupManager spatialGroupManager;

    public bool spinBullet;
    private int currentBulletDamage;
    private float currentBulletSpeed;
    private float currentBulletRadius;
    private Transform modelTransform;

    private int spatialGroup;
    private Vector2 movementDirection = Vector2.zero;
    private List<int> surroundingSpatialGroups = new List<int>();

    private bool isDestroyed = false;

    public delegate void BulletEnemyContactAction(Transform parentBullet);

    public event BulletEnemyContactAction OnContactWithEnemy;

    public Vector2 MovementDirection
    {
        get => movementDirection;
        set => movementDirection = value;
    }

    public void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        spatialGroup = spatialGroupManager.GetSpatialGroupStatic(transform.position.x, transform.position.y);
        surroundingSpatialGroups = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, movementDirection);

        //// Trigger the spawn event
        //OnBulletSpawned?.Invoke();
    }

    public void EveryFrameLogic()
    {
        // Move the bullet
        transform.position += (Vector3)(movementDirection * currentBulletSpeed * Time.deltaTime);

        // Rotate bullet if necessary
        if (spinBullet)
        {
            modelTransform.Rotate(0, 0, 10f);
        }
        else
        {
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg - 90f;
            modelTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Update spatial group
        UpdateSpatialGroup();

        // Check for collisions with enemies
        CheckCollisionWithEnemy();
    }

    private void UpdateSpatialGroup()
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

    private void CheckCollisionWithEnemy()
    {
        List<Enemy> surroundingEnemies = spatialGroupManager.GetAllEnemiesInSpatialGroups(surroundingSpatialGroups);

        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null) continue;

            if (Vector2.Distance(transform.position, enemy.transform.position) < currentBulletRadius)
            {
                OnContactWithEnemy?.Invoke(transform);
                enemy.ChangeHealth(-currentBulletDamage);
                DestroyBullet();
                break;
            }
        }
    }

    public void IntervalLogic()
    {
        if(!isDestroyed && spatialGroupManager.IsOutOfBounds((Vector2)transform.position))
        {
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        if (isDestroyed) return;

        spatialGroupManager.bulletSpatialGroups[spatialGroup].Remove(this);
        Destroy(gameObject);
        isDestroyed = true;
    }
}