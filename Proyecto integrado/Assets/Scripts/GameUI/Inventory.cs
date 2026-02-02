using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    // Lista de nombres de objetos que tiene el jugador
    private List<string> items = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }


    /// Comprueba si el jugador tiene un objeto con ese nombre
    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    /// Añade un objeto al inventario.
    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
            items.Add(itemName);
    }

    /// Quita un objeto del inventario.
    public void RemoveItem(string itemName)
    {
        if (items.Contains(itemName))
            items.Remove(itemName);
    }

    /// Listar todos los objetos (opcional, útil para depuración)
    public List<string> GetAllItems()
    {
        return new List<string>(items);
    }
}
