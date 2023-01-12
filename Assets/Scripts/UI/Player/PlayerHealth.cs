using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float maxHealth;
    float health;

    private void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(float damage) 
    {
        health -= damage;

        if(health < 0) 
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "EnemyBullet" && collision.gameObject.layer == LayerMask.NameToLayer("Bullet")) 
        {
            TakeDamage(collision.gameObject.GetComponent<Bullet>().bulletDamage);
        }
    }

}
