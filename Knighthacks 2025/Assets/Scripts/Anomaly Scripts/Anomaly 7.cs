using UnityEngine;

public class Anomaly7 : Anomaly
{
    //Handles the Creepy Character appearance change
    public Sprite characterChangeSprite;

    public Anomaly7()
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
