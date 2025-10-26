using System.Collections.Generic;
using UnityEngine;

public class Anomaly10 : Anomaly
{
    public int customerIndex;

    public Anomaly10()
    {

    }
    public void checkSpawnConditions(List<Customer> customerList)
    {
        if (customerList.Count != 0)
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
    public override bool CanSpawn()
    {
        return customerIndex > -1;
    }
    public override void ApplyToCustomer(Customer customer)
    {
        throw new System.NotImplementedException();
    }

    public void changeGameObjectSprite(GameObject kettle, SpriteRenderer normalKettle, Sprike hotKettle, float xpos, float ypos, float zpos)
    {
        kettle.transform.position = new Vector3(xpos, ypos, zpos);
        normalKettle.sprite = hotKettle;
    }
}
