using System.Timers;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public List<Customer> customers = new List<Customer>();
    public AnomalyManager AnomalyManager;
    static Timer customerSpawnTimer;
    bool spawnRequested = false;
    bool stopSpawning = false;
    string[] pastryTypes = { "cookie", "cake", "cheese", "cracker" };
    string[] teaFlowerTypes = { "rose", "bluebell", "daisy" };
    string[] drinkTypes = { "black", "green", "oolong" };

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
        //Generate Prefab
        GameObject newCustomer = Instantiate(customerPrefab, new Vector3(Random.Range(-5f, 5f), 0, 0), Quaternion.identity);
        Customer customer = newCustomer.GetComponent<Customer>();

        //Generate Order
        int snackRoll = Random.Range(0, pastryTypes.Length);
        int flowerRoll = Random.Range(0, teaFlowerTypes.Length);
        int drinkRoll = Random.Range(0, drinkTypes.Length);
        string[] choices = { drinkTypes[drinkRoll], pastryTypes[snackRoll], teaFlowerTypes[flowerRoll] };
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
    }
}
