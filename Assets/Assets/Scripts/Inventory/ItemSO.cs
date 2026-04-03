using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    [Header("Oggetti da lanciare (es. Acqua Santa)")]
    public GameObject throwablePrefab;

    public AttributeToChange attributeToChange = new AttributeToChange();
    public int amountToChangeAttribute;

    public bool UseItem()
    {
        // 1. POZIONE DELLA SALUTE
        if (statToChange == StatToChange.health)
        {
            PlayerHealth playerHealth = GameObject.Find("HeroKnightz").GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.IsAtMaxHealth())
                {
                    Debug.Log("Vita già al massimo! La pozione non è stata usata.");
                    return false;
                }
                playerHealth.Heal(amountToChangeStat);
                return true;
            }
        }
        // 2. ACQUA SANTA (Oggetto da lanciare)
        else if (statToChange == StatToChange.holyWater)
        {
            HeroKnight player = GameObject.Find("HeroKnightz").GetComponent<HeroKnight>();
            if (player != null && throwablePrefab != null)
            {
                player.ThrowItem(throwablePrefab);
                return true;
            }
        }
        // 3. POWER UP (Forza e Velocità)
        else if (statToChange == StatToChange.powerUp)
        {
            HeroKnight player = GameObject.Find("HeroKnightz").GetComponent<HeroKnight>();
            if (player != null)
            {
                // Applica un moltiplicatore 1.5x per 30 secondi
                player.ApplyPowerUp(1.5f, 30f);
                return true;
            }
        }

        return false;
    }

    public enum StatToChange
    {
        none,
        health,
        holyWater,
        powerUp
    }
    public enum AttributeToChange
    {
        none,
        strength,
        defence,
        speed
    }
}