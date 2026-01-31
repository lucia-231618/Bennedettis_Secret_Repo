using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;                 //Para que otros scripts puedan llamar a este script sin tener que arrastrar referencias
    [SerializeField] private DialoguePanel dialoguePanel;   //Llama al script de la UI 
    private Dialogue currentDialogue;
    private int currentLineIndex = 0;                       //Controla qué línea del array se está mostrando

   

    private void Awake() 
    {
        //evitamos duplicados de este script en la escena
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
    }

    // Método que inicia un diálogo (se hace pasando el diálogo al DialoguePanel)
    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];
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
    }
}
