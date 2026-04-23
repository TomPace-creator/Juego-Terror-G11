using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Inventario del Jugador")]
    // AHORA ES PRIVADA: Solo el GameManager puede modificar la lista directamente
    [SerializeField] private List<string> inventory = new List<string>();

    // --- EVENTOS ---
    public event Action<string, string> OnMissionChanged;
    public event Action<string, string> OnSecondaryMissionChanged;
    public event Action<string, string> OnTertiaryMissionChanged; // <-- NUEVO
    public event Action<string, float> OnSubtitleTriggered;
    public event Action OnPillsConsumed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    // --- SISTEMA DE INVENTARIO ---
    public void AddItemToInventory(string itemName)
    {
        inventory.Add(itemName);
    }

    public bool HasItem(string itemName)
    {
        return inventory.Contains(itemName);
    }

    public void RemoveItemFromInventory(string itemName)
    {
        if (inventory.Contains(itemName))
        {
            inventory.Remove(itemName);
        }
    }

    public int GetItemCount(string itemName)
    {
        int count = 0;
        foreach (string item in inventory)
        {
            if (item == itemName) count++;
        }
        return count;
    }

    // --- SISTEMA DE UI Y MISIONES ---
    public void UpdateMission(string title, string details)
    {
        OnMissionChanged?.Invoke(title, details);
        Debug.Log("Misión Principal actualizada: " + title);
    }

    public void UpdateSecondaryMission(string title, string details)
    {
        OnSecondaryMissionChanged?.Invoke(title, details);
        Debug.Log("Misión Secundaria actualizada: " + title);
    }

    public void UpdateTertiaryMission(string title, string details) // <-- NUEVO
    {
        OnTertiaryMissionChanged?.Invoke(title, details);
        Debug.Log("Misión Terciaria actualizada: " + title);
    }

    public void ShowSubtitle(string text, float durationInSeconds)
    {
        OnSubtitleTriggered?.Invoke(text, durationInSeconds);
        Debug.Log("Subtítulo mostrado: " + text);
    }

    // --- SISTEMA DE EVENTOS GLOBALES ---
    public void NotifyPillsConsumed()
    {
        OnPillsConsumed?.Invoke();
    }
}