using UnityEngine;

public class Anomaly5 : Anomaly
{

    public Anomaly5()
    {

    }
    public override bool canSpawn()
    {
        return true;
    }

    public void makeGameObjectInvisible(SpriteRenderer obj)
    {
        Color invisibleColor = obj.color;
        invisibleColor.a = 0f;
        obj.color = invisibleColor;
    }
}
