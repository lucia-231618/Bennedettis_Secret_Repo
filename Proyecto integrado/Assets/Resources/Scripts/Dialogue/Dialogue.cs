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
}

[System.Serializable] 
public class DialogueChoice
{
    public string choiceText;       // Texto de la elección (seguramente nombres)
    public Sprite choiceImage;      // Imagen de las elecciones
    public Dialogue nextDialogue;   // Qué diálogo se dispara si lo elige
}

