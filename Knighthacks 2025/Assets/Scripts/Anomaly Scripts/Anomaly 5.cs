using UnityEngine;

public class Anomaly5 : Anomaly
{

    public Anomaly5()
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
    public void makeGameObjectInvisible(SpriteRenderer obj)
    {
        Color invisibleColor = obj.color;
        invisibleColor.a = 0f;
        obj.color = invisibleColor;
    }
}
