using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 2f;
    public float chaseSpeed = 3.5f;

    [Header("Sensori (Distanze)")]
    public float sightRange = 6f;
    public float attackRange = 1.2f;

    [Header("Combattimento")]
    public Transform attackPoint;
    public float attackHitRadius = 0.5f;
    public LayerMask playerLayer;
    public int attackDamage = 20;

    public float attackCooldown = 4f;

    [Header("Riferimenti")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private bool facingRight = true;

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

                    // --- MODIFICA QUI ---
                    // Invece di int randomAttack = Random.Range(1, 3);
                    // Diciamo all'Animator di usare SEMPRE e SOLO l'indice 1
                    animator.SetInteger("AttackIndex", 1);
                    animator.SetTrigger("Attack");

                    stateTimer = attackCooldown;
                    currentState = State.Cooldown;
                    break;
                }

                animator.SetBool("isWalking", true);
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

    public void TriggerAttackHit()
    {
        if (attackPoint == null) return;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackHitRadius, playerLayer);

        foreach (Collider2D playerHit in hitPlayers)
        {
            PlayerHealth health = playerHit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage, transform);
            }
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
}