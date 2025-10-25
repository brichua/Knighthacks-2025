using UnityEngine;

public class Anomaly6 : Anomaly
{

    public Anomaly6()
    {

    }
    public override bool CanSpawn()
    {
        return true;
    }
    public override void ApplyToCustomer(Customer customer)
    {
        throw new System.NotImplementedException();
    }

    // Set temp sprite so that we can swap sprites
    public void swapSprites(SpriteRenderer stock1, SpriteRenderer stock2, Sprite sprite1, Sprite sprite2)
    {
        Sprite tempSprite1 = sprite1;
        stock1.sprite = sprite2;
        stock2.sprite = tempSprite1;
    }
}
