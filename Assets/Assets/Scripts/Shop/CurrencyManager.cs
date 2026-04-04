using UnityEngine;
using TMPro; // Per usare la UI (TextMeshPro)

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Valuta")]
    public int gemmeTotali = 0;

    [Header("UI")]
    public TextMeshProUGUI testoGemmeUI; // Trascina qui il tuo testo della UI

    void Awake()
    {
        // Pattern Singleton per accedervi da qualsiasi script
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AggiungiGemme(int quantita)
    {
        gemmeTotali += quantita;

        if (testoGemmeUI != null)
        {
            testoGemmeUI.text = gemmeTotali.ToString();
        }
    }
    public bool SpendiGemme(int costo)
    {
        if (gemmeTotali >= costo)
        {
            gemmeTotali -= costo;
            if (testoGemmeUI != null) testoGemmeUI.text = gemmeTotali.ToString();
            return true;
        }
        return false;
    }
}