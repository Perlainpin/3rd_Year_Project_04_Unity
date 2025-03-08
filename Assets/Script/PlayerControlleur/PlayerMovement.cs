using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;
    public float vaultSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float dashSpeed;
    public float dashSpeedChangeFactor;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Input")]
    private PlayerInput action;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crounchAction;
    private InputAction reloadAction;
    private InputAction fireAction;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Camera Effects")]
    public PlayerCam cam;
    public float grappleFov = 95f;

    [Header("References")]
    public Climbing climbingScript;
    
    public GameObject gunSpawn;
    private GameObject ActiveGun;
    private bool gunInHand = true;
    private Animator GunAnimator;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection; 

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        sprinting,
        wallrunning,
        climbing,
        vaulting,
        dashing,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool climbing;
    public bool vaulting;
    public bool dashing;

    public bool activeGrappel;

    public bool freeze;
    public bool unlimited;

    public bool restricted;
    private void Awake()
    {
        action = new PlayerInput();
    }

    private void OnEnable()
    {

        moveAction = action.Player.Move;
        lookAction = action.Player.Look;
        jumpAction = action.Player.Jump;
        sprintAction = action.Player.Sprint;
        crounchAction = action.Player.Crounch;
        reloadAction = action.Player.Reload;
        fireAction = action.Player.Fire;

        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        crounchAction.Enable();
        reloadAction.Enable();
        fireAction.Enable();

       jumpAction.performed+= Jump;
        crounchAction.performed += Crounch;

    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crounchAction.Disable();
        reloadAction.Disable();
        fireAction.Disable();

        jumpAction.performed -= Jump;
        crounchAction.performed -= Crounch;
    }


    private void Start()
    {
        climbingScript = GetComponent<Climbing>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
        
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 1f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded && !activeGrappel)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        Vector3 lookDirection = orientation.forward;
        lookDirection.y = 0;
        if(lookDirection != Vector3.zero)
        {
            transform.forward = lookDirection;
        }

        if (gunSpawn.transform.childCount != 0)
        {
            ActiveGun = gunSpawn.transform.GetChild(0).gameObject;
            GunAnimator = ActiveGun.GetComponent<Animator>();
        }
    }

    private void MyInput()
    { 
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

    }


    private void FixedUpdate()
    {
        MovePlayer();

        if(gunSpawn.transform.childCount != 0 && ActiveGun != null)
        {
            if (fireAction.ReadValue<float>() > 0)
            {

                if (ActiveGun.GetComponent<Firing>().readyToShoot && !ActiveGun.GetComponent<Firing>().reloading && ActiveGun.GetComponent<Firing>().bulletsLeft <= 0)
                {
                    ActiveGun.GetComponent<Firing>().Reload();
                }

                if (ActiveGun.GetComponent<Firing>().readyToShoot && !ActiveGun.GetComponent<Firing>().reloading && ActiveGun.GetComponent<Firing>().bulletsLeft > 0)
                {
                    ActiveGun.GetComponent<Firing>().bulletsShot = 0;

                    GunAnimator.Play("Shoot");
                    ActiveGun.GetComponent<Firing>().Shoot();
                }
            }
            if(reloadAction.ReadValue<float>() > 0)
            {
                if (ActiveGun.GetComponent<Firing>().bulletsLeft < ActiveGun.GetComponent<Firing>().magazineSize && !ActiveGun.GetComponent<Firing>().reloading)
                    ActiveGun.GetComponent<Firing>().Reload();
            }
        }
    }

    private MovementState lastState;
    bool keepMomentum;
    private void StateHandler()
    {
        //Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }

        //Unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
        }

        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            keepMomentum = true;
        }


        //Vaulting
        else if (vaulting)
        {
            state = MovementState.vaulting;
            desiredMoveSpeed = vaultSpeed;
        }

        //Climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        //Wallrunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        //Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }

            else
                desiredMoveSpeed = sprintSpeed;
        }

        //Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        //Sprinting
        else if (grounded && sprintAction.ReadValue<float>() > 0)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        //Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        //Air
        else
        {
            state = MovementState.air;

            if (moveSpeed < sprintSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;


        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

        // deactivate keepMomentum
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }


    private void MovePlayer()
    {
        if (climbingScript.exitingWall) return;
        if (restricted) return;
        if (activeGrappel)return;

        if (state == MovementState.dashing) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrappel) return; 

        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
        exitingSlope = true;

        if (grounded && readyToJump)
        {
            //rb.velocity = new Vector3(rb.velocity.y, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            readyToJump = false;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrappel = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestriction), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
        cam.DoFov(grappleFov);

    }

    public void ResetRestriction(){ 
        activeGrappel = false;
        cam.DoFov(85f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestriction();
            GetComponent<Grappling>().StopGrapple();
        }
    }


    private void Crounch(InputAction.CallbackContext context)
    { 
        if (transform.localScale.y == startYScale) 
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
        }
        else 
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouching = false;
        }
    }


    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 1f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up,slopeHit.normal);
            return angle < maxSlopeAngle&& angle != 0;  
        }

        return false;

    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }


    public Vector3 CalculateJumpVelocity(Vector3 startPoint , Vector3 endPoint ,float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }



    private void OnSwitch(InputAction.CallbackContext context)
    {
        gunInHand = !gunInHand;
    }


}
