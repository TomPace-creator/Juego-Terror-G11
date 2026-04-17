using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroCinematic : MonoBehaviour
{
    [Header("Referencias de la Escena")]
    [SerializeField] private Controller playerController;
    [SerializeField] private PlayerFootsteps playerFootsteps; // <-- El script de los pasos
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private Transform clockTarget;
    [SerializeField] private Image blackScreen;
    [SerializeField] private Transform sleepPosition;
    [SerializeField] private LightSwitch bedsideLamp;
    [SerializeField] private AudioSource kitchenNoiseSource;
    [SerializeField] private AudioClip spookySound;

    [Header("Tiempos de la Cinemática")]
    [SerializeField] private float sleepTime = 2f;
    [SerializeField] private float wakeUpSpeed = 1f;
    [SerializeField] private float ceilingStareTime = 2f;
    [SerializeField] private float lookAtClockSpeed = 2f;
    [SerializeField] private float clockStareTime = 3f;
    [SerializeField] private float standUpSpeed = 1.5f;

    private Vector3 defaultStandPos;
    private Quaternion defaultStandRot;

    private void Start()
    {
        // 1. Apagamos el cerebro de movimiento
        if (playerController != null) playerController.enabled = false;

        // 2. Apagamos el script de pasos
        if (playerFootsteps != null)
        {
            playerFootsteps.enabled = false;
        }
        else
        {
           
            Debug.LogError("error");
        }

        playerController.DisableMovement();
        playerController.DisableLook();

        defaultStandPos = cameraContainer.localPosition;
        defaultStandRot = cameraContainer.localRotation;

        cameraContainer.position = sleepPosition.position;
        cameraContainer.rotation = sleepPosition.rotation;

        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        if (kitchenNoiseSource != null && spookySound != null)
        {
            kitchenNoiseSource.PlayOneShot(spookySound);
        }

        yield return new WaitForSeconds(sleepTime);
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * wakeUpSpeed;
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(ceilingStareTime);

        Quaternion startRotation = cameraContainer.rotation;
        Vector3 directionToClock = clockTarget.position - cameraContainer.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToClock);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * lookAtClockSpeed;
            cameraContainer.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.4f);
        if (bedsideLamp != null && !bedsideLamp.GetIsOn())
        {
            bedsideLamp.Interact();
        }

        yield return new WaitForSeconds(clockStareTime);

        Vector3 currentBedPos = cameraContainer.localPosition;
        Quaternion currentBedRot = cameraContainer.localRotation;

        float standT = 0;
        while (standT < 1)
        {
            standT += Time.deltaTime * standUpSpeed;
            cameraContainer.localPosition = Vector3.Lerp(currentBedPos, defaultStandPos, standT);
            cameraContainer.localRotation = Quaternion.Slerp(currentBedRot, defaultStandRot, standT);
            yield return null;
        }

        // --- RECUPERAR EL CONTROL ---
        if (playerController != null) playerController.enabled = true;
        if (playerFootsteps != null) playerFootsteps.enabled = true;

        playerController.EnableMovement();
        playerController.EnableLook();

        // --- DIÁLOGOS Y MISIÓN ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowSubtitle("<i>Ruth - ¿Qué fue eso?...</i>", 4f);
            yield return new WaitForSeconds(4f);

            GameManager.Instance.ShowSubtitle("<i>Parece que vino desde afuera...</i>", 4f);
            yield return new WaitForSeconds(4f);

            GameManager.Instance.ShowSubtitle("<i>No veo muy bien sin mis lentes...</i>", 4f);
            yield return new WaitForSeconds(4f);

            GameManager.Instance.UpdateMission("Ponte lentes", "Agarra los lentes en la mesita de luz");
        }

        Destroy(gameObject);
    }
}