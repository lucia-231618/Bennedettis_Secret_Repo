using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;   // Prefab del slot
    public Transform slotsParent;   // Donde se van a instanciar los slots

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Limpiar slots anteriores
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        // Añadir slots nuevos para cada objeto del inventario
        foreach (string itemName in InventoryManager.Instance.GetAllItems())
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            slot.GetComponentInChildren<Text>().text = itemName;

            // Opcional: si tienes un icono asignado, puedes usarlo
            // slot.GetComponentInChildren<Image>().sprite = itemIcon;
        }
    }
}
