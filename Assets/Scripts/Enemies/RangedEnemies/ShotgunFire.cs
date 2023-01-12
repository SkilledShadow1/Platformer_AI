using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunFire : RangedController
{
    int bulletCount;
    float spread;
    public ShotgunFire(Transform pivot, Transform player, Transform thisEnemy, Transform bulletSpawn, GameObject bulletPrefab, float viewRadius, float bulletDamage, 
                       float bulletSpeed, float bulletLifetime, float fireRate, float smallestAngleToFire, float gunRotationSpeed, int bulletCount, float spread) :
                       base(pivot, player, thisEnemy, bulletSpawn, bulletPrefab, viewRadius, bulletDamage, bulletSpeed, bulletLifetime, fireRate, smallestAngleToFire, gunRotationSpeed)
    {
        this.bulletCount = bulletCount;
        this.spread = spread;
    }

    public override void Fire()
    {
        if (canFire)
        {
            for(int i = 0; i < bulletCount; i++) 
            {
                GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.position, pivot.rotation * bulletPrefab.transform.rotation);
                Vector3 bulletVelocity = Vector3.Normalize((bullet.transform.right + new Vector3(0, Random.Range(-spread, spread), 0)))* bulletSpeed;
                bullet.GetComponent<Rigidbody2D>().velocity = bulletVelocity;
                bullet.GetComponent<Bullet>().BulletStats(bulletDamage, bulletLifetime);
                float rotation = Mathf.Atan2(bulletVelocity.y, bulletVelocity.x) * Mathf.Rad2Deg;
                bullet.transform.eulerAngles = new Vector3(0, 0, rotation);
            }
            currentFireTime = 0;
        }

    }
}
