using UnityEngine;
using System.Collections;

public class WizardAI : MonoBehaviour
{
    [Header("Movimento")]
    public float runSpeed = 2f;
    public float chaseSpeed = 3.5f;

    [Header("Sensori (Distanze)")]
    public float sightRange = 8f;
    public float attackRange = 3.5f;
    public float evadeRange = 2.5f;

    [Header("Combattimento - Attacco 1")]
    public Transform attackPoint1;
    public float attackHitRadius1 = 0.6f;

    [Header("Combattimento - Attacco 2 (Aereo/Pesante)")]
    public Transform attackPoint2;
    public float attackHitRadius2 = 0.8f;

    [Header("Combattimento - Statistiche")]
    public LayerMask playerLayer;
    public int attackDamage = 20;
    public float attackCooldown = 3f;

    [Header("Combattimento - Tempistiche Combo & Salto")]
    public float attack1Duration = 0.5f;
    public float attack2Duration = 0.6f;
    public float jumpForceY = 12f;
    public float jumpSpeedX = 6f;
    public float evadeCooldown = 6f;

    [Header("Riferimenti")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private bool facingRight = true;

    // --- VARIABILI PER IL LAMPEGGIO ---
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    // ----------------------------------

    private enum State { Idle, Wander, Chase, Attacking, Cooldown }
    private State currentState;

    private float stateTimer;
    private float evadeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- PRENDIAMO IL COLORE INIZIALE ---
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        // ------------------------------------

        currentState = State.Idle;
        stateTimer = Random.Range(1f, 2f);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // Gestione Timer
        if (evadeTimer > 0) evadeTimer -= Time.deltaTime;

        // --- FIX: SPOSTATO IN CIMA! ---
        // Ora legge SEMPRE la gravità e aggiorna l'animator, 
        // anche se sta facendo l'attacco speciale!
        if (rb.linearVelocity.y > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        else if (rb.linearVelocity.y < -0.1f)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        // ------------------------------

        // Ora possiamo fermare il "cervello" AI senza bloccare le animazioni del corpo
        if (currentState == State.Attacking) return;

        float distToPlayer = 100f;
        if (player != null) distToPlayer = Vector2.Distance(transform.position, player.position);

        // IL RIFLESSO ASSOLUTO
        if (distToPlayer <= evadeRange && evadeTimer <= 0)
        {
            currentState = State.Attacking;
            StartCoroutine(EvadeAndPlungeRoutine());
            return;
        }

        switch (currentState)
        {
            case State.Idle:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isRunning", false);
                if (distToPlayer <= sightRange) { currentState = State.Chase; break; }

                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    int wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;
                    Flip(wanderDirection);
                    rb.linearVelocity = new Vector2(runSpeed * wanderDirection, rb.linearVelocity.y);
                    animator.SetBool("isRunning", true);
                    stateTimer = Random.Range(1.5f, 3f);
                    currentState = State.Wander;
                }
                break;

            case State.Wander:
                stateTimer -= Time.deltaTime;
                if (distToPlayer <= sightRange || stateTimer <= 0)
                {
                    currentState = State.Idle;
                }
                break;

            case State.Chase:
                animator.SetBool("isRunning", true);

                if (distToPlayer <= attackRange)
                {
                    currentState = State.Attacking;
                    StartCoroutine(BaseComboRoutine());
                    break;
                }

                if (distToPlayer > sightRange) { currentState = State.Idle; break; }

                int chaseDir = player.position.x > transform.position.x ? 1 : -1;
                rb.linearVelocity = new Vector2(chaseSpeed * chaseDir, rb.linearVelocity.y);
                Flip(chaseDir);
                break;

            case State.Cooldown:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { currentState = State.Idle; }
                break;
        }
    }

    private IEnumerator BaseComboRoutine()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);

        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);

        int lookDir = player.position.x > transform.position.x ? 1 : -1;
        Flip(lookDir);

        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(attack1Duration);

        animator.SetTrigger("Attack2");
        yield return new WaitForSeconds(attack2Duration);

        stateTimer = attackCooldown;
        currentState = State.Cooldown;
    }

    private IEnumerator EvadeAndPlungeRoutine()
    {
        evadeTimer = evadeCooldown;
        animator.SetBool("isRunning", false);

        int jumpDir = player.position.x > transform.position.x ? 1 : -1;
        Flip(jumpDir);

        rb.linearVelocity = new Vector2(jumpDir * jumpSpeedX, jumpForceY);

        yield return new WaitUntil(() => rb.linearVelocity.y < 0);

        int newLookDir = player.position.x > transform.position.x ? 1 : -1;
        Flip(newLookDir);

        animator.SetBool("isFalling", false);
        animator.SetTrigger("Attack2");

        yield return new WaitUntil(() => rb.linearVelocity.y >= -0.1f && rb.linearVelocity.y <= 0.1f);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        stateTimer = attackCooldown;
        currentState = State.Cooldown;
    }

    public void InterruptCombo()
    {
        StopAllCoroutines();

        // --- SICUREZZA LAMPEGGIO ---
        // Se interrompiamo una Coroutine di lampeggio a metà, assicuriamoci 
        // che il mago non rimanga bianco per sempre!
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        stateTimer = 1.0f;
        currentState = State.Cooldown;

        // Fai partire il lampeggio per il colpo appena subito
        StartCoroutine(FlashRoutine());
    }

    // --- COROUTINE DEL LAMPEGGIO ---
    private IEnumerator FlashRoutine()
    {
        if (spriteRenderer != null)
        {
            // Diventa bianco (puoi usare Color.red se preferisci un flash rosso!)
            spriteRenderer.color = Color.white;

            // Aspetta un decimo di secondo (durata perfetta per un flash di impatto)
            yield return new WaitForSeconds(0.1f);

            // Torna normale
            spriteRenderer.color = originalColor;
        }
    }

    public void TriggerAttackHit(int attackIndex)
    {
        if (player != null)
        {
            int lookDir = player.position.x > transform.position.x ? 1 : -1;
            Flip(lookDir);
        }

        Transform activePoint = (attackIndex == 1) ? attackPoint1 : attackPoint2;
        float activeRadius = (attackIndex == 1) ? attackHitRadius1 : attackHitRadius2;

        if (activePoint == null) return;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(activePoint.position, activeRadius, playerLayer);
        foreach (Collider2D playerHit in hitPlayers)
        {
            PlayerHealth health = playerHit.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(attackDamage, transform);
        }
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, evadeRange);

        if (attackPoint1 != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint1.position, attackHitRadius1);
        }
        if (attackPoint2 != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint2.position, attackHitRadius2);
        }
    }
}