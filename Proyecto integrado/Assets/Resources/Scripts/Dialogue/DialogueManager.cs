using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    [SerializeField] private DialoguePanel dialoguePanel;

    private Dialogue currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    // Evento para notificar que un diálogo terminó
    public event Action<Dialogue> EndDialogueEvent;

    // AudioSource para reproducir sonidos
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

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
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (dialoguePanel.TypingCoroutineRunning)
            {
                dialoguePanel.SkipTyping();
            }
            else
            {
                NextLine();
            }
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        // Check de null para dialogue
        if (dialogue == null)
        {
            Debug.LogError("[DialogueManager] StartDialogue llamado con dialogue null. Revisa AutoDialogueManager.cs.");
            return;
        }

        Debug.Log($"[DialogueManager] Iniciando diálogo: {dialogue.name}");

        // Verificar si es un diálogo especial y cambiar escena en lugar de mostrarlo
        if (dialogue.name == "PassTheGame")
        {
            ResetState();               
            if (SceneController.Instance == null)
            {
                Debug.LogError("[DialogueManager] SceneController.Instance es null. Asegúrate de que SceneController esté en la escena.");
                return;
            }
            SceneController.Instance.LoadScene("VICTORY");
            return; // No iniciar el diálogo normal
        }
        else if (dialogue.name == "EndGame")
        {
            ResetState();
            if (SceneController.Instance == null)
            {
                Debug.LogError("[DialogueManager] SceneController.Instance es null. Asegúrate de que SceneController esté en la escena.");
                return;
            }
            SceneController.Instance.LoadScene("ENDGAME");
            return; // No iniciar el diálogo normal
        }

        // Si no es especial, proceder con el diálogo normal
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        // Cambiar estado del juego a Dialogue (check de null)
        if (GameManager.Instance == null)
        {
            Debug.LogError("[DialogueManager] GameManager.Instance es null. Asegúrate de que GameManager esté en la escena.");
        }
        else
        {
            GameManager.Instance.SetState(GameManager.GameState.Dialogue);
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null) return;

        DialogueLine line = currentDialogue.lines[currentLineIndex];

        if (line.sound != null)
        {
            audioSource.PlayOneShot(line.sound);
        }

        dialoguePanel.ShowDialogue(line.character, line.text);
    }

    public void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
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

    public void OnChoiceSelected(DialogueChoice choice)
    {
        if (choice.nextDialogue != null)
        {
            StartDialogue(choice.nextDialogue);
        }
    }

    public void EndDialogue()
    {
        if (currentDialogue != null && currentDialogue.consumesItem)
        {
            if (InventoryManager.Instance.HasItem(currentDialogue.itemToConsume))
            {
                InventoryManager.Instance.ConsumeItem(currentDialogue.itemToConsume);
            }
        }

        dialoguePanel.HideDialogue();

        // Revertir estado del juego a Exploring (check de null)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameManager.GameState.Exploring);
        }

        // Invocar el evento ANTES de resetear currentDialogue
        EndDialogueEvent?.Invoke(currentDialogue);

        currentDialogue = null;
        currentLineIndex = 0;
        isDialogueActive = false;
    }

    public void StartConditionalDialogue(Dialogue dialogue, Action onDialogueEnd = null, Action onDialogueCannotStart = null)
    {
        if (dialogue == null) return;

        if (dialogue.consumesItem && !InventoryManager.Instance.HasItem(dialogue.itemToConsume))
        {
            onDialogueCannotStart?.Invoke();
            return;
        }

        if (onDialogueEnd != null)
        {
            Action<Dialogue> wrapper = null;
            wrapper = (Dialogue d) =>
            {
                onDialogueEnd.Invoke();
                EndDialogueEvent -= wrapper;
            };
            EndDialogueEvent += wrapper;
        }

        StartDialogue(dialogue);
    }

    public void ResetState()
    {
        currentDialogue = null;
        currentLineIndex = 0;
        isDialogueActive = false;

        PlayerPrefs.DeleteAll();

        Debug.Log("[DialogueManager] Estado reseteado por cambio de escena.");
    }
}