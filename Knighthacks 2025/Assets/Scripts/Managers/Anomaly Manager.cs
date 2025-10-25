using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public GameObject[] possibleAnomalies;
    public GameObject[] activeAnomalies;

    public bool rollForAnomaly()
    {
        int roll = UnityEngine.Random.Range(1, possibleAnomalies.Length);
        if (roll <= 1)
        {
            //Succeeds, generate random Anomaly
            return true;
        }
        //Fails
        return false;
    }
}
