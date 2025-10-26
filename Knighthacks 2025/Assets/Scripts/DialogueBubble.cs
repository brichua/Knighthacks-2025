using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DialogueBubble : MonoBehaviour
{
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

    private Customer customer;
    private Canvas uiCanvas;
    private Camera worldCamera;
    private RectTransform canvasRect;
    private RectTransform rect;
    private Coroutine sequenceCoroutine;
    private Coroutine mouthCoroutine;
    public event Action onBubbleDestroyed;

    void Awake() => rect = GetComponent<RectTransform>();

    void Update()
    {
        if (customer == null || uiCanvas == null) return;

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

    public void Initialize(Customer customer, Canvas canvas, Camera cam, Sprite[] orderSprites = null, string[] orderIds = null)
    {
        this.customer = customer;
        this.uiCanvas = canvas ?? FindObjectOfType<Canvas>();
        this.worldCamera = cam ?? Camera.main;
        this.canvasRect = uiCanvas != null ? uiCanvas.GetComponent<RectTransform>() : null;

        if (speechBubbleImage != null && speechBubbleSprite != null)
            speechBubbleImage.sprite = speechBubbleSprite;

        foreach (var img in itemImages) if (img != null) img.gameObject.SetActive(false);
        ForceUpdateFollowPosition();
        orderSprites = customer.orderSprites;
        Restart(orderSprites, orderIds);
    }

    public void Restart(Sprite[] orderSprites = null, string[] orderIds = null)
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(SequenceCoroutine(orderSprites, orderIds));
    }

    public void ForceHide()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if (mouthCoroutine != null) StopCoroutine(mouthCoroutine);
        DestroySelf();
    }

    private IEnumerator SequenceCoroutine(Sprite[] orderSprites, string[] orderIds)
    {
        if (speechBubbleImage != null) speechBubbleImage.gameObject.SetActive(true);
        foreach (var img in itemImages) if (img != null) img.gameObject.SetActive(false);

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

        if (custRenderer != null && (openSprite != null || closedSprite != null))
            mouthCoroutine = StartCoroutine(MouthToggleCoroutine(custRenderer, openSprite, closedSprite, mouthToggleInterval));

        for (int i = 0; i < itemImages.Length; i++)
        {
            SetSlotSprite(itemImages[i], orderSprites, orderIds, i);
            yield return new WaitForSeconds(revealDelay);
        }

        if (mouthCoroutine != null) { StopCoroutine(mouthCoroutine); mouthCoroutine = null; }
        if (custRenderer != null && closedSprite != null) custRenderer.sprite = closedSprite;

        if (keepUntilServed && customer != null)
        {
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