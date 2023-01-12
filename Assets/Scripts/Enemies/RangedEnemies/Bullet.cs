using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletDamage = 10f;
    float bulletLifetime = 10000000f;
    LayerMask[] ignoreLayers;


    public void BulletStats(float bulletDamage, float bulletLifetime, LayerMask[] ignoreLayers = null) 
    {
        this.bulletDamage = bulletDamage;
        this.bulletLifetime = bulletLifetime;
        if (ignoreLayers == null) 
        {
            this.ignoreLayers = new LayerMask[] { LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("Enemy")};
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (LayerMask mask in ignoreLayers) 
        {
            if (mask.value == collision.gameObject.layer)
            {
                return;
            }                

        }
        
        Destroy(gameObject);

    }

    private void Start()
    {
        Destroy(gameObject, bulletLifetime);
        ignoreLayers = new LayerMask[] { LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("Player") };
    }


}