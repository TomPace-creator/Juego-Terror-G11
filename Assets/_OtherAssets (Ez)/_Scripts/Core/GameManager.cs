using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Inventario del Jugador")]
    [SerializeField] private List<string> inventory = new List<string>();

  
    public event Action<string, string> OnMissionChanged;
    public event Action<string, string> OnSecondaryMissionChanged;
    public event Action<string, string> OnTertiaryMissionChanged;
    public event Action<string, float> OnSubtitleTriggered;
    public event Action OnPillsConsumed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    // inventario
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

    //ui y misiones
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

    public void UpdateTertiaryMission(string title, string details) 
    {
        OnTertiaryMissionChanged?.Invoke(title, details);
        Debug.Log("Misión Terciaria actualizada: " + title);
    }

    public void ShowSubtitle(string text, float durationInSeconds)
    {
        OnSubtitleTriggered?.Invoke(text, durationInSeconds);
        Debug.Log("Subtítulo mostrado: " + text);
    }

    // eventos glob
    public void NotifyPillsConsumed()
    {
        OnPillsConsumed?.Invoke();
    }
}