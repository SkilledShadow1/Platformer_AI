using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearExtraBullet : MonoBehaviour { 
    GameObject[] bulletObjects;
    [SerializeField] float maxDistance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bulletObjects = GameObject.FindGameObjectsWithTag("firedBullet");
        foreach (GameObject bullet in bulletObjects) {
            if (Vector3.Distance(bullet.transform.position, transform.position) > maxDistance) GameObject.Destroy(bullet);
        }
    }
}
