using UnityEngine;

public abstract class Anomaly : MonoBehaviour
{
    //public string name;

    public Anomaly()
    {
    }

    public abstract bool canSpawn();
}
