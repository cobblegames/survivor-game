using System.Collections;
using UnityEngine;
public class WeaponKnife : Weapon
{
    
    protected override void PerformAttack(Vector3 origin, Vector3 direction)
    {
        if (!spatialGroupManager.IsInitialized)
        {
            Debug.Log("waiting for spatial manager");
            return;
        }
        base.PerformAttack(origin, direction);
        StartCoroutine(SpawnBullets(origin, direction));

     
    }

    private IEnumerator SpawnBullets(Vector3 origin, Vector3 direction)
    {
        for (int i = 0; i < weaponData.BaseBulletCount; i++)
        {
            BulletKnife bullet = Instantiate(bulletPrefab as BulletKnife);
            bullet.transform.position = origin;
            bullet.Initialize(new IControllable[] { spatialGroupManager, this });

            yield return new WaitForSeconds(0.2f);
        }
    }

}
