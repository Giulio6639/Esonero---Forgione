using System.Collections;
using UnityEngine;
using UnityEngine.UI; // ATTENZIONE: Questa riga è FONDAMENTALE per usare la UI!

public class PlayerHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Salute")]
    public Image healthBarFill; // Il riferimento alla nostra barra rossa

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
        animator = GetComponent<Animator>();
        playerController = GetComponent<HeroKnight>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Aggiorna la barra della vita appena il gioco inizia
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

    // --- NUOVA FUNZIONE PER LA UI ---
    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // Calcola la percentuale (es. 50 / 100 = 0.5f)
            // Usiamo (float) per costringere Unity a calcolare i decimali
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public void TakeDamage(int damageAmount, Transform attacker)
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
                Debug.Log("Parata Normale: Danno dimezzato, scudo su!");
                damageAmount = Mathf.Max(1, damageAmount / 2);

                currentHealth -= damageAmount;
                UpdateHealthUI(); // Aggiorna la UI dopo aver parato

                isInvincible = true;
                iFramesTimer = iFramesDuration;

                if (currentHealth <= 0) Die();
                else StartCoroutine(ApplyBlockKnockback(attacker));

                return;
            }
        }

        currentHealth -= damageAmount;
        UpdateHealthUI(); // Aggiorna la UI per un colpo normale

        isInvincible = true;
        iFramesTimer = iFramesDuration;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(ApplyKnockback(attacker));
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

    private void Die()
    {
        animator.SetTrigger("Death");
        playerController.isStunned = true;
        rb.linearVelocity = Vector2.zero;

        // Assicurati che la vita non vada sotto zero visivamente
        currentHealth = 0;
        UpdateHealthUI();
    }
}