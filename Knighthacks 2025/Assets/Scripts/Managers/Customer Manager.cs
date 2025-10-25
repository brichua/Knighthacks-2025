using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class CustomerManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public List<Customer> customers = new List<Customer>();
    public List<GameObject> customerGO = new List<GameObject>();
    public List<GameObject> customerLine = new List<GameObject>();
    public int maxCustomers = 12;
    public AnomalyManager AnomalyManager;
    public DialogueManager DialogueManager;
    public TaskManager TaskManager;
    public Camera UICamera;
    static Timer customerSpawnTimer;
    bool spawnRequested = false;
    public bool stopSpawning = false;
    public bool isOrdering;

    string[] pastryTypes = { "cookie", "cake", "cracker" };
    string[] teaFlowerTypes = { "rose", "bluebell", "daisy" };
    string[] drinkTypes = { "black", "green", "oolong" };
    string[] drinkSize = { "large", "small" };

    public Sprite[] drinkSprites; 
    public Sprite[] pastrySprites;
    public Sprite[] teaFlowerSprites;


    void Start()
    {
        double interval = Random.Range(20000, 40000);
        customerSpawnTimer = new Timer(interval);
        customerSpawnTimer.Elapsed += (s, e) => { spawnRequested = true; };
        customerSpawnTimer.Start();
    }

    void Update()
    {
        if (spawnRequested)
        {
            spawnRequested = false;
            SpawnCustomer();
            double interval = Random.Range(20000, 40000);
            customerSpawnTimer = new Timer(interval);
            customerSpawnTimer.Elapsed += (s, e) => { spawnRequested = true; };
            customerSpawnTimer.Start();
        }
        if (stopSpawning) 
        {
            customerSpawnTimer.Stop();
        }
    }

    void SpawnCustomer()
    {
        if (maxCustomers == 0) { return; }
        //Generate Prefab
        GameObject newCustomer = Instantiate(customerPrefab, new Vector3(13f, -0.76f, 10f), Quaternion.identity);
        Customer customer = newCustomer.GetComponent<Customer>();
        maxCustomers--;

        // If the prefab has a Canvas, assign its render camera to the UICamera field (if provided)
        // and force the Canvas to render above the customer's SpriteRenderers.
        Canvas canvas = newCustomer.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            if (UICamera != null)
            {
                // Ensure canvas uses a camera-based render mode so worldCamera takes effect.
                if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                }
                canvas.worldCamera = UICamera;
            }
            else
            {
                Debug.LogWarning("CustomerManager.UICamera not assigned. Canvas.worldCamera not set.");
            }

            // Ensure this prefab's Canvas always renders on top of the prefab sprites:
            // - overrideSorting makes the canvas use its own sortingOrder regardless of parent.
            // - set sortingOrder high enough to be above sprite renderers (adjust as needed).
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;
            // Optional: place on a known sorting layer (uncomment if you have a "UI" sorting layer).
            // canvas.sortingLayerName = "UI";
        }

        // Assign the prefab's Button (which likely lives under the Canvas child) to call TaskManager.takeOrder(customer)
        Button orderButton = null;
        if (canvas != null)
        {
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            if (buttons.Length == 1)
            {
                orderButton = buttons[0];
            }
            else if (buttons.Length > 1)
            {
                // prefer a button whose GameObject name contains "order"; otherwise take the first
                foreach (var b in buttons)
                {
                    if (b.gameObject.name.ToLower().Contains("order"))
                    {
                        orderButton = b;
                        break;
                    }
                }
                if (orderButton == null)
                    orderButton = buttons[0];
            }
        }
        else
        {
            // fallback: find any Button in children (covers non-Canvas or unexpected hierarchies)
            orderButton = newCustomer.GetComponentInChildren<Button>(true);
        }

        if (orderButton != null)
        {
            if (TaskManager != null)
            {
                orderButton.onClick.RemoveAllListeners();
                orderButton.onClick.AddListener(() => TaskManager.takeOrder(customer));
            }
            else
            {
                Debug.LogWarning("CustomerManager.TaskManager reference not set in inspector. Cannot assign takeOrder listener.");
            }
        }
        else
        {
            Debug.LogWarning("Customer prefab is missing a Button component (searched Canvas child first).");
        }
        // -----------------------------------------------------------------------

        //Generate Order
        int snackRoll = Random.Range(0, pastryTypes.Length);
        int flowerRoll = Random.Range(0, teaFlowerTypes.Length);
        int drinkRoll = Random.Range(0, drinkTypes.Length);
        int drinkSizeRoll = Random.Range(0, drinkSize.Length);
        string[] choices = { drinkTypes[drinkRoll], drinkSize[drinkSizeRoll], pastryTypes[snackRoll], teaFlowerTypes[flowerRoll] };
        Sprite[] orderSprites = new Sprite[3];
        //Based on the order, assign the appropriate sprite
        orderSprites[0] = drinkSprites[(drinkRoll * 2) + drinkSizeRoll];
        orderSprites[1] = pastrySprites[snackRoll];
        orderSprites[2] = teaFlowerSprites[flowerRoll];
        
        //Generate Customer Sprite
        int spriteIndex = Random.Range(0, 2);

        //Roll for if customer will be an anomaly
        if (AnomalyManager.rollForAnomaly()) {
            // Customer is an anomaly
            Debug.Log("Anomaly Spawned");
            customer.Initialize(choices, true, spriteIndex, orderSprites);
            AnomalyManager.generateAnomaly(customer);
        }
        else {
            // normal customer
            Debug.Log("Normal Ass Customer Spawned");
            customer.Initialize(choices, false, spriteIndex, orderSprites);
        }
        newCustomer.SetActive(true);
        customers.Add(customer);
        customerGO.Add(newCustomer);

        moveCustomerToRegister(newCustomer);
        if (DialogueManager.PlayOrderSequenceForCustomer(customer, customer.orderSprites))
        {
            moveCustomerToWaitingLine(newCustomer);
        }

        //StartCoroutine(timewaste(newCustomer, 7.0f));
    }

    //Function that moves le customer to the cashier
    public void moveCustomerToRegister(GameObject customer)
    {
        Vector3 targetPosition = new Vector3(7.4f, -0.76f, 10f);
        float speed = 5f;
        StartCoroutine(MoveCustomerCoroutine(customer, targetPosition, speed));
    }

    //Function that moves le customer to the waiting line
    public void moveCustomerToWaitingLine(GameObject customer)
    {
        Vector3 targetPosition = new Vector3(0, 0, 10f);
        float speed = 5f;
        for (int i = 0; i < customerGO.Count; i++)
        {
            if (customerGO[i] != null) 
            {
                switch (i){
                    case 0:
                        targetPosition.x = -8.08f;
                        targetPosition.y = 0.53f;
                        break;
                    case 1:
                        targetPosition.x = -4.65f;
                        targetPosition.y = 0.53f;
                        break;
                    case 2:
                        targetPosition.x = -1.18f;
                        targetPosition.y = 0.53f;
                        break;
                }
            }
        }
        StartCoroutine(MoveCustomerCoroutine(customer, targetPosition, speed));
    }
    //Smoothly moves the customer
    private IEnumerator MoveCustomerCoroutine(GameObject customer, Vector3 targetPos, float speed)
    {
        while (Vector3.Distance(customer.transform.position, targetPos) > 0.01f)
        {
            customer.transform.position = Vector3.MoveTowards(
                customer.transform.position,
                targetPos,
                speed * Time.deltaTime
            );
            yield return null;
        }
    }

    //DELETE THIS SIDDU, BRI, JUAN, JAHYR
    private IEnumerator timewaste(GameObject newCustomer, float time)
    {
        yield return new WaitForSeconds(time);
        moveCustomerToWaitingLine(newCustomer);
    }
}
