using System.Timers;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public Customer[] customers;
    static Timer customerSpawnTimer;
    bool spawnRequested = false;
    int anomalyChance = 10;
    string[] pastryTypes = { "cookie", "cake", "cheese", "cracker" };
    string[] flowerTypes = { "rose", "bluebell", "daisy" };
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
        int flowerRoll = Random.Range(0, flowerTypes.Length);
        int drinkRoll = Random.Range(0, drinkTypes.Length);
        string snack = flowerTypes[flowerRoll] + " " + drinkTypes[drinkRoll];
        string[] choices = { snack, pastryTypes[snackRoll] };
        if (Random.Range(0, anomalyChance) <= 1) {
            // idk evil customer
            anomalyChance = 10;
            Customer customer = new Customer(choices);
            customer.updateHallucination();
        }
        else {
            // normal customer
            anomalyChance -= 2;
            Customer customer = new Customer(choices);
        }
    }
}
