using UnityEngine;

public abstract class Anomaly
{
    //public int anomalyIndex;
    public abstract bool CanSpawn();
    public abstract void ApplyToCustomer(Customer customer);

}
