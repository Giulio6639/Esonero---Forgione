using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class SwordBeam : MonoBehaviour
{
    [Header("Statistiche")]
    public int damage = 20;
    public float lifeTime = 1.5f;

    [Header("Effetti di Luce")]
    [Tooltip("Trascina qui la Light 2D figlia del SwordBeam")]
    public Light2D beamLight;
    [Tooltip("Velocità con cui la luce si spegne all'impatto")]
    public float lightFadeSpeed = 15f;

    [Header("Audio (SFX)")]
    [Tooltip("L'AudioSource attaccato a questo Prefab")]
    public AudioSource audioSource;
    [Tooltip("Suono quando il raggio viene sparato")]
    public AudioClip launchSound;
    [Tooltip("Suono quando il raggio colpisce qualcosa")]
    public AudioClip impactSound;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D coll;

    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // --- SFX LANCIO ---
        if (audioSource != null && launchSound != null)
        {
            // Variazione di pitch per non renderlo monotono se spammi l'attacco
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(launchSound);
        }
        // ------------------

        // --- LA CINTURA DI SICUREZZA ---
        Destroy(gameObject, lifeTime + 1f);
        // -------------------------------

        StartCoroutine(AutoVanishRoutine());
    }

    private IEnumerator AutoVanishRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!hasHit)
        {
            TriggerVanish();
        }
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Ignora se stesso, il giocatore, o altri trigger (come sensori visivi)
        if (hasHit || hitInfo.CompareTag("Player") || hitInfo.isTrigger) return;

        EnemyHealth enemy = hitInfo.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform, false);
        }

        TriggerVanish();
    }

    private void TriggerVanish()
    {
        hasHit = true;
        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;

        // --- SFX IMPATTO ---
        if (audioSource != null && impactSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(impactSound);
        }
        // -------------------

        // 1. Inizia a spegnere la luce dolcemente
        if (beamLight != null)
        {
            StartCoroutine(FadeLightRoutine());
        }

        // 2. Fai partire l'animazione di impatto
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            DestroyProjectile();
        }
    }

    // --- COROUTINE PER SPEGNERE LA LUCE ---
    private IEnumerator FadeLightRoutine()
    {
        // Finché la luce esiste e la sua intensità è maggiore di 0...
        while (beamLight != null && beamLight.intensity > 0)
        {
            // ... riduci l'intensità gradualmente
            beamLight.intensity -= lightFadeSpeed * Time.deltaTime;
            yield return null; // Aspetta il prossimo frame
        }
    }
    // --------------------------------------

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}