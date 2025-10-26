using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Button))]
public class DialogueBubble : MonoBehaviour
{
    private Button completeOrderButton;
    public TaskManager taskManager;
    public GameManager gameManager;
    public Image speechBubbleImage;
    public Sprite speechBubbleSprite;
    public Image[] itemImages = new Image[3];

    [Header("Behavior")]
    public Vector3 bubbleWorldOffset = new Vector3(0f, 20f, 0f);
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

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        completeOrderButton = GetComponent<Button>();
        if (completeOrderButton != null)
            completeOrderButton.onClick.AddListener(CompleteOrder);
    }

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
        
        // Find managers if they haven't been assigned
        if (taskManager == null)
            taskManager = FindObjectOfType<TaskManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

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

    private IEnumerator QuarantineAndDestroy()
    {
        yield return new WaitForSeconds(2f);
        if (customer != null && taskManager != null)
        {
            taskManager.customerManager.destroyCustomer(customer);
        }
    }

    public void CompleteOrder()
    {
        if (customer == null || taskManager == null || gameManager == null) return;
    
        bool sizeMatch = taskManager.tea == customer.order[0];
        bool teaMatch = taskManager.size == customer.order[1];
        bool snackMatch = taskManager.snack == customer.order[2];
        bool flowerMatch = taskManager.flower == customer.order[3];
        bool orderMatch = sizeMatch && teaMatch && snackMatch && flowerMatch;
    
        bool accusationCorrect = Accusation.accuse(taskManager.anomaly, customer);
        bool finalCorrect = orderMatch && accusationCorrect;

        // Update player health
        if (!finalCorrect) gameManager.subtractHealth();

        // Serve only if correct
        if (finalCorrect) customer.served = true;

        // Hide held items
        if (taskManager.snackObject != null) taskManager.snackObject.SetActive(false);
        if (taskManager.teaObject != null) taskManager.teaObject.SetActive(false);
        if (taskManager.decorationObject != null) taskManager.decorationObject.SetActive(false);
        if (taskManager.teaFlower != null) taskManager.teaFlower.SetActive(false);

        // Update tray background
        GameObject[] remainingCustomers = GameObject.FindGameObjectsWithTag("Customer");
        taskManager.background.sprite = (remainingCustomers.Length <= 1)
            ? taskManager.noTrayBackground
            : taskManager.trayBackground;
        taskManager.tray = remainingCustomers.Length > 1;

        // Show result + fade out
        StartCoroutine(ShowResultAndRemoveCustomer());
        StartCoroutine(FadeEffect(finalCorrect));


        taskManager.resetTasks();
        DestroySelf();
    }

    private IEnumerator ShowResultAndRemoveCustomer()
    {
        if (customer == null) yield break;

        // Fade and destroy
        if (taskManager != null && taskManager.customerManager != null)
        {
            yield return taskManager.customerManager.StartCoroutine(
                taskManager.customerManager.FadeOutSprite(customer.gameObject)
            );
            Destroy(customer.gameObject);
        }
    }

    private IEnumerator FadeEffect (bool isCorrect)
    {
        if (customer == null) yield break;

        // Activate the correct image
        GameObject resultImage = isCorrect ? customer.correctImage : customer.wrongImage;
        if (resultImage != null)
            resultImage.SetActive(true);


        if (taskManager != null && taskManager.customerManager != null)
        {
            yield return taskManager.customerManager.StartCoroutine(
                taskManager.customerManager.FadeOutSprite(resultImage)
            );
        }
    }


    private void OnDestroy()
    {
        if (completeOrderButton != null)
            completeOrderButton.onClick.RemoveListener(CompleteOrder);
    }

    private void DestroySelf()
    {
        onBubbleDestroyed?.Invoke();
        Destroy(gameObject);
    }
}