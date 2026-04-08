using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 60; // RICORDA: nell'Inspector per il fungo, alza questo valore a 150 o 200!
    public int currentHealth;

    [Header("Impostazioni Boss")]
    public bool isBoss = false; // NUOVO: Spunta questa casella nell'Inspector per il Mago!

    [Header("Danno da Contatto")]
    public bool dealsContactDamage = true;
    public int contactDamage = 10;

    [Header("Knockback")]
    public float knockbackForceX = 3f;
    public float knockbackForceY = 2f;
    public float knockbackDuration = 0.2f;

    [Header("Audio SFX (Nemici)")]
    [Tooltip("L'AudioSource collegato a questo nemico (usato per i danni)")]
    public AudioSource audioSource;
    [Tooltip("Suono riprodotto ogni volta che il nemico prende danno (Hurt)")]
    public AudioClip hurtSound;
    [Tooltip("Suono riprodotto quando il nemico muore")]
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 1f; // Volume generale per i suoni di questo nemico

    [Header("Drop Gemme")]
    public List<GemDrop> tabellaDropGemme;
    public float forzaEsplosioneGemme = 4f; // Per far schizzare le gemme in aria

    [Header("Morte e Particelle")]
    public float disappearDelay = 2.5f;
    public GameObject deathParticles;

    [Header("Riferimenti")]
    public Behaviour aiScript;
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Sicurezza: se hai dimenticato di collegare l'AudioSource nell'Inspector, lo cerca da solo
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentHealth <= 0 || !dealsContactDamage) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage, transform);
        }
    }

    public void TakeDamage(int damageAmount, Transform attacker, bool applyKnockback = true)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;

        // --- SFX DANNO ---
        // Suona l'effetto "Hurt" solo se non è un colpo letale (altrimenti suona la morte)
        if (currentHealth > 0 && audioSource != null && hurtSound != null)
        {
            // Usiamo un leggero cambio di pitch randomico per variare il suono dei colpi ripetuti
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hurtSound, soundVolume);
        }
        // -----------------

        bool ignoreStun = false;
        FungusAI fungusAI = GetComponent<FungusAI>();
        if (fungusAI != null)
        {
            fungusAI.InterruptCombo();
            if (fungusAI.hasSuperArmor) ignoreStun = true;
            fungusAI.PlayFlashEffect();
        }

        GoblinAI goblinAI = GetComponent<GoblinAI>();
        if (goblinAI != null) goblinAI.InterruptCombo();

        WizardAI wizardAI = GetComponent<WizardAI>();
        if (wizardAI != null)
        {
            if (wizardAI.hasSuperArmor)
            {
                ignoreStun = true;
                wizardAI.PlayFlashEffect();
            }
            else
            {
                wizardAI.InterruptCombo();
            }
        }
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (ignoreStun)
            {
                return; // Il nemico prende danno, ma non si ferma!
            }

            animator.SetTrigger("Hurt");
            StartCoroutine(HitReaction(attacker, applyKnockback));
        }

        SamuraiAI samuraiAI = GetComponent<SamuraiAI>();
        if (samuraiAI != null)
        {
            if (samuraiAI.hasSuperArmor)
            {
                ignoreStun = true;
                samuraiAI.PlayFlashEffect();
            }
            else
            {
                samuraiAI.InterruptCombo();
            }
        }
    }

    private IEnumerator HitReaction(Transform attacker, bool applyKnockback)
    {
        // Disabilita lo script AI per lo stordimento (ignorando quelli che si gestiscono da soli)
        if (aiScript != null && GetComponent<GoblinAI>() == null && GetComponent<FungusAI>() == null && GetComponent<WizardAI>() == null)
            aiScript.enabled = false;

        if (applyKnockback)
        {
            float direction = transform.position.x < attacker.position.x ? -1f : 1f;
            rb.linearVelocity = new Vector2(direction * knockbackForceX, knockbackForceY);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(knockbackDuration);

        // Riabilita
        if (currentHealth > 0 && aiScript != null && GetComponent<GoblinAI>() == null && GetComponent<FungusAI>() == null && GetComponent<WizardAI>() == null)
        {
            aiScript.enabled = true;
        }
    }

    private void Die()
    {
        // --- SFX MORTE ---
        if (deathSound != null)
        {
            // Usiamo PlayClipAtPoint così il suono finisce anche se il nemico scompare subito
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }
        // -----------------

        // --- LA MODIFICA CHIAVE ---
        if (animator != null)
        {
            if (isBoss)
            {
                // Se è il boss, subisce il colpo e aspetta il dialogo
                animator.SetTrigger("Hurt");
            }
            else
            {
                // Se è un nemico normale, muore subito
                animator.SetBool("isDead", true);
            }

            DropGemme();
        }

        // Spegni tutte le IA possibili
        GoblinAI goblin = GetComponent<GoblinAI>();
        if (goblin != null) goblin.enabled = false;

        SkeletonAI skeleton = GetComponent<SkeletonAI>();
        if (skeleton != null) skeleton.enabled = false;

        FungusAI fungus = GetComponent<FungusAI>();
        if (fungus != null) fungus.enabled = false;

        WizardAI wizard = GetComponent<WizardAI>();
        if (wizard != null) wizard.enabled = false;

        if (aiScript != null) aiScript.enabled = false;

        SamuraiAI samurai = GetComponent<SamuraiAI>();
        if (samurai != null) samurai.enabled = false;

        // Blocca il corpo
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Spegni i collider
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        // Se NON è un boss, sparisce dopo 2.5 secondi. Il boss invece rimane lì finché non lo decide il Manager!
        if (!isBoss)
        {
            StartCoroutine(DisappearRoutine());
        }

    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);

        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    [System.Serializable]
    public class GemDrop
    {
        public GameObject gemmaPrefab;
        public int quantitaMinima = 1;
        public int quantitaMassima = 3;
        [Range(0f, 100f)] public float probabilitaDrop = 100f; // Percentuale di drop
    }

    private void DropGemme()
    {
        foreach (GemDrop drop in tabellaDropGemme)
        {
            // Tiriamo il dado per vedere se questa gemma deve droppare
            float randomChance = Random.Range(0f, 100f);
            if (randomChance <= drop.probabilitaDrop)
            {
                // Scegliamo quante gemme di questo tipo droppare
                int quantita = Random.Range(drop.quantitaMinima, drop.quantitaMassima + 1);

                for (int i = 0; i < quantita; i++)
                {
                    GameObject gemma = Instantiate(drop.gemmaPrefab, transform.position, Quaternion.identity);

                    // Se la gemma ha un Rigidbody2D, le diamo una spinta verso l'alto/lati per un bell'effetto "esplosione di monete"
                    Rigidbody2D rbGemma = gemma.GetComponent<Rigidbody2D>();
                    if (rbGemma != null)
                    {
                        Vector2 direzioneCasuale = new Vector2(Random.Range(-1f, 1f), Random.Range(0.8f, 1.5f)).normalized;
                        rbGemma.AddForce(direzioneCasuale * forzaEsplosioneGemme, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}