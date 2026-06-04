using System.Collections;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    [Header("configuracion de luces")]
    [SerializeField] private Light[] lightningLights;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5000f;
    [SerializeField] private int minFlashes = 1;
    [SerializeField] private int maxFlashes = 3;

    [Header("tiempos de tormenta")]
    [SerializeField] private float minTimeBetweenStorms = 5f;
    [SerializeField] private float maxTimeBetweenStorms = 15f;
    [SerializeField] private float flashDuration = 0.3f; // ahora podes subirlo mas porque va a titilar

    [Header("audio (opcional)")]
    [SerializeField] private AudioSource thunderAudioSource;
    [SerializeField] private AudioClip[] thunderClips;

    void Start()
    {
        ApagarLuces();
        StartCoroutine(StormLoop());
    }

    private IEnumerator StormLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenStorms, maxTimeBetweenStorms);
            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(TriggerLightning());
        }
    }

    private IEnumerator TriggerLightning()
    {
        int flashes = Random.Range(minFlashes, maxFlashes);

        for (int i = 0; i < flashes; i++)
        {
            float burstTimer = 0f;

            // en vez de quedar fijo, titila rapido mientras dure el flashDuration
            while (burstTimer < flashDuration)
            {
                // cambia la intensidad de golpe entre el 30% y el 100% del maximo
                float jitterIntensity = Random.Range(maxIntensity * 0.3f, maxIntensity);
                CambiarIntensidadLuces(jitterIntensity);

                // espera un juego de milisegundos y suma al temporizador
                float jitterTime = 0.02f;
                yield return new WaitForSeconds(jitterTime);
                burstTimer += jitterTime;
            }

            // se apaga un instante antes del proximo chispazo de la tanda
            ApagarLuces();
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }

        PlayThunderSound();
    }

    private void CambiarIntensidadLuces(float intensidad)
    {
        foreach (Light luz in lightningLights)
        {
            if (luz != null)
            {
                luz.intensity = intensidad;
            }
        }
    }

    private void ApagarLuces()
    {
        foreach (Light luz in lightningLights)
        {
            if (luz != null)
            {
                luz.intensity = minIntensity;
            }
        }
    }

    private void PlayThunderSound()
    {
        if (thunderAudioSource != null && thunderClips.Length > 0)
        {
            AudioClip randomClip = thunderClips[Random.Range(0, thunderClips.Length)];
            thunderAudioSource.PlayOneShot(randomClip);
        }
    }
}