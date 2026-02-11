using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryOverlayManager : MonoBehaviour
{

    public static InventoryOverlayManager Instance;

    private bool isInventoryOpen = false;                   //Controla si el inventario está abierto o cerradp
    private string inventorySceneName = "INVENTORY";        //Nombre exacto de la escena

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))   //Detecta el botón I
        {
            Debug.Log("I pressed");
            if (isInventoryOpen)
            {
                CloseInventory();         // Si inventario está abierto cierra el inventario al apretar la tecla
            }
            else
            {
                OpenInventory();          // Si inventario está cerrado abre el inventario al apretar la tecla
            }
        }
    }

    private void OpenInventory()
    {
        // Evita cargar otra vez si ya está cargada
        if (!SceneManager.GetSceneByName(inventorySceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(inventorySceneName, LoadSceneMode.Additive);
            isInventoryOpen = true;
            Time.timeScale = 0f; // pausa el juego (Movimientos y físicas)
        }                                                 
    }

    private void CloseInventory()
    {
        if (SceneManager.GetSceneByName(inventorySceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(inventorySceneName);
            isInventoryOpen = false;
            Time.timeScale = 1f; // reanuda el juego
        }                                                  // reanuda el juego
    }
}
