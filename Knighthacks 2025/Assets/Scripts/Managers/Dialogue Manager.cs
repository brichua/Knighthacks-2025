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
    public Image plusImage;
    // Database to resolve item name -> sprite
    public ItemDatabase itemDatabase;

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

    // Public entry point: plays the ordered reveal sequence for a given customer.
    // If order is null it will use the customer's `order` field.
    public void PlayOrderSequenceForCustomer(Customer customer, string[] order = null)
    {
        if (customer == null) return;
        if (order == null) order = customer.order;
        // cancel any running sequence
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(OrderSequenceCoroutine(customer, order));
    }

    // Core coroutine: handles bubble, images and mouth animation
    private IEnumerator OrderSequenceCoroutine(Customer customer, string[] order)
    {
        // Prepare UI
        if (orderImagesContainer != null) orderImagesContainer.SetActive(true);

        // Hide all image slots and plus sign initially
        for (int i = 0; i < orderImages.Length; i++)
            if (orderImages[i] != null) orderImages[i].gameObject.SetActive(false);
        if (plusImage != null) plusImage.gameObject.SetActive(false);

        // Start mouth animation (if possible)
        SpriteRenderer custRenderer = null;
        Sprite closedSprite = null;
        Sprite openSprite = null;
        if (customer != null)
        {
            custRenderer = customer.SpriteRenderer;
            // try to resolve closed/open sprites from Customer.possibleNormalSprites using spriteIndex
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
            SetSlotSprite(orderImages[0], order, 0);
        yield return new WaitForSeconds(revealDelay);

        // Step 2: reveal plus sign
        if (plusImage != null)
            plusImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(revealDelay);

        // Step 3: reveal second item (index 1)
        if (orderImages.Length >= 2 && orderImages[1] != null)
            SetSlotSprite(orderImages[1], order, 1);
        yield return new WaitForSeconds(revealDelay);

        // Step 4: reveal third item (index 2)
        if (orderImages.Length >= 3 && orderImages[2] != null)
            SetSlotSprite(orderImages[2], order, 2);
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

    // helper to set the sprite for a slot from order[] using itemDatabase
    private void SetSlotSprite(Image slotImage, string[] order, int index)
    {
        if (slotImage == null) return;
        if (order != null && index < order.Length && !string.IsNullOrEmpty(order[index]))
        {
            Sprite s = itemDatabase != null ? itemDatabase.GetSprite(order[index]) : null;
            slotImage.sprite = s;
            slotImage.gameObject.SetActive(s != null);
        }
        else
        {
            slotImage.gameObject.SetActive(false);
        }
    }

    // toggles customer's mouth between open and closed until stopped
    private IEnumerator MouthToggleCoroutine(SpriteRenderer renderer, Sprite openSprite, Sprite closedSprite, float interval)
    {
        // prefer toggling between provided open and closed; if openSprite missing, toggle visibility using closed only
        bool showOpen = true;
        while (true)
        {
            if (renderer != null)
            {
                if (showOpen && openSprite != null) renderer.sprite = openSprite;
                else if (!showOpen && closedSprite != null) renderer.sprite = closedSprite;
                // if one of the sprites is null we simply leave whatever sprite is available
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
        if (plusImage != null) plusImage.gameObject.SetActive(false);
    }
}