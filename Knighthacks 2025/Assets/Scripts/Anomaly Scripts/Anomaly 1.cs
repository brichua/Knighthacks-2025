using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Anomaly1 : Anomaly
{
    //Variables
    public int customerIndex;

    public Anomaly1()
    {
        //do nothing lmao
    }

    //Spawn Conditions:
    // -Customers exist (denoted by n)
    // -Customer has ordered and is still waiting for order
    // -If conditions are met, return index of customer to hallucinate.
    // -If conditions are NOT met, return -1
    public void checkSpawnConditions(List<Customer> customerList)
    {
        if(customerList.Count != 0)
        {
            for (int i = 0; i < customerList.Count; i++) 
            {
                if (customerList[i].hasOrdered == true && customerList[i].served == false)
                {
                    customerIndex = i;
                    return;
                }
            }
        }
        customerIndex = -1;
    }

    //Actually returns whether or not a customer can spawn
    public override bool canSpawn()
    {
        return customerIndex > -1;
    }
}
