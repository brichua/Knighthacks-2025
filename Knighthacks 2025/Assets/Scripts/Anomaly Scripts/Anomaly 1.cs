using Unity.VisualScripting;
using UnityEngine;

public class Anomaly1 : Anomaly
{
    //Variables
    private int customerIndex;

    public Anomaly1()
    {
        //do nothing lmao
    }

    //Spawn Conditions:
    // -Customers exist (denoted by n)
    // -Customer has ordered and is still waiting for order
    // -If conditions are met, return index of customer to hallucinate.
    // -If conditions are NOT met, return -1
    public void checkSpawnConditions(Customer[] customerList)
    {
        if(customerList.Length != 0)
        {
            for (int i = 0; i < customerList.Length; i++) 
            {
                if (customerList[i].hasOrdered == true && customerList[i].served == false)
                {
                    customerIndex = i;
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
