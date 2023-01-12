using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    [SerializeField] float forceMultiplier;
    [SerializeField] float forceX = 0;
    [SerializeField] float speedX = 0;
    [SerializeField] float maxSpeedX = 100;
    [SerializeField] float minSpeed = -100;
    [SerializeField] float drag = 10;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        TestAddForce((int) Input.GetAxisRaw("Horizontal"));
        speedX -= drag;
        if (speedX < 0)
            speedX = 0;
        speedX += forceX * .95f;
        forceX -= forceX * .95f;
        if (speedX > maxSpeedX)
            speedX = maxSpeedX;
        rb.velocity = new Vector2(speedX + rb.velocity.x, rb.velocity.y);
        
    }

    private void TestAddForce(int x)
    {
        float forceAdded = x * forceMultiplier;
        forceX += forceAdded;
    }
}
