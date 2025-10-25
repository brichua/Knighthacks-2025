using UnityEngine;

public class Anomaly8 : Anomaly
{
    //Handles the minor change to room appearance
    public Sprite roomChangeSprite;

    public Anomaly8()
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

    public void changeGameObjectSprite(SpriteRenderer obj)
    {
        obj.sprite = roomChangeSprite;
    }
}
