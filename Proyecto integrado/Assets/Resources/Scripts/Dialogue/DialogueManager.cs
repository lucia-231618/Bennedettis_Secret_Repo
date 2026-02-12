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
}