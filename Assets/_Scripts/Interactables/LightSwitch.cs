using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class LightSwitch : InteractableObject
{
    [Header("Configuración de Luz")]
    [SerializeField] private Light[] targetLights;
    [SerializeField] private bool isOn = false;

    [Header("Animación de la Tecla")]
    [SerializeField] private Transform switchModel;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float flipAngle = 30f;
    [SerializeField] private float flipSpeed = 15f;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private AudioClip turnOffSound;
    [Tooltip("Sonido de cortocircuito para el parpadeo (Para tu parcial)")]
    [SerializeField] private AudioClip sparkSound;

    private Quaternion offRotation;
    private Quaternion onRotation;

    private bool isSabotaged = false;

    void Start()
    {
        interactText = isOn ? "\"Apagar Luz [E]\"" : "\"Prender Luz [E]\"";

       
        ApplyLightState();

        if (switchModel != null)
        {
            offRotation = switchModel.localRotation;
            onRotation = switchModel.localRotation * Quaternion.Euler(rotationAxis * flipAngle);
            switchModel.localRotation = isOn ? onRotation : offRotation;
        }
    }

    public bool GetIsOn()
    {
        return isOn;
    }

    public override void Interact()
    {
        // si el bicho la apago no podemos prender.
        if (isSabotaged)
        {
            if (GameManager.Instance != null) GameManager.Instance.ShowSubtitle("<i>No funciona... parece que corto la corriente.</i>", 2f);
            return;
        }

        isOn = !isOn;
        interactText = isOn ? "\"Apagar Luz [E]\"" : "\"Prender Luz [E]\"";

        StopAllCoroutines();
        ApplyLightState();

        if (audioSource != null)
        {
            if (isOn && turnOnSound != null) audioSource.PlayOneShot(turnOnSound);
            else if (!isOn && turnOffSound != null) audioSource.PlayOneShot(turnOffSound);
        }

        if (switchModel != null)
        {
            Quaternion targetRotation = isOn ? onRotation : offRotation;
            StartCoroutine(AnimateSwitch(targetRotation));
        }
    }

    // funcion nueva de luces.
    public void SabotageByEnemy(float disableDuration)
    {
        // si esta apagada se ignora
        if (isSabotaged || !isOn) return;

        StartCoroutine(SabotageRoutine(disableDuration));
    }

    private IEnumerator SabotageRoutine(float duration)
    {
        isSabotaged = true;
        interactText = ""; 

       
        int flickers = Random.Range(3, 7);
        for (int i = 0; i < flickers; i++)
        {
            isOn = !isOn;
            ApplyLightState();

            if (audioSource != null && sparkSound != null) audioSource.PlayOneShot(sparkSound, 0.4f);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        
        isOn = false;
        ApplyLightState();

        if (switchModel != null) StartCoroutine(AnimateSwitch(offRotation));
        if (audioSource != null && turnOffSound != null) audioSource.PlayOneShot(turnOffSound);

      
        yield return new WaitForSeconds(duration);

        
        isSabotaged = false;
        interactText = "\"Prender Luz [E]\"";
    }

    private void ApplyLightState()
    {
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.enabled = isOn;
                LensFlareComponentSRP lensFlare = light.GetComponent<LensFlareComponentSRP>();
                if (lensFlare != null) lensFlare.enabled = isOn;
            }
        }
    }

    private IEnumerator AnimateSwitch(Quaternion targetRot)
    {
        while (Quaternion.Angle(switchModel.localRotation, targetRot) > 0.1f)
        {
            switchModel.localRotation = Quaternion.Slerp(switchModel.localRotation, targetRot, Time.deltaTime * flipSpeed);
            yield return null;
        }
        switchModel.localRotation = targetRot;
    }
}