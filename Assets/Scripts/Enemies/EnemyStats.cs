using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] EnemyStatsSO enemyStats;
    //Health
    float health;
    
    private void Start()
    {
        health = enemyStats.enemyHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Bullet") && collision.gameObject.tag == "PlayerBullet") 
        {
            TakeDamage(collision.gameObject.GetComponent<Bullet>().bulletDamage);
            Debug.Log(health);
            
        }
    }
}
