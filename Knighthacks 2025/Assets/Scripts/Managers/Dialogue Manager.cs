using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum NPCType { Human, Ghost }

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Order UI")]
    // Container that groups the images / speech bubble (toggle to show/hide)
    public GameObject orderImagesContainer;
    // Three item slots (left-to-right). Only these three are used by the sequence.
    public Image[] orderImages = new Image[3];
    // Plus sign image shown between first and second item
    public Image speechBubble;

    [Header("Sequence Timing")]
    // time between reveals (image -> plus -> image -> image)
    public float revealDelay = 0.5f;
    // how fast the customer's mouth toggles while speaking
    public float mouthToggleInterval = 0.12f;
    // after sequence ends, how long before auto-hiding the bubble (0 = don't auto-hide)
    public float autoHideAfter = 0.5f;

    // runtime
    private Coroutine sequenceCoroutine;
    private Coroutine mouthCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        HideOrderImages();
    }

    // Public entry point:
    // - Provide `orderSprites` to use sprites directly (preferred).
    // - Or provide `orderIds` or let the manager use the customer's `order` strings and attempt Resources.Load with path "Resources/ItemSprites/{id}".
    public bool PlayOrderSequenceForCustomer(Customer customer, Sprite[] orderSprites = null, string[] orderIds = null)
    {
        if (customer == null) return false;

        // Determine fallback ids from customer if none provided
        string[] customerIds = orderIds;
        if (customerIds == null)
            customerIds = customer.order;

        // cancel any running sequence
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(OrderSequenceCoroutine(customer, customer.orderSprites, customerIds));
        return true;
    }

    // Core coroutine: handles bubble, images and mouth animation
    private IEnumerator OrderSequenceCoroutine(Customer customer, Sprite[] orderSprites, string[] orderIds)
    {
        // Prepare UI
        if (orderImagesContainer != null) orderImagesContainer.SetActive(true);

        // Hide all image slots and plus sign initially
        for (int i = 0; i < orderImages.Length; i++)
            if (orderImages[i] != null) orderImages[i].gameObject.SetActive(false);
        if (speechBubble != null) speechBubble.gameObject.SetActive(false);

        // Start mouth animation (if possible)
        SpriteRenderer custRenderer = null;
        Sprite closedSprite = null;
        Sprite openSprite = null;
        if (customer != null)
        {
            custRenderer = customer.SpriteRenderer;
            if (customer.possibleNormalSprites != null)
            {
                int baseIndex = customer.spriteIndex * 2;
                if (baseIndex >= 0 && baseIndex < customer.possibleNormalSprites.Length)
                    closedSprite = customer.possibleNormalSprites[baseIndex];
                int openIndex = baseIndex + 1;
                if (openIndex >= 0 && openIndex < customer.possibleNormalSprites.Length)
                    openSprite = customer.possibleNormalSprites[openIndex];
            }
        }

        // If we have a spriteRenderer and at least one mouth sprite, start toggling
        if (custRenderer != null && (openSprite != null || closedSprite != null))
        {
            mouthCoroutine = StartCoroutine(MouthToggleCoroutine(custRenderer, openSprite, closedSprite, mouthToggleInterval));
        }

        // Step 1: reveal first item (index 0)
        if (orderImages.Length >= 1 && orderImages[0] != null)
            SetSlotSprite(orderImages[0], orderSprites, orderIds, 0);
        yield return new WaitForSeconds(revealDelay);

        // Step 3: reveal second item (index 1)
        if (orderImages.Length >= 2 && orderImages[1] != null)
            SetSlotSprite(orderImages[1], orderSprites, orderIds, 1);
        yield return new WaitForSeconds(revealDelay);

        // Step 4: reveal third item (index 2)
        if (orderImages.Length >= 3 && orderImages[2] != null)
            SetSlotSprite(orderImages[2], orderSprites, orderIds, 2);
        yield return new WaitForSeconds(revealDelay);

        // Sequence finished: stop mouth animation and set closed sprite
        if (mouthCoroutine != null)
        {
            StopCoroutine(mouthCoroutine);
            mouthCoroutine = null;
        }
        if (custRenderer != null && closedSprite != null)
            custRenderer.sprite = closedSprite;

        // Optional auto-hide
        if (autoHideAfter > 0f)
        {
            yield return new WaitForSeconds(autoHideAfter);
            HideOrderImages();
        }

        sequenceCoroutine = null;
    }

    // helper to set the sprite for a slot. Prefer `orderSprites` if provided.
    // If `orderSprites` is null, manager will attempt to load from Resources/ItemSprites/{id} using `orderIds[index]`.
    private void SetSlotSprite(Image slotImage, Sprite[] orderSprites, string[] orderIds, int index)
    {
        if (slotImage == null) return;

        Sprite s = null;
        // Use provided sprites first
        if (orderSprites != null && index < orderSprites.Length)
            s = orderSprites[index];

        // Fallback: try to load by ID from Resources
        if (s == null && orderIds != null && index < orderIds.Length && !string.IsNullOrEmpty(orderIds[index]))
            s = LoadSpriteFromResources(orderIds[index]);

        if (s != null)
        {
            slotImage.sprite = s;
            slotImage.gameObject.SetActive(true);
        }
        else
        {
            slotImage.gameObject.SetActive(false);
        }
    }

    // Attempt to load a sprite from Resources/ItemSprites/{id}
    private Sprite LoadSpriteFromResources(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        // Place your item sprites under Assets/Resources/ItemSprites/ named exactly as the id string
        return Resources.Load<Sprite>($"ItemSprites/{id}");
    }

    // toggles customer's mouth between open and closed until stopped
    private IEnumerator MouthToggleCoroutine(SpriteRenderer renderer, Sprite openSprite, Sprite closedSprite, float interval)
    {
        bool showOpen = true;
        while (true)
        {
            if (renderer != null)
            {
                if (showOpen && openSprite != null) renderer.sprite = openSprite;
                else if (!showOpen && closedSprite != null) renderer.sprite = closedSprite;
            }
            showOpen = !showOpen;
            yield return new WaitForSeconds(interval);
        }
    }

    // hide the whole speech bubble + images (also cancels running coroutines)
    public void HideOrderImages()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        if (mouthCoroutine != null)
        {
            StopCoroutine(mouthCoroutine);
            mouthCoroutine = null;
        }

        if (orderImagesContainer != null) orderImagesContainer.SetActive(false);
        if (orderImages != null)
        {
            for (int i = 0; i < orderImages.Length; i++)
                if (orderImages[i] != null) orderImages[i].gameObject.SetActive(false);
        }
    }
}