using UnityEditor.Build;
using UnityEngine;

public class Customer
{
    //Customer Variables
    //Order index 0 is for the Drink, order index 1 is for the Snack, order index 2 is for tea flower.
    public string[] order = new string[3];
    public bool hasOrdered;
    public bool served;
    public bool isAnomaly;
    public SpriteRenderer SpriteRenderer;

    //Anomaly Variables
    public bool hallucinating;
    public string[] fakeOrder;

    public Customer (string[] order, bool isAnomaly)
    {
        this.order = order;
        this.isAnomaly = isAnomaly;
    }

    public void updateHallucination()
    {
        hallucinating = true;
        fakeOrder = order;
        fakeOrder[1] = null;
    }
}
