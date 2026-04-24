using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

//herencia de interactable
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

    private Quaternion offRotation;
    private Quaternion onRotation;

    void Start()
    {
        
        interactText = isOn ? "\"Apagar Luz [E]\"" : "\"Prender Luz [E]\"";
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.enabled = isOn;

                LensFlareComponentSRP lensFlare = light.GetComponent<LensFlareComponentSRP>();
                if (lensFlare != null) lensFlare.enabled = isOn;
            }
        }

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
        isOn = !isOn;

        interactText = isOn ? "\"Apagar Luz [E]\"" : "\"Prender Luz[E]\"";

        StopAllCoroutines();

        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.enabled = isOn;

                LensFlareComponentSRP lensFlare = light.GetComponent<LensFlareComponentSRP>();
                if (lensFlare != null) lensFlare.enabled = isOn;
            }
        }

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