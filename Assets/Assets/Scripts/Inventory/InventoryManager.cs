using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    public bool menuActivated;
    public ItemSlot[] itemSlot;
    public ItemSO[] itemSOs;

    void Start()
    {
        
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
    }
    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
            if (itemSOs[i].itemName == itemName)
            {
                bool usable = itemSOs[i].UseItem();
                return usable;
            }
        }
        return false;
    }
    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemName == itemName)
            {
                int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                return leftOverItems;
            }
        }
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].quantity == 0)
            {
                int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                return leftOverItems;
            }
        }
        return quantity;
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
