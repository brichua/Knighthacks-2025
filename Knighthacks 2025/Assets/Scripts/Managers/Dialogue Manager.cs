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
    // The speech-bubble graphic (optional - part of the container)
    // This `Image` will display the assigned `speechBubbleSprite`.
    public Image speechBubble;
    // Assign a Sprite asset from the project here (drag a Sprite into this field).
    public Sprite speechBubbleSprite;

    [Header("Follow settings")]
    // Canvas that contains `orderImagesContainer`. If null, the manager will attempt to find one.
    public Canvas uiCanvas;
    // Camera used to project world -> screen. If null, Camera.main will be used.
    public Camera worldCamera;
    // Offset in world units to position the bubble above the customer's head.
    public Vector3 bubbleWorldOffset = new Vector3(0f, 2.0f, 0f);

    [Header("Sequence Timing")]
    // time between reveals (image -> image -> image)
    public float revealDelay = 0.5f;
    // how fast the customer's mouth toggles while speaking
    public float mouthToggleInterval = 0.12f;
    // after sequence ends, how long before auto-hiding the bubble (0 = don't auto-hide)
    public float autoHideAfter = 0.5f;

    public bool customerBubbleUntilServed = true;

    // runtime
    private Coroutine sequenceCoroutine;
    private Coroutine mouthCoroutine;

    // follow state
    private Customer trackedCustomer;
    private RectTransform containerRect;
    private RectTransform canvasRect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // cache references, try to auto-find if not assigned
        if (uiCanvas == null) uiCanvas = FindObjectOfType<Canvas>();
        if (worldCamera == null) worldCamera = Camera.main;
        if (orderImagesContainer != null) containerRect = orderImagesContainer.GetComponent<RectTransform>();
        if (uiCanvas != null) canvasRect = uiCanvas.GetComponent<RectTransform>();

        // If a sprite is assigned, apply it to the Image so it's ready
        if (speechBubble != null && speechBubbleSprite != null)
            speechBubble.sprite = speechBubbleSprite;

        HideOrderImages();
    }

    void Update()
    {
        // If we are tracking a customer, update the UI container position each frame
        if (trackedCustomer != null && orderImagesContainer != null && canvasRect != null)
        {
            // World position to place bubble above customer's head
            Vector3 worldPos = trackedCustomer.transform.position + bubbleWorldOffset;
            Camera cam = worldCamera != null ? worldCamera : Camera.main;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

            // Convert screen point to canvas local point
            Vector2 localPoint;
            // Choose camera for ScreenPoint -> LocalPoint depending on render mode
            Camera camParam = (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : cam;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, camParam, out localPoint))
            {
                containerRect.localPosition = localPoint;
            }
        }
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
        orderSprites = customer.orderSprites;
        Debug.Log("Lagtrain");

        // ensure we are tracking this customer so Update() moves the bubble
        trackedCustomer = customer;

        // ensure speechBubble Image uses the assigned sprite (in case it changed at runtime)
        if (speechBubble != null && speechBubbleSprite != null)
            speechBubble.sprite = speechBubbleSprite;

        if (orderImagesContainer != null) orderImagesContainer.SetActive(true);
        sequenceCoroutine = StartCoroutine(OrderSequenceCoroutine(customer, orderSprites, customerIds));
        return true;
    }

    // Core coroutine: handles bubble, images and mouth animation
    private IEnumerator OrderSequenceCoroutine(Customer customer, Sprite[] orderSprites, string[] orderIds)
    {
        // Prepare UI (already activated in PlayOrderSequenceForCustomer, but be safe)
        if (orderImagesContainer != null) orderImagesContainer.SetActive(true);

        // Hide all image slots and bubble initially
        for (int i = 0; i < 3; i++)
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

        // Ensure bubble graphic visible before reveals and apply sprite if present
        if (speechBubble != null)
        {
            if (speechBubbleSprite != null)
                speechBubble.sprite = speechBubbleSprite;
            speechBubble.gameObject.SetActive(true);
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
        Debug.Log("Rainy boots");

        // Step 2: reveal second item (index 1)
        if (orderImages.Length >= 2 && orderImages[1] != null)
            SetSlotSprite(orderImages[1], orderSprites, orderIds, 1);
        yield return new WaitForSeconds(revealDelay);

        // Step 3: reveal third item (index 2)
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
        if (customerBubbleUntilServed && customer != null) 
        {
            while (customer != null && !customer.served)
                yield return null;

            // Small safety delay (optional) to allow immediate UI feedback
            if (autoHideAfter > 0f)
                yield return new WaitForSeconds(autoHideAfter);

            HideOrderImages();
        }
        else
        {
            if (autoHideAfter > 0f)
            {
                yield return new WaitForSeconds(autoHideAfter);
                HideOrderImages();
            }
            else
            {
                // stop following but leave visible if autoHideAfter == 0 and keepBubbleUntilServed==false
                trackedCustomer = null;
            }
        }

        sequenceCoroutine = null;
    }

    // helper to set the sprite for a slot. Prefer `orderSprites` if provided.
    // If `orderSprites` is null, manager will attempt to load from Resources/ItemSprites/{id} using `orderIds[index]`.
    private void SetSlotSprite(Image slotImage, Sprite[] orderSprites, string[] orderIds, int index)
    {

        Sprite s = null;
        // Use provided sprites first
        Debug.Log(index + " is index, as opposed to " + orderSprites.Length);
        if (orderSprites != null && index < orderSprites.Length)
        {
            Debug.Log("Lost Umbrella");
            s = orderSprites[index];
        }

        // Fallback: try to load by ID from Resources
        if (s != null)
        {
            
            slotImage.sprite = s;
            slotImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("");
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

        trackedCustomer = null;

        if (orderImagesContainer != null) orderImagesContainer.SetActive(false);
        if (orderImages != null)
        {
            for (int i = 0; i < orderImages.Length; i++)
                if (orderImages[i] != null) orderImages[i].gameObject.SetActive(false);
        }
        if (speechBubble != null) speechBubble.gameObject.SetActive(false);
    }
}