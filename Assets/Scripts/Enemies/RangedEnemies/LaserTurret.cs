using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] Transform laserSpawn;
    [SerializeField] RangedSO rangedScriptableObject;
    RangedSO.LaserSO rangedSO;
    RangedController controller;
    LineRenderer linerenderer;

    float bulletDamage;

    // Start is called before the first frame update
    void Start()
    {
        rangedSO = (RangedSO.LaserSO) rangedScriptableObject.data;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform pivot = gameObject.transform.GetChild(0);
        controller = new RangedController(pivot, player.transform, this.transform, rangedSO.gunRotationSpeed);
        bulletDamage = rangedSO.bulletDamage;
        linerenderer = GetComponent<LineRenderer>();
        linerenderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        controller.GunLookAtPlayer();
        CreateLaser();
    }

    void CreateLaser() 
    {
        linerenderer.SetPosition(0, laserSpawn.position);
        RaycastHit2D hit = Physics2D.Raycast(laserSpawn.position, laserSpawn.right, Mathf.Infinity);
        Debug.DrawRay(laserSpawn.position, laserSpawn.right * 10000, Color.yellow);
        if (hit)
        {
            linerenderer.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("Player")) 
            {
                hit.collider.gameObject.GetComponent<PlayerHealth>().TakeDamage(bulletDamage * Time.deltaTime);
            }
        }
        else 
        {
            linerenderer.SetPosition(1, laserSpawn.right * 10000);//Laser goes to infinity
        }
        

    }
}
