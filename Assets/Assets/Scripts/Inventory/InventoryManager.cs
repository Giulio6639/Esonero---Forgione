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

                // --- NASCONDI IL MOUSE QUANDO CHIUDI L'INVENTARIO ---
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                // ----------------------------------------------------
            }
            else
            {
                InventoryMenu.SetActive(true);
                menuActivated = true;
                isInventoryOpen = true;
                Time.timeScale = 0f;

                // --- MOSTRA IL MOUSE QUANDO APRI L'INVENTARIO ---
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                // ------------------------------------------------
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
        int actualQuantityToAdd = quantity;

        // Logica del CAP Globale per le Red Potion
        if (itemName == "Red Potion")
        {
            int currentTotal = 0;

            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].itemName == itemName)
                {
                    currentTotal += itemSlot[i].quantity;
                }
            }

            int maxCap = 3;
            int spaceLeft = maxCap - currentTotal;

            if (spaceLeft <= 0)
            {
                Debug.Log("Hai già il massimo di Red Potion (3).");
                return 0;
            }

            actualQuantityToAdd = Mathf.Min(quantity, spaceLeft);
        }

        int leftOverItems = actualQuantityToAdd;

        // 1. Cerca negli slot che contengono GIÀ questo oggetto (Stacking)
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

        // 2. Se avanza qualcosa, cerca uno slot VUOTO, ma che ACCETTI questo oggetto
        if (leftOverItems > 0)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].itemName == "")
                {
                    // --- LA MODIFICA È QUI ---
                    // Controllo: lo slot è vuoto ("") OPPURE il suo allowedName è uguale al nome dell'oggetto?
                    bool canPlaceHere = string.IsNullOrEmpty(itemSlot[i].allowedItemName) || itemSlot[i].allowedItemName == itemName;

                    if (canPlaceHere)
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
        }

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