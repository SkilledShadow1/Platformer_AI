using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerEnemy : MonoBehaviour
{
    RangedController controller;
    [SerializeField] RangedSO rangedScriptableObject;
    RangedSO.BasicSO rangedSO;
    [SerializeField] Transform bulletSpawn;
    private void Start()
    {
        rangedSO = (RangedSO.BasicSO)rangedScriptableObject.data;
        Transform gun = gameObject.transform.GetChild(0);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        controller = new RangedController(gun, player.transform, this.transform, bulletSpawn, rangedSO.bulletPrefab, rangedSO.viewRadius, rangedSO.bulletDamage, 
                                         rangedSO.bulletSpeed, rangedSO.bulletLifetime, rangedSO.fireRate, rangedSO.smallestAngleToFire, rangedSO.gunRotationSpeed);
    }

    private void Update()
    {
        controller.Reload();
        if (controller.CanView())
            controller.Fire();
    }

    private void OnDrawGizmos()
    {
        rangedSO = (RangedSO.BasicSO)rangedScriptableObject.data;
        Gizmos.color = new Color(0, 0, 255);
        Gizmos.DrawWireSphere(transform.position, rangedSO.viewRadius);
    }
}
