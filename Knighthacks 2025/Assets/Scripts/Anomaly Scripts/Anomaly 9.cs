using UnityEngine;

public class Anomaly9 : Anomaly
{
    public Sprite goodTVSprite;
    public Sprite evilTVSprite;
    public Sprite tvRoomSprite;
    public Sprite regularRoomSprite;

    public Anomaly9(Sprite goodTVSprite, Sprite evilTVSprite)
    {
        this.goodTVSprite = goodTVSprite;
        this.evilTVSprite = evilTVSprite;
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

    public void restoreTVSprite(SpriteRenderer obj) 
    {
        obj.sprite = goodTVSprite;
    }
}
