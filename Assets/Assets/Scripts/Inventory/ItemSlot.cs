using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
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
        inventoryManager = GameObject.Find("Canvas").GetComponent<InventoryManager>();
    }

    public int AddItem(string newItemName, int newQuantity, Sprite newItemSprite, string newItemDescription)
    {
        // Se lo slot è già completamente pieno, rifiutiamo in blocco la quantità in arrivo
        if (isFull)
        {
            return newQuantity;
        }

        this.itemName = newItemName;
        this.itemSprite = newItemSprite;
        this.itemImage.sprite = itemSprite;
        this.itemDescription = newItemDescription;

        this.quantity += newQuantity;

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
        if (thisItemSelected && quantity > 0)
        {
            bool usable = inventoryManager.UseItem(itemName);

            if (usable)
            {
                quantity--;
                quantityText.text = quantity.ToString();

                if (quantity <= 0)
                {
                    EmptySlot();
                }
            }
        }

        inventoryManager.DeselectAllSlots();
        selectedShader.SetActive(true);
        thisItemSelected = true;

        if (quantity > 0)
        {
            ItemDescriptionNameText.text = itemName;
            ItemDescriptionText.text = itemDescription;
            ItemDescriptionImage.sprite = itemSprite;
        }
        else
        {
            ItemDescriptionNameText.text = "";
            ItemDescriptionText.text = "";
            ItemDescriptionImage.sprite = emptySprite;
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

    public void OnRightClick()
    {
        
    }

}