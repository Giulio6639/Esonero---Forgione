using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private string itemName;

    [SerializeField]
    private int itemQuantity;

    [SerializeField]
    private Sprite sprite;

    [TextArea]
    [SerializeField]
    private string itemDescription;

    private InventoryManager inventoryManager;

    void Start()
    {
        inventoryManager = GameObject.Find("Canvas").GetComponent<InventoryManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // AGGIUNTO IL SIMBOLO "=" QUI SOTTO
            int leftOverItems = inventoryManager.AddItem(itemName, itemQuantity, sprite, itemDescription);

            if (leftOverItems <= 0)
            {
                Destroy(gameObject); // Ricordati di distruggere l'oggetto a terra se l'hai raccolto tutto!
            }
            else
            {
                itemQuantity = leftOverItems;
            }
        }
    }
}