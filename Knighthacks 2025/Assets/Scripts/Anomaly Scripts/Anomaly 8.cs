using UnityEngine;

public class Anomaly8 : Anomaly
{
    //Handles the minor change to room appearance
    public Sprite roomChangeSprite;

    public Anomaly8()
    {

    }
    public override bool canSpawn()
    {
        return true;
    }

    public void changeGameObjectSprite(SpriteRenderer obj)
    {
        obj.sprite = roomChangeSprite;
    }
}
