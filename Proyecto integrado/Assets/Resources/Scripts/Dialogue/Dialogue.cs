using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{

    [Header("Dialogue Content")]
    public DialogueLine[] lines;

    [Header("Choices")]
    public bool hasChoices = false;
    public DialogueChoice[] choices;

    [Header("Inventory Effect")]
    public bool consumesItem = false;    //Marca si un diálogo debe eliminar algo del inventario
    public string itemToConsume;        //Nombre exacto del Item que tiene que eliminar
}

[System.Serializable]
public class DialogueLine
{
    public string character;
    public string text;
    public AudioClip sound;
}

[System.Serializable] 
public class DialogueChoice
{
    [Tooltip("Index of the button in DialoguePanel's choiceButtons array (0 for first button, 1 for second, etc.).")]
    public int buttonIndex = 0;  // Índice para seleccionar el botón fijo
    [Tooltip("The next dialogue to load if this choice is selected.")]
    public Dialogue nextDialogue;
}

