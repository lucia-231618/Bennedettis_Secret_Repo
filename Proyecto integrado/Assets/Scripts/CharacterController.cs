using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    //Variables de referencia
    private Rigidbody2D playerRb;
    private Animator anim;
    private float horizontalInput;
    private SpriteRenderer spriteRenderer;

    //Variables de estadística del player
    public float speed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
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
        anim.SetBool("isWalking", horizontalInput != 0);
    }

    void Flip()
    {
        if (horizontalInput > 0) 
        {
            spriteRenderer.flipX = false; 
        }
        else if (horizontalInput < 0) 
        {
            spriteRenderer.flipX = true; 
        }
    }
}
