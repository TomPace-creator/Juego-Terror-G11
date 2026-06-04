using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // NUEVO: libreria para eventos

public class DigitalClock : MonoBehaviour
{
    [Header("Referencias del HUD")]
    [SerializeField] private TextMeshProUGUI clockText;

    [Header("Configuración Inicial (Inicio)")]
    [SerializeField] private int startHour = 3;
    [SerializeField] private int startMinute = 33;

    [Header("Configuración Final (Amanecer)")]
    [SerializeField] private int endHour = 6;
    [SerializeField] private int endMinute = 0;

    [Header("Duración Real")]
    [Tooltip("¿Cuántos minutos reales debe durar la partida entera?")]
    [SerializeField] private float realLifeDurationMinutes = 10f;

    [Header("Eventos de Tiempo")]
    [SerializeField] private UnityEvent onFiveAM; // NUEVO: evento que se dispara a las 5
    private bool fiveAMTriggered = false; // NUEVO: control para que no se dispare mil veces

    private float currentInGameMinutes;
    private float targetInGameMinutes;
    private float timeMultiplier;
    private bool gameEnded = false;

    private void Start()
    {
        currentInGameMinutes = (startHour * 60) + startMinute;
        targetInGameMinutes = (endHour * 60) + endMinute;

        float inGameMinutesToPass = targetInGameMinutes - currentInGameMinutes;
        float totalRealSeconds = realLifeDurationMinutes * 60f;

        timeMultiplier = inGameMinutesToPass / totalRealSeconds;

        UpdateClockHUD();
    }

    private void Update()
    {
        if (gameEnded) return;

        currentInGameMinutes += Time.deltaTime * timeMultiplier;

        if (currentInGameMinutes >= targetInGameMinutes)
        {
            TriggerVictory();
        }

        UpdateClockHUD();
    }

    private void UpdateClockHUD()
    {
        if (clockText == null) return;

        int displayHours = Mathf.FloorToInt(currentInGameMinutes / 60f);
        int displayMins = Mathf.FloorToInt(currentInGameMinutes % 60f);

        // NUEVO: si son las 5 am y todavia no se disparo el evento, lo dispara
        if (displayHours == 5 && !fiveAMTriggered)
        {
            fiveAMTriggered = true;
            if (onFiveAM != null) onFiveAM.Invoke();
        }

        clockText.text = string.Format("{0:00}:{1:00} AM", displayHours, displayMins);
    }

    private void TriggerVictory()
    {
        gameEnded = true;

        currentInGameMinutes = targetInGameMinutes;
        UpdateClockHUD();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Win");
    }
}