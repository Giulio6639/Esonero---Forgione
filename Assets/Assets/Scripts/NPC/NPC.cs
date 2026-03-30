using UnityEngine;
using UnityEngine.InputSystem;

public abstract class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer _interactSprite;

    [Header("Impostazioni Range")]
    [Tooltip("Distanza a cui compare la freccia")]
    [SerializeField] private float _visualRange = 4f;

    private Transform _playerTransform;
    private bool _isPlayerInRange = false; // Gestito dal BoxCollider

    private void Start()
    {
        // Riprendiamo il riferimento al player come nel tuo script originale
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        // Nascondiamo lo sprite all'avvio
        if (_interactSprite != null)
        {
            _interactSprite.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_playerTransform == null) return;

        // --- 1. GESTIONE FRECCIA (Range ampio visivo) ---
        // Calcola la distanza matematica come facevi prima
        bool isCloseEnoughForSprite = Vector2.Distance(_playerTransform.position, transform.position) < _visualRange;

        // Accende o spegne la freccia in base alla distanza
        if (_interactSprite != null && _interactSprite.gameObject.activeSelf != isCloseEnoughForSprite)
        {
            _interactSprite.gameObject.SetActive(isCloseEnoughForSprite);
        }

        // --- 2. GESTIONE INTERAZIONE (Range stretto fisico) ---
        if (_isPlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Recuperiamo lo script HeroKnight dal player
            HeroKnight playerScript = _playerTransform.GetComponent<HeroKnight>();

            if (playerScript != null && (playerScript.canInteract || Time.timeScale == 0f))
            {
                Interact();
            }
        }
    }

    public abstract void Interact();

    // Si attiva SOLO quando tocchi fisicamente il BoxCollider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInRange = true;
        }
    }

    // Si disattiva quando esci dal BoxCollider
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }
}