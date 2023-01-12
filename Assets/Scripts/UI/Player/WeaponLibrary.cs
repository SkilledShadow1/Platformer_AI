using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon{
    public string name;
    public float damage;
    float fireRate;//persecond
    float bulletSpeed;
    float recoil;
    int bulletsPerShot;
    float spread;
    bool cooldown;
    public int clipSize;
    float reloadTime;
    public int ammoCount;

    public int currentClipAmmo;
    GameObject weaponObject;
    GameObject bulletObject; //this has to be prefab and has the tag "firingBullet"
    GameObject[] firingObjects;

    //constructor
    public Weapon(string name, GameObject weaponObject, GameObject bulletObject, float damage, float fireRate, float bulletSpeed, float recoil, int bulletsPerShot, float spread, int ammoCount, int clipSize, float reloadTime) {
        this.name = name;
        this.weaponObject = weaponObject;
        this.bulletObject = bulletObject;
        this.damage = damage;
        this.fireRate = fireRate;
        this.bulletSpeed = bulletSpeed;
        this.recoil = recoil;
        this.bulletsPerShot = bulletsPerShot;
        this.spread = spread;
        this.ammoCount = ammoCount-clipSize;
        this.clipSize = clipSize;
        this.reloadTime = reloadTime;
        currentClipAmmo = clipSize;
        
    }

    public void UpdateRotation(Vector2 aimingDir) {
        Vector3 temp = new Vector3(aimingDir.x, aimingDir.y, 0);
        weaponObject.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), temp);
    }

    public IEnumerator Fire(Vector2 aimingDir) {//accepts a normalized vector
        if (cooldown || currentClipAmmo == 0) {
            Debug.LogWarning("Can't fire now! ");
            //play empty trigger sound

            yield return null;
        } else {
            for (int i = 0; i < bulletsPerShot; i++) {
                GameObject.Instantiate(bulletObject, weaponObject.transform.position, weaponObject.transform.rotation);
                GameObject firing = GameObject.FindWithTag("firingBullet");
                Vector2 spreadVector = new Vector2(aimingDir.y, aimingDir.x).normalized * Random.Range(-spread, spread);
                firing.GetComponent<Rigidbody2D>().velocity = bulletSpeed * (1+Random.Range(-spread,spread)/30) * new Vector2(aimingDir.x, aimingDir.y) + spreadVector;
                firing.tag = "firedBullet";
            }
            currentClipAmmo--;
            //play shooting sound



            float timer = 0;
            GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().AddForce(recoil * -new Vector2(aimingDir.x,aimingDir.y*5));//recoil force
            cooldown = true;
            while (timer <= 1 / fireRate) {
                timer += Time.deltaTime;
                yield return null;
            }

            cooldown = false;
            yield return null;
        }
    }

    public IEnumerator Reload() {
        float timer = 0;

        //play reloading animation here

        while (timer <= reloadTime) {
            timer += Time.deltaTime;
            yield return null;
        }

        int reloadAmount = clipSize - currentClipAmmo;
        if (ammoCount > reloadAmount) {
            currentClipAmmo = clipSize;
            ammoCount -= reloadAmount;
        }
    }
}


