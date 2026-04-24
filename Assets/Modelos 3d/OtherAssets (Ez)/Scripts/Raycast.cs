using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Raycast : MonoBehaviour
{
    [Header("Interacción")]
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private Transform cameraTransform;

    [Header("Interfaz (HUD) - Objetos")]
    [SerializeField] private GameObject crosshairDotObject;
    [SerializeField] private GameObject defaultCrosshairHandObject;
    [SerializeField] private GameObject interactCrosshairClosedHandObject;
    [SerializeField] private GameObject interactTextObject;

    [Header("Interfaz (HUD) - Texto a Modificar")]
    [SerializeField] private TextMeshProUGUI interactTextComponent;

    [Header("Configuración de Interacción Visual")]
    [SerializeField] private float interactionVisualDuration = 0.5f;

    private bool isInteracting = false;

    void Update()
    {
        if (!isInteracting)
        {
            CheckForInteractables();
        }


    }

    private void CheckForInteractables()
    {
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactRange))
        {
         
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
      
                interactTextComponent.text = interactable.GetInteractText();

                SetCrosshairState(false, true, false, true);
                return;
            }
        }

        SetCrosshairState(true, false, false, false);
    }

    private void TryInteract()
    {
        if (isInteracting) return;

        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                StartCoroutine(HandleInteractingVisuals());

              
                interactable.Interact();
            }
        }
    }

    private IEnumerator HandleInteractingVisuals()
    {
        isInteracting = true;
        SetCrosshairState(false, false, true, true);
        yield return new WaitForSeconds(interactionVisualDuration);
        isInteracting = false;
    }

    private void SetCrosshairState(bool dot, bool defaultHand, bool interactHand, bool text)
    {
        if (crosshairDotObject != null) crosshairDotObject.SetActive(dot);
        if (defaultCrosshairHandObject != null) defaultCrosshairHandObject.SetActive(defaultHand);
        if (interactCrosshairClosedHandObject != null) interactCrosshairClosedHandObject.SetActive(interactHand);
        if (interactTextObject != null) interactTextObject.SetActive(text);
    }
  
    public void OnInteractAction(InputAction.CallbackContext context)
    {
        // "context.started" significa que reacciona justo al presionar el btn
        if (context.started)
        {
            TryInteract();
        }
    }
}

