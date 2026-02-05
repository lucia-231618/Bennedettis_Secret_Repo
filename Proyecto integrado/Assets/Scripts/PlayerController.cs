using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Variables de referencia
    private Rigidbody2D playerRb;
    private Animator anim;
    private float horizontalInput;

    //Variables de estadística del player
    public float speed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        // Si hay diálogo activo, no hacemos nada
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            return;

        Movement();
        Flip();
    }

   // Todas las referencias que van dentro del Update
    void Movement()
    {
        if (Keyboard.current == null) return;

        horizontalInput =
            (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? -1 : 0) +
            (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1 : 0);

        playerRb.linearVelocity = new Vector2(horizontalInput * speed, playerRb.linearVelocity.y);
        anim.SetBool("Walking", horizontalInput != 0);
        Debug.Log(horizontalInput);
    }

    void Flip()
    {
        Vector3 scale = transform.localScale; //coge la escala actual del player

        if (horizontalInput > 0)  // si me muevo a la derecha, escala positiva 
        {
            scale.x = Mathf.Abs(scale.x); 
        }
        else if (horizontalInput < 0)
        {
            scale.x = -Mathf.Abs(scale.x); // si me muevo a la izquierda, escala negativa 
        }

        transform.localScale = scale;
    }
}
