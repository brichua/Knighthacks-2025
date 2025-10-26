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
        go.transform.rotation = Quaternion.Euler(0, 0, 45f);
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
