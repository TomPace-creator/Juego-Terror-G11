using UnityEngine;
using System.Collections;

public class CabinetDoor : InteractableObject
{
    [Header("Configuración de Alacena")]
   
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 3f;

    [Header("Puerta Doble (Opcional)")]
    [SerializeField] private CabinetDoor twinDoor;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool isOpen = false;
    private Quaternion closedRotation;

    void Start()
    {
        closedRotation = transform.rotation;

        
        interactText = isOpen ? "\"Cerrar Puerta [E]\"" : "\"Abrir Puerta [E]\"";
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

        if (audioSource != null)
        {
            if (isOpen && openSound != null) audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null) audioSource.PlayOneShot(closeSound);
        }

        if (isOpen)
        {
           
            Quaternion openRotation = Quaternion.Euler(closedRotation.eulerAngles + Vector3.up * openAngle);
            StartCoroutine(AnimateDoor(openRotation));
        }
        else
        {
            StartCoroutine(AnimateDoor(closedRotation));
        }

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