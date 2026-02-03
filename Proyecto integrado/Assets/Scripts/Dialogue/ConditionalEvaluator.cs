using UnityEngine;


public class ConditionalEvaluator : MonoBehaviour
{
    public static ConditionalEvaluator Instance; // INSTANCE --> (Es un singleton): Permite que otros scripts accedan a este objeto desde cualquier lugar sin tener que arrastrar referencias.

    private void Awake()
    {
        //Evitamos duplicados de este script en la escena
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public bool CanTrigger(ConditionalDialogue cond) //Devuelve true o false según si se cumplen todas las condiciones 
    {
        // Evalúa objeto requerido
        if (cond.requiresItem && !InventoryManager.Instance.HasItem(cond.itemName))
            return false;

        // Evalúa estado global
        if (cond.requiresState && !GameManager.Instance.CheckState(cond.stateName))
            return false;

        return true; // Todas las condiciones se cumplen
    }
}
