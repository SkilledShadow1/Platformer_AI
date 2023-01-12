using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedController
{
    //protected = child class can access
    protected float gunRotationSpeed;
    protected float smallestAngleToFire;
    protected float viewRadius;
    protected float minDegToLook = 2f;

    protected float fireRate;
    protected float currentFireTime;
    protected bool canFire = false;

    protected float bulletDamage;
    protected float bulletLifetime;
    protected float bulletSpeed;
    protected GameObject bulletPrefab;

    protected Transform pivot;
    protected Transform player;
    protected Transform thisEnemy;
    protected Transform bulletSpawn;
    
    public RangedController(Transform pivot, Transform player, Transform thisEnemy, Transform bulletSpawn, GameObject bulletPrefab, float viewRadius, 
                            float bulletDamage, float bulletSpeed, float bulletLifetime, float fireRate, float smallestAngleToFire, float gunRotationSpeed)
    {
        this.gunRotationSpeed = gunRotationSpeed;
        this.pivot = pivot;
        this.player = player;
        this.thisEnemy = thisEnemy;
        this.bulletPrefab = bulletPrefab;
        this.viewRadius = viewRadius;
        this.bulletSpawn = bulletSpawn;
        this.bulletDamage = bulletDamage;
        this.bulletSpeed = bulletSpeed;
        this.bulletLifetime = bulletLifetime;
        this.fireRate = fireRate;
        this.smallestAngleToFire = smallestAngleToFire;
    }

    //This is for the laser turret
    public RangedController(Transform pivot, Transform player, Transform thisEnemy, float gunRotationSpeed) 
    {
        this.gunRotationSpeed = gunRotationSpeed;
        this.pivot = pivot;
        this.player = player;
        this.thisEnemy = thisEnemy;
    }
    
    public void Reload() 
    {
        if (fireRate != Mathf.Infinity)
        {
            if (currentFireTime < fireRate)
            {
                canFire = false;
                currentFireTime += Time.deltaTime;
            }

            else
                canFire = true;
        }
    }

    public virtual void Fire() 
    {
        if (canFire) 
        {
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.position, pivot.rotation * bulletPrefab.transform.rotation);
            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.right * bulletSpeed;
            bullet.GetComponent<Bullet>().BulletStats(bulletDamage, bulletLifetime);
            currentFireTime = 0;
        }

            
    }

    public bool GunLookAtPlayer()
    {
        //turn into angle between 0-360
        float destinationRotation = -Vector2.SignedAngle(thisEnemy.position - player.position, -Vector2.up);
        if (destinationRotation < 0)
            destinationRotation += 360;
        int destinationQuadrant = GetQuadrant(destinationRotation);

        float pivotRotation = pivot.eulerAngles.z; //this is for angle offset cause the diamond needs to be on a vector.up axis while the gun is not
        if (pivotRotation < 0)
            pivotRotation += 360;
        pivotRotation %= 360;
        int pivotQuadrant = GetQuadrant(pivotRotation);

        //Debug.Log("Destination Rotation: " + destinationRotation + "    " + "Pivot Rotation: " + pivotRotation);

        Vector3 clockwise = new Vector3(0, 0, pivot.eulerAngles.z + (gunRotationSpeed * Time.deltaTime));
        Vector3 counterClockwise = new Vector3(0, 0, pivot.eulerAngles.z - (gunRotationSpeed * Time.deltaTime));

        //If it is essentially looking at the player (minor offset cause Time.deltaTime is not perfect)
        if (Mathf.Abs(pivotRotation - destinationRotation) > minDegToLook && Mathf.Abs(pivotRotation - destinationRotation) < 360 - minDegToLook)
        {
            int quadrantDifference = Mathf.Abs(GetQuadrant(pivotRotation) - GetQuadrant(destinationRotation));

            switch (quadrantDifference)
            {
                case 0:
                    if (pivotRotation > destinationRotation)
                    {
                        pivot.eulerAngles = counterClockwise;
                    }
                    else
                    {
                        pivot.eulerAngles = clockwise;
                    }

                    break;
                case 1:
                    if (destinationQuadrant < pivotQuadrant)//if quadrant destination is 4 and current is 1
                    {
                        pivot.eulerAngles = counterClockwise;
                    }
                    else
                    {
                        pivot.eulerAngles = clockwise;
                    }


                    break;

                case 2:
                    if (destinationQuadrant > pivotQuadrant)
                    {
                        if (Mathf.Abs(destinationRotation - pivotRotation) <= 180)//checking if clockwise works
                        {
                            pivot.eulerAngles = clockwise;
                        }
                        else
                        {
                            pivot.eulerAngles = counterClockwise;
                        }
                    }

                    if (destinationQuadrant < pivotQuadrant)
                    {
                        if (Mathf.Abs(destinationRotation - pivotRotation) <= 180)//checking if clockwise works
                        {
                            pivot.eulerAngles = counterClockwise;
                        }
                        else
                        {
                            pivot.eulerAngles = clockwise;
                        }
                    }

                    break;

                case 3: //only if quadrants are 1 and 4
                    if (destinationQuadrant > pivotQuadrant)//if quadrant destination is 4 and current is 1
                    {
                        pivot.eulerAngles = counterClockwise;
                    }
                    else
                    {
                        pivot.eulerAngles = clockwise;
                    }
                    break;
            }

        }

        if (smallestAngleToFire != Mathf.Infinity) //Checks if the parameter has been filled out
        {
            if (Mathf.Abs(pivotRotation - destinationRotation) <= smallestAngleToFire || Mathf.Abs(pivotRotation - destinationRotation) >= 360 - smallestAngleToFire) //If it is close to the player, Fire
                return true;
        }

        return false;
    }

    private int GetQuadrant(float degrees) 
    {
        if(degrees >= 0 && degrees <= 90) 
        {
            return 1;
        }
        else if(degrees > 90 && degrees <= 180) 
        {
            return 2;
        }
        else if(degrees > 180 && degrees <= 270) 
        {
            return 3;
        }
        else if(degrees > 270 && degrees <= 360) 
        {
            return 4;
        }
        else 
        {
            return 1;
        }
    }

    public bool CanView()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(thisEnemy.position, viewRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                if (GunLookAtPlayer())
                {
                    return true;
                }

            }

        }

        return false;
    }

    /*
     * OLD ITERATION
             //turn into angle between 0-360
        float destinationRotation = - Vector2.SignedAngle(thisEnemy.position - player.position, -Vector2.up);
        Debug.Log(destinationRotation);
        if (destinationRotation == -180)
            destinationRotation = 180;
        float oppositeRotation = (destinationRotation + 180);
        float pivotRotation = pivot.eulerAngles.z;
        //Debug.Log(pivotRotation);

        //If it is essentially looking at the player (minor offset cause Time.deltaTime is not perfect)
        if (Mathf.Abs(pivotRotation - destinationRotation) % 180 > minRad)
        {
            //Debug.Log(Mathf.Abs(pivotRotation - destinationRotation));
            if (pivotRotation < oppositeRotation)
                pivot.eulerAngles = new Vector3(0, 0, pivot.eulerAngles.z + (gunRotationSpeed * Time.deltaTime));

            if (pivotRotation > oppositeRotation)
                pivot.eulerAngles = new Vector3(0, 0, pivot.eulerAngles.z - (gunRotationSpeed * Time.deltaTime));

        }
     */
}
