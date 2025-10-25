using UnityEngine;

public class Anomaly9 : Anomaly
{
    public Sprite evilTVSprite;

    public Anomaly9()
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
        obj.sprite = evilTVSprite;
    }
}
