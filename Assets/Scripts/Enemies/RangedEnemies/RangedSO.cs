using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/RangedEnemyData", order = 1)]
public class RangedSO : ScriptableObject
{ 
    public enum enemyType
    {
        Basic,
        Shotgun,
        Laser
    }
    
    [HideInInspector] public BasicSO basicScriptableObject;
    [HideInInspector] public ShotgunSO shotgunScriptableObject;
    [HideInInspector] public LaserSO laserScriptableObject;


    // Determines how to create an Item from each ItemType
    public static RangedEnemy CreateBlankData(enemyType type)
    {
        switch (type)
        {
            default:
            case enemyType.Basic:
                return new BasicSO();

            case enemyType.Shotgun:
                return new ShotgunSO();

            case enemyType.Laser:
                return new LaserSO();
        }
    }

    private void OnValidate()
    {
        switch(type)
        {
            default:
            case enemyType.Basic:
                // To avoid losing already filled in data
                // simply check if the current data already has the correct type
                if(data != null && data.GetType() != typeof(BasicSO))
                {
                    data = new BasicSO();
                }
                break;

            case enemyType.Shotgun:
        
                if(data != null && data.GetType() != typeof(ShotgunSO))
                {
                    data = new ShotgunSO();
                }
                break;

            case enemyType.Laser:

                if (data != null && data.GetType() != typeof(LaserSO))
                {
                    data = new LaserSO();
                }
                break;
        }
    }

    [System.Serializable]
    public class RangedEnemy
    {
        [Header("All Class Variables")]
        public float gunRotationSpeed;
        public float bulletDamage;
    }

    [System.Serializable]
    public class BasicSO : RangedEnemy
    {
        [Header("View")]
        public float viewRadius;
        public float smallestAngleToFire;

        [Header("Attack Speed")]
        public float fireRate;
        public float bulletSpeed;

        [Header("Bullet")]
        public GameObject bulletPrefab;
        public float bulletLifetime;

        public BasicSO() { gunRotationSpeed = 100; }
    }

    [System.Serializable]
    public class ShotgunSO : RangedEnemy
    {
        [Header("View")]
        public float viewRadius;
        public float smallestAngleToFire;

        [Header("Attack Speed")]
        public float fireRate;
        public float bulletSpeed;

        [Header("Bullet")]
        public GameObject bulletPrefab;
        public float bulletLifetime;
        public int bulletCount;
        public float bulletSpread;

    }

    [System.Serializable]
    public class LaserSO : RangedEnemy
    {
    }

    public enemyType type;
    [SerializeReference] public RangedEnemy data;
}