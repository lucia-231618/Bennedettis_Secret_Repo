using UnityEngine;

public class AdrianaBenedetti : MonoBehaviour
{
    public GameObject npcPrefab;
    private GameObject npcInstance;

    [Header("Spawn Points")]
    public Transform inicioNivelPoint;
    public Transform objetoEncontradoPoint;

    [Header("Diálogos")]
    public ConditionalDialogue dialogo1;
    public ConditionalDialogue dialogo2;
    public ConditionalDialogue dialogo3;
    public ConditionalDialogue dialogo4;
    public ConditionalDialogue dialogo5;

    private void Start()
    {
        // Disparar los diálogos iniciales
        IniciarDialogosIniciales();
    }

    private void IniciarDialogosIniciales()
    {
        Aparecer(inicioNivelPoint, dialogo1, () =>
        {
            Aparecer(inicioNivelPoint, dialogo2);
        });
    }

    public void OnObjetoEncontrado()
    {
        Aparecer(objetoEncontradoPoint, dialogo3, () =>
        {
            Aparecer(objetoEncontradoPoint, dialogo4);
        });
    }

    public void OnMisionesCompletas()
    {
        // Aparece donde esté el jugador
        Transform playerPos = GameManager.Instance.GetPlayerTransform();
        Aparecer(playerPos, dialogo5);
    }

    private void Aparecer(Transform spawnPoint, ConditionalDialogue cond, System.Action onDialogueEnd = null)
    {
        if (cond == null || cond.dialogue == null)
            return;

        if (!ConditionalEvaluator.Instance.CanTrigger(cond))
            return;

        if (npcInstance == null)
            npcInstance = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        else
        {
            npcInstance.transform.position = spawnPoint.position;
            npcInstance.transform.rotation = spawnPoint.rotation;
            npcInstance.SetActive(true);
        }

        DialogueManager.Instance.StartConditionalDialogue(cond, onDialogueEnd);
    }

    public void Desaparecer()
    {
        if (npcInstance != null)
            npcInstance.SetActive(false);
    }
}
