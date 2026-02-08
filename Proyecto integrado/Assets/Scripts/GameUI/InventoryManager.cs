using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private ItemData[] allItems; // Lista de todos los objetos con nombre y sprite

    // Lista de nombres de objetos que tiene el jugador
    private List<string> items = new List<string>();

    private void Awake() //Evita duplicados
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public Sprite itemSprite;
    }

    // Devuelve el sprite de un item por nombre
    public Sprite GetItemSprite(string itemName)
    {
        foreach (ItemData item in allItems)
        {
            if (item.itemName == itemName)
                return item.itemSprite;
        }
        return null; // Si no encuentra el item
    }

    /// Comprueba si el jugador tiene un objeto con ese nombre
    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    /// Añade un objeto al inventario (automático, sin interacción)
    public void AddItem(string itemName)
    {
        if (items.Contains(itemName)) return;

        items.Add(itemName);
        Debug.Log("Objeto añadido al inventario: " + itemName);
    }

    /// Elimina un objeto del inventario (Al interactuar con el NPC concreto)
    public void ConsumeItem(string itemName)
    {
        if (!items.Contains(itemName)) return;

        items.Remove(itemName);
        Debug.Log("Objeto consumido por diálogo: " + itemName);
    }

    /// Listar todos los objetos (opcional, útil para depuración)
    public List<string> GetAllItems()
    {
        return new List<string>(items);
    }
}
