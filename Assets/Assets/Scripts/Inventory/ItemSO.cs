using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributeToChange attributeToChange = new AttributeToChange();
    public int amountToChangeAttribute;
    public bool UseItem()
    {
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

        return false;
    }

    public enum StatToChange
    {
        none,
        health
    }
    public enum AttributeToChange
    {
        none,
        strength,
        defence,
        speed
    }
}