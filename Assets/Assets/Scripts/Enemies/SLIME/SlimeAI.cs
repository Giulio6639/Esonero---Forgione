using UnityEngine;
using System.Collections; // Necessario per le Coroutine!

public class SlimeAI : MonoBehaviour
{
    [Header("Movimento e Salto")]
    public float walkSpeed = 1f;
    public float chaseSpeed = 1.5f;
    public float jumpForceX = 4f;
    public float jumpForceY = 6.5f; // Un po' più alto per fare un bell'arco
    public float jumpDelay = 0.3f;  // --- NUOVO: Quanto tempo ci mette a "caricarsi" prima di staccarsi da terra?

    [Header("Sensori (Distanze)")]
    public float sightRange = 6f;
    public float attackRange = 2f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Combattimento")]
    public float attackCooldown = 2f;

    [Header("Riferimenti")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private bool facingRight = true;
    private bool isGrounded;
    private bool isChargingJump = false; // --- NUOVO: Ci dice se sta "caricando" le molle

    private enum State { Idle, Wander, Chase, Cooldown }
    private State currentState;

    private float stateTimer;
    private int wanderDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Idle;
        stateTimer = Random.Range(2f, 4f);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        animator.SetBool("isGrounded", isGrounded);

        float distToPlayer = 100f;
        if (player != null) distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                if (isGrounded) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isWalking", false);

                if (distToPlayer <= sightRange) { currentState = State.Chase; break; }

                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;
                    stateTimer = Random.Range(1.5f, 3f);
                    currentState = State.Wander;
                }
                break;

            case State.Wander:
                if (isGrounded) rb.linearVelocity = new Vector2(walkSpeed * wanderDirection, rb.linearVelocity.y);
                animator.SetBool("isWalking", true);
                Flip(wanderDirection);

                if (distToPlayer <= sightRange) { currentState = State.Chase; break; }

                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    stateTimer = Random.Range(2f, 4f);
                    currentState = State.Idle;
                }
                break;

            case State.Chase:
                if (distToPlayer <= attackRange && isGrounded)
                {
                    animator.SetBool("isWalking", false);

                    int lookDir = player.position.x > transform.position.x ? 1 : -1;
                    Flip(lookDir);

                    // --- MODIFICA: Invece di applicare subito la forza, avviamo la Coroutine! ---
                    animator.SetTrigger("JumpTrig");
                    isChargingJump = true; // Accendiamo il semaforo della carica

                    stateTimer = attackCooldown;
                    currentState = State.Cooldown;

                    StartCoroutine(DelayedJumpRoutine(lookDir));
                    break;
                }

                animator.SetBool("isWalking", true);
                if (distToPlayer > sightRange) { currentState = State.Idle; break; }

                if (isGrounded)
                {
                    int chaseDir = player.position.x > transform.position.x ? 1 : -1;
                    rb.linearVelocity = new Vector2(chaseSpeed * chaseDir, rb.linearVelocity.y);
                    Flip(chaseDir);
                }
                break;

            case State.Cooldown:
                // --- MODIFICA: Lo slime si ferma a terra SOLO se non sta caricando il salto ---
                if (isGrounded && !isChargingJump) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { currentState = State.Idle; }
                break;
        }
    }

    // --- NUOVA COROUTINE DEL SALTO RITARDATO ---
    private IEnumerator DelayedJumpRoutine(int direction)
    {
        // 1. Ferma fisicamente lo slime mentre fa l'animazione di "schiacciamento"
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 2. Aspetta i secondi esatti prima dello slancio
        yield return new WaitForSeconds(jumpDelay);

        // 3. SPINTA! Lo lanciamo in avanti e in alto
        rb.linearVelocity = new Vector2(direction * jumpForceX, jumpForceY);

        // 4. IL TRUCCO: Aspettiamo un decimo di secondo PRIMA di dire al gioco
        // che la carica è finita. Così gli diamo il tempo fisico di sollevarsi da terra!
        yield return new WaitForSeconds(0.1f);

        // 5. Ora il semaforo si spegne e lui volerà col suo arco perfetto
        isChargingJump = false;
    }

    private void Flip(int direction)
    {
        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}