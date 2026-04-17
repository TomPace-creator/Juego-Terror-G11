using UnityEngine;
using System.Collections.Generic;
using System; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Inventario del Jugador")]
    public List<string> inventory = new List<string>();


    public event Action<string, string> OnMissionChanged;


    public event Action<string, float> OnSubtitleTriggered;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

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



    public void UpdateMission(string title, string details)
    {
        OnMissionChanged?.Invoke(title, details);
        Debug.Log("Misión actualizada: " + title);
    }

   
    public void ShowSubtitle(string text, float durationInSeconds)
    {
        OnSubtitleTriggered?.Invoke(text, durationInSeconds);
        Debug.Log("Subtítulo mostrado: " + text);
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
    // --- NUEVO: SISTEMA DE MISIONES SECUNDARIAS ---
    // El evento que avisa a la UI que hay una nueva misión secundaria
    public event System.Action<string, string> OnSecondaryMissionChanged;

    
    public void UpdateSecondaryMission(string title, string details)
    {
        // ¡OJO ACÁ! Tiene que decir OnSecondaryMissionChanged, NO OnMissionChanged
        if (OnSecondaryMissionChanged != null)
        {
            OnSecondaryMissionChanged(title, details);
        }
    }
}