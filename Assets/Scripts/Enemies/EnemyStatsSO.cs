using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EnemyStats", order = 1)]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Health")]
    public float enemyHealth;

}
