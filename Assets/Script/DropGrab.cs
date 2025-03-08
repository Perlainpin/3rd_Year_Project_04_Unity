using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

public class DropAndGrab : MonoBehaviour
{
    //public Firing gun;
    public Rigidbody rb;
    public BoxCollider coll;

    public Transform player,spawnGun;

    public Camera fpsCam;

    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    private static bool slotFull;
    private Transform ActiveGun;

    private Transform highlight;
    private RaycastHit raycastHit;

    private PlayerInput actions;
    private InputAction GrabAction;
    private InputAction DropAction;

    public AudioClip DropSound;
    public AudioClip GrabSound;

    private void Awake()
    {
        actions = new PlayerInput();
    }

    private void OnEnable()
    {
        GrabAction = actions.Player.Grab;
        DropAction = actions.Player.Drop;
        GrabAction.Enable();
        DropAction.Enable();
    }

    private void OnDisable()
    {
        GrabAction.Disable();
        DropAction.Disable();
    }

    void Update()
    {
        if(highlight != null)
        {
            highlight.gameObject.GetComponent<Outline>().enabled = false;
            highlight = null;
        }

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;
            if (highlight.gameObject.tag.Equals("Weapon"))
            {
                highlight.gameObject.GetComponent<Outline>().OutlineColor = Color.red;
                highlight.gameObject.GetComponent<Outline>().OutlineWidth = 7f;
                highlight.gameObject.GetComponent<Outline>().enabled = true;

                targetPoint = raycastHit.point;
                Vector3 distanceToPlayer = targetPoint - player.position;

                if (!slotFull && distanceToPlayer.magnitude <= pickUpRange && GrabAction.ReadValue<float>() > 0) PickUp(highlight);

            }
            else
            {
                highlight = null;
            }
        }
        if (slotFull && DropAction.ReadValue<float>() > 0) Drop();
    }


    private void PickUp(Transform gun)
    {
        GetComponent<AudioSource>().PlayOneShot(GrabSound);

        slotFull = true;
        ActiveGun = gun;

        gun.GetComponent<Rigidbody>().isKinematic = true;
        gun.SetParent(spawnGun);
        
        gun.position = spawnGun.position;
        gun.rotation = new Quaternion(0,0,0,0); 
    }

    private void Drop()
    {
        GetComponent<AudioSource>().PlayOneShot(DropSound); // Joue le son de tir

        slotFull = false;

        ActiveGun.GetComponent<Rigidbody>().isKinematic = false;

        ActiveGun.GetComponent<Rigidbody>().velocity = player.GetComponent<Rigidbody>().velocity;

        ActiveGun.GetComponent<Rigidbody>().AddForce(fpsCam.transform.forward * dropForwardForce,ForceMode.Impulse);
        ActiveGun.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * dropUpwardForce, ForceMode.Impulse);

        float random = UnityEngine.Random.Range(-1f, 1f);
        ActiveGun.GetComponent<Rigidbody>().AddForce(new Vector3(random, random, random) * 10);

        ActiveGun.SetParent(null);

    }
}
