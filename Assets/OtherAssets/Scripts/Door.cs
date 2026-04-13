using UnityEngine;
using System.Collections;

// hereda de InteractableObject
public class Door : InteractableObject
{
    [Header("Door Config")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;

    [Header("Puerta Doble (Opcional)")]
    [SerializeField] private Door twinDoor;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool isOpen = false;
    private Quaternion closedRotation;

    void Start()
    {
        closedRotation = transform.rotation;

        // Asignamos el texto inicial
        interactText = isOpen ? "\"Cerrar Puerta [E]\"" : "\"Abrir Puerta [E]\"";
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

   
    public override void Interact()
    {
        
        Transform playerTransform = Camera.main.transform;

       
        ToggleDoor(playerTransform, false);
    }

  
    public void ToggleDoor(Transform playerTransform, bool isTriggeredByTwin = false)
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
            Vector3 localPlayerPosition = transform.InverseTransformPoint(playerTransform.position);
            float angleToApply = (localPlayerPosition.z > 0) ? -openAngle : openAngle;

            Quaternion openRotation = Quaternion.Euler(closedRotation.eulerAngles + Vector3.up * angleToApply);
            StartCoroutine(AnimateDoor(openRotation));
        }
        else
        {
            StartCoroutine(AnimateDoor(closedRotation));
        }

        if (twinDoor != null && !isTriggeredByTwin)
        {
            twinDoor.ToggleDoor(playerTransform, true);
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