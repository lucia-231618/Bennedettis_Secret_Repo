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

    private void Awake() 
    {
        //evitamos duplicados de este script en la escena
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
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
            NextLine();
        }
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
        dialoguePanel.HideDialogue();
        currentDialogue = null;
        currentLineIndex = 0;
        isDialogueActive = false;
    }
}
