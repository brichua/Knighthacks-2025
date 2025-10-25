using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Prefab / Canvas")]
    // Prefab for one speech bubble (must have DialogueBubble component)
    public GameObject bubblePrefab;
    // Canvas parent for instantiated bubbles (optional - will FindObjectOfType if null)
    public Canvas uiCanvas;
    // Camera for world->screen projection (optional)
    public Camera worldCamera;

    // Runtime map: one bubble per customer
    private readonly Dictionary<Customer, DialogueBubble> activeBubbles = new Dictionary<Customer, DialogueBubble>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiCanvas == null) uiCanvas = FindObjectOfType<Canvas>();
        if (worldCamera == null) worldCamera = Camera.main;
    }

    // Start (or reuse) a bubble sequence for a customer. Returns the created DialogueBubble or null on failure.
    public DialogueBubble PlayOrderSequenceForCustomer(Customer customer, Sprite[] orderSprites = null, string[] orderIds = null)
    {
        if (customer == null || bubblePrefab == null) return null;

        // If we already have a bubble for this customer, reuse / restart it
        if (activeBubbles.TryGetValue(customer, out var existing))
        {
            existing.Restart(orderSprites, orderIds);
            return existing;
        }

        // instantiate under canvas
        Transform parent = uiCanvas != null ? uiCanvas.transform : null;
        GameObject go = Instantiate(bubblePrefab, parent);
        DialogueBubble bubble = go.GetComponent<DialogueBubble>();
        if (bubble == null)
        {
            Debug.LogError("DialogueManager: bubblePrefab must contain a DialogueBubble component.");
            Destroy(go);
            return null;
        }

        // Initialize and keep track
        bubble.Initialize(customer, uiCanvas, worldCamera, orderSprites, orderIds);
        activeBubbles[customer] = bubble;

        // subscribe to bubble finished so we can remove it
        bubble.onBubbleDestroyed += () =>
        {
            if (activeBubbles.ContainsKey(customer))
                activeBubbles.Remove(customer);
        };

        return bubble;
    }

    public void HideBubbleForCustomer(Customer customer)
    {
        if (customer == null) return;
        if (activeBubbles.TryGetValue(customer, out var bubble))
        {
            bubble.ForceHide();
            activeBubbles.Remove(customer);
        }
    }

    public void HideAllBubbles()
    {
        foreach (var b in new List<DialogueBubble>(activeBubbles.Values))
            if (b != null) b.ForceHide();
        activeBubbles.Clear();
    }
}

[RequireComponent(typeof(RectTransform))]
public class DialogueBubble : MonoBehaviour
{
    // assign in prefab
    public Image speechBubbleImage;
    public Sprite speechBubbleSprite;
    public Image[] itemImages = new Image[3];

    [Header("Behavior")]
    public Vector3 bubbleWorldOffset = new Vector3(0f, 2f, 0f);
    public float revealDelay = 0.5f;
    public float mouthToggleInterval = 0.12f;
    public float autoHideAfter = 0.5f;
    public bool keepUntilServed = true;
    public float followLerp = 0.15f;

    // runtime
    private Customer customer;
    private Canvas uiCanvas;
    private Camera worldCamera;
    private RectTransform canvasRect;
    private RectTransform rect;
    private Coroutine sequenceCoroutine;
    private Coroutine mouthCoroutine;
    public event Action onBubbleDestroyed;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (customer == null || uiCanvas == null) return;

        // Follow the customer's world position to canvas anchoredPosition
        Vector3 worldPos = customer.transform.position + bubbleWorldOffset;
        Camera cam = worldCamera != null ? worldCamera : Camera.main;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        Camera camParam = (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : cam;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, camParam, out Vector2 localPoint))
        {
            if (followLerp > 0f)
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, localPoint, followLerp);
            else
                rect.anchoredPosition = localPoint;
        }
    }

    // Initialize and start sequence
    public void Initialize(Customer customer, Canvas canvas, Camera cam, Sprite[] orderSprites = null, string[] orderIds = null)
    {
        this.customer = customer;
        this.uiCanvas = canvas ?? FindObjectOfType<Canvas>();
        this.worldCamera = cam ?? Camera.main;
        this.canvasRect = uiCanvas != null ? uiCanvas.GetComponent<RectTransform>() : null;

        if (speechBubbleImage != null && speechBubbleSprite != null)
            speechBubbleImage.sprite = speechBubbleSprite;

        // hide images at start
        foreach (var img in itemImages) if (img != null) img.gameObject.SetActive(false);

        // immediate position
        ForceUpdateFollowPosition();

        // start sequence
        Restart(orderSprites, orderIds);
    }

    public void Restart(Sprite[] orderSprites = null, string[] orderIds = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(SequenceCoroutine(orderSprites, orderIds));
    }

    // Force hide / destroy immediately
    public void ForceHide()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (mouthCoroutine != null) StopCoroutine(mouthCoroutine);
        DestroySelf();
    }

    private IEnumerator SequenceCoroutine(Sprite[] orderSprites, string[] orderIds)
    {
        // Ensure bubble visible
        if (speechBubbleImage != null) speechBubbleImage.gameObject.SetActive(true);

        // Ensure item images hidden then reveal in order
        foreach (var img in itemImages) if (img != null) img.gameObject.SetActive(false);

        // mouth sprites resolution (if customer holds them)
        SpriteRenderer custRenderer = null;
        Sprite closedSprite = null;
        Sprite openSprite = null;
        if (customer != null)
        {
            custRenderer = customer.SpriteRenderer;
            if (customer.possibleNormalSprites != null)
            {
                int baseIndex = customer.spriteIndex;
                if (baseIndex >= 0 && baseIndex < customer.possibleNormalSprites.Length)
                    closedSprite = customer.possibleNormalSprites[baseIndex];
                int openIndex = baseIndex + 1;
                if (openIndex >= 0 && openIndex < customer.possibleNormalSprites.Length)
                    openSprite = customer.possibleNormalSprites[openIndex];
            }
        }

        // start mouth toggle
        if (custRenderer != null && (openSprite != null || closedSprite != null))
            mouthCoroutine = StartCoroutine(MouthToggleCoroutine(custRenderer, openSprite, closedSprite, mouthToggleInterval));

        // reveal items
        for (int i = 0; i < itemImages.Length; i++)
        {
            SetSlotSprite(itemImages[i], orderSprites, orderIds, i);
            yield return new WaitForSeconds(revealDelay);
        }

        // stop mouth and set closed sprite
        if (mouthCoroutine != null) { StopCoroutine(mouthCoroutine); mouthCoroutine = null; }
        if (custRenderer != null && closedSprite != null) custRenderer.sprite = closedSprite;

        // lifetime handling
        if (keepUntilServed && customer != null)
        {
            // wait until served or destroyed
            while (customer != null && !customer.served) yield return null;
            if (autoHideAfter > 0f) yield return new WaitForSeconds(autoHideAfter);
            DestroySelf();
        }
        else
        {
            if (autoHideAfter > 0f) { yield return new WaitForSeconds(autoHideAfter); DestroySelf(); }
        }
    }

    private void SetSlotSprite(Image slotImage, Sprite[] orderSprites, string[] orderIds, int index)
    {
        if (slotImage == null) return;
        Sprite s = null;
        if (orderSprites != null && index < orderSprites.Length) s = orderSprites[index];
        // fallback to orderIds if you want to implement Resources loading here (not required)
        if (s != null)
        {
            slotImage.sprite = s;
            slotImage.gameObject.SetActive(true);
        }
        else
            slotImage.gameObject.SetActive(false);
    }

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

    private void ForceUpdateFollowPosition()
    {
        if (customer == null || uiCanvas == null || canvasRect == null) return;
        Vector3 worldPos = customer.transform.position + bubbleWorldOffset;
        Camera cam = worldCamera != null ? worldCamera : Camera.main;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        Camera camParam = (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : cam;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, camParam, out Vector2 localPoint))
            rect.anchoredPosition = localPoint;
    }

    private void DestroySelf()
    {
        onBubbleDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
