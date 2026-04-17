using UnityEngine;
using System.Collections;

public class PillsCinematic : InteractableObject
{
    [Header("Referencias del Jugador")]
    [SerializeField] private Controller playerController;
    [SerializeField] private PlayerFootsteps playerFootsteps;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraContainer;
    [Tooltip("El script PlayerSanity del jugador para curarlo")]
    [SerializeField] private PlayerSanity playerSanity;

    [Header("Referencias del Terror")]
    [SerializeField] private GameObject scaryEntity;

    [Header("Configuración de Curación")]
    [Tooltip("Cuánta cordura recupera este frasco especial")]
    [SerializeField] private float sanityToRestore = 30f;
    [Tooltip("Cuántos segundos tarda en hacer efecto gradualmente")]
    [SerializeField] private float restoreDuration = 10f; 

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

        // --- FASE 2: SUBIR LA CABEZA Y CURARSE ---
        Quaternion initialCamRot = cameraContainer.localRotation;
        Quaternion lookUpRot = Quaternion.Euler(-40f, 0, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            cameraContainer.localRotation = Quaternion.Slerp(initialCamRot, lookUpRot, t);
            yield return null;
        }

        if (audioSource != null && drinkWaterSound != null)
        {
            audioSource.PlayOneShot(drinkWaterSound);
        }

        // ¡AQUÍ TE CURAS! Usamos las variables que ahora están en el Inspector
        if (playerSanity != null)
        {
            playerSanity.RestoreSanityGradual(sanityToRestore, restoreDuration);
        }

        yield return new WaitForSeconds(1.2f);

        // --- FASE 3: EL SUSTO (GIRO Y CENTRADO DE CÁMARA) ---
        if (scaryEntity != null) scaryEntity.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        Vector3 lockedPosition = playerTransform.position;
        Quaternion playerStartRot = playerTransform.rotation;
        Quaternion playerTargetRot = playerStartRot * Quaternion.Euler(0, 180f, 0);
        Quaternion cameraStartRot = cameraContainer.localRotation;
        Quaternion cameraTargetRot = Quaternion.Euler(0f, 0f, 0f);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;

            playerTransform.rotation = Quaternion.Slerp(playerStartRot, playerTargetRot, t);
            playerTransform.position = lockedPosition;
            cameraContainer.localRotation = Quaternion.Slerp(cameraStartRot, cameraTargetRot, t);

            yield return null;
        }

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
            GameManager.Instance.UpdateMission("Sobrevive la noche", "Escapa o escóndete por la casa algo no esta bien");
        }

        // --- FASE 5: DEVOLVER EL CONTROL ---
        if (characterController != null) characterController.enabled = true;
        if (playerController != null) playerController.enabled = true;
        if (playerFootsteps != null) playerFootsteps.enabled = true;

        if (playerController != null)
        {
            playerController.EnableMovement();
            playerController.SincronizarRotacionCamara();
            playerController.EnableLook();
        }

        Destroy(gameObject);
    }
}