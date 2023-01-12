using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    private static PlayerState _instance;
    public static PlayerState Instance { get { return _instance; } }
    Rigidbody2D rb;
    float defaultGravity;
    public enum playerState 
    {
        IDLE,
        GRAPPLING
    }

    public playerState currentState;

    void Awake() 
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }


    }

    private void Start()
    {
        currentState = playerState.IDLE;
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
        defaultGravity = rb.gravityScale;
    }

    private void Update()
    {
        switch(currentState) 
        {
            case playerState.IDLE:
                IdleState();
                break;
            case playerState.GRAPPLING:
                GrappleState();
                break;

        }

    }

    void IdleState() 
    {
        rb.gravityScale = defaultGravity;
    }

    void GrappleState() 
    {
        rb.gravityScale = 0;
        //HERE make it so all player movement input is disabled
    }

    //Used for organizational purposes as it finds all references for you
    public void ChangeState(playerState state) 
    {
        currentState = state;
    }
}
