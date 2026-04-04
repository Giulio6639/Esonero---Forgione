using UnityEngine;
using System.Collections;

public class SamuraiAI : MonoBehaviour
{
    [Header("Movimento")]
    public float runSpeed = 3f;
    public float chaseSpeed = 5.0f;

    [Header("Sensori (Distanze)")]
    public float sightRange = 10f;
    public float attackRange = 2.5f;

    [Header("Combattimento - Statistiche")]
    public Transform attackPoint;
    public float attackHitRadius = 0.8f;
    public LayerMask playerLayer;
    public int attackDamage = 30; // Danno alto, è il boss finale!
    public float attackCooldown = 2.5f;

    [Header("Combattimento - Tempistiche Combo")]
    public float attack1Duration = 0.5f;
    public float comboPause = 0.2f;
    public float lungeSpeed = 8f; // Velocità dello scatto in avanti durante l'Attack2
    public float attack2Duration = 0.6f;

    [Header("Riferimenti")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private bool facingRight = true;

    // --- VARIABILI PER IL LAMPEGGIO ---
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    // ----------------------------------

    private enum State { Idle, Wander, Chase, Attacking, Cooldown }
    private State currentState;

    private float stateTimer;

    // --- PROPRIETÀ SUPER ARMOR ---
    // Il Samurai non viene interrotto mentre esegue la sua combo
    public bool hasSuperArmor
    {
        get { return currentState == State.Attacking; }
    }
    // -----------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        currentState = State.Idle;
        stateTimer = Random.Range(1.5f, 3f);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // Gestione animazioni Salto e Caduta
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

        // Se sta attaccando, la Coroutine gestisce tutto
        if (currentState == State.Attacking) return;

        float distToPlayer = 100f;
        if (player != null) distToPlayer = Vector2.Distance(transform.position, player.position);

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
                if (distToPlayer <= sightRange) { currentState = State.Chase; break; }

                if (stateTimer <= 0)
                {
                    currentState = State.Idle;
                }
                break;

            case State.Chase:
                animator.SetBool("isRunning", true);

                if (distToPlayer <= attackRange)
                {
                    currentState = State.Attacking;
                    StartCoroutine(SamuraiComboRoutine());
                    break;
                }

                if (distToPlayer > sightRange) { currentState = State.Idle; break; }

                int chaseDir = player.position.x > transform.position.x ? 1 : -1;
                rb.linearVelocity = new Vector2(chaseSpeed * chaseDir, rb.linearVelocity.y);
                Flip(chaseDir);
                break;

            case State.Cooldown:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isRunning", false);
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { currentState = State.Idle; }
                break;
        }
    }

    private IEnumerator SamuraiComboRoutine()
    {
        // Si ferma per preparare l'attacco
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);

        int direction = player.position.x > transform.position.x ? 1 : -1;
        Flip(direction);

        // PRIMO ATTACCO (Fendente sul posto)
        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(attack1Duration);

        yield return new WaitForSeconds(comboPause);

        // SECONDO ATTACCO (Scatto in avanti)
        animator.SetTrigger("Attack2");
        // Applica una forza in avanti per simulare l'affondo
        rb.linearVelocity = new Vector2(direction * lungeSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(attack2Duration);

        // Fine combo, reset velocità e stato
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        stateTimer = attackCooldown;
        currentState = State.Cooldown;
    }

    // --- METODO CHIAMATO DA ENEMYHEALTH ---
    public void InterruptCombo()
    {
        StopAllCoroutines();

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        stateTimer = 1.0f; // Pausa post-hit
        currentState = State.Cooldown;

        PlayFlashEffect();
    }

    // --- FUNZIONE PER IL LAMPEGGIO (FEEDBACK DANNO) ---
    public void PlayFlashEffect()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    // --- METODO CHIAMATO DAGLI ANIMATION EVENTS ---
    public void TriggerAttackHit()
    {
        if (attackPoint == null) return;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackHitRadius, playerLayer);
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

        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRadius);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}