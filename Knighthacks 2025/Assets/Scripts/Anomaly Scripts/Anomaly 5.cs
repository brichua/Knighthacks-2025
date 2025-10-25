using UnityEngine;

public class Anomaly5 : Anomaly
{
    //Handles the Creepy Character appearance change
    public Sprite characterChangeSprite;

    public Anomaly5()
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
