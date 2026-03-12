using UnityEngine;
using System.Collections;

public class GoblinAI : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 2f;
    public float chaseSpeed = 4.0f;

    [Header("Sensori (Distanze)")]
    public float sightRange = 6f;
    public float attackRange = 1.5f;

    [Header("Combattimento - Statistiche")]
    public Transform attackPoint;
    public float attackHitRadius = 0.5f;
    public LayerMask playerLayer;
    public int attackDamage = 15;
    public float attackCooldown = 3f;

    [Header("Combattimento - Tempistiche Combo")]
    public float attack1Duration = 0.4f;
    public float hopBackSpeed = 5f;
    public float hopBackDuration = 0.2f;
    public float lungeForwardSpeed = 6f;
    public float lungeDuration = 0.3f;

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
                    stateTimer = Random.Range(1.5f, 3f);
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
                    StartCoroutine(GoblinComboRoutine(lookDir));
                    break;
                }

                animator.SetBool("isWalking", true);
                if (distToPlayer > sightRange) { currentState = State.Idle; break; }

                int chaseDir = player.position.x > transform.position.x ? 1 : -1;
                rb.linearVelocity = new Vector2(chaseSpeed * chaseDir, rb.linearVelocity.y);
                Flip(chaseDir);
                break;

            case State.Attacking:
                // La Coroutine comanda qui
                break;

            case State.Cooldown:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { currentState = State.Idle; }
                break;
        }
    }

    private IEnumerator GoblinComboRoutine(int direction)
    {
        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(attack1Duration);

        animator.SetTrigger("Attack2");

        rb.linearVelocity = new Vector2(-direction * hopBackSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(hopBackDuration);

        rb.linearVelocity = new Vector2(direction * lungeForwardSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(lungeDuration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        stateTimer = attackCooldown;
        currentState = State.Cooldown;
    }

    // --- METODO CHIAMATO DA ENEMYHEALTH ---
    public void InterruptCombo()
    {
        StopAllCoroutines();

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        stateTimer = 1.0f; // Pausa di 1 secondo dopo aver preso un colpo
        currentState = State.Cooldown;
    }

    public void TriggerAttackHit()
    {
        if (attackPoint == null) return;

        // AGGIUNGI QUESTA RIGA:
        Debug.Log("IL GOBLIN HA DATO UNA COLTELLATA!");

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