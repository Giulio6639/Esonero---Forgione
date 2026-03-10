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
    public int comboExtraDamage = 20;  // Danno bonus per il terzo colpo
    public float missCooldown = 0.6f;     // Quanto tempo rimani "scoperto" se manchi il colpo
    private bool attackConnected = false; // Il semaforo: ha colpito qualcuno?
    public float attackActiveDuration = 0.15f;

    [Header("Schivata (Roll)")]
    public string playerLayerName = "Player";
    public string enemyLayerName = "Enemy";
    public float rollIFrames = 0.3f;
    public float rollCooldown = 0.8f;
    private float rollCooldownTimer = 0f;
    public bool isRollInvincible { get { return m_rolling && m_rollCurrentTime <= rollIFrames; } }

    [Header("Parry System")]
    public float parryWindow = 0.25f; // Quanti secondi dura la finestra del Parry perfetto
    private float parryTimer = 0f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 5.0f;
    public float wallJumpForceY = 6.5f;
    public float wallJumpInputFreeze = 0.2f;
    private float wallJumpTimer = 0f;

    // Metodi per permettere allo script della Salute di "leggere" questi stati
    public bool isBlocking { get { return m_blocking; } }
    public bool isParrying { get { return parryTimer > 0f; } }
    public int facingDirection { get { return m_facingDirection; } }

    public bool isStunned = false;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private bool m_blocking = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
    }

    void Update()
    {
        m_timeSinceAttack += Time.deltaTime;

        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;
            if (m_rollCurrentTime > m_rollDuration)
            {
                m_rolling = false;
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemyLayerName), false);
            }
        }

        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }
        if (m_grounded && !m_groundSensor.State())
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

        // --- 1. LEGGIAMO SUBITO I MURI ---
        bool touchingRightWall = m_wallSensorR1.State() && m_wallSensorR2.State();
        bool touchingLeftWall = m_wallSensorL1.State() && m_wallSensorL2.State();
        bool touchingWallInAir = !m_grounded && (touchingRightWall || touchingLeftWall);

        // --- 2. GESTIONE SCIVOLATA ---
        m_isWallSliding = (touchingRightWall && rawInputX > 0) || (touchingLeftWall && rawInputX < 0);
        m_animator.SetBool("WallSlide", m_isWallSliding);

        if (m_isWallSliding && m_body2d.linearVelocity.y < 0)
        {
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, Mathf.Max(m_body2d.linearVelocity.y, -2f));
        }

        // --- 3. FLIP VISIVO (PROTETTO!) ---
        // NON ti giri se stai toccando un muro a mezz'aria. Questo "congela" lo sguardo verso il muro
        // finché non ti sei staccato fisicamente, nascondendo il glitch dell'animazione!
        if (m_timeSinceAttack >= 0.4f && wallJumpTimer <= 0 && !touchingWallInAir)
        {
            if (inputX > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
                m_facingDirection = 1;
                if (attackPoint != null) attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
            }
            else if (inputX < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
                m_facingDirection = -1;
                if (attackPoint != null) attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
            }
        }

        // --- 4. MOVIMENTO ---
        if (!m_rolling)
        {
            if (m_blocking || (m_grounded && m_timeSinceAttack < 0.4f))
            {
                m_body2d.linearVelocity = new Vector2(0, m_body2d.linearVelocity.y);
            }
            else if (wallJumpTimer <= 0)
            {
                m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);
            }
        }

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        if (Input.GetKeyDown("t") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }

        // --- 5. AZIONI (Combattimento, Roll, Salti) ---
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
            m_rollCurrentTime = 0f;
            rollCooldownTimer = rollCooldown;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemyLayerName), true);
        }
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling && m_timeSinceAttack >= 0.4f && !m_blocking)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        // --- WALL JUMP (Migliorato col Coyote Time!) ---
        // Ora puoi saltare anche se hai appena premuto la direzione opposta, basta che stai toccando il muro
        else if (Input.GetKeyDown("space") && touchingWallInAir && !m_grounded && !m_rolling)
        {
            int wallDir = touchingRightWall ? 1 : -1;

            m_body2d.linearVelocity = new Vector2(-wallDir * wallJumpForceX, wallJumpForceY);
            wallJumpTimer = wallJumpInputFreeze;

            // Forza l'orientamento corretto subito
            GetComponent<SpriteRenderer>().flipX = (wallDir == 1);
            m_facingDirection = -wallDir;

            if (attackPoint != null)
            {
                attackPoint.localPosition = new Vector3(m_facingDirection * Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
            }

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

    void AE_SlideDust()
    {
        if (m_slideDust != null && m_groundSensor != null)
        {
            Vector3 spawnPosition = m_groundSensor.transform.position;
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
    }

    private void OnDisable()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(enemyLayerName), false);
    }
}