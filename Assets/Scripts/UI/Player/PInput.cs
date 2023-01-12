using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PInput : MonoBehaviour
{
    public int horizontalInput;
    public int vercticalInput;
    public bool pressingSwitch;
    public bool pressingGrapple;
    private GrappleRadius grappleRadius;
    [SerializeField] GameObject player;

    bool notAttached;
    private void Start()
    {
        //If you forgot its tag
        if (!gameObject.CompareTag("PlayerInput"))
            Debug.LogError("Change " + gameObject.name + "'s tag to PlayerInput");

        //Just checks if you forgot player input component
        if (GetComponent<PlayerInput>() == null) 
        {
            Debug.LogError("Attach Input To " + gameObject.name);
            notAttached = true;
            return;
        }

        grappleRadius = player.GetComponent<GrappleRadius>();
    }


    public void Move(InputAction.CallbackContext value) 
    {
        float input = value.ReadValue<float>();
        horizontalInput = Mathf.RoundToInt(input);
    }

    public void Grapple(InputAction.CallbackContext context) 
    {
        if (context.started) 
        {
            Debug.Log("aa");
            grappleRadius.FindNodes();
        } 
    }

    public void Switch() 
    {
        //Switch Gun Here
    }

    public void Jump()
    {
        //Jump Here
    }


}
