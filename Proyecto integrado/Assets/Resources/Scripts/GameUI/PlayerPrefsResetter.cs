using UnityEngine;

public class PlayerPrefsResetter : MonoBehaviour
{
    private void Start()
    {
        // Borra todos los PlayerPrefs solo en el Editor (no en builds)
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs reseteados automáticamente en el Editor.");
#endif
    }
}