using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CustomerManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public GameManager gameManager;
    public TaskManager taskManager;
    public List<Customer> customers = new List<Customer>();
    public List<GameObject> customerGO = new List<GameObject>();
    public List<GameObject> customerLine = new List<GameObject>();
    public GameObject[] customerWaiting = new GameObject[3];
    public int maxCustomers = 12;
    public AnomalyManager AnomalyManager;
    public DialogueManager DialogueManager;
    public TaskManager TaskManager;
    public Camera UICamera;
    static System.Timers.Timer customerSpawnTimer;
    bool spawnRequested = false;
    public bool stopSpawning = false;
    public bool isOrdering;
    public bool registerOccupied;

    string[] pastryTypes = { "cookie", "cake", "cracker" };
    string[] teaFlowerTypes = { "rose", "bluebell", "daisy" };
    string[] drinkTypes = { "black", "green", "oolong" };
    string[] drinkSize = { "small", "large" };

    public Sprite[] drinkSprites; 
    public Sprite[] pastrySprites;
    public Sprite[] teaFlowerSprites;

    [Header("Audio")]
    // Assign either an AudioSource to play via PlayOneShot, or leave sfxSource null to fallback to PlayClipAtPoint.
    public AudioSource sfxSource;
    public AudioClip registerSfxClip;

    void Start()
    {
        double interval = Random.Range(20000, 40000);
        customerSpawnTimer = new System.Timers.Timer(interval);
        customerSpawnTimer.Elapsed += (s, e) => { spawnRequested = true; };
        customerSpawnTimer.Start();
    }

    void Update()
    {
        if (spawnRequested)
        {
            //Check queues to make sure customer can actually spawn
            if (customerGO.Count < 3 && customerLine.Count < 3 && maxCustomers > 0 && gameManager.getStartGame())
            {
                //Spawn can happen
                SpawnCustomer();
            }
            //Reset timer regardless of whether the customer can spawn or not
            Debug.Log("Spawn Timer Reset");
            spawnRequested = false;
            double interval = Random.Range(20000, 40000);
            customerSpawnTimer = new System.Timers.Timer(interval);
            customerSpawnTimer.Elapsed += (s, e) => { spawnRequested = true; };
            customerSpawnTimer.Start();
        }

        if (stopSpawning) 
        {
            customerSpawnTimer.Stop();
        }

        //Add if statement to check if register is occupied, if not, move next customer in line to register
        if (!registerOccupied && customerLine.Count > 0) 
        {
            moveCustomerToRegister(customerLine[0]);
            customerLine.RemoveAt(0);
            registerOccupied = true;
        }
    }

    public void SpawnCustomer()
    {
        GameObject newCustomer = Instantiate(customerPrefab, new Vector3(13f, -0.76f, 10f), Quaternion.identity);
        Customer customer = newCustomer.GetComponent<Customer>();
        maxCustomers--;

        if(taskManager.getInBack() == false)
        {
            newCustomer.SetActive(true);
        }

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
                orderButton.onClick.AddListener(() => TaskManager.takeOrder(newCustomer, customer));
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

        //Generate Customer Order
        int snackRoll = Random.Range(0, pastryTypes.Length);
        int flowerRoll = Random.Range(0, teaFlowerTypes.Length);
        int drinkRoll = Random.Range(0, drinkTypes.Length);
        int drinkSizeRoll = Random.Range(0, drinkSize.Length);
        string[] choices = { drinkTypes[drinkRoll], drinkSize[drinkSizeRoll], pastryTypes[snackRoll], teaFlowerTypes[flowerRoll] };
        Sprite[] orderSprites = new Sprite[3];
        //Based on the order, assign the appropriate sprite
        orderSprites[0] = drinkSprites[drinkRoll * 2 + drinkSizeRoll];
        orderSprites[1] = pastrySprites[snackRoll];
        orderSprites[2] = teaFlowerSprites[flowerRoll];
        
        //Generate Customer Sprite
        int spriteIndex = Random.Range(0, 2) * 2;

        //Roll for if customer will be an anomaly
        if (AnomalyManager.rollForAnomaly()) {
            // Customer is an anomaly
            Debug.Log("Anomaly Spawned");
            customer.Initialize(choices, true, spriteIndex * 2, orderSprites);
            AnomalyManager.generateAnomaly(customer);
        }
        else {
            // normal customer
            Debug.Log("Normal Ass Customer Spawned");
            customer.Initialize(choices, false, spriteIndex, orderSprites);
        }
        //Set newCustomer GameObject as active, add customer object to list, add
        //newCustomer GameObject to customerGO
        customers.Add(customer);
        customerGO.Add(newCustomer);
        customerLine.Add(newCustomer);
        /*if (DialogueManager.PlayOrderSequenceForCustomer(customer, customer.orderSprites))
        {
            moveCustomerToWaitingLine(newCustomer);
        }
        */

        //StartCoroutine(timewaste(newCustomer, 7.0f));
        SetCustomerOrderButtonActive(newCustomer, false);
    }

    //Function that moves le customer to the cashier
    public void moveCustomerToRegister(GameObject customer)
    {
        // Play the register SFX once when the customer starts moving to register
        if (registerSfxClip != null)
        {
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(registerSfxClip);
            }
            else
            {
                // fallback: play at customer position
                AudioSource.PlayClipAtPoint(registerSfxClip, customer != null ? customer.transform.position : Vector3.zero);
            }
        }

        Vector3 targetPosition = new Vector3(7.4f, -0.76f, 10f);
        float speed = 5f;
        StartCoroutine(MoveCustomerCoroutine(customer, targetPosition, speed));
        SetCustomerOrderButtonActive(customer, true);
    }

    //Function that moves le customer to the waiting line
    public void moveCustomerToWaitingLine(GameObject customer)
    {
        SetCustomerOrderButtonActive(customer, false);
        Canvas canvas = customer.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            //canvas.gameObject.SetActive(false);
        }   
        Vector3 targetPosition = new Vector3(0, 0, 10f);
        float speed = 5f;
        for (int i = 0; i < customerWaiting.Length; i++)
        {
            if (customerWaiting[i] == null) 
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
                for (int j = 0; j < customerGO.Count; j++)
                {
                    if(customerGO[j] == customer)
                    {
                        customerWaiting[i] = customerGO[j];
                        customers[j].waitingIndex = i;
                        break;
                    }
                }
                break;
            }
        }
        StartCoroutine(MoveCustomerCoroutine(customer, targetPosition, speed));
        registerOccupied = false;
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

    //Function that deletes a customer when their order has been fulfilled
    public bool destroyCustomer(int index)
    {
        if(!(index >= customerGO.Count))
        {
            //Make the Game Object slowly fade away
            StartCoroutine(FadeOutSprite(customerGO[index]));
            //Check if the Customer was an anomaly
            if (customers[index].isAnomaly)
            {
                deleteAnomalyDamage(customers[index], index);
            }
            customerWaiting[customers[index].waitingIndex] = null;
            customers.RemoveAt(index);
            customerGO.RemoveAt(index);

            return true;
        }
        return false;
    }

    public bool destroyCustomer(Customer customer)
    {
        int index = 0;
        bool isEqual = true;
        for (int i = 0; i < customers.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (customers[i].order[j] != customer.order[j])
                {
                    isEqual = false;
                }
            }
            if (isEqual)
            {
                index = i;
                break;
            }
        }
        if (!(index >= customerGO.Count))
        {
            //Make the Game Object slowly fade away
            StartCoroutine(FadeOutSprite(customerGO[index]));
            if (customers[index].isAnomaly)
            {
                deleteAnomalyDamage(customers[index], index);
            }
            //Remove customer from customers, customerGO, and customerWaiting lists/arrays
            customers.RemoveAt(index);
            customerGO.RemoveAt(index);
            customerWaiting[customer.waitingIndex] = null;

            return true;
        }
        return false;
    }

    public void deleteAnomalyDamage(Customer customer, int index)
    {
        switch (customer.anomalyIndex)
        {
            case 0:
                if(index != 0)
                {
                    if (customers[0] != null)
                    {
                        customers[0].dispelHallicination();
                    }
                }
                break;
            case 1:
                //Do nothing lmao
                break;
            case 2:
                //Do nothing lmao
                break;
            case 3:
                //Do nothing lmao
                break; 
            case 4:
                AnomalyManager.backroomRenderer.sprite = AnomalyManager.normalBackRoomSprite;
                break;
            case 5:
                AnomalyManager.backroomRenderer.sprite = AnomalyManager.normalBackRoomSprite;
                break;
            case 6:
                AnomalyManager.backroomRenderer.sprite = AnomalyManager.normalBackRoomSprite;
                break;
            case 7:
                AnomalyManager.mainRoomRenderer.sprite = AnomalyManager.normalRoomSprite;
                break;
            case 8:
                AnomalyManager.TVRenderer.sprite = AnomalyManager.currentTVSprite;
                break;
            case 9:
                //Do nothing lmao?
                break;
        }
    }

    public IEnumerator FadeOutSprite(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Color color = sr.color;
        float startAlpha = color.a;

        float fadeDuration = 2f;

        for(float t = 0; t < fadeDuration; t+= Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, 0f, normalizedTime);
            sr.color = color;
            yield return null;
        }

        color.a = 0f;
        sr.color = color;
    }

    public void StartCustomerSpawnTimer()
    {
        if (customerSpawnTimer != null)
        {
            customerSpawnTimer.Stop();
            customerSpawnTimer.Dispose();
        }

        double interval = Random.Range(5000, 10000); // 5-10 seconds
        customerSpawnTimer = new System.Timers.Timer(interval);
        customerSpawnTimer.Elapsed += (s, e) => { spawnRequested = true; };
        customerSpawnTimer.Start();
    }

    private void SetCustomerOrderButtonActive(GameObject customer, bool active)
    {
        if (customer == null) return;

        Button orderButton = customer.GetComponentInChildren<Button>(true);
        if (orderButton != null)
        {
            orderButton.gameObject.SetActive(active);
        }
    }

    //DELETE THIS SIDDU, BRI, JUAN, JAHYR
    private IEnumerator timewaste(GameObject newCustomer, float time)
    {
        yield return new WaitForSeconds(time);
        moveCustomerToWaitingLine(newCustomer);
    }
}
