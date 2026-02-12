using System.Collections;
using UnityEngine;

public class AutoDialogueManager : MonoBehaviour
{

    [SerializeField] private AdrianaBenedetti adrianaManager; // Referencia a AdrianaBenedetti

    private void Start()
    {
        // Lanzamos automáticamente EntradaMansion después de 2 segundos
        StartCoroutine(LanzarEntradaMansion());
    }

    private IEnumerator LanzarEntradaMansion()
    {
        yield return new WaitForSeconds(1f);

        // Carga el ScriptableObject del diálogo por nombre
        Dialogue dialogo = Resources.Load<Dialogue>("Dialogues/EntradaMansion");

        if (dialogo == null)
        {
            yield break;
        }

        // Lanza el diálogo y espera a que termine
        DialogueManager.Instance.StartDialogue(dialogo);
        yield return new WaitUntil(() => DialogueManager.Instance.IsDialogueActive());
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

        // Después de EntradaMansion, dispara la aparición de Adriana y PuertaCerrada
        adrianaManager?.AparecerTrasEntradaMansion();
    }

    public void LanzarDialogo(string dialogueName, float delay = 0f, System.Action callback = null)
    {
        StartCoroutine(LanzarDialogoCoroutine(dialogueName, delay, callback));
    }

    private IEnumerator LanzarDialogoCoroutine(string dialogueName, float delay, System.Action callback)
    {
        Dialogue dialogo = Resources.Load<Dialogue>($"Dialogues/{dialogueName}");

        if (dialogo == null)
        {
            Debug.LogWarning($"AutoDialogueManager: No se encontró el diálogo '{dialogueName}' en Resources/Dialogues/");
            yield break;
        }
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

        DialogueManager.Instance.StartDialogue(dialogo);

        yield return new WaitUntil(() => DialogueManager.Instance.IsDialogueActive());
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
    }
}
