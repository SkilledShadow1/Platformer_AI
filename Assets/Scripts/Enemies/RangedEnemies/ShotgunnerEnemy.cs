using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunnerEnemy : MonoBehaviour
{

    RangedController controller;
    [SerializeField] RangedSO rangedScriptableObject;
    [SerializeField] Transform bulletSpawn;
    RangedSO.ShotgunSO rangedSO;

    private void Start()
    {
        Transform gun = gameObject.transform.GetChild(0);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        rangedSO = (RangedSO.ShotgunSO)rangedScriptableObject.data;
        controller = new ShotgunFire(gun, player.transform, this.transform, bulletSpawn, rangedSO.bulletPrefab, rangedSO.viewRadius, rangedSO.bulletDamage,rangedSO.bulletSpeed, 
                                     rangedSO.bulletLifetime, rangedSO.fireRate, rangedSO.smallestAngleToFire, rangedSO.gunRotationSpeed, rangedSO.bulletCount, rangedSO.bulletSpread);
    }

    private void Update()
    {
        controller.Reload();
        if (controller.CanView())
            controller.Fire();
    }

}
