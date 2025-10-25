using UnityEditor.Build;
using UnityEngine;

public class Customer
{
    //Customer Variables
    //Order index 0 is for the Drink, order index 1 is for the Snack.
    public string[] order;
    public bool hasOrdered;
    public bool served;
    public bool isAnomaly;

    //Anomaly Variables
    public bool hallucinating;
    public string[] fakeOrder;

    public Customer (string[] order)
    {
        this.order = order;
    }

    public void updateHallucination()
    {
        hallucinating = true;
        fakeOrder = order;
        fakeOrder[1] = null;
    }
}
