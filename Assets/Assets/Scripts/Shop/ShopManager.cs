using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public GameObject shopPanel;
    public ShopSlot[] shopSlots;

    [Header("UI Descrizione Upgrade")]
    public TMP_Text upgradeNameText;
    public TMP_Text upgradeDescriptionText;
    public TMP_Text upgradeCostText;

    [Header("Pannello di Conferma Acquisto")]
    public GameObject pannelloConferma; // Il nuovo pannello con i 2 bottoni
    public TMP_Text testoMessaggioConferma;

    private ShopSlot slotSelezionato;

    // Metodo per permettere allo Slot di sapere chi è selezionato
    public ShopSlot GetSlotSelezionato() { return slotSelezionato; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Update()
    {
        // Controlla se il pannello è aperto E se il giocatore preme il tasto ESC
        if (shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ChiudiShop();
        }
    }
    public void ApriShop()
    {
        shopPanel.SetActive(true);
        Time.timeScale = 0f;
        PulisciSelezione();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ChiudiShop()
    {
        shopPanel.SetActive(false);
        ChiudiPannelloConferma(); // Assicuriamoci che si chiuda anche questo
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SelezionaSlot(ShopSlot slotCliccato)
    {
        DeselectAllSlots();
        ChiudiPannelloConferma(); // Se clicco su un altro slot, nascondo la conferma

        slotSelezionato = slotCliccato;
        slotSelezionato.selectedShader.SetActive(true);

        upgradeNameText.text = slotSelezionato.upgradeData.upgradeName;
        upgradeDescriptionText.text = slotSelezionato.upgradeData.description;

        if (slotSelezionato.currentLevel >= slotSelezionato.upgradeData.maxLevel)
        {
            upgradeCostText.text = "MASSIMIZZATO";
        }
        else
        {
            int costoAttuale = slotSelezionato.upgradeData.baseCost + (slotSelezionato.currentLevel * slotSelezionato.upgradeData.costIncreasePerLevel);
            upgradeCostText.text = "Costo: " + costoAttuale + " Gemme";
        }
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] != null && shopSlots[i].selectedShader != null)
                shopSlots[i].selectedShader.SetActive(false);
        }
    }

    private void PulisciSelezione()
    {
        DeselectAllSlots();
        slotSelezionato = null;
        upgradeNameText.text = "Seleziona un Potenziamento";
        upgradeDescriptionText.text = "";
        upgradeCostText.text = "";
        ChiudiPannelloConferma();
    }

    // --- NUOVA LOGICA DI CONFERMA ---

    public void ApriPannelloConferma()
    {
        if (slotSelezionato == null || slotSelezionato.currentLevel >= slotSelezionato.upgradeData.maxLevel)
            return; // Non fa niente se è maxato o vuoto

        int costoAttuale = slotSelezionato.upgradeData.baseCost + (slotSelezionato.currentLevel * slotSelezionato.upgradeData.costIncreasePerLevel);

        pannelloConferma.SetActive(true);
        testoMessaggioConferma.text = "Vuoi acquistare " + slotSelezionato.upgradeData.upgradeName + "\nper " + costoAttuale + " Gemme?";
    }

    public void ChiudiPannelloConferma()
    {
        if (pannelloConferma != null)
            pannelloConferma.SetActive(false);
    }

    // Questa funzione va attaccata al bottone "Acquista" dentro il Pannello di Conferma
    public void ConfermaAcquisto()
    {
        int costoAttuale = slotSelezionato.upgradeData.baseCost + (slotSelezionato.currentLevel * slotSelezionato.upgradeData.costIncreasePerLevel);

        if (CurrencyManager.Instance.SpendiGemme(costoAttuale))
        {
            ApplicaPotenziamento(slotSelezionato.upgradeData);

            slotSelezionato.currentLevel++;
            slotSelezionato.AggiornaTestoLivello();
            SelezionaSlot(slotSelezionato); // Aggiorna i testi del costo

            ChiudiPannelloConferma(); // Compra e chiude il popup
        }
        else
        {
            Debug.Log("Non hai abbastanza gemme!");
            testoMessaggioConferma.text = "<color=red>Gemme insufficienti!</color>";
        }
    }

    private void ApplicaPotenziamento(UpgradeSO data)
    {
        HeroKnight player = Object.FindFirstObjectByType<HeroKnight>();
        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();

        if (player == null || health == null) return;

        switch (data.type)
        {
            case UpgradeSO.UpgradeType.MaxHealth:
                health.IncreaseMaxHealth(data.amountToIncrease);
                break;
            case UpgradeSO.UpgradeType.AttackDamage:
                player.attackDamage += data.amountToIncrease;
                break;
            case UpgradeSO.UpgradeType.Speed:
                Debug.Log("Velocità aumentata!");
                break;
            case UpgradeSO.UpgradeType.SwordBeam: // <--- NUOVO CASO
                player.hasSwordBeamUnlocked = true;
                Debug.Log("Raggio della Spada SBLOCCATO!");
                break;
        }
    }
}