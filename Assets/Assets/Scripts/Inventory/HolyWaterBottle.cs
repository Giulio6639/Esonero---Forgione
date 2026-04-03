using UnityEngine;

public class HolyWaterBottle : MonoBehaviour
{
    [Header("Impostazioni Lancio")]
    public float throwForceX = 5f;
    public float throwForceY = 4f;

    [Header("Cosa genera all'impatto?")]
    public GameObject firePrefab; // Qui metterai il Prefab delle fiamme

    public void Initialize(int direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Diamo la spinta in avanti e verso l'alto (effetto arco)
        rb.linearVelocity = new Vector2(throwForceX * direction, throwForceY);

        // Facciamo girare lo sprite in base alla direzione
        transform.localScale = new Vector3(direction, 1, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.isTrigger) return;

        if (firePrefab != null)
        {
            // Creiamo un "Offset" (uno scarto). 
            // Alziamo la posizione Y di 0.5 (puoi aumentare questo numero se la fiamma è enorme)
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            // Generiamo il fuoco sulla nuova posizione rialzata
            Instantiate(firePrefab, spawnPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}