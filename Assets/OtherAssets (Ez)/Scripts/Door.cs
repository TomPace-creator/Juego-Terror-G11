using UnityEngine;
using System.Collections;

public class Door : InteractableObject
{
    [Header("Door Config")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;

    [Header("Sistema de Llaves")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string requiredKey = "Llave_Atico";
    [SerializeField] private string lockedMessage = "<i>Mmm esta cerrada con llave... el atico lleva años cerrado y su llave perdida..</i>";

    [Header("Misión Secundaria al fallar (Fase 1)")]
    [Tooltip("Se activa automáticamente si el jugador intenta abrir sin tener la llave")]
    [SerializeField] private string sideQuestTitle = "[Opcional] La llave del Ático";
    [SerializeField] private string sideQuestDetails = "Encuentra la vieja llave perdida por la casa.";

    [Header("Borrar Misión al Abrir")]
    [Tooltip("Título de la misión que se completará al abrir la puerta con éxito")]
    [SerializeField] private string questToClearOnUnlock = "[Opcional] EL ATICO";

    [Header("Puerta Doble (Opcional)")]
    [SerializeField] private Door twinDoor;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip unlockSound;

    public bool isOpen { get; private set; } = false;
    private Quaternion closedRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        interactText = isOpen ? "\"Cerrar Puerta [E]\"" : "\"Abrir Puerta [E]\"";
    }

    public override void Interact()
    {
        Transform playerTransform = Camera.main.transform;
        ToggleDoor(playerTransform, false);
    }

    public void ToggleDoor(Transform playerTransform, bool isTriggeredByTwin = false)
    {
        if (isLocked && !isTriggeredByTwin)
        {
            if (GameManager.Instance != null && GameManager.Instance.HasItem(requiredKey))
            {
                isLocked = false;
                if (audioSource != null && unlockSound != null) audioSource.PlayOneShot(unlockSound);
                GameManager.Instance.ShowSubtitle("<i>*Click* Puerta desbloqueada.</i>", 3f);

                // ¡EL CAMBIO ESTÁ ACÁ! 
                // Borramos la misión de "Fase 2" usando el nuevo título.
                if (!string.IsNullOrEmpty(questToClearOnUnlock))
                {
                    GameManager.Instance.UpdateSecondaryMission(questToClearOnUnlock, "");
                }
            }
            else
            {
                if (audioSource != null && lockedSound != null) audioSource.PlayOneShot(lockedSound);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowSubtitle(lockedMessage, 4f);

                    if (!string.IsNullOrEmpty(sideQuestTitle))
                    {
                        GameManager.Instance.UpdateSecondaryMission(sideQuestTitle, sideQuestDetails);
                    }
                }
                return;
            }
        }

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

        if (twinDoor != null && !isTriggeredByTwin) twinDoor.ToggleDoor(playerTransform, true);
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