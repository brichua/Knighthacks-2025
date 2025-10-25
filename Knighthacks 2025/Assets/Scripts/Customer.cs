using System;
using UnityEditor.Build;
using UnityEngine;

public class Customer : MonoBehaviour
{
    //Customer Variables
    public Sprite[] possibleNormalSprites;
    public SpriteRenderer SpriteRenderer;

    //Order index 0 is for the Drink, order index 1 is for the Snack, order index 2 is for tea flower.
    public string[] order = new string[3];
    public bool hasOrdered;
    public bool served;
    public bool isAnomaly;
    public int spriteIndex;

    //Anomaly Variables
    public bool hallucinating;
    public string[] fakeOrder;

    public void Initialize (string[] order, bool isAnomaly, int spriteIndex)
    {
        this.order = order;
        this.isAnomaly = isAnomaly;
        this.spriteIndex = spriteIndex;
        this.SpriteRenderer.sprite = possibleNormalSprites[spriteIndex * 2 + UnityEngine.Random.Range(0, 2)];
    }

    public void updateHallucination()
    {
        hallucinating = true;
        fakeOrder = (string[])order.Clone();
        fakeOrder[1] = null;
    }
}
