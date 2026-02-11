using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;                 //Para que otros scripts puedan llamar a este script sin tener que arrastrar referencias
    [SerializeField] private DialoguePanel dialoguePanel;   //Llama al script de la UI 
    
    private Dialogue currentDialogue;
    private int currentLineIndex = 0;                       //Controla qué línea del array se está mostrando
    private bool isDialogueActive = false;                  //Evita que se disparen los diálogos cuando no hay diálogos (Interruptor)

    // Para controlar triggers que esperan tecla E
    private ConditionalDialogue currentTriggerDialogue = null;
    private bool playerInTrigger = false;

    // Evento para notificar que un diálogo terminó
    public event Action<Dialogue> EndDialogueEvent;

    // AudioSource para reproducir sonidos
    private AudioSource audioSource;

    private void Awake() 
    {
        //evitamos duplicados de este script en la escena
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);

        // Agrega AudioSource si no existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    private void Update()
    {
        if (playerInTrigger && currentTriggerDialogue != null && Input.GetKeyDown(KeyCode.E))
        {
            // Inicia el diálogo solo si el jugador pulsa E
            StartDialogue(currentTriggerDialogue.dialogue);
            currentTriggerDialogue = null; // Evita que se vuelva a iniciar hasta nuevo trigger
        }

        // Avanza líneas si el diálogo está activo y se pulsa espacio
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (dialoguePanel.TypingCoroutineRunning) // si todavía se está escribiendo
            {
                dialoguePanel.SkipTyping(); // mostrar texto completo
            }
            else
            {
                NextLine(); // avanzar a la siguiente línea
            }
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    // Este método lo llamará el trigger cuando el jugador esté en contacto
    public void SetCurrentTriggerDialogue(ConditionalDialogue dialogue)
    {
        currentTriggerDialogue = dialogue;
        playerInTrigger = true;
    }

    // Este método indicará que no está en ningún trigger por tanto no se puede iniciar diálogo
    public void PlayerLeftTrigger()
    {
        playerInTrigger = false;
        currentTriggerDialogue = null;
    }


    // Método que inicia un diálogo (se hace pasando el diálogo al DialoguePanel)
    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null) return;

        DialogueLine line = currentDialogue.lines[currentLineIndex];
        Debug.Log($"[DialogueManager] Mostrando línea {currentLineIndex}: {line.character} -> {line.text}");

        // Reproduce sonido si la línea tiene uno asignado
        if (line.sound != null)
        {
            audioSource.PlayOneShot(line.sound);
            Debug.Log($"[DialogueManager] Reproduciendo sonido para línea {currentLineIndex}: {line.sound.name}");
        }
        else
        {
            Debug.Log($"[DialogueManager] No hay sonido para línea {currentLineIndex}");
        }

        dialoguePanel.ShowDialogue(line.character, line.text);
    }

    // Método que se llama cuando el jugador pulsa "Siguiente"
    public void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            // Fin del bloque de diálogo
            if (currentDialogue.hasChoices && currentDialogue.choices.Length > 0)
            {
                dialoguePanel.ShowChoices(currentDialogue.choices);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    // Método que se llama cuando el jugador eliga una opción en el momento de las elecciones.
    public void OnChoiceSelected(DialogueChoice choice)
    {
        if (choice.nextDialogue != null)
        {
            StartDialogue(choice.nextDialogue);
        }
    }

    // Método para cerrar el panel del Diálogo
    public void EndDialogue()
    {
        // Inventario
        if (currentDialogue != null && currentDialogue.consumesItem)
        {
            if (InventoryManager.Instance.HasItem(currentDialogue.itemToConsume))
            {
                InventoryManager.Instance.ConsumeItem(currentDialogue.itemToConsume);
            }
        }

        dialoguePanel.HideDialogue();
        currentDialogue = null;
        currentLineIndex = 0;
        isDialogueActive = false;

        // Disparar evento para que Adriana pueda saber que terminó
        EndDialogueEvent?.Invoke(currentDialogue);
    }

    public void StartConditionalDialogue(ConditionalDialogue cond, Action onDialogueEnd = null)
    {
        if (cond == null || cond.dialogue == null) return;

        if (!ConditionalEvaluator.Instance.CanTrigger(cond))
        return;

        if (onDialogueEnd != null)
        {
            // Crear un wrapper compatible con Action<Dialogue>
             Action<Dialogue> wrapper = null;
            wrapper = (Dialogue d) =>
            {
                onDialogueEnd.Invoke();
                EndDialogueEvent -= wrapper; // Desuscribimos para que solo se ejecute una vez
            };
            EndDialogueEvent += wrapper;
        }

    StartDialogue(cond.dialogue);
    }
}
