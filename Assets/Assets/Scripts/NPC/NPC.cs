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

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (InventoryManager.isInventoryOpen) return;

            if (_isPlayerInRange)
            {
                HeroKnight playerScript = _playerTransform.GetComponent<HeroKnight>();
                if (playerScript != null)
                {
                    if (playerScript.canInteract || Time.timeScale == 0f)
                    {
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