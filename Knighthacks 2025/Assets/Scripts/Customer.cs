using UnityEditor.Build;
using UnityEngine;

public class Customer
{
    //Customer Variables
    public string[] order;
    public bool hasOrdered;
    public bool served;

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
