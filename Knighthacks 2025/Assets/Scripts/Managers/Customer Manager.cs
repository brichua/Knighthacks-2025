using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public List<Customer> customers = new List<Customer>();
    public List<GameObject> customerGO = new List<GameObject>();
    public List<GameObject> customerLine = new List<GameObject>();
    public int maxCustomers = 12;
    public AnomalyManager AnomalyManager;
    static Timer customerSpawnTimer;
    bool spawnRequested = false;
    public bool stopSpawning = false;

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

        //Generate Order
        int snackRoll = Random.Range(0, pastryTypes.Length);
        int flowerRoll = Random.Range(0, teaFlowerTypes.Length);
        int drinkRoll = Random.Range(0, drinkTypes.Length);
        int drinkSizeRoll = Random.Range(0, drinkSize.Length);
        string[] choices = { drinkTypes[drinkRoll], drinkSize[drinkSizeRoll], pastryTypes[snackRoll], teaFlowerTypes[flowerRoll] };
        //Generate Customer Sprite
        int spriteIndex = Random.Range(0, 2);

        //Roll for if customer will be an anomaly
        if (AnomalyManager.rollForAnomaly()) {
            // Customer is an anomaly
            Debug.Log("Anomaly Spawned");
            customer.Initialize(choices, true, spriteIndex);
            AnomalyManager.generateAnomaly(customer);
        }
        else {
            // normal customer
            Debug.Log("Normal Ass Customer Spawned");
            customer.Initialize(choices, false, spriteIndex);
        }
        newCustomer.SetActive(true);
        customers.Add(customer);
        customerGO.Add(newCustomer);
        moveCustomerToRegister(newCustomer);
        StartCoroutine(timewaste(newCustomer, 7.0f));
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
