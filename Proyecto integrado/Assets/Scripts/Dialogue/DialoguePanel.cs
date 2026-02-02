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
    [SerializeField] private GameObject choiceButtonPrefab; // Prefab de botón con imagen + texto


    private void Awake() //Para q al iniciar el juego esté apagado
    {
        dialoguePanel.SetActive(false); 
        choicesContainer.SetActive(false);
    }

    public void ShowDialogue(string character, string text)
    {
        dialoguePanel.SetActive(true);
        choicesContainer.SetActive(false);

        if (characterText != null) characterText.text = character;
        if (dialogueText != null) dialogueText.text = text;
    }


    // Mostrar botones de elección
    public void ShowChoices(DialogueChoice[] choices)
    {
        choicesContainer.SetActive(true);

        // Limpiar botones antiguos
        foreach (Transform child in choicesContainer.transform)
            Destroy(child.gameObject);

        // Crear botones nuevos
        foreach (DialogueChoice choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Image buttonImage = buttonObj.GetComponentInChildren<Image>();

            if (buttonText != null) buttonText.text = choice.choiceText;
            if (buttonImage != null && choice.choiceImage != null) buttonImage.sprite = choice.choiceImage;

            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                buttonComp.onClick.AddListener(() =>
                {
                    choicesContainer.SetActive(false);
                    DialogueManager.Instance.OnChoiceSelected(choice);
                });
            }
        }
    }

    public void HideDialogue() //Volver a apagarlo
    {
        dialoguePanel.SetActive(false);
        choicesContainer.SetActive(false);
    }
}