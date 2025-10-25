using UnityEngine;

public abstract class Anomaly
{
    public abstract bool CanSpawn();
    public abstract void ApplyToCustomer(Customer customer);

}
