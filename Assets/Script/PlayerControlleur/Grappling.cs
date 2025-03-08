using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;
    public Animator graplingGunAnimator;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;


    private PlayerInput actions;
    private InputAction grappleAction;

    [Header("Cooldown")]
    public float grappleCd;
    private float grappleCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    public bool grappling;

    private void Awake()
    {
        actions = new PlayerInput();
    }

    private void OnEnable()
    {
       grappleAction = actions.Player.Grapple;
        grappleAction.Enable();
    }

    private void OnDisable()
    {
        grappleAction.Disable();
    }



    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (grappleAction.triggered)
        {
            StartGrapple();
        }

        if (grappleCdTimer > 0)
        {
            grappleCdTimer -= Time.deltaTime;
        }
    }

    private void StartGrapple()
    {
        if (grappleCdTimer > 0) return;

        grappling = true;

        pm.freeze = true;

        graplingGunAnimator.SetBool("Pulling", false);
        graplingGunAnimator.SetBool("Throwing", true);

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit , maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
       else 
       {
           grapplePoint = cam.position +cam.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
       }

    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;

        graplingGunAnimator.SetBool("Pulling", true);
        graplingGunAnimator.SetBool("Throwing", false);

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, grapplePoint.z);

        float grapplePointRelativeY = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeY + overshootYAxis;

        if (grapplePointRelativeY < 0)
        {
            highestPointOnArc = overshootYAxis;
        }

        pm.JumpToPosition(grapplePoint,highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        pm.freeze = false;

        grappling = false;

        grappleCdTimer = grappleCd;

        graplingGunAnimator.SetBool("Pulling", false);
        graplingGunAnimator.SetBool("Throwing", false);
    }


    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

}
