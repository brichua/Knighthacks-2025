using System;
using UnityEditor.Build;
using UnityEngine;

public class Customer : MonoBehaviour
{
    //Customer Variables
    public Sprite[] possibleNormalSprites;
    public SpriteRenderer SpriteRenderer;
    //Order index 0 is for the Drink, order index 1 is for the drink size, order index 2
    //is the Snack, order index 3 is for tea flower.
    public string[] order = new string[4];
    //Declare sprites for order
    public Sprite[] orderSprites = new Sprite[3];
    public bool hasOrdered;
    public bool served;
    public bool isAnomaly;
    public int spriteIndex;

    //Anomaly Variables
    public bool hallucinating;
    public string[] fakeOrder;

    public void Initialize (string[] order, bool isAnomaly, int spriteIndex, Sprite[] orderSprites)
    {
        this.order = order;
        this.isAnomaly = isAnomaly;
        this.spriteIndex = spriteIndex;
        this.SpriteRenderer.sprite = possibleNormalSprites[spriteIndex * 2 + UnityEngine.Random.Range(0, 2)];
        this.orderSprites = orderSprites;
    }

    public void updateHallucination()
    {
        hallucinating = true;
        fakeOrder = (string[])order.Clone();
        fakeOrder[2] = null;
    }
}
