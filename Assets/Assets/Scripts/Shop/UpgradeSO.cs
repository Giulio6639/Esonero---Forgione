using UnityEngine;

[CreateAssetMenu(fileName = "NuovoUpgrade", menuName = "Shop/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Costi e Livelli")]
    public int baseCost = 50;
    public int costIncreasePerLevel = 25; // Quanto aumenta il costo ogni volta che lo compri
    public int maxLevel = 5;

    [Header("Effetto")]
    public UpgradeType type;
    public int amountToIncrease = 20; // Es. Aumenta la vita di 20

    public enum UpgradeType
    {
        MaxHealth,
        AttackDamage,
        Speed,
        SwordBeam
    }
}