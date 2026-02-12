using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [Header("Panel y Texto")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI characterText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Opciones")]
    [SerializeField] private GameObject choicesContainer; // Panel que contendrá los botones de las elecciones (Objeto vacío)
    [SerializeField] private GameObject[] choiceButtons; // Array de los botones fijos (arrastra cada uno aquí desde el Inspector)

    private Coroutine typingCoroutine;
    private bool typingCoroutineRunning = false; // controla si la animación está activa
    [SerializeField] private float lettersPerSecond = 30f; // velocidad de escritura
    private string currentLineText; // guarda el texto completo de la línea actual
    public bool TypingCoroutineRunning => typingCoroutineRunning;

    private void Awake() // Para que al iniciar el juego esté apagado
    {
        dialoguePanel.SetActive(false);
        choicesContainer.SetActive(false);
    }

    public void ShowDialogue(string character, string text)
    {
        dialoguePanel.SetActive(true);
        choicesContainer.SetActive(false);

        if (characterText != null) characterText.text = character;

        currentLineText = text; // Guardamos el texto completo

        // Detener cualquier animación previa 
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // Iniciar nueva corutina para máquina de escribir
        typingCoroutine = StartCoroutine(TypeText(text, lettersPerSecond));
    }

    private IEnumerator TypeText(string text, float lettersPerSecond)
    {
        typingCoroutineRunning = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        typingCoroutineRunning = false;
        typingCoroutine = null;
    }

    // Permite mostrar todo el texto instantáneamente
    public void SkipTyping()
    {
        if (typingCoroutineRunning) // solo si está escribiendo
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentLineText; // poner el texto completo
            typingCoroutineRunning = false;
            typingCoroutine = null;
        }
    }

    // Mostrar botones de elección (usando índices para asignar botones fijos)
    public void ShowChoices(DialogueChoice[] choices)
    {
        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            Debug.LogError("choiceButtons no está asignado o vacío en DialoguePanel. Arrastra los botones fijos al array en el Inspector.");
            return;
        }

        choicesContainer.SetActive(true);

        // Limpiar listeners antiguos para evitar duplicados
        foreach (GameObject buttonObj in choiceButtons)
        {
            if (buttonObj != null)
            {
                Button buttonComp = buttonObj.GetComponent<Button>();
                if (buttonComp != null)
                {
                    buttonComp.onClick.RemoveAllListeners(); // Limpiar listeners previos
                }
            }
        }

        // Desactivar todos los botones primero
        foreach (GameObject buttonObj in choiceButtons)
        {
            if (buttonObj != null)
            {
                buttonObj.SetActive(false);
            }
        }

        // Configurar cada botón basado en el índice de la choice
        foreach (DialogueChoice choice in choices)
        {
            int index = choice.buttonIndex;
            if (index < 0 || index >= choiceButtons.Length)
            {
                Debug.LogWarning($"buttonIndex {index} está fuera de rango. Usando índice 0 por defecto.");
                index = 0;
            }

            GameObject buttonObj = choiceButtons[index];
            if (buttonObj == null) continue; // Saltar si el botón es null

            buttonObj.SetActive(true); // Activar el botón

            // Configurar onClick
            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                buttonComp.onClick.AddListener(() =>
                {
                    choicesContainer.SetActive(false);
                    // Asume que tienes un DialogueManager con este método
                    DialogueManager.Instance.OnChoiceSelected(choice);
                });
            }
        }
    }

    public void HideDialogue() // Volver a apagarlo
    {
        dialoguePanel.SetActive(false);
        choicesContainer.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutineRunning = false;
        typingCoroutine = null;
    }
}