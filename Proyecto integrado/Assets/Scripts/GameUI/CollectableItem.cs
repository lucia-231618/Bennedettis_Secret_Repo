using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public string itemName;

    //Se recoge el objeto, desaparece del mapa y no vuelve a tocarse en el inventario
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddItem(itemName);   // Añade automáticamente al inventario
            Debug.Log("Added item: " + itemName);
            Destroy(gameObject);                           // Desaparece del mundo
        }
    }
}