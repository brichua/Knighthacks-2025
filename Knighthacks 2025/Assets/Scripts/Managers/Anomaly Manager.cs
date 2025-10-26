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
    public Sprite tvRoomSprite;


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
        // Only trigger anomalies when the player has looked back
        if (customerManager.TaskManager.lookedBack)
        {
            customerManager.TaskManager.lookedBack = false;

            Customer currentOrdering = customerManager.TaskManager.currentOrderingCustomer;
            Debug.Log(currentOrdering);

            for (int i = 0; i < customerManager.customers.Count; i++)
            {
                Customer c = customerManager.customers[i];

                // Skip if this customer is the one currently being served at the register
                if (c == currentOrdering)
                    break;

                // If the customer is an anomaly and hasn't been activated yet, activate it
                if (c.isAnomaly && !c.hasActivated)
                {
                    Debug.Log("Activating anomaly for customer at index " + i + " with anomaly index " + c.anomalyIndex);
                    switch (c.anomalyIndex)
                    {
                        case 0:
                            c.updateHallucination();
                            c.hasActivated = true;
                            break;
                        case 1:
                            c.anomaly.ApplyToCustomer(c);
                            break;
                        case 2:
                            c.anomaly.ApplyToCustomer(c);
                            break;
                        case 3:
                            c.anomaly.ApplyToCustomer(c);
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
                            //case 8:
                            //    currentTVSprite = TVRenderer.sprite;
                            //    TVRenderer.sprite = tvErrorSprite;
                            //    mainRoomRenderer.sprite = tvRoomSprite;
                            break;
                            // case 9:
                            //     c.anomaly.changeGameObjectSprite(kettle, kettleRenderer, kettleSprite, -227f, 26f, 10);
                            //     break;
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
                odds = 6;
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
        while (true)
        {
            if (iterations++ == 1000) { return false; }
            rollAnomaly = Random.Range(1, possibleAnomalies.Length);
            switch (rollAnomaly)
            {
                case 0:
                    //Hallucinate Order Anomaly
                    Anomaly1 anomaly1 = new Anomaly1();
                    anomaly1.checkSpawnConditions(customerManager.customers);
                    if (anomaly1.CanSpawn() == true)
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
                    if (anomaly2.CanSpawn() == true)
                    {
                        //Generate Anomaly!
                        activeAnomalies.Add(possibleAnomalies[1]);
                        customer.anomaly = anomaly2;
                        customer.anomalyIndex = 1;
                        //anomaly2.ApplyToCustomer(customer);
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
                    Anomaly5 anomaly5 = new Anomaly5(normalRoomSprite, missingBackRoomSprite);
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
                    Anomaly6 anomaly6 = new Anomaly6(normalRoomSprite, swappedBackRoomSprite);
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
                    Anomaly7 anomaly7 = new Anomaly7(normalRoomSprite, sameLabelBackRoomSprite);
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
                //case 7:
                //    // Minor room change
                //    if (gameManager.getHealth() == 1)
                //    {
                //        normalTVSprite = tvStatus1;
                //    }
                //    else if (gameManager.getHealth() == 2)
                //    {
                //        normalTVSprite = tvStatus2;
                //    }
                //    else
                //    {
                //        normalTVSprite = tvStatus3;
                //    }
                //        Anomaly9 anomaly9 = new Anomaly9(normalTVSprite, anomalyTVSprite);
                //    if (anomaly9.CanSpawn() == true)
                //    {
                //        activeAnomalies.Add(possibleAnomalies[8]);
                //        customer.anomaly = anomaly9;
                //        customer.anomalyIndex = 8;
                //        // Swap in sprite for the TV sprite renderer
                //        //anomaly9.changeGameObjectSprite(customer.SpriteRenderer);
                //        return true;
                //    }
                //    break;
                /*case 9:
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
                    break;*/
                default:
                    //Do nothing lmao?
                    break;
            }
        }
    }
}