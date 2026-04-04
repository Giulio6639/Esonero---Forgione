using UnityEngine;

public class GemmaPickup : MonoBehaviour
{
    public int valore = 1;

    // Cambiato da OnTriggerEnter2D a OnCollisionEnter2D
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<HeroKnight>() != null)
        {
            CurrencyManager.Instance.AggiungiGemme(valore);
            Destroy(gameObject);
        }
    }
}