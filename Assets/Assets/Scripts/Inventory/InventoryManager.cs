using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    public bool menuActivated;
    public ItemSlot[] itemSlot;
    public ItemSO[] itemSOs;

    [Header("UI Oggetto Equipaggiato")]
    public Image equippedIcon;
    public TMP_Text equippedAmountText;
    public Sprite emptyEquippedSprite;

    private string currentEquippedItemName = "";
    private Sprite currentEquippedSprite;

    public static bool isInventoryOpen = false;
    void Start()
    {
        UpdateEquippedUI();
    }

    void Update()
    {
        // Intercettiamo il tasto Q come primissima cosa
        if (Input.GetKeyDown("q"))
        {
            // Controlliamo cosa c'è a schermo
            bool isShopOpen = ShopManager.Instance != null && ShopManager.Instance.shopPanel.activeSelf;
            bool isDialogueOpen = DialogueController.isDialogueActive;

            if (isShopOpen || isDialogueOpen)
            {
                return;
            }

            if (menuActivated)
            {
                InventoryMenu.SetActive(false);
                menuActivated = false;
                isInventoryOpen = false;
                Time.timeScale = 1f;
            }
            else
            {
                InventoryMenu.SetActive(true);
                menuActivated = true;
                isInventoryOpen = true;
                Time.timeScale = 0f; 
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            UseEquippedItem();
        }
    }

    public void EquipItem(string itemName, Sprite itemSprite)
    {
        currentEquippedItemName = itemName;
        currentEquippedSprite = itemSprite;
        UpdateEquippedUI();
    }

    public void UpdateEquippedUI()
    {
        if (string.IsNullOrEmpty(currentEquippedItemName))
        {
            equippedIcon.sprite = emptyEquippedSprite;
            equippedAmountText.text = "";
            return;
        }

        int totalQuantity = 0;
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == currentEquippedItemName)
            {
                totalQuantity += itemSlot[i].quantity;
            }
        }

        equippedIcon.sprite = currentEquippedSprite;
        equippedAmountText.text = totalQuantity.ToString();

        if (totalQuantity > 0)
        {
            equippedAmountText.color = Color.white;
        }
        else
        {
            equippedAmountText.color = Color.red;
        }
    }

    private void UseEquippedItem()
    {
        if (string.IsNullOrEmpty(currentEquippedItemName)) return;

        ItemSlot slotToUse = null;
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == currentEquippedItemName && itemSlot[i].quantity > 0)
            {
                slotToUse = itemSlot[i];
                break;
            }
        }

        if (slotToUse != null)
        {
            bool usable = UseItem(currentEquippedItemName);

            if (usable)
            {
                slotToUse.ConsumeOne();
                UpdateEquippedUI();
            }
        }
    }

    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
            if (itemSOs[i].itemName == itemName)
            {
                return itemSOs[i].UseItem();
            }
        }
        return false;
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        // Variabile per la quantità effettiva che andremo ad aggiungere
        int actualQuantityToAdd = quantity;

        // --- NUOVO: Logica del CAP Globale per le Red Potion ---
        if (itemName == "Red Potion")
        {
            int currentTotal = 0;

            // 1. Contiamo quante Red Potion abbiamo già in tutto l'inventario
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].itemName == itemName)
                {
                    currentTotal += itemSlot[i].quantity;
                }
            }

            // 2. Calcoliamo quanto spazio rimane per arrivare al massimo di 3
            int maxCap = 3;
            int spaceLeft = maxCap - currentTotal;

            // Se siamo già a 3 o più, restituiamo 0 (l'oggetto a terra/fontana scompare ma non aggiungiamo nulla)
            if (spaceLeft <= 0)
            {
                Debug.Log("Hai già il massimo di Red Potion (3).");
                return 0;
            }

            // 3. Limitiamo la quantità: se ne raccogliamo 3 ma ne manca solo 1, actualQuantityToAdd diventa 1
            actualQuantityToAdd = Mathf.Min(quantity, spaceLeft);
        }

        // --- DA QUI IN POI USIAMO 'actualQuantityToAdd' INVECE DI 'quantity' ---
        int leftOverItems = actualQuantityToAdd;

        // Cerca negli slot che contengono già questo oggetto
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == itemName)
            {
                leftOverItems = itemSlot[i].AddItem(itemName, leftOverItems, itemSprite, itemDescription);

                if (string.IsNullOrEmpty(currentEquippedItemName))
                    EquipItem(itemName, itemSprite);
                else
                    UpdateEquippedUI();

                if (leftOverItems == 0) return 0;
            }
        }

        // Se avanza qualcosa, cerca slot vuoti
        if (leftOverItems > 0)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].itemName == "")
                {
                    leftOverItems = itemSlot[i].AddItem(itemName, leftOverItems, itemSprite, itemDescription);

                    if (string.IsNullOrEmpty(currentEquippedItemName))
                        EquipItem(itemName, itemSprite);
                    else
                        UpdateEquippedUI();

                    if (leftOverItems == 0) return 0;
                }
            }
        }

        // Se itemName era "Red Potion", restituiamo comunque 0 perché vogliamo che l'eccesso sparisca
        if (itemName == "Red Potion") return 0;

        return leftOverItems;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }
}