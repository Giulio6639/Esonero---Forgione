using UnityEngine;

public class HolyWaterBottle : MonoBehaviour
{
    [Header("Impostazioni Lancio")]
    public float throwForceX = 5f;
    public float throwForceY = 4f;

    [Header("Cosa genera all'impatto?")]
    public GameObject firePrefab;

    [Header("Regolazione Posizione Fuoco (Offset)")]
    [Tooltip("Sposta il fuoco a destra (valori positivi) o a sinistra (valori negativi)")]
    public float spawnOffsetX = 0f;
    [Tooltip("Sposta il fuoco in alto (valori positivi) o in basso (valori negativi)")]
    public float spawnOffsetY = 0.5f;

    // Questa variabile ci serve per capire da che parte stiamo guardando, 
    // così l'offset sull'asse X si adatta alla direzione!
    private int currentDirection = 1;

    public void Initialize(int direction)
    {
        currentDirection = direction; // Salviamo la direzione per usarla nell'impatto

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Diamo la spinta
        rb.linearVelocity = new Vector2(throwForceX * direction, throwForceY);

        // Orientiamo la bottiglia
        transform.localScale = new Vector3(direction, 1, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignoriamo il Player e i trigger (come la visuale dei nemici)
        if (collision.CompareTag("Player") || collision.isTrigger) return;

        if (firePrefab != null)
        {
            // Calcoliamo la posizione esatta. 
            // Moltiplichiamo l'offsetX per la direzione, così se lanci a sinistra
            // l'offset non finisce dalla parte sbagliata!
            Vector3 spawnPosition = new Vector3(
                transform.position.x + (spawnOffsetX * currentDirection),
                transform.position.y + spawnOffsetY,
                transform.position.z
            );

            // Spawna il fuoco con le nuove coordinate modificate
            Instantiate(firePrefab, spawnPosition, Quaternion.identity);
        }

        // Distruggi la bottiglia all'impatto
        Destroy(gameObject);
    }
}