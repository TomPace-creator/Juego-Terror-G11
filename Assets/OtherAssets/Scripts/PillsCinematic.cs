using UnityEngine;
using System.Collections;

public class PillsCinematic : InteractableObject
{
    [Header("Referencias del Jugador")]
    [SerializeField] private Controller playerController;
    [SerializeField] private PlayerFootsteps playerFootsteps;
    [Tooltip("La cápsula de físicas de Unity para evitar teletransportes")]
    [SerializeField] private CharacterController characterController; // <-- ¡NUEVO!
    [Tooltip("El Transform raíz del jugador (Player Ez) para girarlo 180 grados")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("La cámara para hacer el cabeceo de tomar las pastillas")]
    [SerializeField] private Transform cameraContainer;

    [Header("Referencias del Terror")]
    [Tooltip("El modelo 3D del monstruo que está a tus espaldas")]
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
        // --- FASE 1: CONGELAR AL JUGADOR Y APAGAR FÍSICAS ---

        // APAGAMOS LA CÁPSULA DE FÍSICAS PARA EVITAR EL TELETRANSPORTE
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

        // --- FASE 2: ANIMACIÓN DE TOMAR LAS PASTILLAS ---
        Quaternion startCamRot = cameraContainer.localRotation;
        Quaternion lookDownRot = startCamRot * Quaternion.Euler(40f, 0, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            cameraContainer.localRotation = Quaternion.Slerp(startCamRot, lookDownRot, t);
            yield return null;
        }

        if (audioSource != null && drinkWaterSound != null)
        {
            audioSource.PlayOneShot(drinkWaterSound);
        }

        yield return new WaitForSeconds(1.2f);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2.5f;
            cameraContainer.localRotation = Quaternion.Slerp(lookDownRot, startCamRot, t);
            yield return null;
        }

        // --- FASE 3: EL SUSTO (GIRO Y MONSTRUO) ---
        if (scaryEntity != null) scaryEntity.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // ¡EL TRUCO ANTITELETRANSPORTE! Memorizamos la posición exacta
        Vector3 lockedPosition = playerTransform.position;

        Quaternion playerStartRot = playerTransform.rotation;
        Quaternion playerTargetRot = playerStartRot * Quaternion.Euler(0, 180f, 0);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            playerTransform.rotation = Quaternion.Slerp(playerStartRot, playerTargetRot, t);

            // Clavamos los pies al piso en cada frame del giro
            playerTransform.position = lockedPosition;

            yield return null;
        }

        // Nos aseguramos por última vez antes de prender las físicas
        playerTransform.position = lockedPosition;

        if (audioSource != null && jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        yield return new WaitForSeconds(1.5f);

        // --- FASE 4: DESAPARICIÓN Y NUEVA MISIÓN ---
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

        // --- FASE 5: DEVOLVER EL CONTROL Y PRENDER FÍSICAS ---

        // VOLVEMOS A PRENDER LA CÁPSULA DE FÍSICAS
        if (characterController != null) characterController.enabled = true;

        if (playerController != null) playerController.enabled = true;
        if (playerFootsteps != null) playerFootsteps.enabled = true;
        if (playerController != null)
        {
            playerController.EnableMovement();
            playerController.EnableLook();
        }

        Destroy(gameObject);
    }
}