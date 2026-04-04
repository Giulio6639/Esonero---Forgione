using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Sistema di Vite")]
    public int startingLives = 3;
    public int currentLives;
    public Transform respawnPoint;

    [Header("UI Salute")]
    public Image healthBarFill;
    public RectTransform healthBarContainer; // Il riquadro da allungare
    public float pixelPerPuntoVita = 2f;     // Moltiplicatore: quanto è largo 1 HP?

    [Header("Impostazioni Invincibilità")]
    public float iFramesDuration = 1.5f;
    private float iFramesTimer;
    private bool isInvincible = false;

    [Header("Knockback Normale")]
    public float knockbackForceX = 4f;
    public float knockbackForceY = 3f;
    public float knockbackDuration = 0.25f;

    [Header("Knockback in Parata")]
    public float blockKnockbackX = 2f;
    public float blockKnockbackY = 0f;
    public float blockKnockbackDuration = 0.1f;

    [Header("Riferimenti")]
    private Animator animator;
    private HeroKnight playerController;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        currentLives = startingLives;

        animator = GetComponent<Animator>();
        playerController = GetComponent<HeroKnight>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        AggiornaDimensioneBarra(); // <--- AGGIUNGI QUI
        UpdateHealthUI();
    }

    void Update()
    {
        if (isInvincible)
        {
            iFramesTimer -= Time.deltaTime;
            if (iFramesTimer > 0)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.PingPong(Time.time * 10f, 1f));
            }
            else
            {
                isInvincible = false;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return;
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log("Curato di " + healAmount + ". Vita attuale: " + currentHealth);
    }

    public void TakeDamage(int damageAmount, Transform attacker, bool isComboFinisher = false)
    {
        if (currentHealth <= 0 || isInvincible || playerController.isRollInvincible) return;

        float dirToAttacker = attacker.position.x - transform.position.x;
        bool attackFromFront = (dirToAttacker > 0 && playerController.facingDirection == 1) ||
                               (dirToAttacker < 0 && playerController.facingDirection == -1);

        if (attackFromFront && playerController.isBlocking)
        {
            if (playerController.isParrying)
            {
                Debug.Log("PARRY PERFETTO! 0 Danni.");
                return;
            }
            else
            {
                damageAmount = Mathf.Max(1, damageAmount / 2);
                currentHealth -= damageAmount;
                UpdateHealthUI();
                isInvincible = true;
                iFramesTimer = iFramesDuration;
                if (currentHealth <= 0) Die();
                else StartCoroutine(ApplyBlockKnockback(attacker));
                return;
            }
        }

        currentHealth -= damageAmount;
        UpdateHealthUI();
        isInvincible = true;
        iFramesTimer = iFramesDuration;

        if (currentHealth <= 0) Die();
        else
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(ApplyKnockback(attacker));
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        AggiornaDimensioneBarra(); // <--- AGGIUNGI QUI
        UpdateHealthUI();

        Debug.Log("Vita massima aumentata! Nuova vita massima: " + maxHealth);
    }
    private void AggiornaDimensioneBarra()
    {
        if (healthBarContainer != null)
        {
            // Cambia solo la larghezza (X) in base alla maxHealth, lascia inalterata l'altezza (Y)
            healthBarContainer.sizeDelta = new Vector2(maxHealth * pixelPerPuntoVita, healthBarContainer.sizeDelta.y);
        }
    }
    private IEnumerator ApplyKnockback(Transform attacker)
    {
        playerController.isStunned = true;
        float direction = transform.position.x < attacker.position.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * knockbackForceX, knockbackForceY);
        yield return new WaitForSeconds(knockbackDuration);
        if (currentHealth > 0) playerController.isStunned = false;
    }

    private IEnumerator ApplyBlockKnockback(Transform attacker)
    {
        playerController.isStunned = true;
        float direction = transform.position.x < attacker.position.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * blockKnockbackX, blockKnockbackY);
        yield return new WaitForSeconds(blockKnockbackDuration);
        if (currentHealth > 0) playerController.isStunned = false;
    }

    public bool IsAtMaxHealth() { return currentHealth >= maxHealth; }

    private void Die()
    {
        animator.SetTrigger("Death");
        playerController.isStunned = true;
        rb.linearVelocity = Vector2.zero;
        currentHealth = 0;
        UpdateHealthUI();
        currentLives--;

        if (currentLives > 0) StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(2f);
        if (respawnPoint != null) transform.position = respawnPoint.position;
        currentHealth = maxHealth;
        UpdateHealthUI();
        animator.ResetTrigger("Death");
        animator.Play("Idle");
        playerController.isStunned = false;
        isInvincible = true;
        iFramesTimer = 2f;
    }
}