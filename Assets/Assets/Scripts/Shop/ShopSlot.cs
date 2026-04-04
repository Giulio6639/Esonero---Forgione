using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopSlot : MonoBehaviour, IPointerClickHandler
{
    public UpgradeSO upgradeData;

    [HideInInspector]
    public int currentLevel = 0;

    [Header("UI dello Slot")]
    public Image iconImage;
    public TMP_Text levelText;
    public GameObject selectedShader;

    private void Start()
    {
        if (upgradeData != null)
        {
            iconImage.sprite = upgradeData.icon;
            AggiornaTestoLivello();
        }
        selectedShader.SetActive(false);
    }

    public void AggiornaTestoLivello()
    {
        if (currentLevel >= upgradeData.maxLevel)
            levelText.text = "MAX";
        else
            levelText.text = "Lv." + currentLevel;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && upgradeData != null)
        {
            // --- LA MODIFICA È QUI ---
            // Se questo slot è GIÀ quello selezionato, chiediamo la conferma
            if (ShopManager.Instance.GetSlotSelezionato() == this)
            {
                ShopManager.Instance.ApriPannelloConferma();
            }
            else
            {
                // Altrimenti, lo selezioniamo normalmente
                ShopManager.Instance.SelezionaSlot(this);
            }
        }
    }
}