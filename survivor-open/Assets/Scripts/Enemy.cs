using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int batchId;
    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    public float movementSpeed = 3f;
    public Vector3 currentMovementDirection = Vector3.zero;
    public int spatialGroup = 0;

    int health = 10;
    int damage = 5;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    private SpatialGroupManager spatialGroupManager;

    // SpatialGroupManager Injection
    public void Initialize(SpatialGroupManager manager)
    {
        this.spatialGroupManager = manager;
    }



    public void ChangeHealth(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            KillEnemy();
        }
    }

    public void KillEnemy()
    {
        Debug.Log("Enemy Killed");

        spatialGroupManager.RemoveFromSpatialGroup(batchId, this);

        spatialGroupManager.enemySpatialGroups[spatialGroup].Remove(this);

      

        Destroy(gameObject);
    }
}