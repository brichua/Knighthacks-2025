using UnityEngine;

public class Anomaly8 : Anomaly
{
    //Handles the Creepy Character appearance change
    public Sprite characterChangeSprite;

    public Anomaly8()
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
