using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;


public class AnomalyManager : MonoBehaviour
{
    //Variables for AnomalyManager
    public CustomerManager customerManager;
    public GameManager gameManager;
    public TaskManager taskManager;
    public GameObject[] possibleAnomalies;
    public List<GameObject> activeAnomalies;
    public int startingOdds = 5;
    private int odds;

    //Variables that will be passed into different anomaly types
    public Sprite[] possibleObviousSprites;
    public Sprite[] possibleSubtleSprites;
    public Sprite[] possibleSpookySprites;

    public SpriteRenderer mainRoomRenderer;
    public Sprite normalRoomSprite;
    public Sprite anomalyRoomSprite;

    public SpriteRenderer backroomRenderer;
    public Sprite normalBackRoomSprite;
    public Sprite missingBackRoomSprite;
    public Sprite swappedBackRoomSprite;
    public Sprite sameLabelBackRoomSprite;


    public Sprite tvStatus1;
    public Sprite tvStatus2;
    public Sprite tvStatus3;
    public Sprite normalTVSprite;
    public Sprite anomalyTVSprite;
    public Sprite currentTVSprite;

    public SpriteRenderer TVRenderer;
    public Sprite tvErrorSprite;

    public GameObject kettle;
    public SpriteRenderer kettleRenderer;
    public Sprite kettleSprite;


    void Awake() 
    {
        odds = startingOdds;
    }

    private void Update()
    {
        //This will check if the user is turned to the backside to trigger anomalies
        if (customerManager.TaskManager.lookedBack == true)
        {
            customerManager.TaskManager.lookedBack = false;
            for (int i = 0; i < customerManager.customers.Count; i++)
            {
                //If the customer is an anomaly AND has not been activated, activate it
                if (customerManager.customers[i].isAnomaly == true && customerManager.customers[i].hasActivated == false)
                {
                    switch (customerManager.customers[i].anomalyIndex)
                    {
                        case 0:
                            customerManager.customers[i].updateHallucination();
                            customerManager.customers[i].hasActivated = true;
                            break;
                        case 1:
                            customerManager.customers[i].anomaly.ApplyToCustomer(customerManager.customers[i]);
                            break;
                        case 2:
                            customerManager.customers[i].anomaly.ApplyToCustomer(customerManager.customers[i]);
                            break;
                        case 3:
                            customerManager.customers[i].anomaly.ApplyToCustomer(customerManager.customers[i]);
                            break;
                        case 4:
                            backroomRenderer.sprite = missingBackRoomSprite;
                            break;
                        case 5:
                            backroomRenderer.sprite = swappedBackRoomSprite;
                            break;
                        case 6:
                            backroomRenderer.sprite = sameLabelBackRoomSprite;
                            break;
                        case 7:
                            mainRoomRenderer.sprite = anomalyRoomSprite;
                            break;
                        case 8:
                            currentTVSprite = TVRenderer.sprite;
                            TVRenderer.sprite = tvErrorSprite;
                            break;
                        case 9:
                            customerManager.customers[i].anomaly.changeGameObjectSprite(kettle, kettleRenderer, kettleSprite, -227f, 26f, 10);
                            break;
                    }
                }
            }
        }
    }

    //Determines if a customer rolls into being an anomaly
    public bool rollForAnomaly()
    {
        while (true)
        {
            int roll = Random.Range(1, odds);
            if (roll <= 1)
            {
                //Succeeds, generate random Anomaly
                odds = 5;
                return true;
            }
            //Fails
            odds -= 2;
            return false;
        }
    }

    //Generates an anomoly, if it succeeds, add the anomaly to the active anomalies list and return true.
    //If it bizarrely fails, return false
    public bool generateAnomaly(Customer customer)
    {
        int iterations = 0;
        int rollAnomaly;
        while(true)
        {
            if(iterations++ == 1000) { return false; }
            rollAnomaly = Random.Range(0, possibleAnomalies.Length);
            switch (rollAnomaly){
                case 0:
                    //Hallucinate Order Anomaly
                    Anomaly1 anomaly1 = new Anomaly1();
                    anomaly1.checkSpawnConditions(customerManager.customers);
                    if(anomaly1.CanSpawn() == true)
                    {
                        //Generate Anomaly!
                        activeAnomalies.Add(possibleAnomalies[0]);
                        customer.anomaly = anomaly1;
                        customer.anomalyIndex = 0;
                        //customer.updateHallucination();
                        return true;
                    }
                    break;
                case 1:
                    //Obvious Sprite Change Anomaly
                    Anomaly2 anomaly2 = new Anomaly2(possibleObviousSprites);
                    if(anomaly2.CanSpawn() == true)
                    {
                        //Generate Anomaly!
                        activeAnomalies.Add(possibleAnomalies[1]);
                        customer.anomaly = anomaly2;
                        customer.anomalyIndex = 1;
                        //anomaly2.ApplyToCustomer(customer);
                        return true;
                    }
                    break;
                case 2:
                    //Subtle Sprite Change Anomaly
                    Anomaly3 anomaly3 = new Anomaly3(possibleSubtleSprites);
                    if (anomaly3.CanSpawn() == true)
                    {
                        //Generate Anomaly
                        activeAnomalies.Add(possibleAnomalies[2]);
                        customer.anomaly = anomaly3;
                        customer.anomalyIndex = 2;
                        //anomaly3.ApplyToCustomer(customer);
                        return true;
                    }
                    break;
                case 3:
                    //Spooky Sprite Change Anomaly
                    Anomaly4 anomaly4 = new Anomaly4(possibleSpookySprites);
                    if (anomaly4.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[3]);
                        customer.anomaly = anomaly4;
                        customer.anomalyIndex = 3;
                        //anomaly4.ApplyToCustomer(customer);
                        return true;
                    }
                    break;
                case 4:
                    // Stock disappearance anomaly
                    Anomaly5 anomaly5 = new Anomaly5();
                    if (anomaly5.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[4]);
                        customer.anomaly = anomaly5;
                        customer.anomalyIndex = 4;
                        // Make sure to find way to make this stock later
                        //anomaly5.makeGameObjectInvisible(customer.SpriteRenderer);
                        return true;
                    }
                    break;
                case 5:
                    // Stock swap
                    Anomaly6 anomaly6 = new Anomaly6();
                    if (anomaly6.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[5]);
                        customer.anomaly = anomaly6;
                        customer.anomalyIndex = 5;
                        // get this in later lmao
                        //anomaly6.swapSprites();
                        return true;
                    }
                    break;
                case 6:
                    // Stock label copy
                    Anomaly7 anomaly7 = new Anomaly7();
                    if (anomaly7.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[6]);
                        customer.anomaly = anomaly7;
                        customer.anomalyIndex = 6;
                        // get this in later
                        //anomaly7.copyLabels(customer.SpriteRenderer);
                        return true;
                    }
                    break;
                case 7:
                    // Major room change
                    Anomaly8 anomaly8 = new Anomaly8(normalRoomSprite, anomalyRoomSprite);
                    if (anomaly8.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[7]);
                        customer.anomaly = anomaly8;
                        customer.anomalyIndex = 7;
                        // Rooms! (said in same intonation as Log! from CR)
                        //anomaly8.changeGameObjectSprite(customer.SpriteRenderer);
                        return true;
                    }
                    break;
                case 8:
                    // Minor room change
                    if (gameManager.getHealth() == 1)
                    {
                        normalTVSprite = tvStatus1;
                    }
                    else if (gameManager.getHealth() == 2)
                    {
                        normalTVSprite = tvStatus2;
                    }
                    else
                    {
                        normalTVSprite = tvStatus3;
                    }
                        Anomaly9 anomaly9 = new Anomaly9(normalTVSprite, anomalyTVSprite);
                    if (anomaly9.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[8]);
                        customer.anomaly = anomaly9;
                        customer.anomalyIndex = 8;
                        // Swap in sprite for the TV sprite renderer
                        //anomaly9.changeGameObjectSprite(customer.SpriteRenderer);
                        return true;
                    }
                    break;
                case 9:
                    // The water boiler
                    Anomaly10 anomaly10 = new Anomaly10();
                    if (anomaly10.CanSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[9]);
                        customer.anomaly = anomaly10;
                        customer.anomalyIndex = 9;
                        // Implement this, just put spriterenderer for normal kettle on first
                        // and sprite for boil kettle on second
                        //anomaly4.changeGameObjectSprite(customer.SpriteRenderer);
                        return true;
                    }
                    break;
                default:
                    //Do nothing lmao?
                    break;
            }
        }
    }
}
