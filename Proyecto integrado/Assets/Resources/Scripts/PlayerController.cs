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


    //Variables de triggers
    private bool hitWall = false;
    private bool wallOnLeft = false;
    private bool wallOnRight = false;



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

        // Si hemos tocado pared, no dejamos que siga caminando hacia ella
        if (hitWall && horizontalInput != 0)
        {
            if ((horizontalInput > 0 && wallOnRight) || (horizontalInput < 0 && wallOnLeft))
            {
                horizontalInput = 0;
            }
        }


        playerRb.linearVelocity = new Vector2(horizontalInput * speed, playerRb.linearVelocity.y);
        anim.SetBool("Walking", horizontalInput != 0);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            // Detectar si la pared está a la derecha o izquierda del jugador
            if (collision.transform.position.x > transform.position.x)
                wallOnRight = true;
            else
                wallOnLeft = true;

            hitWall = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            if (collision.transform.position.x > transform.position.x)
                wallOnRight = false;
            else
                wallOnLeft = false;

            if (!wallOnLeft && !wallOnRight)
                hitWall = false;
        }
    }
}
