using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public string itemName;

    // Se recoge el objeto, desaparece del mapa y no vuelve a tocarse en el inventario
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddItem(itemName);   // Añade automáticamente al inventario
            Debug.Log("Added item: " + itemName);

            // NUEVO: Si es la Music Box, activa la secuencia de Adriana
            if (itemName == "Music Box")
            {
                AdrianaBenedetti adriana = Object.FindFirstObjectByType<AdrianaBenedetti>();
                if (adriana != null)
                {
                    adriana.OnMusicBoxCollected();
                    Debug.Log("Music Box collected: Triggering Adriana sequence.");
                }
                else
                {
                    Debug.LogWarning("CollectableItem: AdrianaBenedetti component not found in scene. Music Box sequence not triggered.");
                }
            }

            Destroy(gameObject);                           // Desaparece del mundo
        }
    }
}