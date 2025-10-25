using UnityEngine;

public class Anomaly7 : Anomaly
{

    public Anomaly7()
    {

    }
    public override bool CanSpawn()
    {
        return true;
    }
    public override void ApplyToCustomer(Customer customer)
    {
        throw new System.NotImplementedException();
    }

    public void copyLabels(SpriteRenderer changedItem, Sprite copiedItem)
    {
        changedItem.sprite = copiedItem;
    }
}
