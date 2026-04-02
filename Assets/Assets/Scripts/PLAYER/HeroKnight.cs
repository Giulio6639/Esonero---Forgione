using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroKnight : MonoBehaviour
{
    [Header("Azioni")]
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    [Header("Combattimento")]
    public Transform attackPoint;
    public float attackRadius = 0.6f;
    public LayerMask enemyLayer;
    public int attackDamage = 30;
    public int comboExtraDamage = 20;
    public float missCooldown = 0.6f;
    private bool attackConnected = false;
    public float attackActiveDuration = 0.15f;

    [Header("Sword Beam (Full Health)")]
    public GameObject swordBeamPrefab;
    public float beamSpeed = 10f;
    public Transform beamSpawnPoint;
    public float beamSpawnDelay = 0.1f;

    [Header("Sensori (Raycast & Slopes)")]
    public LayerMask groundLayer;
    public string stairsLayerName = "Stairs";
    public float dropThroughTime = 0.6f;
    public float wallCheckDistance = 0.4f;
    public float wallCheckHeight = 0.5f;

    [Header("Schivata (Roll)")]
    public string enemyLayerName = "Enemy";
    public float rollIFrames = 0.3f;
    public float rollCooldown = 0.8f;
    private float rollCooldownTimer = 0f;
    public bool isRollInvincible { get { return m_rolling && m_rollCurrentTime <= rollIFrames; } }

    [Header("Parry System")]
    public float parryWindow = 0.25f;
    private float parryTimer = 0f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 5.0f;
    public float wallJumpForceY = 6.5f;
    public float wallJumpInputFreeze = 0.2f;
    private float wallJumpTimer = 0f;

    public bool isBlocking { get { return m_blocking; } }
    public bool isParrying { get { return parryTimer > 0f; } }
    public int facingDirection { get { return m_facingDirection; } }
    public bool isStunned = false;

    private Animator m_animator;
    private Rigidbody2D m_body2d;

    private bool m_isWallSliding = false;
    public bool m_grounded = false;
    private bool m_rolling = false;
    private bool m_blocking = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private float defaultGravity;

    private PlayerHealth playerHealth;
    private Collider2D playerCol;

    // Variabili Layer e Scale
    private int playerLayerID;
    private int stairsLayerID;
    private bool isDroppingThrough = false;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        playerCol = GetComponent<Collider2D>();

        defaultGravity = m_body2d.gravityScale;

        // IL FIX AUTOMATICO: Prende il Layer reale del tuo GameObject senza che tu debba scriverlo!
        playerLayerID = gameObject.layer;
        stairsLayerID = LayerMask.NameToLayer(stairsLayerName);

        if (stairsLayerID == -1)
        {
            Debug.LogError("ATTENZIONE: Non esiste nessun Layer chiamato 'Stairs'. Crealo in alto a destra su Unity!");
        }
    }
    public bool canInteract
    {
        get
        {
            return m_grounded && !m_rolling && !m_blocking && !isStunned && (m_timeSinceAttack >= 0.4f) && !isDroppingThrough;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        m_timeSinceAttack += Time.deltaTime;

        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;
            if (m_rollCurrentTime > m_rollDuration)
            {
                m_rolling = false;
                Physics2D.IgnoreLayerCollision(playerLayerID, LayerMask.NameToLayer(enemyLayerName), false);
            }


        }

        // --- LA MASCHERA DINAMICA ---
        bool isGhostToStairs = false;
        if (stairsLayerID != -1)
        {
            isGhostToStairs = Physics2D.GetIgnoreLayerCollision(playerLayerID, stairsLayerID);
        }

        int currentGroundMask = groundLayer.value;
        if (isGhostToStairs && stairsLayerID != -1)
        {
            // Se siamo fantasma, rimuoviamo le scale dalla vista dei sensori
            currentGroundMask &= ~(1 << stairsLayerID);
        }
        // ----------------------------

        // --- CALCOLO SENSORI ---
        Vector2 groundCheckPos = new Vector2(playerCol.bounds.center.x, playerCol.bounds.min.y + 0.1f);
        Vector2 groundCheckSize = new Vector2(playerCol.bounds.size.x * 0.5f, 0.3f);
        RaycastHit2D groundHit = Physics2D.BoxCast(groundCheckPos, groundCheckSize, 0f, Vector2.down, 0.3f, currentGroundMask);
        bool isGroundedNow = groundHit.collider != null;

        RaycastHit2D slopeHit = Physics2D.Raycast(new Vector2(playerCol.bounds.center.x, playerCol.bounds.min.y + 0.2f), Vector2.down, 0.7f, currentGroundMask);
        bool isOnSlope = slopeHit.collider != null && slopeHit.normal.y < 0.99f && slopeHit.normal.y > 0.1f;

        if (!isGroundedNow || !isOnSlope) m_body2d.gravityScale = defaultGravity;

        // --- LA MAGIA DELL'ATTRAVERSAMENTO SCALE ---
        if (stairsLayerID != -1)
        {
            if (isDroppingThrough)
            {
                Physics2D.IgnoreLayerCollision(playerLayerID, stairsLayerID, true);
            }
            else
            {
                bool isJumpingUp = m_body2d.linearVelocity.y > 0.1f && !m_grounded;
                bool isFalling = m_body2d.linearVelocity.y < -0.1f && !m_grounded;

                if (isJumpingUp)
                {
                    Physics2D.IgnoreLayerCollision(playerLayerID, stairsLayerID, true);
                }
                else if (isFalling)
                {
                    Physics2D.IgnoreLayerCollision(playerLayerID, stairsLayerID, false);
                }
                else if (m_grounded && !isOnSlope)
                {
                    Physics2D.IgnoreLayerCollision(playerLayerID, stairsLayerID, true);
                }
            }
        }
        // -------------------------------------------

        float centerY = playerCol.bounds.center.y;
        Vector2 topWallPos = new Vector2(transform.position.x, centerY + wallCheckHeight);
        Vector2 botWallPos = new Vector2(transform.position.x, centerY - wallCheckHeight + 0.2f);

        bool touchingRightWall = Physics2D.Raycast(topWallPos, Vector2.right, wallCheckDistance, currentGroundMask) &&
                                 Physics2D.Raycast(botWallPos, Vector2.right, wallCheckDistance, currentGroundMask);

        bool touchingLeftWall = Physics2D.Raycast(topWallPos, Vector2.left, wallCheckDistance, currentGroundMask) &&
                                 Physics2D.Raycast(botWallPos, Vector2.left, wallCheckDistance, currentGroundMask);

        // --- SCENDERE DALLE SCALE CON 'S' ---
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && m_grounded && !m_rolling)
        {
            StartCoroutine(DropThroughStairsRoutine());
        }
        // ------------------------------------

        if (!m_grounded && isGroundedNow)
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }
        if (m_grounded && !isGroundedNow)
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (parryTimer > 0) parryTimer -= Time.deltaTime;
        if (rollCooldownTimer > 0) rollCooldownTimer -= Time.deltaTime;
        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;

        if (isStunned)
        {
            m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);
            return;
        }

        float inputX = Input.GetAxis("Horizontal");
        float rawInputX = Input.GetAxisRaw("Horizontal");

        bool touchingWallInAir = !m_grounded && (touchingRightWall || touchingLeftWall);

        m_isWallSliding = (touchingRightWall && rawInputX > 0) || (touchingLeftWall && rawInputX < 0);
        m_animator.SetBool("WallSlide", m_isWallSliding);

        if (m_isWallSliding && m_body2d.linearVelocity.y < 0)
        {
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, Mathf.Max(m_body2d.linearVelocity.y, -2f));
        }

        if (m_timeSinceAttack >= 0.4f && wallJumpTimer <= 0 && !touchingWallInAir)
        {
            if (inputX > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
                m_facingDirection = 1;
                if (attackPoint != null) attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
                if (beamSpawnPoint != null) beamSpawnPoint.localPosition = new Vector3(Mathf.Abs(beamSpawnPoint.localPosition.x), beamSpawnPoint.localPosition.y, beamSpawnPoint.localPosition.z);
            }
            else if (inputX < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
                m_facingDirection = -1;
                if (attackPoint != null) attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
                if (beamSpawnPoint != null) beamSpawnPoint.localPosition = new Vector3(-Mathf.Abs(beamSpawnPoint.localPosition.x), beamSpawnPoint.localPosition.y, beamSpawnPoint.localPosition.z);
            }
        }

        if (!m_rolling)
        {
            if (m_blocking || (m_grounded && m_timeSinceAttack < 0.4f))
            {
                m_body2d.linearVelocity = new Vector2(0, m_body2d.linearVelocity.y);
                if (isOnSlope) m_body2d.gravityScale = 0f;
            }
            else if (wallJumpTimer <= 0)
            {
                float targetVelX = inputX * m_speed;
                float targetVelY = m_body2d.linearVelocity.y;

                if (m_grounded && isOnSlope)
                {
                    if (Mathf.Abs(inputX) < 0.01f)
                    {
                        targetVelX = 0f;
                        targetVelY = 0f;
                        m_body2d.gravityScale = 0f;
                    }
                    else
                    {
                        m_body2d.gravityScale = defaultGravity;
                        Vector2 slopePerp = new Vector2(slopeHit.normal.y, -slopeHit.normal.x);
                        targetVelX = slopePerp.x * inputX * m_speed;
                        targetVelY = slopePerp.y * inputX * m_speed;
                        if (targetVelY < 0) targetVelY -= 2f;
                    }
                }

                m_body2d.linearVelocity = new Vector2(targetVelX, targetVelY);
            }
        }

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        if (Input.GetKeyDown("t") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }

        float requiredCooldown = (attackConnected || m_timeSinceAttack > 1.0f) ? 0.25f : missCooldown;

        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack >= requiredCooldown && !m_rolling && !m_blocking)
        {
            if (m_timeSinceAttack > 1.0f || !attackConnected) m_currentAttack = 1;
            else
            {
                m_currentAttack++;
                if (m_currentAttack > 3) m_currentAttack = 1;
            }

            m_animator.SetTrigger("Attack" + m_currentAttack);

            if (m_currentAttack == 1 && swordBeamPrefab != null && playerHealth != null && playerHealth.IsAtMaxHealth())
            {
                StartCoroutine(ShootSwordBeamRoutine());
            }

            m_timeSinceAttack = 0.0f;
            attackConnected = false;
        }
        else if (Input.GetMouseButtonDown(1) && m_grounded && !m_rolling && m_timeSinceAttack >= 0.4f)
        {
            m_blocking = true;
            parryTimer = parryWindow;
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (!Input.GetMouseButton(1) && m_blocking)
        {
            m_blocking = false;
            m_animator.SetBool("IdleBlock", false);
        }
        else if (m_blocking && !m_grounded)
        {
            m_blocking = false;
            m_animator.SetBool("IdleBlock", false);
        }
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding && !m_blocking && rollCooldownTimer <= 0f)
        {
            m_rolling = true;
            m_body2d.gravityScale = defaultGravity;
            m_rollCurrentTime = 0f;
            rollCooldownTimer = rollCooldown;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
            Physics2D.IgnoreLayerCollision(playerLayerID, LayerMask.NameToLayer(enemyLayerName), true);
        }
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling && m_timeSinceAttack >= 0.4f && !m_blocking)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_body2d.gravityScale = defaultGravity;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
        }
        else if (Input.GetKeyDown("space") && touchingWallInAir && !m_grounded && !m_rolling)
        {
            int wallDir = touchingRightWall ? 1 : -1;

            m_body2d.linearVelocity = new Vector2(-wallDir * wallJumpForceX, wallJumpForceY);
            wallJumpTimer = wallJumpInputFreeze;

            GetComponent<SpriteRenderer>().flipX = (wallDir == 1);
            m_facingDirection = -wallDir;

            if (attackPoint != null) attackPoint.localPosition = new Vector3(m_facingDirection * Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
            if (beamSpawnPoint != null) beamSpawnPoint.localPosition = new Vector3(m_facingDirection * Mathf.Abs(beamSpawnPoint.localPosition.x), beamSpawnPoint.localPosition.y, beamSpawnPoint.localPosition.z);

            m_animator.SetTrigger("Jump");
            m_isWallSliding = false;
        }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    private IEnumerator DropThroughStairsRoutine()
    {
        isDroppingThrough = true;
        yield return new WaitForSeconds(dropThroughTime);
        isDroppingThrough = false;
    }

    private IEnumerator ShootSwordBeamRoutine()
    {
        yield return new WaitForSeconds(beamSpawnDelay);
        ShootSwordBeam();
    }

    void ShootSwordBeam()
    {
        Transform spawnPointToUse = (beamSpawnPoint != null) ? beamSpawnPoint : attackPoint;
        if (spawnPointToUse == null) return;
        GameObject beam = Instantiate(swordBeamPrefab, spawnPointToUse.position, Quaternion.identity);
        Rigidbody2D beamRb = beam.GetComponent<Rigidbody2D>();

        if (beamRb != null)
        {
            beamRb.linearVelocity = new Vector2(m_facingDirection * beamSpeed, 0);
            if (m_facingDirection < 0)
            {
                Vector3 scale = beam.transform.localScale;
                scale.x *= -1;
                beam.transform.localScale = scale;
            }
        }
    }

    void AE_SlideDust()
    {
        if (m_slideDust != null && playerCol != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y -= playerCol.bounds.extents.y;
            spawnPosition.x -= m_facingDirection * 0.3f;
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            dust.transform.localScale = new Vector3(-m_facingDirection, 1, 1);
            dust.transform.Rotate(0f, 0f, 90f * m_facingDirection);
        }
    }

    void AE_AttackHit()
    {
        if (attackPoint == null) return;
        StartCoroutine(ActiveHitboxRoutine());
    }

    private IEnumerator ActiveHitboxRoutine()
    {
        float timer = attackActiveDuration;
        List<Collider2D> enemiesAlreadyHit = new List<Collider2D>();
        bool isComboFinisher = (m_currentAttack == 3);
        int finalDamage = isComboFinisher ? (attackDamage + comboExtraDamage) : attackDamage;

        while (timer > 0)
        {
            Collider2D[] currentHits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
            foreach (Collider2D enemy in currentHits)
            {
                if (!enemiesAlreadyHit.Contains(enemy))
                {
                    enemiesAlreadyHit.Add(enemy);
                    attackConnected = true;
                    EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                    if (health != null) health.TakeDamage(finalDamage, transform, isComboFinisher);
                }
            }
            timer -= Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        if (beamSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(beamSpawnPoint.position, 0.15f);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.green;
        Vector2 groundPos = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.1f);
        Vector2 groundSize = new Vector2(col.bounds.size.x * 0.5f, 0.3f);
        Gizmos.DrawWireCube(groundPos + Vector2.down * 0.15f, groundSize);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(col.bounds.center.x, col.bounds.min.y + 0.2f), new Vector2(col.bounds.center.x, col.bounds.min.y - 0.5f));

        Gizmos.color = Color.yellow;
        float centerY = col.bounds.center.y;
        Vector2 topPos = new Vector2(transform.position.x, centerY + wallCheckHeight);
        Vector2 botPos = new Vector2(transform.position.x, centerY - wallCheckHeight + 0.2f);

        Gizmos.DrawLine(topPos, topPos + Vector2.right * wallCheckDistance);
        Gizmos.DrawLine(botPos, botPos + Vector2.right * wallCheckDistance);
        Gizmos.DrawLine(topPos, topPos + Vector2.left * wallCheckDistance);
        Gizmos.DrawLine(botPos, botPos + Vector2.left * wallCheckDistance);
    }

    private void OnDisable()
    {
        Physics2D.IgnoreLayerCollision(playerLayerID, LayerMask.NameToLayer(enemyLayerName), false);
    }
    
}