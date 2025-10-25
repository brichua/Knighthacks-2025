using UnityEngine;

public class Anomaly7 : Anomaly
{

    public Anomaly7()
    {

    }
    public override bool canSpawn()
    {
        return true;
    }

    public void copyLabels(SpriteRenderer changedItem, Sprite copiedItem)
    {
        changedItem.sprite = copiedItem;
    }
}
