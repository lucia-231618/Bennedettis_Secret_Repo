using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryOverlayManager : MonoBehaviour
{
    private bool isInventoryOpen = false;                   //Controla si el inventario está abierto o cerradp
    private string inventorySceneName = "INVENTORY";   //Nombre exacto de la escena

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))   //Detecta el botón I
        {
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
        SceneManager.LoadSceneAsync(inventorySceneName, LoadSceneMode.Additive);  //Abre el inventario sin cerrar la escena actual
        isInventoryOpen = true;                                                   
        Time.timeScale = 0f;                                                     // pausa el juego (Movimientos y físicas)
    }

    private void CloseInventory()
    {
        SceneManager.UnloadSceneAsync(inventorySceneName);                       //Cierra el inventario sin tocar la escena actual
        isInventoryOpen = false;
        Time.timeScale = 1f;                                                     // reanuda el juego
    }
}
