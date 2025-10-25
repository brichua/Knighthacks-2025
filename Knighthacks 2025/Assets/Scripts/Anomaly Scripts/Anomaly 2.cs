using UnityEngine;

public class Anomaly2 : Anomaly
{
    //Handles the Obvious Character appearance change
    public Sprite characterChangeSprite;

    public Anomaly2()
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
