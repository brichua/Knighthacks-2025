using UnityEngine;

public class Anomaly3 : Anomaly
{
    //Handles the Subtle Character appearance change
    public Sprite characterChangeSprite;

    public Anomaly3()
    {

    }
    public override bool canSpawn()
    {
        return true;
    }

    public void changeGameObjectSprite(SpriteRenderer obj)
    {
        obj.sprite = characterChangeSprite;
    }
}
