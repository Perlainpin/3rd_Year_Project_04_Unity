using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem.Users;

public class PlayerCam : MonoBehaviour
{
    public float sensMouseX = 10f;
    public float sensMouseY = 10f;

    public float sensGamepadX = 100f;
    public float sensGamepadY = 100f;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    private PlayerInput actions;
    private InputAction lookAction;

    public Image damageImage;

    private void Awake()
    {
        actions = new PlayerInput();

    }

    private void OnEnable()
    {
        lookAction = actions.Player.Look;
        lookAction.Enable();
    }

    private void OnDisable()
    {
        lookAction.Disable();
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (damageImage != null)
        {
            damageImage.color = new Color(1f, 0f, 0f, 0f); // Couleur rouge, transparence à 0 (invisible)
        }
    }

    private void Update()
    {
        Vector2 looInput = lookAction.ReadValue<Vector2>();
        float sensX = sensMouseX, sensY = sensMouseY;
        if(Gamepad.all.Count > 0)
        {
            if (Gamepad.current.IsActuated())
            {
                sensX = sensGamepadX;
                sensY = sensGamepadY;
            }
            else
            {
                sensX = sensMouseX;
                sensY = sensMouseY;
            }
        }
        else
        {
            sensX = sensMouseX;
            sensY = sensMouseY;
        }

        float mouseX = looInput.x * Time.deltaTime * sensX;
        float mouseY = looInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    public void ShowDamageEffect()
    {
        if (damageImage != null)
        {
            // Rendre l'image rouge (clignotement rapide)
            damageImage.color = new Color(1f, 0f, 0f, 0.5f); // Couleur rouge, opacité à 0.5 pour voir l'effet
            DOTween.To(() => damageImage.color.a, x => damageImage.color = new Color(1f, 0f, 0f, x), 0f, 0.3f); // L'effet disparaît après 0.3 secondes
        }
    }
}
