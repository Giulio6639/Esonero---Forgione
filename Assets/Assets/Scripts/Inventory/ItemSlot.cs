using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    // --- NUOVA VARIABILE ---
    [Header("Restrizioni Slot")]
    [Tooltip("Scrivi il nome esatto dell'oggetto. Lascia vuoto per accettare qualsiasi cosa.")]
    public string allowedItemName = "";
    // -----------------------

    // ========ITEM DATA=========
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    [SerializeField]
    public int maxNumberOfItems;

    // ========ITEM SLOT=========
    [SerializeField]
    private TMP_Text quantityText;

    //========ITEM DESCRIPTION SLOT=========
    public Image ItemDescriptionImage;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    [SerializeField]
    private Image itemImage;

    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
    }

    public int AddItem(string newItemName, int newQuantity, Sprite newItemSprite, string newItemDescription)
    {
        if (isFull) return newQuantity;

        this.itemName = newItemName;
        this.itemSprite = newItemSprite;
        this.itemImage.sprite = itemSprite;
        this.itemDescription = newItemDescription;

        this.quantity += newQuantity;

        if (this.quantity > 0)
        {
            quantityText.color = Color.white;
        }

        if (this.quantity >= maxNumberOfItems)
        {
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            quantityText.text = this.quantity.ToString();
            quantityText.enabled = true;
            isFull = true;
            return extraItems;
        }

        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;
        isFull = false;
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        inventoryManager.DeselectAllSlots();
        selectedShader.SetActive(true);
        thisItemSelected = true;

        if (itemName != "")
        {
            ItemDescriptionNameText.text = itemName;
            ItemDescriptionText.text = itemDescription;
            ItemDescriptionImage.sprite = itemSprite;

            inventoryManager.EquipItem(itemName, itemSprite);
        }
        else
        {
            ItemDescriptionNameText.text = "";
            ItemDescriptionText.text = "";
            ItemDescriptionImage.sprite = emptySprite;
        }
    }

    public void ConsumeOne()
    {
        quantity--;

        // --- IL FIX E' QUI ---
        // Se abbiamo consumato un oggetto, lo slot sicuramente NON è più pieno
        if (quantity < maxNumberOfItems)
        {
            isFull = false;
        }
        // ---------------------

        if (quantity <= 0)
        {
            quantity = 0;
            quantityText.text = quantity.ToString();
            quantityText.color = Color.red;
        }
        else
        {
            quantityText.text = quantity.ToString();
        }
    }

    public void EmptySlot()
    {
        itemName = "";
        quantity = 0;
        itemSprite = emptySprite;
        itemDescription = "";
        isFull = false;

        quantityText.enabled = false;
        itemImage.sprite = emptySprite;
    }

    public void OnRightClick() { }
}