using UnityEngine;

[System.Serializable]
public class ConditionalDialogue
{
    public Dialogue dialogue;           // ScriptableObject del bloque
    public bool requiresItem = false;   //Condición: Necesita objeto
    public string itemName;             // Nombre del objeto que debe tener
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
            {
                Debug.Log($"[DialogueTrigger] → El diálogo '{cond.dialogue.name}' ya se mostró y no puede repetirse. Se ignora.");
                continue;
            }

            bool canTrigger = true;

            if (cond.requiresItem) //Revisa si requiere objeto
            {
                if (!InventoryManager.Instance.HasItem(cond.itemName))
                {
                    canTrigger = false;
                    Debug.Log($"[DialogueTrigger] → El diálogo '{cond.dialogue.name}' requiere el objeto '{cond.itemName}', pero el jugador NO lo tiene.");
                }
                else
                {
                    Debug.Log($"[DialogueTrigger] → El jugador tiene el objeto requerido '{cond.itemName}' para el diálogo '{cond.dialogue.name}'.");
                }
            }
         
            // Solo si todas las condiciones se cumplen, avisamos al DialogueManager -- No llama al diálogo desde trigger 
            if (canTrigger)
            {
                Debug.Log($"[DialogueTrigger] → Jugador entró en trigger para diálogo '{cond.dialogue.name}'. Pulsar E para iniciar.");

                // Solo avisamos al DialogueManager
                DialogueManager.Instance.SetCurrentTriggerDialogue(cond);

                if (!cond.canRepeat)
                    triggered = true;

                return; // Salimos del bucle, solo un diálogo activo a la vez
            }
            else
            {
                Debug.Log($"[DialogueTrigger] → No se puede disparar el diálogo '{cond.dialogue.name}' porque NO se cumplen todas las condiciones.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        DialogueManager.Instance.PlayerLeftTrigger();
    }
}

