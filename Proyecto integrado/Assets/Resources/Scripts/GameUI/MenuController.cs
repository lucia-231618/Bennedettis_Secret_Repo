using UnityEngine;
using UnityEngine.Video;

public class MenuController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public float tiempoAnimacion = 5f;      // duración antes de cargar nivel
    public GameObject PanelUI;  // arrastra tu contenedor desde el Inspector

    void Start()
    {

        if (videoPlayer != null)
        {
            videoPlayer.Play();   // lo arrancamos
            videoPlayer.Pause();  // lo dejamos congelado en el primer frame

            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    // Botón Jugar
    public void PlayGame()
    {
        if (PanelUI != null)
            PanelUI.SetActive(false); // ocultamos todo el UI

        if (videoPlayer != null)
            videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneController.Instance.LoadScene("LEVELPLAY");
    }
}
