using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PInput input;
    Rigidbody2D rb;
    //IMPORTANT WE WILL USE Constant Gravity because downward accel doesn't work with platformers (Unused rn)
    [SerializeField] float constantGravity = 9.81f;
    [SerializeField] float maxSpeed;
    [SerializeField] float accel = 5f;
    [SerializeField] float decel = 5f;
    [SerializeField] float currentSpeed;
    [SerializeField] float accelMultiplier;
    [SerializeField] float decelMultiplier;
    float baseAccel;
    float baseDecel;


    private void Start()
    {
        baseAccel = accel;
        baseDecel = decel;
        input = GameObject.FindGameObjectWithTag("PlayerInput").GetComponent<PInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        PlayerMove();
    }


    //Doing movement based on a fixed rate but shotgun is addforce
    int previousInput = 0;
    private void PlayerMove() 
    {
        //Easy naming convention
        int hInput = input.horizontalInput;

        //if player is not going past top speed in either direction
        if (Mathf.Abs(currentSpeed) < maxSpeed)
        {
            //If same input, increase accel
            if (previousInput == hInput && hInput != 0)
            {
                accel += accelMultiplier * Time.deltaTime;
            }
            else
            {
                accel = baseAccel;
            }
            //Get speed that is not player input
            float xSpeed = rb.velocity.x - currentSpeed;

            //increase the speed based on accel and stop increase if it goes beyond
            currentSpeed += hInput * accel * Time.deltaTime;
            if (currentSpeed > maxSpeed) 
            {
                currentSpeed = maxSpeed;
            }
            else if(currentSpeed < -maxSpeed) 
            {
                currentSpeed = -maxSpeed;
            }

            //add player's current input speed to the player
            xSpeed += currentSpeed;
            rb.velocity = new Vector2(xSpeed, rb.velocity.y);
        }


        //Will Add if Grounded Later
        //This is a prototype of hardcoded friction
        if (hInput == 0) 
        {
            if(previousInput == 0) 
            {
                decel += Time.deltaTime * decelMultiplier;
            }
            else 
            {
                decel = baseDecel;
            }
            if(currentSpeed < -0.01f) 
            {
                currentSpeed += decel * Time.deltaTime;
                rb.velocity = new Vector2(rb.velocity.x + (decel * Time.deltaTime), rb.velocity.y);
            }
            else if(currentSpeed > 0.01f) 
            {
                currentSpeed -= decel * Time.deltaTime;
                rb.velocity = new Vector2(rb.velocity.x - (decel * Time.deltaTime), rb.velocity.y);
            }
            else 
            {
                currentSpeed = 0;
                if (rb.velocity.x < 0.05f)
                    rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        //Set the previous input
        previousInput = hInput;

    }

}
