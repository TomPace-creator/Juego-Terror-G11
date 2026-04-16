using UnityEngine;
using TMPro;

public class DigitalClock : MonoBehaviour
{
    [Header("Referencias del HUD")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private GameObject victoryScreen; // Casillero para tu pantalla de victoria

    [Header("Configuración Inicial (Inicio)")]
    [SerializeField] private int startHour = 3;
    [SerializeField] private int startMinute = 33;

    [Header("Configuración Final (Amanecer)")]
    [SerializeField] private int endHour = 6;
    [SerializeField] private int endMinute = 0;

    [Header("Duración Real")]
    [Tooltip("¿Cuántos minutos reales debe durar la partida entera?")]
    [SerializeField] private float realLifeDurationMinutes = 10f;

    private float currentInGameMinutes;
    private float targetInGameMinutes;
    private float timeMultiplier; // Ahora el script calcula esto solo
    private bool gameEnded = false;

    private void Start()
    {
        // Nos aseguramos de que la victoria empiece invisible
        if (victoryScreen != null) victoryScreen.SetActive(false);

        // Convertimos todo a minutos
        currentInGameMinutes = (startHour * 60) + startMinute;
        targetInGameMinutes = (endHour * 60) + endMinute;

        // Calculamos a qué velocidad tiene que pasar el tiempo del juego para que dure exactamente 10 min reales
        float inGameMinutesToPass = targetInGameMinutes - currentInGameMinutes;
        float totalRealSeconds = realLifeDurationMinutes * 60f;

        timeMultiplier = inGameMinutesToPass / totalRealSeconds;

        UpdateClockHUD();
    }

    private void Update()
    {
        if (gameEnded) return; // Si ya ganamos, el reloj se detiene

        // Hacemos avanzar el tiempo
        currentInGameMinutes += Time.deltaTime * timeMultiplier;

        // Revisamos si ya es el amanecer
        if (currentInGameMinutes >= targetInGameMinutes)
        {
            TriggerVictory();
        }

        UpdateClockHUD();
    }

    private void UpdateClockHUD()
    {
        if (clockText == null) return;

        // Extraemos las horas y minutos exactos
        int displayHours = Mathf.FloorToInt(currentInGameMinutes / 60f);
        int displayMins = Mathf.FloorToInt(currentInGameMinutes % 60f);

        // Formateamos el texto y le agregamos el " AM" al final
        clockText.text = string.Format("{0:00}:{1:00} AM", displayHours, displayMins);
    }

    private void TriggerVictory()
    {
        gameEnded = true;

        // Clavamos el tiempo justo en 06:00 AM para que no se pase a 06:01
        currentInGameMinutes = targetInGameMinutes;
        UpdateClockHUD();

        // cambiar
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
        }

        // Pausamos el mundo y liberamos el mouse
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}