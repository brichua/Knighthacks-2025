using System.Timers;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public List<Customer> customers = new List<Customer>();
    public AnomalyManager AnomalyManager;
    static Timer customerSpawnTimer;
    bool spawnRequested = false;
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
    }

    void SpawnCustomer()
    {
        int snackRoll = Random.Range(0, pastryTypes.Length);
        int flowerRoll = Random.Range(0, teaFlowerTypes.Length);
        int drinkRoll = Random.Range(0, drinkTypes.Length);
        string[] choices = { drinkTypes[drinkRoll], pastryTypes[snackRoll], teaFlowerTypes[flowerRoll] };

        //Roll for if customer will be an anomaly
        if (AnomalyManager.rollForAnomaly()) {
            // Customer is an anomaly
            Customer customer = new Customer(choices, true);
            AnomalyManager.generateAnomaly(customer);
        }
        else {
            // normal customer
            Customer customer = new Customer(choices, false);
        }
    }
}
