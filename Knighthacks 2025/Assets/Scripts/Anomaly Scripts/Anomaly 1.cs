using Unity.VisualScripting;
using UnityEngine;

public class Anomaly1
{
    //Spawn Conditions:
    // -Customers exist (denoted by n)
    // -Customer has ordered and is still waiting for order
    // -If conditions are met, return index of customer to hallucinate.
    // -If conditions are NOT met, return -1
    public int canSpawn(Customer[] customerList)
    {
        if(customerList.Length != 0)
        {
            for (int i = 0; i < customerList.Length; i++) 
            {
                if (customerList[i].hasOrdered == true && customerList[i].served == false)
                {
                    return i;
                }
            }
        }
        return -1;
    }
}
