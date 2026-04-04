using UnityEngine;

public class ShopNPC : NPC // Eredita dalla tua classe base!
{
    public override void Interact()
    {
        // Se il pannello è chiuso, lo apre
        if (!ShopManager.Instance.shopPanel.activeSelf)
        {
            ShopManager.Instance.ApriShop();
        }
    }
}