using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Sistema di Vite & Spawn")]
    public int startingLives = 3;
    public int currentLives;
    public Transform respawnPoint; // Ora verrà aggiornato dai Checkpoint!

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("UI Salute")]
    public Image healthBarFill;
    public RectTransform healthBarMaster;
    public float pixelPerPuntoVita = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip healSound;
    public AudioClip perfectParrySound;
    public AudioClip normalBlockSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

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

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        AggiornaDimensioneBarra();
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

        if (currentHealth < maxHealth && audioSource != null && healSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(healSound);
        }

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
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
                if (audioSource != null && perfectParrySound != null)
                {
                    audioSource.pitch = 1f;
                    audioSource.PlayOneShot(perfectParrySound);
                }
                return;
            }
            else
            {
                if (audioSource != null && normalBlockSound != null)
                {
                    audioSource.pitch = 1f;
                    audioSource.PlayOneShot(normalBlockSound);
                }

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

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (audioSource != null && hurtSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(hurtSound);
            }

            animator.SetTrigger("Hurt");
            StartCoroutine(ApplyKnockback(attacker));
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        AggiornaDimensioneBarra();
        UpdateHealthUI();

        if (audioSource != null && healSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(healSound);
        }
    }

    private void AggiornaDimensioneBarra()
    {
        if (healthBarMaster != null)
        {
            healthBarMaster.sizeDelta = new Vector2(maxHealth * pixelPerPuntoVita, healthBarMaster.sizeDelta.y);
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

    // --- NUOVA FUNZIONE PER I CHECKPOINT ---
    public void SetRespawnPoint(Transform newPoint)
    {
        respawnPoint = newPoint;
    }
    // ---------------------------------------

    private void Die()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(deathSound);
        }

        animator.SetTrigger("Death");
        playerController.isStunned = true;
        rb.linearVelocity = Vector2.zero;
        currentHealth = 0;
        UpdateHealthUI();

        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // ==========================================
    // FUNZIONI PER I PULSANTI DELLA UI
    // ==========================================

    public void RetryGame()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // --- PENALITÀ: DIMEZZA LE GEMME ---
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.DimezzaGemme();
        }
        // ----------------------------------

        if (respawnPoint != null) transform.position = respawnPoint.position;

        currentHealth = maxHealth;
        UpdateHealthUI();

        animator.ResetTrigger("Death");
        animator.Play("Idle");
        playerController.isStunned = false;

        isInvincible = true;
        iFramesTimer = 2f;
    }
    // --- AGGIUNGI QUESTA FUNZIONE ALLA FINE DI PLAYERHEALTH ---
    public void InstantDeath()
    {
        // Ignora qualsiasi invincibilità e azzera la vita
        currentHealth = 0;
        UpdateHealthUI();

        // Richiama la tua sequenza di morte (GameOver, Suoni, ecc.)
        Die();
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}