using System.Collections;
using UnityEngine;

public class AdrianaBenedetti : MonoBehaviour
{
    [SerializeField] private string prefabPath = "Prefabs/AdrianaBenedetti"; 
    [SerializeField] private Transform spawnPointTransform; // Arrastra un Empty GameObject aquí para definir la posición
    [SerializeField] private AutoDialogueManager autoDialogos;                   // Referencia opcional; si no se asigna, se agrega automáticamente

    private GameObject npcInstance;
    private bool secuenciaIniciada = false; // Flag para evitar múltiples llamadas

    private void Awake()
    {
        if (autoDialogos == null)
        {
            autoDialogos = gameObject.AddComponent<AutoDialogueManager>();
        }
    }

    public void AparecerTrasEntradaMansion()
    {
        if (secuenciaIniciada)
        {
            Debug.LogWarning("AdrianaBenedetti: Secuencia ya iniciada. Ignorando llamada duplicada.");
            return;
        }

        secuenciaIniciada = true;
        StartCoroutine(SecuenciaAdriana());
    }

    private IEnumerator SecuenciaAdriana()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("AdrianaBenedetti: DialogueManager.Instance no encontrado.");
            yield break;
        }

        if (DialogueManager.Instance.IsDialogueActive())
        {
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        else
        {
            Debug.LogWarning("AdrianaBenedetti: El diálogo EntradaMansion no parece estar activo. Procediendo.");
        }

        // Espera 1 segundos antes de spawnear y hablar
        Debug.Log("AdrianaBenedetti: Esperando 1 segundos antes de spawnear Adriana.");
        yield return new WaitForSeconds(1f);

        Debug.Log("AdrianaBenedetti: Llamando a SpawnNPC.");
        SpawnNPC();

        if (autoDialogos != null)
        {
            autoDialogos.LanzarDialogo("PuertaCerrada");
            Debug.Log("AdrianaBenedetti: Diálogo PuertaCerrada lanzado. Esperando a que termine.");

            //Espera a que termine PuertaCerrada
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

            Debug.Log("AdrianaBenedetti: Diálogo PuertaCerrada terminado. Desapareciendo Adriana.");
            Desaparecer();

            // Lanza automáticamente "AdrianaLaughFirstTime" justo después de que Adriana desaparezca
            Debug.Log("AdrianaBenedetti: Lanzando diálogo AdrianaLaughFirstTime.");
            autoDialogos.LanzarDialogo("AdrianaLaughFirstTime");
        }
        else
        {
            Debug.LogError("AdrianaBenedetti: AutoDialogueManager no disponible.");
        }
    }

    private void SpawnNPC()
    {
        if (spawnPointTransform == null)
        {
            Debug.LogError("AdrianaBenedetti: spawnPointTransform no asignado. Arrastra un Empty GameObject al campo en el Inspector.");
            return;
        }

        if (npcInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"AdrianaBenedetti: No se encontró el prefab en Resources/{prefabPath}. Verifica la ruta y el nombre.");
                return;
            }
            npcInstance = Instantiate(prefab, spawnPointTransform.position, spawnPointTransform.rotation);
            Debug.Log("AdrianaBenedetti: Adriana spawneada exitosamente en la posición del spawnPointTransform.");
        }
        else
        {
            npcInstance.transform.position = spawnPointTransform.position;
            npcInstance.transform.rotation = spawnPointTransform.rotation;
            npcInstance.SetActive(true);
            Debug.Log("AdrianaBenedetti: Adriana reposicionada y activada en la posición del spawnPointTransform.");
        }
    }

    public void Desaparecer()
    {
        if (npcInstance != null)
        {
            npcInstance.SetActive(false);
            Debug.Log("AdrianaBenedetti: Adriana desactivada.");
        }
        else
        {
            Debug.LogWarning("AdrianaBenedetti: No hay instancia de Adriana para desactivar.");
        }
    }
}
