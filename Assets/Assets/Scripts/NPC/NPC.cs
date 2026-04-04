using UnityEngine;

public abstract class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer _interactSprite;

    [Header("Impostazioni Range")]
    [Tooltip("Distanza a cui compare la freccia")]
    [SerializeField] private float _visualRange = 4f;

    private Transform _playerTransform;
    private bool _isPlayerInRange = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        if (_interactSprite != null)
        {
            _interactSprite.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_playerTransform == null) return;

        bool isCloseEnoughForSprite = Vector2.Distance(_playerTransform.position, transform.position) < _visualRange;

        if (_interactSprite != null && _interactSprite.gameObject.activeSelf != isCloseEnoughForSprite)
        {
            _interactSprite.gameObject.SetActive(isCloseEnoughForSprite);
        }

        // --- IL TEST DELLA VERITÀ ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("1. Hai premuto E!");
            Debug.Log("2. Sei dentro il collider (Trigger)? " + _isPlayerInRange);

            if (_isPlayerInRange)
            {
                HeroKnight playerScript = _playerTransform.GetComponent<HeroKnight>();
                if (playerScript != null)
                {
                    Debug.Log("3. L'HeroKnight può interagire (Grounded, non in attacco)? " + playerScript.canInteract);

                    if (playerScript.canInteract || Time.timeScale == 0f)
                    {
                        Debug.Log("4. INTERAZIONE PARTITA!");
                        Interact();
                    }
                }
            }
        }
    }

    public abstract void Interact();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("--- IL PLAYER HA TOCCATO LA FONTANA ---");
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("--- IL PLAYER È USCITO DALLA FONTANA ---");
            _isPlayerInRange = false;
        }
    }
}