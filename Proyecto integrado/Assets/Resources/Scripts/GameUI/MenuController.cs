using UnityEngine;
using UnityEngine.Video;

public class MenuController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public float tiempoAnimacion = 5f;      // duración antes de cargar nivel

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

        if (videoPlayer != null)
            videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneController.Instance.LoadScene("LEVELPLAY");
    }
}
