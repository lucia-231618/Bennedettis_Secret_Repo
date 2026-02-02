using UnityEngine;

[System.Serializable]
public class ConditionalDialogue
{
    public Dialogue dialogue;           // ScriptableObject del bloque
    public bool requiresItem = false;   //Condición: Necesita objeto
    public string itemName;             // Nombre del objeto que debe tener
    public bool requiresState = false;  // Condición: si depende de estado del juego 
    public string stateName;            // Nombre del estado que necesita 
    public bool canRepeat = false;      // Posibilidad de repetir diálogo
}

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private ConditionalDialogue[] dialogues;
    private bool triggered = false;
    //Lista de todos los diálogos que puede disparar este trigger. Evita tb q se active varias veces

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var cond in dialogues)
        {
            // Si ya se disparó y no se puede repetir, lo saltamos
            if (triggered && !cond.canRepeat)
                continue;
            Debug.Log("Si ya se disparó y no se puede repetir, lo saltamos");

            bool canTrigger = true;

            if (cond.requiresItem && !Inventory.Instance.HasItem(cond.itemName))
                canTrigger = false; //Objeto requerido: el jugador no tiene el objeto, canTrigger se pone en false.

            Debug.Log("Objeto requerido");

            if (cond.requiresState && !GameManager.Instance.CheckState(cond.stateName))
                canTrigger = false; //Estado requerido: el estado no está activo, canTrigger se pone en false.

            Debug.Log("Estado requerido");

            //Solo si todas las condiciones se cumplen, canTrigger permanece true.

            if (canTrigger)
            {
                DialogueManager.Instance.StartDialogue(cond.dialogue);

                if (!cond.canRepeat)
                    triggered = true; // Bloquea solo los diálogos que NO se repiten

                Debug.Log("Dispara un diálogo");

                return; // Disparó un diálogo, salimos del bucle
            }
        }
    }
}
