using UnityEngine;

public class Anomaly9 : Anomaly
{
    public Sprite evilTVSprite;

    public Anomaly9()
    {

    }
    public override bool canSpawn()
    {
        return true;
    }

    public void changeGameObjectSprite(SpriteRenderer obj)
    {
        obj.sprite = evilTVSprite;
    }
}
