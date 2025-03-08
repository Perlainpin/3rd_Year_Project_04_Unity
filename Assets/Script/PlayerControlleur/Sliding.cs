using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;


    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    private PlayerInput action;
    private InputAction slideAction;
    private InputAction moveAction;

    float horizontalInput;
    float verticalInput;




    private void Awake()
    {
        action = new PlayerInput();

    }

    private void OnEnable()
    {
        slideAction = action.Player.Slide;
        moveAction = action.Player.Move;

        slideAction.Enable();
        moveAction.Enable();
    }

    private void OnDisable()
    {
        slideAction.Disable();
        moveAction.Disable();
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }


    private void Update()
    {
        // Lire l'entrée pour le mouvement (les valeurs sont prises à chaque frame)
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (slideAction.triggered && !pm.sliding)
        {
            StartSlide();
        }

        if (slideAction.ReadValue<float>() == 0 && pm.sliding)
        {
            StopSlide();
        }
    }


    private void FixedUpdate()
    {
        if (pm.sliding)
        {
            SlidingMovement();
        }
    }


    private void StartSlide()
    {
        if (pm.wallrunning) return;

        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

}
