using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour {
    // Start is called before the first frame update
    Camera mainCamera;
    PlayerInput playerInput;
    Vector2 shootingDir;

    [Header("Player Parameters")]
    [SerializeField] float walkingAcceleration;
    [SerializeField] float maxWalkingVelocity;
    [Header("Objects for Weapon")]
    [SerializeField] GameObject gunObject;
    [SerializeField] GameObject bulletObject;
    

    public Weapon[] weapons;

    float walkingVelocity;
    float inAirVelocity = 0f;
    float inputValue;
    bool firing;
    bool onGround;
    bool pickupInRange;

    Rigidbody2D playerRigidBody;
    WeaponAmmoUI weaponAmmoUI;



    public int currentWeaponIndex;

    void Start() {
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        playerInput = GetComponent<PlayerInput>();
        shootingDir = new Vector2(1, 0);
        currentWeaponIndex = 0;
        firing = false;
        pickupInRange = false;
        playerRigidBody = GetComponent<Rigidbody2D>();

        Weapon shotgun = new Weapon( 
                "shotgun",//shotgun
                gunObject,
                bulletObject,
                10,//damage
                2,//fire rate
                35,//bullet speed
                100,//recoil
                5,//bullets per shot
                3, //spread
                50, //ammo count
                5, //clip size
                2.0f //reload time
            );

        Weapon machineGun = new Weapon(
                "machinegun",
                gunObject,
                bulletObject,
                1,//damage
                10,//fire rate
                50,//bullet speed
                10,//recoil
                1,//bullets per shot
                0.2f, //spread
                500, //ammo count
                250, //clip size
                3.0f //reaload time
            );
        weapons = new Weapon[] {shotgun, machineGun};

        weaponAmmoUI = GameObject.FindWithTag("WeaponAmmoUI").GetComponent<WeaponAmmoUI>();
        weaponAmmoUI.UpdateAllUI(weapons);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (PlayerState.Instance.currentState == PlayerState.playerState.GRAPPLING)
            return;

        if (firing) {
            StartCoroutine(weapons[currentWeaponIndex].Fire(shootingDir));
            weaponAmmoUI.UpdateUI(weapons[currentWeaponIndex],currentWeaponIndex);
        }
        //HERE problem, in air max velocity simple doesn't work with grappling and overall doesn't make sense
        //Ex: if I barely shoot to the left in air then shotgun shot in the air to the right the speed cap would stop it from reaching its full speed
        walkingVelocity = playerRigidBody.velocity.x - inAirVelocity;//update the speed cap
        playerRigidBody.AddForce(new Vector2(inputValue * walkingAcceleration, 0));//walking force
        playerRigidBody.AddForce(new Vector2(-Mathf.Sign(walkingVelocity)*Mathf.Lerp(0, walkingAcceleration, Mathf.Abs(walkingVelocity) / maxWalkingVelocity), 0) / playerRigidBody.mass);//resistant force
        
        
    }

    void OnLook(InputValue input) {
        Vector2 inputVector;
        if (playerInput.currentControlScheme == "Keyboard&Mouse") {
            Vector2 inputVector2 = mainCamera.ScreenToWorldPoint(input.Get<Vector2>());
            inputVector = new Vector2(inputVector2.x - transform.position.x, inputVector2.y - transform.position.y);
        } else inputVector = input.Get<Vector2>();

        shootingDir = inputVector.normalized;
        weapons[currentWeaponIndex].UpdateRotation(inputVector);

        //instantiate bullet prefab, addforce in the direction of shootingDir

        Debug.DrawRay(transform.position, new Vector3(inputVector.x, inputVector.y, 0), Color.green);

    }
    
    
    private void OnMove(InputValue input) {
        inputValue = input.Get<float>();

    }
    
    void OnFireDown() {
        firing = true;
    }
    void OnFireUp() {
        firing = false;
    }

    void OnWeaponChange() {
        if (currentWeaponIndex == weapons.Length - 1) currentWeaponIndex = 0;
        else currentWeaponIndex++;

        //insert code here to update gun sprite
    }

    void OnReload() {
        if (onGround) {
            StartCoroutine(weapons[currentWeaponIndex].Reload());
            weaponAmmoUI.UpdateUI(weapons[currentWeaponIndex], currentWeaponIndex);
        } else Debug.LogWarning("Can't reload in air");
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.CompareTag("Terrain")) {
            onGround = true;
            inAirVelocity = 0;
        }
        if (collision.collider.CompareTag("DroppedItem")) {
            // show UI element for weapon pickup 

            //enable pickup 
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.collider.CompareTag("Terrain")) {
            onGround = false;
            inAirVelocity = playerRigidBody.velocity.x;
        }
    }
}
