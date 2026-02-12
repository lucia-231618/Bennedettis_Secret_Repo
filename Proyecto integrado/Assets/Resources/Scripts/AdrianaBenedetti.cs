using System.Collections;
using UnityEngine;

public class AdrianaBenedetti : MonoBehaviour
{
    [SerializeField] private string prefabPath = "Prefabs/AdrianaBenedetti";
    [SerializeField] private Transform spawnPointTransform; // Arrastra un Empty GameObject aquí para definir la posición (usado en la secuencia original)
    [SerializeField] private Transform musicBoxSpawnPointTransform; // Arrastra un Empty GameObject aquí para definir la posición fija al recoger la Music Box
    [SerializeField] private Transform playerSpawnPoint; // Arrastra el Transform SpawnNextPlayer del player aquí (usado cuando todas las misiones están completadas)
    [SerializeField] private AutoDialogueManager autoDialogos; // Referencia opcional; si no se asigna, se agrega automáticamente

    private GameObject npcInstance;
    private bool secuenciaIniciada = false; // Flag para evitar múltiples llamadas en la secuencia original
    private bool musicBoxSecuenciaIniciada = false; // Flag para evitar múltiples llamadas en la secuencia de Music Box
    private bool allMissionsSecuenciaIniciada = false; // Flag para evitar múltiples llamadas en la secuencia de todas las misiones completadas

    [SerializeField] private float fadeDuration = 2f;
    private bool primeraAparicion = true;

    // Método original para aparecer tras entrada a la mansión
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

    // Método para activar cuando se recoja la Music Box
    public void OnMusicBoxCollected()
    {
        if (musicBoxSecuenciaIniciada)
        {
            Debug.LogWarning("AdrianaBenedetti: Secuencia de Music Box ya iniciada. Ignorando llamada duplicada.");
            return;
        }

        musicBoxSecuenciaIniciada = true;
        StartCoroutine(SecuenciaMusicBox());
    }

    // Método para activar cuando todos los NPCs excepto Adriana estén en estado 2 (es decir, que el player haya interactuado con ellos dada la pista)
    public void OnAllMissionsCompleted()
    {
        if (allMissionsSecuenciaIniciada)
        {
            Debug.LogWarning("AdrianaBenedetti: Secuencia de todas las misiones ya iniciada. Ignorando llamada duplicada.");
            return;
        }

        allMissionsSecuenciaIniciada = true;
        StartCoroutine(SecuenciaAllMissions());
    }

    // Método para verificar si todos los NPCs están en estado 2 y aparecer si es así
    public void CheckAndAppearIfAllCompleted()
    {
        NPCController[] allNPCs = Object.FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        bool allCompleted = true;

        foreach (NPCController npc in allNPCs)
        {
            if (npc.NPCState != 2)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            Debug.Log("AdrianaBenedetti: Todos los NPCs están en estado 2. Activando secuencia de todas las misiones completadas.");
            OnAllMissionsCompleted();
        }
        else
        {
            Debug.Log("AdrianaBenedetti: No todos los NPCs están en estado 2 aún.");
        }
    }

    private IEnumerator SecuenciaAdriana()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("AdrianaBenedetti: DialogueManager.Instance no encontrado.");
            yield break;
        }

        // Spawnea Adriana y captura la corrutina de fade (si es la primera vez)
        Coroutine fade = SpawnNPC();

        // Espera a que termine el fade si existe
        if (fade != null)
            yield return fade;

        Debug.Log("AdrianaBenedetti: Adriana ya apareció completamente. Lanzando PuertaCerrada.");

        if (autoDialogos != null)
        {
            autoDialogos.LanzarDialogo("PuertaCerrada");

            yield return new WaitUntil(() => DialogueManager.Instance.IsDialogueActive());
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

            Debug.Log("AdrianaBenedetti: PuertaCerrada terminado. Desapareciendo Adriana.");
            Desaparecer();

            autoDialogos.LanzarDialogo("AdrianaLaughFirstTime");
        }
        else
        {
            Debug.LogError("AdrianaBenedetti: AutoDialogueManager no asignado.");
        }
    }

    // Corroutina para la secuencia de Music Box, ahora usa el spawn point fijo
    private IEnumerator SecuenciaMusicBox()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("AdrianaBenedetti: DialogueManager.Instance no encontrado.");
            yield break;
        }

        // Verifica que el spawn point esté asignado
        if (musicBoxSpawnPointTransform == null)
        {
            Debug.LogError("AdrianaBenedetti: musicBoxSpawnPointTransform no asignado. Arrastra un Empty GameObject al campo en el Inspector para definir la posición.");
            yield break;
        }

        Debug.Log("AdrianaBenedetti: Spawneando Adriana en el spawn point fijo para Music Box.");
        SpawnNPC(musicBoxSpawnPointTransform.position, musicBoxSpawnPointTransform.rotation); // Usa el spawn point fijo

        if (autoDialogos != null)
        {
            autoDialogos.LanzarDialogo("FoundAdrianaObject");
            Debug.Log("AdrianaBenedetti: Diálogo FoundAdrianaObject lanzado. Esperando a que termine.");

            // Espera a que termine FoundAdrianaObject
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

            Debug.Log("AdrianaBenedetti: Diálogo FoundAdrianaObject terminado. Desapareciendo Adriana.");
            Desaparecer();

            // Lanza automáticamente "AdrianaLaughSecondTime" justo después de que Adriana desaparezca
            Debug.Log("AdrianaBenedetti: Lanzando diálogo AdrianaLaughSecondTime.");
            autoDialogos.LanzarDialogo("AdrianaLaughSecondTime");
        }
        else
        {
            Debug.LogError("AdrianaBenedetti: AutoDialogueManager no disponible.");
        }
    }

    // Corroutina para la secuencia de todas las misiones completadas
    private IEnumerator SecuenciaAllMissions()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("AdrianaBenedetti: DialogueManager.Instance no encontrado.");
            yield break;
        }

        // Espera a que termine cualquier diálogo activo antes de proceder
        if (DialogueManager.Instance.IsDialogueActive())
        {
            Debug.Log("AdrianaBenedetti: Esperando a que termine el diálogo activo antes de spawnear Adriana.");
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }

        // Verifica que el spawn point del player esté asignado
        if (playerSpawnPoint == null)
        {
            Debug.LogError("AdrianaBenedetti: playerSpawnPoint no asignado. Arrastra el Transform SpawnNextPlayer del player al campo en el Inspector.");
            yield break;
        }

        Debug.Log("AdrianaBenedetti: Spawneando Adriana en el SpawnNextPlayer del player para todas las misiones completadas.");
        SpawnNPC(playerSpawnPoint.position, playerSpawnPoint.rotation); // Usa el spawn point del player

        if (autoDialogos != null)
        {
            autoDialogos.LanzarDialogo("AllMisionsCompleted");
            Debug.Log("AdrianaBenedetti: Diálogo AllMisionsCompleted lanzado. Esperando a que termine.");

            // Espera a que termine AllMisionsCompleted
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

            Debug.Log("AdrianaBenedetti: Diálogo AllMisionsCompleted terminado. Desapareciendo Adriana.");
            Desaparecer();
        }
        else
        {
            Debug.LogError("AdrianaBenedetti: AutoDialogueManager no disponible.");
        }
    }

    public Coroutine SpawnNPC(Vector3? customPosition = null, Quaternion? customRotation = null)
    {
        Vector3 position = customPosition ?? (spawnPointTransform != null ? spawnPointTransform.position : Vector3.zero);
        Quaternion rotation = customRotation ?? (spawnPointTransform != null ? spawnPointTransform.rotation : Quaternion.identity);

        if (position == Vector3.zero && spawnPointTransform == null)
        {
            Debug.LogError("AdrianaBenedetti: Ni spawnPointTransform asignado ni posición personalizada proporcionada.");
            return null;
        }

        if (npcInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"AdrianaBenedetti: No se encontró el prefab en Resources/{prefabPath}.");
                return null;
            }

            npcInstance = Instantiate(prefab, position, rotation);

            if (primeraAparicion)
            {
                primeraAparicion = false;
                // Devuelve la corrutina para que se pueda esperar en SecuenciaAdriana
                return StartCoroutine(FadeInNPC());
            }

            Debug.Log("AdrianaBenedetti: Adriana spawneada exitosamente.");
        }
        else
        {
            npcInstance.transform.position = position;
            npcInstance.transform.rotation = rotation;
            npcInstance.SetActive(true);
            Debug.Log("AdrianaBenedetti: Adriana reposicionada y activada.");
        }

        return null;
    }

    private IEnumerator FadeInNPC()
    {
        if (npcInstance == null) yield break;

        SpriteRenderer[] renderers = npcInstance.GetComponentsInChildren<SpriteRenderer>();

        float t = 0f;

        // poner invisible
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = 0f;
            r.color = c;
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);

            foreach (var r in renderers)
            {
                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }

            yield return null;
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