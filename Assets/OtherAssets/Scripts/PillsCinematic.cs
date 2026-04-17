using UnityEngine;
using System.Collections;

public class PillsCinematic : InteractableObject
{
    [Header("Referencias del Jugador")]
    [SerializeField] private Controller playerController;
    [SerializeField] private PlayerFootsteps playerFootsteps;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerTransform;
    [Tooltip("La cámara del jugador, para obligarla a mirar al frente")]
    [SerializeField] private Transform cameraContainer; // <-- ¡VOLVIÓ LA CÁMARA!

    [Header("Referencias del Terror")]
    [SerializeField] private GameObject scaryEntity;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip drinkWaterSound;
    [SerializeField] private AudioClip jumpscareSound;

    private void Start()
    {
        interactText = "\"Tomar Pastillas [E]\"";

        if (scaryEntity != null)
        {
            scaryEntity.SetActive(false);
        }
    }

    public override void Interact()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        StartCoroutine(CinematicRoutine());
    }

    private IEnumerator CinematicRoutine()
    {
        // --- FASE 1: CONGELAR ---
        if (characterController != null) characterController.enabled = false;
        if (playerController != null) playerController.enabled = false;
        if (playerFootsteps != null) playerFootsteps.enabled = false;

        if (playerController != null)
        {
            playerController.DisableMovement();
            playerController.DisableLook();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateMission("", "");
            GameManager.Instance.ShowSubtitle("<i>*Tragas las pastillas con un poco de agua*</i>", 3f);
        }

        // --- FASE 2: 
        Quaternion initialCamRot = cameraContainer.localRotation;
        Quaternion lookDownRot = Quaternion.Euler(-40f, 0, 0); 

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            cameraContainer.localRotation = Quaternion.Slerp(initialCamRot, lookDownRot, t);
            yield return null;
        }

        if (audioSource != null && drinkWaterSound != null)
        {
            audioSource.PlayOneShot(drinkWaterSound);
        }

        yield return new WaitForSeconds(1.2f);

        // --- FASE 3: EL SUSTO (GIRO Y CENTRADO DE CÁMARA) ---
        if (scaryEntity != null) scaryEntity.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        Vector3 lockedPosition = playerTransform.position;

        // Rotación del cuerpo (180 grados)
        Quaternion playerStartRot = playerTransform.rotation;
        Quaternion playerTargetRot = playerStartRot * Quaternion.Euler(0, 180f, 0);

        // Rotación de la cabeza (0 grados, mirando perfectamente recto)
        Quaternion cameraStartRot = cameraContainer.localRotation;
        Quaternion cameraTargetRot = Quaternion.Euler(0f, 0f, 0f);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;

            // 1. Gira el cuerpo
            playerTransform.rotation = Quaternion.Slerp(playerStartRot, playerTargetRot, t);
            playerTransform.position = lockedPosition;

            // 2. Levanta la cabeza para mirar a los ojos del monstruo
            cameraContainer.localRotation = Quaternion.Slerp(cameraStartRot, cameraTargetRot, t);

            yield return null;
        }

        // Aseguramos que quede perfectamente centrado
        playerTransform.position = lockedPosition;
        cameraContainer.localRotation = cameraTargetRot;

        if (audioSource != null && jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        yield return new WaitForSeconds(1.5f);

        // --- FASE 4: DESAPARICIÓN ---
        if (scaryEntity != null) scaryEntity.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowSubtitle("<i>¡¿Qué demonios fue eso?!</i>", 3f);
        }

        yield return new WaitForSeconds(2f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateMission("> Sobrevive hasta el amanecer", "Escapa o escóndete en el apartamento");
        }

        // --- FASE 5: DEVOLVER EL CONTROL ---
        if (characterController != null) characterController.enabled = true;
        if (playerController != null) playerController.enabled = true;
        if (playerFootsteps != null) playerFootsteps.enabled = true;

        if (playerController != null)
        {
            playerController.EnableMovement();

            // ¡EL ARREGLO ESTÁ AQUÍ! Sincronizamos la memoria antes de dejarlo mirar
            playerController.SincronizarRotacionCamara();

            playerController.EnableLook();
        }

        Destroy(gameObject);
    }
}