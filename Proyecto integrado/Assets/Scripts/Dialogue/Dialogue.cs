using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{

    public DialogueLine[] lines;
    public bool hasChoices = false;
    public DialogueChoice[] choices;
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

