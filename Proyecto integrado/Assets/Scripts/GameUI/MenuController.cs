using UnityEngine;

public class MenuController : MonoBehaviour
{
    public Animator menuAnimator;           // animator del menú
    public float tiempoAnimacion = 5f;      // duración antes de cargar nivel

    void Start()
    {
        if (menuAnimator != null)
            menuAnimator.speed = 0f; // empieza congelado
    }

    // Botón Jugar
    public void PlayGame()
    {
        if (menuAnimator != null)
            menuAnimator.speed = 1f; // inicia animación

        Invoke(nameof(CargarNivel), tiempoAnimacion);
    }

    void CargarNivel()
    {
        SceneController.Instance.LoadScene("LEVELPLAY");
    }
}
