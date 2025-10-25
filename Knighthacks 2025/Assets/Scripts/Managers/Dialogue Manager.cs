using UnityEngine;
using UnityEngine.UI;

public enum NPCType { Human, Ghost }

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Order UI")]
    // Container that groups the 3 images (toggle to show/hide)
    public GameObject orderImagesContainer;
    // Assign three UI Images in the Inspector (left-to-right order)
    public Image[] orderImages = new Image[10];
    // Database to resolve item name -> sprite
    public ItemDatabase itemDatabase;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        HideOrderImages();
    }

    // Show images for a plain string[] order (same format as Customer.order)
    public void ShowOrderImages(string[] order)
    {
        if (orderImagesContainer != null) orderImagesContainer.SetActive(true);

        for (int i = 0; i < orderImages.Length; i++)
        {
            if (i < order.Length && !string.IsNullOrEmpty(order[i]))
            {
                Sprite s = itemDatabase != null ? itemDatabase.GetSprite(order[i]) : null;
                if (orderImages[i] != null)
                {
                    orderImages[i].sprite = s;
                    orderImages[i].gameObject.SetActive(s != null);
                }
            }
            else
            {
                if (orderImages[i] != null) orderImages[i].gameObject.SetActive(false);
            }
        }
    }

    // Convenience: show images directly from a Customer instance
    public void ShowOrderForCustomer(Customer customer)
    {
        if (customer == null) return;
        ShowOrderImages(customer.order);
    }

    // Hide the order UI
    public void HideOrderImages()
    {
        if (orderImagesContainer != null) orderImagesContainer.SetActive(false);
        for (int i = 0; i < orderImages.Length; i++)
            if (orderImages[i] != null) orderImages[i].gameObject.SetActive(false);
    }
}