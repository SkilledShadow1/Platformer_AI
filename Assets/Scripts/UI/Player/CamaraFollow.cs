using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraFollow : MonoBehaviour
{
    [SerializeField] float cameraDampTime;
    Transform playerTransform;
    Vector2 curPos;
    Vector2 playerPos;
    Vector2 vel = Vector2.one;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curPos = transform.position;
        playerPos = playerTransform.position;
        transform.position = Vector2.SmoothDamp(curPos, playerTransform.position, ref vel, cameraDampTime);
        transform.position += new Vector3(0, 0, -10);
    }
}
