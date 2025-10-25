using UnityEngine;

public class Anomaly10 : Anomaly
{
    //Handles the Creepy Character appearance change
    public Sprite characterChangeSprite;

    public Anomaly10()
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
