using UnityEngine;
using System.Collections;

public class CabinetDoor : InteractableObject
{
    [Header("configuracion alacena")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 3f;

    [Header("puerta doble")]
    [SerializeField] private CabinetDoor twinDoor;

    [Header("luz interna")]
    [Tooltip("luz que prende al abrir la puerta")]
    [SerializeField] private Light internalLight;

    [Header("audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool isOpen = false;
    private Quaternion closedRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        interactText = isOpen ? "\"Cerrar Puerta [E]\"" : "\"Abrir Puerta [E]\"";

        // apago luz si arranca cerrada
        if (internalLight != null)
        {
            internalLight.enabled = isOpen;
        }
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    public override void Interact()
    {
        ToggleDoor(false);
    }

    public void ToggleDoor(bool isTriggeredByTwin = false)
    {
        isOpen = !isOpen;

        interactText = isOpen ? "\"Cerrar Puerta [E]\"" : "\"Abrir Puerta [E]\"";

        StopAllCoroutines();

        // audio
        if (audioSource != null)
        {
            if (isOpen && openSound != null) audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null) audioSource.PlayOneShot(closeSound);
        }

        // luz interna
        if (internalLight != null)
        {
            internalLight.enabled = isOpen;
        }

        // animacion
        if (isOpen)
        {
            Quaternion openRotation = Quaternion.Euler(closedRotation.eulerAngles + Vector3.up * openAngle);
            StartCoroutine(AnimateDoor(openRotation));
        }
        else
        {
            StartCoroutine(AnimateDoor(closedRotation));
        }

        // abre la otra puerta si es doble
        if (twinDoor != null && !isTriggeredByTwin)
        {
            twinDoor.ToggleDoor(true);
        }
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }
}