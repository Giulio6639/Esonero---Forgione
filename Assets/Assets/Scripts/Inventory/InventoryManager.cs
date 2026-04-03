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

    void Start()
    {
        UpdateEquippedUI();
    }

    void Update()
    {
        if (Input.GetKeyDown("q") && menuActivated)
        {
            InventoryMenu.SetActive(false);
            menuActivated = false;
            Time.timeScale = 1f;
        }
        else if (Input.GetKeyDown("q") && !menuActivated)
        {
            InventoryMenu.SetActive(true);
            menuActivated = true;
            Time.timeScale = 0f;
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
        int leftOverItems = quantity;

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == itemName)
            {
                leftOverItems = itemSlot[i].AddItem(itemName, leftOverItems, itemSprite, itemDescription);

                if (string.IsNullOrEmpty(currentEquippedItemName))
                {
                    EquipItem(itemName, itemSprite);
                }
                else
                {
                    UpdateEquippedUI();
                }

                if (leftOverItems == 0) return 0;
            }
        }

        if (leftOverItems > 0)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].itemName == "")
                {
                    leftOverItems = itemSlot[i].AddItem(itemName, leftOverItems, itemSprite, itemDescription);

                    if (string.IsNullOrEmpty(currentEquippedItemName))
                    {
                        EquipItem(itemName, itemSprite);
                    }
                    else
                    {
                        UpdateEquippedUI();
                    }

                    if (leftOverItems == 0) return 0;
                }
            }
        }

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