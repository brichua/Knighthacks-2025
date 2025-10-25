using UnityEngine;
using System.Collections.Generic;


public class AnomalyManager : MonoBehaviour
{
    public CustomerManager customerManager;
    public GameObject[] possibleAnomalies;
    public List<GameObject> activeAnomalies;
    public int odds;

    //Determines if a customer rolls into being an anomaly
    public bool rollForAnomaly()
    {
        while (true)
        {
            int roll = Random.Range(1, odds);
            if (roll <= 1)
            {
                //Succeeds, generate random Anomaly
                odds = 10;
                return true;
            }
            //Fails
            odds -= 2;
            return false;
        }
    }

    //Generates an anomoly, if it succeeds, add the anomaly to the active anomalies list and return true.
    //If it bizarrely fails, return false
    public bool generateAnomaly()
    {
        int iterations = 0;
        int roll;
        while(true)
        {
            if(iterations++ == 1000) { return false; }
            roll = Random.Range(0, possibleAnomalies.Length);
            switch (roll){
                case 0:
                    Anomaly1 anomaly1 = new Anomaly1();
                    anomaly1.checkSpawnConditions(customerManager.customers);
                    if(anomaly1.canSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[0]);
                        return true;
                    }
                    break;
                case 1:
                    Anomaly2 anomaly2 = new Anomaly2();
                    if(anomaly2.canSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[1]);
                        return true;
                    }
                    break;
                case 2:
                    Anomaly3 anomaly3 = new Anomaly3();
                    if (anomaly3.canSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[2]);
                        return true;
                    }
                    break;
                case 3:
                    Anomaly4 anomaly4 = new Anomaly4();
                    if (anomaly4.canSpawn() == true)
                    {
                        activeAnomalies.Add(possibleAnomalies[3]);
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
