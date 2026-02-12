using System;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Diálogos Obligatorios")]
    public Dialogue dialogue1;
    public Dialogue dialogueNoItem;
    public Dialogue dialogueWithItem;
    public Dialogue dialogueFinal;

    [Header("Configuración")]
    public string itemRequired;

    private int npcState = 0;
    private Collider2D triggerCollider;
    private bool playerInTrigger = false;

    private string stateKey => $"{gameObject.name}_NPCState";

    private void Start()
    {
        DialogueManager.Instance.EndDialogueEvent += OnDialogueEnd;
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;

        npcState = PlayerPrefs.GetInt(stateKey, 0);
        Debug.Log($"{gameObject.name}: Estado cargado: {npcState}");
    }

    private void OnDestroy()
    {
        DialogueManager.Instance.EndDialogueEvent -= OnDialogueEnd;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log($"{gameObject.name}: Player entró en trigger. Estado actual: {npcState}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log($"{gameObject.name}: Player salió de trigger.");
        }
    }

    private void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            Dialogue selectedDialogue = GetSelectedDialogue();
            if (selectedDialogue != null)
            {
                Debug.Log($"{gameObject.name}: Iniciando diálogo. Estado: {npcState}, Diálogo: {selectedDialogue.name}");
                DialogueManager.Instance.StartConditionalDialogue(
                    selectedDialogue,
                    onDialogueEnd: () => { /* Opcional */ },
                    onDialogueCannotStart: () => {
                        Debug.Log($"{gameObject.name}: No se pudo iniciar el diálogo (no tienes el item).");
                    }
                );
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: No hay diálogo seleccionado. Estado: {npcState}");
            }
        }
    }

    private Dialogue GetSelectedDialogue()
    {
        if (npcState == 0)
        {
            return dialogue1;
        }
        else if (npcState == 1)
        {
            if (InventoryManager.Instance.HasItem(itemRequired))
            {
                Debug.Log($"{gameObject.name}: Tiene el item '{itemRequired}', mostrando dialogueWithItem.");
                return dialogueWithItem;
            }
            else
            {
                Debug.Log($"{gameObject.name}: No tiene el item '{itemRequired}', mostrando dialogueNoItem.");
                return dialogueNoItem;
            }
        }
        else if (npcState == 2)
        {
            Debug.Log($"{gameObject.name}: Estado final, mostrando dialogueFinal.");
            return dialogueFinal;
        }

        Debug.LogWarning($"{gameObject.name}: Estado inválido: {npcState}");
        return null;
    }

    private void OnDialogueEnd(Dialogue endedDialogue)
    {
        if (endedDialogue == null)
        {
            Debug.LogWarning($"{gameObject.name}: endedDialogue es null. No se puede procesar.");
            return;
        }

        Debug.Log($"{gameObject.name}: Diálogo terminado: {endedDialogue.name}");

        // Comparación por nombre del asset (más robusta que == para ScriptableObjects)
        if (endedDialogue.name == dialogue1?.name)
        {
            npcState = 1;
            Debug.Log($"{gameObject.name}: Estado cambiado a 1 (después de dialogue1)");
        }
        else if (endedDialogue.name == dialogueWithItem?.name)
        {
            npcState = 2;
            Debug.Log($"{gameObject.name}: Estado cambiado a 2 (después de dialogueWithItem)");
        }

        PlayerPrefs.SetInt(stateKey, npcState);
        PlayerPrefs.Save();
    }
}