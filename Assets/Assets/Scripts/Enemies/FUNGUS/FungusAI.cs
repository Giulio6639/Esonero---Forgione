using UnityEngine;
using System.Collections;

public class FungusAI : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 1f;
    public float chaseSpeed = 2.0f;

    [Header("Sensori (Distanze)")]
    public float sightRange = 6f;
    public float attackRange = 1.8f;

    [Header("Combattimento - Statistiche")]
    public Transform attackPoint;
    public float attackHitRadius = 0.7f;
    public LayerMask playerLayer;
    public int attackDamage = 25;
    public float attackCooldown = 4f;

    [Header("Combattimento - Tempistiche Combo")]
    public float attack1Duration = 0.8f;
    public float comboPause = 0.3f;
    public float heavyStepSpeed = 3f;
    public float attack2Duration = 0.6f;

    // --- NUOVE VARIABILI SUPER ARMOR ---
    [Header("Super Armor (Poise)")]
    public int hitsToEnrage = 3;          // Quanti colpi servono per farlo infuriare
    public float armorDuration = 3.5f;    // Quanto dura l'armatura in secondi

    [HideInInspector]
    public bool hasSuperArmor = false;    // La variabile che legge EnemyHealth

    private int hitCounter = 0;           // Conta i colpi ricevuti
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    // -----------------------------------

    [Header("Riferimenti")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private bool facingRight = true;

    private enum State { Idle, Wander, Chase, Attacking, Cooldown }
    private State currentState;

    private float stateTimer;
    private int wanderDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

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
        float distToPlayer = 100f;
        if (player != null) distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isWalking", false);
                if (distToPlayer <= sightRange) { currentState = State.Chase; break; }

                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;
                    stateTimer = Random.Range(2f, 4f);
                    currentState = State.Wander;
                }
                break;

            case State.Wander:
                rb.linearVelocity = new Vector2(walkSpeed * wanderDirection, rb.linearVelocity.y);
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
                if (distToPlayer <= attackRange)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    animator.SetBool("isWalking", false);

                    int lookDir = player.position.x > transform.position.x ? 1 : -1;
                    Flip(lookDir);

                    currentState = State.Attacking;
                    StartCoroutine(FungusComboRoutine(lookDir));
                    break;
                }

                animator.SetBool("isWalking", true);
                if (distToPlayer > sightRange) { currentState = State.Idle; break; }

                int chaseDir = player.position.x > transform.position.x ? 1 : -1;
                rb.linearVelocity = new Vector2(chaseSpeed * chaseDir, rb.linearVelocity.y);
                Flip(chaseDir);
                break;

            case State.Attacking:
                break;

            case State.Cooldown:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { currentState = State.Idle; }
                break;
        }
    }

    private IEnumerator FungusComboRoutine(int direction)
    {
        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(attack1Duration);

        yield return new WaitForSeconds(comboPause);

        animator.SetTrigger("Attack2");
        rb.linearVelocity = new Vector2(direction * heavyStepSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(attack2Duration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        stateTimer = attackCooldown;
        currentState = State.Cooldown;
    }

    // --- NUOVA VERSIONE: INTERROMPE LA COMBO SOLO SE NON HA L'ARMATURA ---
    public void InterruptCombo()
    {
        // Se l'armatura è attiva, ignora questo comando!
        if (hasSuperArmor) return;

        hitCounter++;

        if (hitCounter >= hitsToEnrage)
        {
            // Attiva la Super Armor!
            StartCoroutine(ActivateSuperArmor());
        }
        else
        {
            // Stordimento normale
            StopAllCoroutines();
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            stateTimer = 1.5f;
            currentState = State.Cooldown;
        }
    }

    // --- COROUTINE PER LA SUPER ARMOR ---
    private IEnumerator ActivateSuperArmor()
    {
        hasSuperArmor = true;
        hitCounter = 0; // Azzera i colpi per la prossima volta

        // Opzionale: fa diventare il fungo un po' rosso/scuro per far capire al giocatore che è arrabbiato
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.5f, 0.5f);

        // Aspetta che finisca il tempo
        yield return new WaitForSeconds(armorDuration);

        hasSuperArmor = false;
        if (spriteRenderer != null) spriteRenderer.color = originalColor; // Torna normale
    }

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

    // --- METODO PER IL LAMPEGGIO (FEEDBACK DANNO) ---
    public void PlayFlashEffect()
    {
        // Facciamo partire il lampeggio solo se non ne sta già facendo uno
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Colore bianco flash (o un rosso molto acceso)
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(0.1f); // Durata brevissima

        // Torna al colore dell'armatura se è ancora attiva, altrimenti originale
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hasSuperArmor ? new Color(1f, 0.5f, 0.5f) : originalColor;
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}