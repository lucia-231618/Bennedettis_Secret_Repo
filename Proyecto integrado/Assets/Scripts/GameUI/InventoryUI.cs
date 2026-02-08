using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;   // Prefab del slot
    public Transform slotsParent;   // Donde se van a instanciar los slots

    private void OnEnable()
    {
        RefreshUI();        //Asegura de refrescar la Ui cuando cargue en la escena
    }

    public void RefreshUI()
    {
        Debug.Log("Refreshing Inventory UI");

        if (slotPrefab == null) { Debug.LogError("slotPrefab is null!"); return; }
        if (slotsParent == null) { Debug.LogError("slotsParent is null!"); return; }

        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        foreach (string itemName in InventoryManager.Instance.GetAllItems())
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);

            // Asignar nombre
            TextMeshProUGUI textComponent = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = itemName;
            else
                Debug.LogWarning("Prefab missing TextMeshProUGUI component!");

            // Asignar sprite
            Image iconImage = slot.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                Sprite sprite = InventoryManager.Instance.GetItemSprite(itemName);
                if (sprite != null)
                {
                    iconImage.sprite = sprite;
                    iconImage.preserveAspect = true; // Mantener proporción del sprite
                }
                else
                {
                    Debug.LogWarning($"No sprite found for item '{itemName}' in InventoryManager.allItems");
                }
            }
            else
            {
                Debug.LogWarning("Prefab missing Image component!");
            }
        }
    }
}
