/*
 * Whip Weapon which spawns a non moving bullet near player position, all enemies within the hit zone gets damage
 * Whip bullet vanishes after less then a second
 * New whip bullet spawn on the oposite side of the previous bullet
 */

using System.Collections;
using UnityEngine;

public class WeaponWhip : Weapon
{
    private bool isZig = true;

    protected override void PerformAttack(Vector3 origin, Vector3 direction)
    {
        if (!spatialGroupManager.IsInitialized)
        {
            Debug.Log("waiting for spatial manager");
            return;
        }
        base.PerformAttack(origin, direction);
        StartCoroutine(SpawnBullets(origin, direction));

        isZig = true; // reset zig zag
    }

    private IEnumerator SpawnBullets(Vector3 origin, Vector3 direction)
    {
        for (int i = 0; i < weaponData.BaseBulletCount; i++)
        {
            SpawnBulletZigZag(origin);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SpawnBulletZigZag(Vector3 origin)
    {
        // Normalize the direction to ensure consistent distance
        //  direction = direction.normalized;

        // Calculate the right vector perpendicular to the direction
        Vector3 right = Vector3.right.normalized;

        // Determine the spawn position
        Vector3 offset = isZig ? right : -right;
        Vector3 spawnPosition = origin + offset;

        // Spawn the bullet
        BulletWhip bullet = Instantiate(bulletPrefab as BulletWhip);
        bullet.transform.position = spawnPosition;
        //      bullet.transform.forward = direction; // Face the bullet towards the direction
        bullet.Initialize(new IControllable[] { spatialGroupManager, this });
        // Toggle the zig-zag state for the next bullet
        isZig = !isZig;
    }
}